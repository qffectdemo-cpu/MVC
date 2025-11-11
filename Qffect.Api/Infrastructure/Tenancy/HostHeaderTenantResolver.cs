using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Qffect.Infrastructure.Persistence;
using Qffect.SharedKernel.Tenancy;

namespace Qffect.Api.Infrastructure.Tenancy;

/// <summary>
/// Production resolver:
/// - Tenant slug comes from:
///   1) Header "X-Qffect-Tenant" (reverse proxies / on-prem)
///   2) Subdomain:  acme.myqffect.com  -> "acme"  (config Tenancy:BaseDomain)
///   3) Fallback Tenancy:DefaultTenant
/// - Loads tenant + enabled modules from settings DB
/// - Caches per slug for Tenancy:CacheSeconds (default 60)
/// </summary>
public sealed class HostHeaderTenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _http;
    private readonly IConfiguration _cfg;
    private readonly QffectSettingsDbContext _db;
    private readonly IMemoryCache _cache;

    public HostHeaderTenantResolver(
        IHttpContextAccessor http,
        IConfiguration cfg,
        QffectSettingsDbContext db,
        IMemoryCache cache)
    {
        _http = http;
        _cfg = cfg;
        _db = db;
        _cache = cache;
    }

    public async Task<TenantContext> ResolveAsync(CancellationToken ct = default)
    {
        var httpContext = _http.HttpContext
            ?? throw new InvalidOperationException(
                "No HttpContext. Ensure AddHttpContextAccessor() is registered and call within a request.");

        // 1) Header wins (simple & explicit for proxies/on-prem)
        string? slug = null;
        if (httpContext.Request.Headers.TryGetValue("X-Qffect-Tenant", out StringValues hv) &&
            !StringValues.IsNullOrEmpty(hv))
        {
            slug = hv.ToString();
        }

        // 2) Otherwise, use subdomain (e.g., acme.myqffect.com)
        if (string.IsNullOrWhiteSpace(slug))
        {
            var host = httpContext.Request.Host.Host.ToLowerInvariant();
            var baseDomain = _cfg.GetValue<string>("Tenancy:BaseDomain"); // e.g. "myqffect.com"
            if (!string.IsNullOrWhiteSpace(baseDomain) &&
                host.EndsWith(baseDomain, StringComparison.OrdinalIgnoreCase))
            {
                var left = host[..^baseDomain.Length].TrimEnd('.');
                if (!string.IsNullOrWhiteSpace(left))
                {
                    // take rightmost label before base domain: foo.bar.myqffect.com -> "bar"
                    var parts = left.Split('.', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0) slug = parts[^1];
                }
            }
        }

        // 3) Fallback
        if (string.IsNullOrWhiteSpace(slug))
            slug = _cfg.GetValue<string>("Tenancy:DefaultTenant") ?? "default";

        var cacheSeconds = Math.Max(5, _cfg.GetValue<int?>("Tenancy:CacheSeconds") ?? 60);
        var cacheKey = $"tenantctx::{slug}";

        if (_cache.TryGetValue(cacheKey, out TenantContext? cached) && cached is not null)
            return cached;

        var tenant = await _db.Tenants
            .AsNoTracking()
            .Include(t => t.ModuleSettings)
            .FirstOrDefaultAsync(t => t.Slug == slug, ct);

        if (tenant is null)
            throw new InvalidOperationException(
                $"Unknown tenant slug '{slug}'. Configure ADM.Tenants in the settings database.");

        var enabled = tenant.ModuleSettings
            .Where(m => m.IsEnabled)
            .Select(m => m.ModuleCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Map to TenantContext (adjust Name/Edition if your columns differ)
        var context = new TenantContext
        {
            TenantId = tenant.TenantId,
            TenantName = tenant.Name ?? slug,   // <-- adjust if your columns differ
            Edition = tenant.Edition ?? "Standard", // <-- adjust if your columns differ
            EnabledModules = enabled
        };

        _cache.Set(cacheKey, context, TimeSpan.FromSeconds(cacheSeconds));
        return context;
    }
}

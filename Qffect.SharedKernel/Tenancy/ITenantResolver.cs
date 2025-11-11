namespace Qffect.SharedKernel.Tenancy;

public interface ITenantResolver
{
    // No HttpContext here; stays web-agnostic.
    Task<TenantContext> ResolveAsync(CancellationToken ct = default);
}

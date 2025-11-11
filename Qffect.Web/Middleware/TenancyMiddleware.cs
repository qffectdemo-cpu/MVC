using Microsoft.AspNetCore.Http;
using Qffect.SharedKernel.Tenancy;

namespace Qffect.Web.Middleware;

public sealed class TenancyMiddleware
{
    private readonly RequestDelegate _next;
    public TenancyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantResolver resolver)
    {
        var tenant = await resolver.ResolveAsync(context.RequestAborted);
        context.Items[nameof(TenantContext)] = tenant;
        await _next(context);
    }
}

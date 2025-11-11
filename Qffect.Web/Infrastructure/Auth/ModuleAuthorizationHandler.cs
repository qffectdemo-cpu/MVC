using Microsoft.AspNetCore.Authorization;
using Qffect.SharedKernel.Tenancy;

namespace Qffect.Web.Infrastructure.Auth;

public sealed class ModuleAuthorizationHandler : AuthorizationHandler<ModuleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ModuleRequirement requirement)
    {
        // TenantContext is attached to HttpContext.Items by tenancy middleware.
        var httpCtx = (context.Resource as Microsoft.AspNetCore.Http.DefaultHttpContext) ??
                      (context.Resource as Microsoft.AspNetCore.Http.HttpContext);
        var tctx = (httpCtx as Microsoft.AspNetCore.Http.HttpContext)?.Items["TenantContext"] as TenantContext;

        if (tctx is not null && IsModuleGranted(tctx, requirement.ModuleCode))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static bool IsModuleGranted(TenantContext tctx, string requestedCode)
    {
        if (tctx.EnabledModules.Contains(requestedCode))
            return true;

        foreach (var granted in tctx.EnabledModules)
        {
            if (requestedCode.Length > granted.Length &&
                requestedCode.StartsWith(granted, StringComparison.OrdinalIgnoreCase) &&
                requestedCode[granted.Length] == '.')
            {
                return true;
            }
        }
        return false;
    }
}

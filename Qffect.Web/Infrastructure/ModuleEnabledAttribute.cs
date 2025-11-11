using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Qffect.SharedKernel.Tenancy;

namespace Qffect.Web.Infrastructure;

/// <summary>
/// Guards Areas/Controllers/Actions by requiring a module code present in TenantContext.EnabledModules.
/// Supports prefix semantics: granting "OPS" implicitly grants any "OPS.*".
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class ModuleEnabledAttribute : ActionFilterAttribute
{
    private readonly string _code;
    public ModuleEnabledAttribute(string code) => _code = code ?? throw new ArgumentNullException(nameof(code));

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Resolve TenantContext (via Items or a dedicated accessor)
        var tctx = context.HttpContext.Items["TenantContext"] as TenantContext;
        if (tctx is null)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            return;
        }

        if (!IsModuleGranted(tctx, _code))
        {
            // 403 is appropriate for an authenticated user lacking permission.
            context.Result = new ForbidResult();
            return;
        }
    }

    /// <summary>
    /// Returns true when an exact code is granted OR any granted code is a prefix of the requested code.
    /// Example: having "OPS" enables "OPS.*"; having "OPS.Inventory" enables "OPS.Inventory.AdjustStock".
    /// </summary>
    private static bool IsModuleGranted(TenantContext tctx, string requestedCode)
    {
        if (tctx.EnabledModules.Contains(requestedCode))
            return true;

        // Prefix rule: "OPS" => grants all "OPS.*"
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Qffect.Web.Infrastructure.Auth;

/// <summary>
/// Creates policies on the fly for names like "Module:OPS.Inventory".
/// </summary>
public sealed class ModulePolicyProvider : IAuthorizationPolicyProvider
{
    public const string Prefix = "Module:";

    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public ModulePolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            var moduleCode = policyName.Substring(Prefix.Length);
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new ModuleRequirement(moduleCode))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallback.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();
}

using Microsoft.AspNetCore.Authorization;

namespace Qffect.Web.Infrastructure.Auth;

public sealed class ModuleRequirement : IAuthorizationRequirement
{
    public string ModuleCode { get; }
    public ModuleRequirement(string moduleCode) => ModuleCode = moduleCode ?? throw new ArgumentNullException(nameof(moduleCode));
}

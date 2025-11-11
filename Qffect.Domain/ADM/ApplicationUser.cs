using Microsoft.AspNetCore.Identity;
using System;

namespace Qffect.Domain.ADM;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }   // required for SaaS isolation
    public string? DisplayName { get; set; }
    public int? DeptId { get; set; }
    public bool IsActive { get; set; } = true;

    // Optional profile
    public string? Locale { get; set; }
    public string? TimeZone { get; set; }
}

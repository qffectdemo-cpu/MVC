using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qffect.Domain.ADM;
using Qffect.SharedKernel.Security;

namespace Qffect.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(RoleManager<ApplicationRole> roles, UserManager<ApplicationUser> users)
    {
        // Roles
        string[] roleNames = { "OrgAdmin", "DeptAdmin", "Manager", "Staff" };
        foreach (var rn in roleNames)
            if (!await roles.Roles.AnyAsync(r => r.Name == rn))
                await roles.CreateAsync(new ApplicationRole { Name = rn, NormalizedName = rn.ToUpper() });

        // Admin user
        var tenantId = Guid.NewGuid(); // replace with fixed GUID when provisioning tenants
        var admin = await users.FindByNameAsync("admin");
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@qffect.local",
                TenantId = tenantId,
                EmailConfirmed = true,
                DisplayName = "System Admin"
            };
            await users.CreateAsync(admin, "Admin#12345");
            await users.AddToRoleAsync(admin, "OrgAdmin");

            // Minimal permissions (add more as modules grow)
            var claims = new[]
            {
                Permissions.ADM.TaskRead, Permissions.ADM.TaskWrite,
                Permissions.QPS.IncidentRead, Permissions.QPS.IncidentWrite
            };
            foreach (var p in claims)
                await users.AddClaimAsync(admin, new System.Security.Claims.Claim("perm", p));
        }
    }
}

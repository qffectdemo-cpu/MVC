using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Qffect.Domain.ADM;

namespace Qffect.Infrastructure.Persistence;

// Identity tables in ADM schema
public class QffectIdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public QffectIdentityDbContext(DbContextOptions<QffectIdentityDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.HasDefaultSchema("ADM");

        b.Entity<ApplicationUser>().ToTable("Users");
        b.Entity<ApplicationRole>().ToTable("Roles");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>().ToTable("UserRoles");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>().ToTable("UserClaims");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>().ToTable("UserLogins");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        b.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>().ToTable("UserTokens");

        b.Entity<ApplicationUser>()
            .HasIndex(u => new { u.TenantId, u.UserName })
            .HasDatabaseName("IX_ADM_Users_Tenant_UserName");
    }
}

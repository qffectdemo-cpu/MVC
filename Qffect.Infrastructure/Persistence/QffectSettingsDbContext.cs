using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Qffect.Infrastructure.Persistence;

public class QffectSettingsDbContext : DbContext
{
    public QffectSettingsDbContext(DbContextOptions<QffectSettingsDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantModuleSetting> TenantModuleSettings => Set<TenantModuleSetting>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("ADM");

        b.Entity<Tenant>(e =>
        {
            e.ToTable("Tenants");
            e.HasKey(x => x.TenantId);
            e.Property(x => x.TenantId).ValueGeneratedNever();
            e.Property(x => x.Slug).HasMaxLength(128).IsRequired();
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Edition).HasMaxLength(32);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasMany(x => x.ModuleSettings)
             .WithOne()
             .HasForeignKey(x => x.TenantId);
        });

        b.Entity<TenantModuleSetting>(e =>
        {
            e.ToTable("TenantModuleSettings");
            e.HasKey(x => new { x.TenantId, x.ModuleCode });
            e.Property(x => x.ModuleCode).HasMaxLength(3).IsRequired(); // ADM, EHS, ...
            e.Property(x => x.IsEnabled).HasDefaultValue(true);
            e.Property(x => x.SettingsJson).HasColumnType("nvarchar(max)");
        });
    }
}

public class Tenant
{
    public Guid TenantId { get; set; }
    public string Slug { get; set; } = ""; // subdomain or identifier
    public string Name { get; set; } = "";
    public string Edition { get; set; } = "Basic";
    public List<TenantModuleSetting> ModuleSettings { get; set; } = new();
}

public class TenantModuleSetting
{
    public Guid TenantId { get; set; }
    public string ModuleCode { get; set; } = "";
    public bool IsEnabled { get; set; } = true;
    public string? SettingsJson { get; set; }
}

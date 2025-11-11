using Microsoft.EntityFrameworkCore;
using Qffect.Domain.ADM;
using Qffect.SharedKernel.Audit;
using System.Reflection;

namespace Qffect.Infrastructure.Persistence;

// Business entities (schema-per-module). You can merge with IdentityDbContext later if desired.
public class QffectDbContext : DbContext
{
    public QffectDbContext(DbContextOptions<QffectDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Employee> Employees { get; set; }
    public DbSet<IncidentRecord> Incidents => Set<IncidentRecord>();
    public DbSet<AuditableEntity> AuditLogs { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<AuditableEntity>();

        // ADM.Tasks mapping
        modelBuilder.Entity<TaskItem>(b =>
        {
            b.ToTable("Tasks", schema: "ADM");
            b.HasKey(x => x.Id).HasName("PK_ADM_Tasks");
            b.Property(x => x.Id).HasColumnName("TaskID");
            b.Property(x => x.TaskTitle).HasMaxLength(200);
            b.Property(x => x.TaskDescription).HasMaxLength(2000);
            b.Property(x => x.RefModuleCode).HasMaxLength(3);
            b.Property(x => x.RefEntity).HasMaxLength(50);
            b.Property(x => x.RefId).HasMaxLength(64);
            b.Property(x => x.OwnerEmpId).HasMaxLength(15);
            b.Property(x => x.IsDeleted).HasDefaultValue(false);
            b.HasIndex(x => new { x.RefModuleCode, x.RefEntity, x.RefId }).HasDatabaseName("IX_ADM_Tasks_Ref");
        });

        base.OnModelCreating(modelBuilder);
    }
    public override int SaveChanges()
    {
        ApplyAuditing();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditing();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditing()
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            // Only audit entities marked [Auditable]
            var isAuditable = entry.Entity.GetType()
                .GetCustomAttribute<AuditableAttribute>() != null;

            if (!isAuditable)
                continue;

            var userId = "system"; // Replace with actual user ID using IHttpContextAccessor

            // Call your method instead of manually setting properties
            entry.Entity.SetAuditInfo(userId);
        }
    }

}

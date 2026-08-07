namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant.Jurisdiction;

using Microsoft.EntityFrameworkCore;
using TubieTools_Aspire.EnterpriseAutomation.MultiTenant.Jurisdiction;

public partial class FoundryDbContext : DbContext
{
    public DbSet<JurisdictionConfig> Jurisdictions { get; set; }
    public DbSet<TenantJurisdictionMapping> TenantJurisdictionMappings { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // JurisdictionConfig configuration
        modelBuilder.Entity<JurisdictionConfig>(entity =>
        {
            entity.HasKey(e => e.JurisdictionId);
            entity.Property(e => e.StateCode).IsRequired().HasMaxLength(2);
            entity.Property(e => e.JurisdictionName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.StateCode).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        // TenantJurisdictionMapping configuration
        modelBuilder.Entity<TenantJurisdictionMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId);
            entity.HasIndex(e => new { e.TenantId, e.JurisdictionId }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.IsPrimary });
            //entity.HasForeignKey(e => e.JurisdictionId)
            //      .HasPrincipalTable(nameof(Jurisdictions))
            //      .HasPrincipalKey(e => e.JurisdictionId)
            //      .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
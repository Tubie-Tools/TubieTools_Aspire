using Microsoft.EntityFrameworkCore;
using TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

namespace TubieTools_CopilotStudio_API.Data;

/// <summary>
/// Entity Framework Core DbContext for Copilot Studio.
/// Configured for SQL Server with proper entity mappings.
/// </summary>
public class CopilotStudioDbContext : DbContext
{
    public CopilotStudioDbContext(DbContextOptions<CopilotStudioDbContext> options)
        : base(options)
    {
    }

    // Copilot core entities
    public DbSet<CopilotApplication> CopilotApplications { get; set; } = null!;
    public DbSet<CopilotModelConfiguration> ModelConfigurations { get; set; } = null!;
    public DbSet<KnowledgeTool> KnowledgeTools { get; set; } = null!;
    public DbSet<CopilotGovernancePolicy> GovernancePolicies { get; set; } = null!;
    public DbSet<CopilotPerformanceMetrics> PerformanceMetrics { get; set; } = null!;
    public DbSet<CopilotDeploymentConfig> DeploymentConfigs { get; set; } = null!;
    public DbSet<CopilotVersion> Versions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CopilotApplication - Primary entity
        modelBuilder.Entity<CopilotApplication>(entity =>
        {
            entity.HasKey(e => e.CopilotId);
            entity.Property(e => e.CopilotId)
                .IsRequired()
                .HasMaxLength(36);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.LandingZone)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.LastModifiedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.LandingZone);
            entity.HasIndex(e => e.IsActive);
        });

        // CopilotModelConfiguration
        modelBuilder.Entity<CopilotModelConfiguration>(entity =>
        {
            entity.HasKey(e => e.ConfigId);
            entity.Property(e => e.ConfigId)
                .IsRequired()
                .HasMaxLength(36);

            entity.Property(e => e.ModelName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.SystemPrompt)
                .HasMaxLength(3000);

            // SafetySettings is an owned entity
            entity.OwnsOne(e => e.SafetySettings);

            // CustomParameters as JSON
            entity.Property(e => e.CustomParameters)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new());
        });

        // KnowledgeTool
        modelBuilder.Entity<KnowledgeTool>(entity =>
        {
            entity.HasKey(e => e.ToolId);
            entity.Property(e => e.ToolId)
                .IsRequired()
                .HasMaxLength(36);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(e => e.Name);
        });

        // CopilotGovernancePolicy
        modelBuilder.Entity<CopilotGovernancePolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId);
            entity.Property(e => e.PolicyId)
                .IsRequired()
                .HasMaxLength(36);

            entity.Property(e => e.LandingZone)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PolicyName)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(e => e.LandingZone);
        });

        // CopilotPerformanceMetrics
        modelBuilder.Entity<CopilotPerformanceMetrics>(entity =>
        {
            entity.HasKey(e => e.MetricsId);
            entity.Property(e => e.MetricsId)
                .IsRequired()
                .HasMaxLength(36);

            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        // CopilotDeploymentConfig
        modelBuilder.Entity<CopilotDeploymentConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigId);
            entity.Property(e => e.ConfigId)
                .IsRequired()
                .HasMaxLength(36);

            entity.Property(e => e.Environment)
                .IsRequired()
                .HasMaxLength(50);

            // HealthCheck is an owned entity
            entity.OwnsOne(e => e.HealthCheck);
        });

        // CopilotVersion
        modelBuilder.Entity<CopilotVersion>(entity =>
        {
            entity.HasKey(e => e.VersionId);
            entity.Property(e => e.VersionId)
                .IsRequired()
                .HasMaxLength(36);

            entity.Property(e => e.VersionNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ReleaseDate)
                .IsRequired();

            // Changes as JSON
            entity.Property(e => e.Changes)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<VersionChange>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new());

            // BreakingChanges as JSON
            entity.Property(e => e.BreakingChanges)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new());

            // Deprecations as JSON
            entity.Property(e => e.Deprecations)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new());
        });
    }
}

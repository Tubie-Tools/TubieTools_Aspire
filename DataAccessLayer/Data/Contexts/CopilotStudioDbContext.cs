using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Data.Entities;

namespace DataAccessLayer.Data.Contexts;

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

            entity.Property(e => e.Capabilities);
            entity.Property(e => e.GuidelinesAdherence);

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.LastModifiedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.LandingZone);
            entity.HasIndex(e => e.IsActive);

            // Foreign key relationships
            entity.HasOne(e => e.ModelConfiguration)
                .WithMany()
                .HasForeignKey("ModelConfigurationId")
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.GovernancePolicy)
                .WithMany(p => p.CopilotApplications)
                .HasForeignKey("GovernancePolicyId")
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PerformanceMetrics)
                .WithMany()
                .HasForeignKey("PerformanceMetricsId")
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.DeploymentConfig)
                .WithMany()
                .HasForeignKey("DeploymentConfigId")
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.KnowledgeTools)
                .WithOne(k => k.CopilotApplication)
                .HasForeignKey(k => k.CopilotApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.VersionHistory)
                .WithOne(v => v.CopilotApplication)
                .HasForeignKey(v => v.CopilotId)
                .OnDelete(DeleteBehavior.Cascade);
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

            // CustomParameters as JSON string
            entity.Property(e => e.CustomParameters);

            // SafetySettings as JSON string
            entity.Property(e => e.SafetySettings);

            entity.HasIndex(e => e.ModelProvider);
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

            entity.Property(e => e.DataSourceConfig);
            entity.Property(e => e.RetrievalConfig);
            entity.Property(e => e.EmbeddingConfig);
            entity.Property(e => e.CacheConfig);
            entity.Property(e => e.AccessControl);
            entity.Property(e => e.PerformanceMetrics);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.CopilotApplicationId);
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

            // JSON fields for governance requirements
            entity.Property(e => e.DataResidency);
            entity.Property(e => e.SecurityRequirements);
            entity.Property(e => e.ComplianceRequirements);
            entity.Property(e => e.DataHandling);
            entity.Property(e => e.ModelGovernance);
            entity.Property(e => e.AuditRequirements);
            entity.Property(e => e.CostManagement);
            entity.Property(e => e.IncidentResponse);

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.LastModifiedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.LandingZone);
            entity.HasIndex(e => e.IsActive);
        });

        // CopilotPerformanceMetrics
        modelBuilder.Entity<CopilotPerformanceMetrics>(entity =>
        {
            entity.HasKey(e => e.MetricsId);
            entity.Property(e => e.MetricsId)
                .IsRequired()
                .HasMaxLength(36);

            entity.Property(e => e.DetailedMetrics);

            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.LastModifiedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.CopilotId);
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

            entity.Property(e => e.HealthCheck);
            entity.Property(e => e.ScalingConfig);
            entity.Property(e => e.ResourceAllocation);
            entity.Property(e => e.LoadBalancing);
            entity.Property(e => e.SecurityConfig);
            entity.Property(e => e.EnvironmentVariables);
            entity.Property(e => e.RollbackInfo);
            entity.Property(e => e.FeatureFlags);

            entity.HasIndex(e => e.Environment);
            entity.HasIndex(e => e.DeploymentStatus);
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

            // JSON fields
            entity.Property(e => e.Changes);
            entity.Property(e => e.BreakingChanges);
            entity.Property(e => e.Deprecations);
            entity.Property(e => e.DeploymentInstructions);
            entity.Property(e => e.RollbackInstructions);

            entity.HasIndex(e => e.VersionNumber);
            entity.HasIndex(e => e.CopilotId);
            entity.HasIndex(e => e.ReleaseDate);
        });
    }
}

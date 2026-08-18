using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;

namespace DataAccessLayer.Data.Contexts
{
    /// <summary>
    /// Entity Framework DbContext for multi-tenant data management
    /// </summary>
    public class TenantDbContext : DbContext
    {
        public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options)
        {
        }

        // DbSets for all tenant-related entities
        public DbSet<TenantConfig> Tenants { get; set; }
        public DbSet<TenantSubscription> Subscriptions { get; set; }
        public DbSet<TenantQuota> Quotas { get; set; }
        public DbSet<TenantCustomAgent> CustomAgents { get; set; }
        public DbSet<TenantTeamMember> TeamMembers { get; set; }
        public DbSet<TenantUsage> UsageStats { get; set; }
        public DbSet<TenantBillingRecord> BillingRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TenantConfig
            modelBuilder.Entity<TenantConfig>(entity =>
            {
                entity.HasKey(e => e.TenantId);

                entity.Property(e => e.TenantId)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.TenantName)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.ApiKey)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.SecretKey)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.CurrentTier)
                    .HasConversion(new EnumToStringConverter<SubscriptionTier>());

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasIndex(e => e.ApiKey).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedDate);
            });

            // Configure TenantSubscription
            modelBuilder.Entity<TenantSubscription>(entity =>
            {
                entity.HasKey(e => e.SubscriptionId);

                entity.Property(e => e.SubscriptionId)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.TenantId)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Tier)
                    .HasConversion(new EnumToStringConverter<SubscriptionTier>());

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasDefaultValue("active");

                entity.Property(e => e.BillingInterval)
                    .HasMaxLength(50)
                    .HasDefaultValue("monthly");

                entity.Property(e => e.StartDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.AutoRenew)
                    .HasDefaultValue(true);

                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.Status);

                // Foreign key relationship
                entity.HasOne<TenantConfig>()
                    .WithMany()
                    .HasForeignKey("TenantId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TenantQuota
            modelBuilder.Entity<TenantQuota>(entity =>
            {
                entity.HasKey(e => e.TenantId);

                entity.Property(e => e.TenantId)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.MonthlyApiCallLimit)
                    .HasDefaultValue(1000);

                entity.Property(e => e.DailyApiCallLimit)
                    .HasDefaultValue(100);

                entity.Property(e => e.QuotaExceeded)
                    .HasDefaultValue(false);

                entity.Property(e => e.ResetDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP + INTERVAL '1 month'");

                entity.HasIndex(e => e.QuotaExceeded);
                entity.HasIndex(e => e.ResetDate);

                // Foreign key relationship
                entity.HasOne<TenantConfig>()
                    .WithMany()
                    .HasForeignKey("TenantId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TenantCustomAgent
            modelBuilder.Entity<TenantCustomAgent>(entity =>
            {
                entity.HasKey(e => e.AgentId);

                entity.Property(e => e.AgentId)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.TenantId)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.AgentName)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.SystemPrompt)
                    .HasMaxLength(2000);

                entity.Property(e => e.PreferredModel)
                    .HasMaxLength(100)
                    .HasDefaultValue("gpt-4");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedDate);

                // Foreign key relationship
                entity.HasOne<TenantConfig>()
                    .WithMany()
                    .HasForeignKey("TenantId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TenantTeamMember
            modelBuilder.Entity<TenantTeamMember>(entity =>
            {
                entity.HasKey(e => e.MemberId);

                entity.Property(e => e.MemberId)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.TenantId)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .HasDefaultValue("user");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.JoinedDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
                entity.HasIndex(e => e.IsActive);

                // Foreign key relationship
                entity.HasOne<TenantConfig>()
                    .WithMany()
                    .HasForeignKey("TenantId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TenantUsage
            modelBuilder.Entity<TenantUsage>(entity =>
            {
                entity.HasKey(e => e.UsageId);

                entity.Property(e => e.UsageId)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.TenantId)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Date)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.ApiCallsCount)
                    .HasDefaultValue(0);

                entity.Property(e => e.TokensUsed)
                    .HasDefaultValue(0);

                entity.Property(e => e.CostInCents)
                    .HasColumnType("decimal(10, 2)");

                entity.HasIndex(e => new { e.TenantId, e.Date });
                entity.HasIndex(e => e.Date);

                // Foreign key relationship
                entity.HasOne<TenantConfig>()
                    .WithMany()
                    .HasForeignKey("TenantId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TenantBillingRecord
            modelBuilder.Entity<TenantBillingRecord>(entity =>
            {
                entity.HasKey(e => e.BillingRecordId);

                entity.Property(e => e.BillingRecordId)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.TenantId)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.BillingPeriodStart)
                    .IsRequired();

                entity.Property(e => e.BillingPeriodEnd)
                    .IsRequired();

                entity.Property(e => e.TotalCostInCents)
                    .HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasDefaultValue("pending");

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.BillingPeriodStart);

                // Foreign key relationship
                entity.HasOne<TenantConfig>()
                    .WithMany()
                    .HasForeignKey("TenantId")
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }

    // Entity models
    public class TenantConfig
    {
        public string TenantId { get; set; }
        public string TenantName { get; set; }
        public string Description { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public SubscriptionTier CurrentTier { get; set; } = SubscriptionTier.Free;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }

    public class TenantSubscription
    {
        public string SubscriptionId { get; set; }
        public string TenantId { get; set; }
        public SubscriptionTier Tier { get; set; }
        public string Status { get; set; } = "active";
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public DateTime? RenewalDate { get; set; }
        public string BillingInterval { get; set; } = "monthly";
        public bool AutoRenew { get; set; } = true;
    }

    public class TenantQuota
    {
        public string TenantId { get; set; }
        public int MonthlyApiCallLimit { get; set; } = 1000;
        public int MonthlyApiCallsUsed { get; set; } = 0;
        public int DailyApiCallLimit { get; set; } = 100;
        public int DailyApiCallsUsed { get; set; } = 0;
        public bool QuotaExceeded { get; set; } = false;
        public DateTime ResetDate { get; set; }
    }

    public class TenantCustomAgent
    {
        public string AgentId { get; set; }
        public string TenantId { get; set; }
        public string AgentName { get; set; }
        public string SystemPrompt { get; set; }
        public List<string> AssignedTools { get; set; } = new List<string>();
        public string PreferredModel { get; set; } = "gpt-4";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }

    public class TenantTeamMember
    {
        public string MemberId { get; set; }
        public string TenantId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } = "user";
        public bool IsActive { get; set; } = true;
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    }

    public class TenantUsage
    {
        public string UsageId { get; set; }
        public string TenantId { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public int ApiCallsCount { get; set; }
        public int TokensUsed { get; set; }
        public decimal CostInCents { get; set; }
    }

    public class TenantBillingRecord
    {
        public string BillingRecordId { get; set; }
        public string TenantId { get; set; }
        public DateTime BillingPeriodStart { get; set; }
        public DateTime BillingPeriodEnd { get; set; }
        public int TotalApiCalls { get; set; }
        public int TotalTokensUsed { get; set; }
        public decimal TotalCostInCents { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }

    public enum SubscriptionTier
    {
        Free,
        Starter,
        Professional,
        Enterprise
    }
}
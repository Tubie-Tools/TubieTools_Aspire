using DataAccessLayer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Data
{
    /// <summary>
    /// Entity Framework Core DbContext for Tubie B2B Care Provider API
    /// </summary>
    public class TubieDbContext : DbContext
    {
        /// <summary>
        /// Constructor for TubieDbContext
        /// </summary>
        /// <param name="options">DbContext options</param>
        public TubieDbContext(DbContextOptions<TubieDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// DbSet for Care Providers
        /// </summary>
        public DbSet<CareProviderEntity> CareProviders { get; set; }

        /// <summary>
        /// DbSet for Contact information
        /// </summary>
        public DbSet<ContactEntity> Contacts { get; set; }

        /// <summary>
        /// DbSet for Addresses
        /// </summary>
        public DbSet<AddressEntity> Addresses { get; set; }

        /// <summary>
        /// DbSet for Payment Configurations
        /// </summary>
        public DbSet<PaymentConfigurationEntity> PaymentConfigurations { get; set; }

        /// <summary>
        /// DbSet for Discount Policies
        /// </summary>
        public DbSet<DiscountPolicyEntity> DiscountPolicies { get; set; }

        /// <summary>
        /// DbSet for Purchase History
        /// </summary>
        public DbSet<PurchaseHistoryEntity> PurchaseHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure CareProvider entity
            modelBuilder.Entity<CareProviderEntity>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<CareProviderEntity>()
                .HasIndex(c => c.ProviderId)
                .IsUnique();

            modelBuilder.Entity<CareProviderEntity>()
                .Property(c => c.ProviderName)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<CareProviderEntity>()
                .Property(c => c.TaxId)
                .HasMaxLength(20);

            modelBuilder.Entity<CareProviderEntity>()
                .Property(c => c.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Active");

            modelBuilder.Entity<CareProviderEntity>()
                .Property(c => c.Tier)
                .IsRequired();

            // Configure relationships
            modelBuilder.Entity<CareProviderEntity>()
                .HasOne(c => c.PrimaryContactEntity)
                .WithMany()
                .HasForeignKey(c => c.PrimaryContactId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CareProviderEntity>()
                .HasOne(c => c.SecondaryContactEntity)
                .WithMany()
                .HasForeignKey(c => c.SecondaryContactId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CareProviderEntity>()
                .HasOne(c => c.BillingAddressEntity)
                .WithMany()
                .HasForeignKey(c => c.BillingAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CareProviderEntity>()
                .HasOne(c => c.ShippingAddressEntity)
                .WithMany()
                .HasForeignKey(c => c.ShippingAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CareProviderEntity>()
                .HasOne(c => c.PaymentConfigurationEntity)
                .WithMany()
                .HasForeignKey(c => c.PaymentConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CareProviderEntity>()
                .HasMany(c => c.DiscountPoliciesEntities)
                .WithOne(d => d.CareProviderEntity)
                .HasForeignKey(d => d.CareProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Contact entity
            modelBuilder.Entity<ContactEntity>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<ContactEntity>()
                .Property(c => c.Email)
                .HasMaxLength(255);

            modelBuilder.Entity<ContactEntity>()
                .Property(c => c.Phone)
                .HasMaxLength(20);

            modelBuilder.Entity<ContactEntity>()
                .Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ContactEntity>()
                .Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ContactEntity>()
                .Property(c => c.Title)
                .HasMaxLength(100);

            modelBuilder.Entity<ContactEntity>()
                .Property(c => c.Department)
                .HasMaxLength(100);

            // Configure Address entity
            modelBuilder.Entity<AddressEntity>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<AddressEntity>()
                .Property(a => a.StreetAddress)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<AddressEntity>()
                .Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<AddressEntity>()
                .Property(a => a.State)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<AddressEntity>()
                .Property(a => a.PostalCode)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<AddressEntity>()
                .Property(a => a.Country)
                .HasMaxLength(100);

            modelBuilder.Entity<AddressEntity>()
                .Property(a => a.AddressType)
                .HasMaxLength(50);

            // Configure PaymentConfiguration entity
            modelBuilder.Entity<PaymentConfigurationEntity>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<PaymentConfigurationEntity>()
                .Property(p => p.PaymentFrequency)
                .IsRequired();

            modelBuilder.Entity<PaymentConfigurationEntity>()
                .Property(p => p.PaymentMethod)
                .HasMaxLength(50);

            modelBuilder.Entity<PaymentConfigurationEntity>()
                .Property(p => p.DiscountPercentage)
                .HasPrecision(5, 4);

            // Configure DiscountPolicy entity
            modelBuilder.Entity<DiscountPolicyEntity>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<DiscountPolicyEntity>()
                .Property(d => d.DiscountType)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<DiscountPolicyEntity>()
                .Property(d => d.PromoCode)
                .HasMaxLength(50);

            modelBuilder.Entity<DiscountPolicyEntity>()
                .Property(d => d.DiscountPercentage)
                .HasPrecision(5, 4);

            modelBuilder.Entity<DiscountPolicyEntity>()
                .Property(d => d.Description)
                .HasMaxLength(500);

            // Configure PurchaseHistory entity
            modelBuilder.Entity<PurchaseHistoryEntity>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<PurchaseHistoryEntity>()
                .HasOne(p => p.CareProviderEntity)
                .WithMany(c => c.PurchaseHistoryEntities)
                .HasForeignKey(p => p.CareProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseHistoryEntity>()
                .Property(p => p.OrderId)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<PurchaseHistoryEntity>()
                .HasIndex(p => p.OrderId)
                .IsUnique();

            modelBuilder.Entity<PurchaseHistoryEntity>()
                .Property(p => p.ProductType)
                .HasMaxLength(100);

            modelBuilder.Entity<PurchaseHistoryEntity>()
                .Property(p => p.DiscountApplied)
                .HasMaxLength(100);

            modelBuilder.Entity<PurchaseHistoryEntity>()
                .Property(p => p.UnitPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PurchaseHistoryEntity>()
                .Property(p => p.LineTotal)
                .HasPrecision(12, 2);

            modelBuilder.Entity<PurchaseHistoryEntity>()
                .Property(p => p.TotalAmount)
                .HasPrecision(12, 2);
        }
    }
}

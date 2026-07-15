namespace DataAccessLayer.Data.Entities
{
    /// <summary>
    /// Entity Framework model for Payment Configuration
    /// </summary>
    public class PaymentConfigurationEntity
    {
        /// <summary>
        /// Primary key - Database ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to CareProvider
        /// </summary>
        public int CareProviderId { get; set; }

        /// <summary>
        /// Payment frequency (1=Lump, 2=Monthly, 3=Quarterly)
        /// </summary>
        public int PaymentFrequency { get; set; }

        /// <summary>
        /// Discount percentage applied (e.g., 0.05 for 5%)
        /// </summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Date of next billing cycle
        /// </summary>
        public DateTime NextBillingDate { get; set; }

        /// <summary>
        /// Whether the subscription auto-renews
        /// </summary>
        public bool AutoRenew { get; set; }

        /// <summary>
        /// Payment method (CreditCard, BankTransfer, Check)
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Date when payment configuration was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date when payment configuration was last modified
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        public PaymentConfigurationEntity()
        {
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
        }
    }
}

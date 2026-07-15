namespace TubieTools_PublicAPI.Data.Entities
{
    /// <summary>
    /// Entity Framework model for Discount Policy
    /// </summary>
    public class DiscountPolicyEntity
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
        /// Navigation property back to CareProvider
        /// </summary>
        public CareProviderEntity CareProviderEntity { get; set; }

        /// <summary>
        /// Discount type (Volume, Tier, Promo)
        /// </summary>
        public string DiscountType { get; set; }

        /// <summary>
        /// Promotional code (if applicable)
        /// </summary>
        public string PromoCode { get; set; }

        /// <summary>
        /// Discount percentage (e.g., 0.10 for 10%)
        /// </summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Minimum order quantity to qualify
        /// </summary>
        public int MinimumOrderQuantity { get; set; }

        /// <summary>
        /// Maximum order quantity (0 = unlimited)
        /// </summary>
        public int MaximumOrderQuantity { get; set; }

        /// <summary>
        /// Discount start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Discount end date
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Whether the discount is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Description of the discount
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Date when discount was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        public DiscountPolicyEntity()
        {
            CreatedDate = DateTime.UtcNow;
            IsActive = true;
        }
    }
}

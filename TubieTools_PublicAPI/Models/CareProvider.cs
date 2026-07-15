namespace TubieTools_PublicAPI.Models
{
    /// <summary>
    /// Represents a Care Provider in the B2B system
    /// </summary>
    public class CareProvider
    {
        /// <summary>
        /// Unique identifier for the care provider
        /// </summary>
        public string ProviderId { get; set; }

        /// <summary>
        /// Legal name of the care provider organization
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Care provider tier classification
        /// </summary>
        public CareProviderTier Tier { get; set; }

        /// <summary>
        /// Federal Tax ID or EIN
        /// </summary>
        public string TaxId { get; set; }

        /// <summary>
        /// Current status of the provider account
        /// </summary>
        public string Status { get; set; } // "Active", "Inactive", "Pending", "Suspended"

        /// <summary>
        /// Expected annual order volume
        /// </summary>
        public int AnnualOrderVolume { get; set; }

        /// <summary>
        /// Primary contact person
        /// </summary>
        public Contact PrimaryContact { get; set; }

        /// <summary>
        /// Secondary contact person
        /// </summary>
        public Contact SecondaryContact { get; set; }

        /// <summary>
        /// Billing address
        /// </summary>
        public Address BillingAddress { get; set; }

        /// <summary>
        /// Shipping address
        /// </summary>
        public Address ShippingAddress { get; set; }

        /// <summary>
        /// Payment configuration and terms
        /// </summary>
        public PaymentConfiguration PaymentConfiguration { get; set; }

        /// <summary>
        /// List of active discount policies
        /// </summary>
        public List<DiscountPolicy> ActiveDiscounts { get; set; }

        /// <summary>
        /// Internal notes about the provider
        /// </summary>
        public string InternalNotes { get; set; }

        /// <summary>
        /// Date when provider was created in system
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date when provider record was last modified
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        public CareProvider()
        {
            ActiveDiscounts = new List<DiscountPolicy>();
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
        }
    }
}

namespace DataAccessLayer.Data.Entities
{
    /// <summary>
    /// Entity Framework model for CareProvider
    /// </summary>
    public class CareProviderEntity
    {
        /// <summary>
        /// Primary key - Database ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique provider identifier
        /// </summary>
        public string ProviderId { get; set; }

        /// <summary>
        /// Legal name of the care provider organization
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Care provider tier classification
        /// </summary>
        public int Tier { get; set; }

        /// <summary>
        /// Federal Tax ID or EIN
        /// </summary>
        public string TaxId { get; set; }

        /// <summary>
        /// Current status of the provider account
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Expected annual order volume
        /// </summary>
        public int AnnualOrderVolume { get; set; }

        /// <summary>
        /// Foreign key to PrimaryContact
        /// </summary>
        public int? PrimaryContactId { get; set; }

        /// <summary>
        /// Navigation property for primary contact
        /// </summary>
        public ContactEntity PrimaryContactEntity { get; set; }

        /// <summary>
        /// Foreign key to SecondaryContact
        /// </summary>
        public int? SecondaryContactId { get; set; }

        /// <summary>
        /// Navigation property for secondary contact
        /// </summary>
        public ContactEntity SecondaryContactEntity { get; set; }

        /// <summary>
        /// Foreign key to BillingAddress
        /// </summary>
        public int? BillingAddressId { get; set; }

        /// <summary>
        /// Navigation property for billing address
        /// </summary>
        public AddressEntity BillingAddressEntity { get; set; }

        /// <summary>
        /// Foreign key to ShippingAddress
        /// </summary>
        public int? ShippingAddressId { get; set; }

        /// <summary>
        /// Navigation property for shipping address
        /// </summary>
        public AddressEntity ShippingAddressEntity { get; set; }

        /// <summary>
        /// Foreign key to PaymentConfiguration
        /// </summary>
        public int? PaymentConfigurationId { get; set; }

        /// <summary>
        /// Navigation property for payment configuration
        /// </summary>
        public PaymentConfigurationEntity PaymentConfigurationEntity { get; set; }

        /// <summary>
        /// Collection of active discount policies
        /// </summary>
        public ICollection<DiscountPolicyEntity> DiscountPoliciesEntities { get; set; }

        /// <summary>
        /// Collection of purchase history for this provider
        /// </summary>
        public ICollection<PurchaseHistoryEntity> PurchaseHistoryEntities { get; set; }

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

        public CareProviderEntity()
        {
            DiscountPoliciesEntities = new List<DiscountPolicyEntity>();
            PurchaseHistoryEntities = new List<PurchaseHistoryEntity>();
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
        }
    }
}

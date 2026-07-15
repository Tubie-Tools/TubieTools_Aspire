namespace TubieTools_PublicAPI.Models.Requests
{
    /// <summary>
    /// Request model for creating a new care provider
    /// </summary>
    public class CreateCareProviderRequest
    {
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
        /// Expected annual order volume
        /// </summary>
        public int AnnualOrderVolume { get; set; }

        /// <summary>
        /// Primary contact person
        /// </summary>
        public Contact PrimaryContact { get; set; }

        /// <summary>
        /// Secondary contact person (optional)
        /// </summary>
        public Contact SecondaryContact { get; set; }

        /// <summary>
        /// Billing address
        /// </summary>
        public Address BillingAddress { get; set; }

        /// <summary>
        /// Shipping address (optional, defaults to billing if not provided)
        /// </summary>
        public Address ShippingAddress { get; set; }

        /// <summary>
        /// Payment configuration preferences
        /// </summary>
        public PaymentConfiguration PaymentConfiguration { get; set; }

        /// <summary>
        /// Internal notes about the provider
        /// </summary>
        public string InternalNotes { get; set; }
    }
}

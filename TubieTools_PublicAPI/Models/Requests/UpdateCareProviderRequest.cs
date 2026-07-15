namespace TubieTools_PublicAPI.Models.Requests
{
    /// <summary>
    /// Request model for updating an existing care provider
    /// </summary>
    public class UpdateCareProviderRequest
    {
        /// <summary>
        /// Legal name of the care provider organization
        /// </summary>
        public string ProviderName { get; set; }

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
        /// Payment configuration
        /// </summary>
        public PaymentConfiguration PaymentConfiguration { get; set; }

        /// <summary>
        /// Internal notes about the provider
        /// </summary>
        public string InternalNotes { get; set; }

        /// <summary>
        /// Current status of the provider account
        /// </summary>
        public string Status { get; set; }
    }
}

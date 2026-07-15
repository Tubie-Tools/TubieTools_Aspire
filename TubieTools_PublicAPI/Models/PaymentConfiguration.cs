namespace TubieTools_PublicAPI.Models
{
    /// <summary>
    /// Payment configuration for a care provider
    /// </summary>
    public class PaymentConfiguration
    {
        public int Id { get; set; }
        public int CareProviderId { get; set; }
        public PaymentFrequency PaymentFrequency { get; set; }
        public decimal DiscountPercentage { get; set; } // e.g., 0.05 for 5% discount
        public DateTime NextBillingDate { get; set; }
        public bool AutoRenew { get; set; }
        public string PaymentMethod { get; set; } // "CreditCard", "BankTransfer", "Check"
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}

namespace TubieTools_PublicAPI.Models
{
    /// <summary>
    /// Discount policy for a care provider based on volume, tier, or promo
    /// </summary>
    public class DiscountPolicy
    {
        public int Id { get; set; }
        public int CareProviderId { get; set; }
        public string DiscountType { get; set; } // "Volume", "Tier", "Promo"
        public string PromoCode { get; set; }
        public decimal DiscountPercentage { get; set; } // e.g., 0.10 for 10% discount
        public int MinimumOrderQuantity { get; set; }
        public int MaximumOrderQuantity { get; set; } // 0 for unlimited
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
    }
}

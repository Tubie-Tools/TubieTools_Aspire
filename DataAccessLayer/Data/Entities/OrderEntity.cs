namespace DataAccessLayer.Data.Entities
{
    /// <summary>
    /// Entity Framework model for Bulk Orders
    /// Represents a complete order with line items and fulfillment tracking
    /// </summary>
    public class OrderEntity
    {
        /// <summary>
        /// Primary key - Database ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique order identifier
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Foreign key to CareProvider
        /// </summary>
        public int CareProviderId { get; set; }

        /// <summary>
        /// Navigation property back to CareProvider
        /// </summary>
        public CareProviderEntity CareProviderEntity { get; set; }

        /// <summary>
        /// Order status (Pending, Processing, Shipped, Delivered, Cancelled)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Total number of units ordered
        /// </summary>
        public int TotalQuantity { get; set; }

        /// <summary>
        /// Subtotal before discounts and taxes
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Total discount amount applied
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Shipping cost
        /// </summary>
        public decimal ShippingCost { get; set; }

        /// <summary>
        /// Total order amount (Subtotal - Discount + Tax + Shipping)
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Payment frequency for this order
        /// </summary>
        public int PaymentFrequency { get; set; }

        /// <summary>
        /// Promo code applied (if any)
        /// </summary>
        public string PromoCode { get; set; }

        /// <summary>
        /// Special instructions or notes for the order
        /// </summary>
        public string SpecialInstructions { get; set; }

        /// <summary>
        /// Expected ship date
        /// </summary>
        public DateTime ExpectedShipDate { get; set; }

        /// <summary>
        /// Actual ship date
        /// </summary>
        public DateTime? ActualShipDate { get; set; }

        /// <summary>
        /// Expected delivery date
        /// </summary>
        public DateTime? ExpectedDeliveryDate { get; set; }

        /// <summary>
        /// Actual delivery date
        /// </summary>
        public DateTime? ActualDeliveryDate { get; set; }

        /// <summary>
        /// Estimated labor hours required for fulfillment
        /// </summary>
        public decimal EstimatedLaborHours { get; set; }

        /// <summary>
        /// Estimated material cost
        /// </summary>
        public decimal EstimatedMaterialCost { get; set; }

        /// <summary>
        /// Date when order was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date when order was last modified
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        public OrderEntity()
        {
            Status = "Pending";
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
            ExpectedShipDate = DateTime.UtcNow.AddDays(2);
        }
    }
}

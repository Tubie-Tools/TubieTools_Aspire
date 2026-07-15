namespace DataAccessLayer.Data.Entities
{
    /// <summary>
    /// Entity Framework model for Purchase History snapshot
    /// Captures a snapshot in time of a purchase transaction
    /// </summary>
    public class PurchaseHistoryEntity
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
        /// Unique order identifier
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Type of product purchased
        /// </summary>
        public string ProductType { get; set; }

        /// <summary>
        /// Quantity ordered
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price at time of purchase
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Discount applied to this line item
        /// </summary>
        public string DiscountApplied { get; set; }

        /// <summary>
        /// Discount percentage applied
        /// </summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Total for this line item (Quantity * UnitPrice * (1 - DiscountPercentage))
        /// </summary>
        public decimal LineTotal { get; set; }

        /// <summary>
        /// Total order amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Payment frequency used for this order
        /// </summary>
        public int PaymentFrequency { get; set; }

        /// <summary>
        /// Timestamp of the order
        /// </summary>
        public DateTime OrderTimestamp { get; set; }

        /// <summary>
        /// Date when record was created in database
        /// </summary>
        public DateTime CreatedDate { get; set; }

        public PurchaseHistoryEntity()
        {
            OrderTimestamp = DateTime.UtcNow;
            CreatedDate = DateTime.UtcNow;
        }
    }
}

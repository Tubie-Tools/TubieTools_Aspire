namespace TubieTools_Aspire.Web.Models;

/// <summary>
/// Payment status enumeration
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment pending
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment processing
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Payment approved/captured
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Payment authorized but not captured
    /// </summary>
    Authorized = 3,

    /// <summary>
    /// Payment declined
    /// </summary>
    Declined = 4,

    /// <summary>
    /// Payment error
    /// </summary>
    Error = 5,

    /// <summary>
    /// Payment refunded
    /// </summary>
    Refunded = 6,

    /// <summary>
    /// Payment held for review
    /// </summary>
    HeldForReview = 7,

    /// <summary>
    /// Payment cancelled
    /// </summary>
    Cancelled = 8
}

/// <summary>
/// Represents an order with payment information
/// </summary>
public class Order
{
    /// <summary>
    /// Unique order ID
    /// </summary>
    public string OrderId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Customer name
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Customer email
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Customer phone
    /// </summary>
    public string CustomerPhone { get; set; } = string.Empty;

    /// <summary>
    /// Order total amount
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Tax amount
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Shipping amount
    /// </summary>
    public decimal ShippingAmount { get; set; }

    /// <summary>
    /// Discount amount
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Order items
    /// </summary>
    public List<OrderItem> OrderItems { get; set; } = new();

    /// <summary>
    /// Payment status
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Authorize.Net transaction ID
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Order creation date
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last modified date
    /// </summary>
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Order notes
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Billing address
    /// </summary>
    public Address? BillingAddress { get; set; }

    /// <summary>
    /// Shipping address
    /// </summary>
    public Address? ShippingAddress { get; set; }

    /// <summary>
    /// Whether this order is from a payment profile/recurring
    /// </summary>
    public bool IsRecurring { get; set; } = false;

    /// <summary>
    /// Subscription ID if recurring
    /// </summary>
    public string? SubscriptionId { get; set; }

    /// <summary>
    /// Subtotal before tax and shipping
    /// </summary>
    public decimal Subtotal => OrderItems.Sum(i => i.TotalPrice);
}

/// <summary>
/// Represents an individual item in an order
/// </summary>
public class OrderItem
{
    /// <summary>
    /// Product ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Product description
    /// </summary>
    public string ProductDescription { get; set; } = string.Empty;

    /// <summary>
    /// Unit price
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Quantity ordered
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Total price for this line item
    /// </summary>
    public decimal TotalPrice => UnitPrice * Quantity;
}

/// <summary>
/// Address information for billing or shipping
/// </summary>
public class Address
{
    /// <summary>
    /// Street address
    /// </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// City
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// State or province
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// ZIP or postal code
    /// </summary>
    public string ZipCode { get; set; } = string.Empty;

    /// <summary>
    /// Country
    /// </summary>
    public string Country { get; set; } = "US";

    /// <summary>
    /// Full address string
    /// </summary>
    public string FullAddress => $"{Street}, {City}, {State} {ZipCode}, {Country}";
}

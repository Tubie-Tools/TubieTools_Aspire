namespace TubieTools_Aspire.Web.Models;

/// <summary>
/// Represents a payment request for Authorize.Net processing
/// </summary>
public class PaymentRequest
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
    /// Billing address
    /// </summary>
    public string BillingAddress { get; set; } = string.Empty;

    /// <summary>
    /// Billing city
    /// </summary>
    public string BillingCity { get; set; } = string.Empty;

    /// <summary>
    /// Billing state
    /// </summary>
    public string BillingState { get; set; } = string.Empty;

    /// <summary>
    /// Billing ZIP code
    /// </summary>
    public string BillingZip { get; set; } = string.Empty;

    /// <summary>
    /// Billing country
    /// </summary>
    public string BillingCountry { get; set; } = "US";

    /// <summary>
    /// Transaction amount in dollars
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Transaction description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Authorize.Net Data Value (encrypted payment data from Accept.js)
    /// </summary>
    public string DataValue { get; set; } = string.Empty;

    /// <summary>
    /// Authorize.Net Data Descriptor (payment method descriptor)
    /// </summary>
    public string DataDescriptor { get; set; } = string.Empty;

    /// <summary>
    /// Optional invoice number
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Optional purchase order number
    /// </summary>
    public string PurchaseOrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Line items for the transaction
    /// </summary>
    public List<LineItem> LineItems { get; set; } = new();

    /// <summary>
    /// Whether to store the payment profile for recurring billing
    /// </summary>
    public bool CreatePaymentProfile { get; set; } = false;

    /// <summary>
    /// IP address of the customer making the purchase
    /// </summary>
    public string CustomerIPAddress { get; set; } = string.Empty;
}

/// <summary>
/// Represents a line item in a payment request
/// </summary>
public class LineItem
{
    /// <summary>
    /// Item ID
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Item name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Item description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Quantity
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Unit price
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total amount for this line item
    /// </summary>
    public decimal Total => Quantity * UnitPrice;
}

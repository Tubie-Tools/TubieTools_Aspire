namespace TubieTools_Aspire.Web.Models;

/// <summary>
/// Represents the response from an Authorize.Net payment transaction
/// </summary>
public class PaymentResponse
{
    /// <summary>
    /// Whether the transaction was successful
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Authorize.Net transaction ID
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Order ID associated with the transaction
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Response code from Authorize.Net (1=Approved, 2=Declined, 3=Error, 4=Held for Review)
    /// </summary>
    public string ResponseCode { get; set; } = string.Empty;

    /// <summary>
    /// Response reason code for more detailed status
    /// </summary>
    public string ResponseReasonCode { get; set; } = string.Empty;

    /// <summary>
    /// Response reason text
    /// </summary>
    public string ResponseText { get; set; } = string.Empty;

    /// <summary>
    /// Authorization code from the card issuer
    /// </summary>
    public string AuthCode { get; set; } = string.Empty;

    /// <summary>
    /// AVS response (Address Verification System)
    /// </summary>
    public string AvsResponse { get; set; } = string.Empty;

    /// <summary>
    /// CVV response code
    /// </summary>
    public string CvvResponse { get; set; } = string.Empty;

    /// <summary>
    /// Transaction amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Transaction timestamp
    /// </summary>
    public DateTime TransactionDateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Error message if transaction failed
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Payment profile ID if profile was created
    /// </summary>
    public string CustomerPaymentProfileId { get; set; } = string.Empty;

    /// <summary>
    /// Customer profile ID if profile was created
    /// </summary>
    public string CustomerProfileId { get; set; } = string.Empty;

    /// <summary>
    /// MD5 hash of the transaction (for webhook validation)
    /// </summary>
    public string TransactionHash { get; set; } = string.Empty;

    /// <summary>
    /// Split result if using Authorize.Net payment split functionality
    /// </summary>
    public SplitFundingResult? SplitFundingResult { get; set; }
}

/// <summary>
/// Result of payment split/settlement
/// </summary>
public class SplitFundingResult
{
    /// <summary>
    /// Settlement amount for this merchant
    /// </summary>
    public decimal SettlementAmount { get; set; }

    /// <summary>
    /// Settlement date
    /// </summary>
    public DateTime SettlementDate { get; set; }
}

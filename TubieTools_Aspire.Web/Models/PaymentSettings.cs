namespace TubieTools_Aspire.Web.Models;

/// <summary>
/// Configuration settings for Authorize.Net payment processing
/// </summary>
public class PaymentSettings
{
    /// <summary>
    /// Authorize.Net API Login ID
    /// </summary>
    public string AuthorizeNetApiLoginId { get; set; } = string.Empty;

    /// <summary>
    /// Authorize.Net Transaction Key
    /// </summary>
    public string AuthorizeNetTransactionKey { get; set; } = string.Empty;

    /// <summary>
    /// Authorize.Net Signature Key for webhooks
    /// </summary>
    public string AuthorizeNetSignatureKey { get; set; } = string.Empty;

    /// <summary>
    /// Authorize.Net Merchant Hash for Accept.js
    /// </summary>
    public string AuthorizeNetMerchantHash { get; set; } = string.Empty;

    /// <summary>
    /// Authorize.Net Client Key for Accept.js
    /// </summary>
    public string AuthorizeNetClientKey { get; set; } = string.Empty;

    /// <summary>
    /// Authorize.Net Environment (sandbox or production)
    /// </summary>
    public string AuthorizeNetEnvironment { get; set; } = "sandbox";

    /// <summary>
    /// Enable Authorize.Net payment processing
    /// </summary>
    public bool Enabled { get; set; } = true;
}

using TubieTools_Aspire.Web.Models;

namespace TubieTools_Aspire.Web.Services;

/// <summary>
/// Interface for payment service operations
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Process a payment transaction
    /// </summary>
    Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a customer payment profile for recurring billing
    /// </summary>
    Task<PaymentResponse> CreatePaymentProfileAsync(PaymentRequest request, string customerName, string customerEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Charge a previously created payment profile
    /// </summary>
    Task<PaymentResponse> ChargePaymentProfileAsync(string customerProfileId, string paymentProfileId, decimal amount, string orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refund a transaction
    /// </summary>
    Task<PaymentResponse> RefundTransactionAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Void an authorized transaction
    /// </summary>
    Task<PaymentResponse> VoidTransactionAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate webhook signature from Authorize.Net
    /// </summary>
    bool ValidateWebhookSignature(string payload, string signature);

    /// <summary>
    /// Get transaction details
    /// </summary>
    Task<PaymentResponse> GetTransactionDetailsAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a recurring billing subscription
    /// </summary>
    Task<PaymentResponse> CreateSubscriptionAsync(PaymentRequest request, string subscriptionName, int intervalLength, string intervalUnit, int totalOccurrences, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a subscription
    /// </summary>
    Task<PaymentResponse> CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);
}

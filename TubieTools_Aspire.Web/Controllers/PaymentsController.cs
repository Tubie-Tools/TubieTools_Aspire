using Microsoft.AspNetCore.Mvc;
using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;

namespace TubieTools_Aspire.Web.Controllers;

/// <summary>
/// API controller for handling payments with multiple payment methods
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentServiceFactory _paymentServiceFactory;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentServiceFactory paymentServiceFactory,
        ILogger<PaymentsController> logger)
    {
        _paymentServiceFactory = paymentServiceFactory;
        _logger = logger;
    }

    /// <summary>
    /// Process a payment with the specified payment method
    /// </summary>
    /// <param name="paymentMethod">Payment method (AuthorizeNet, PayPal, GooglePay, ApplePay)</param>
    /// <param name="request">Payment request details</param>
    [HttpPost("process/{paymentMethod}")]
    public async Task<ActionResult<PaymentResponse>> ProcessPayment(
        string paymentMethod,
        [FromBody] PaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing {PaymentMethod} payment for order {OrderId}", paymentMethod, request.OrderId);

            var paymentService = _paymentServiceFactory.GetPaymentService(paymentMethod);
            var response = await paymentService.ProcessPaymentAsync(request, cancellationToken);

            if (!response.IsSuccessful)
            {
                _logger.LogWarning("Payment failed for order {OrderId}: {ErrorMessage}", 
                    request.OrderId, response.ErrorMessage);
                return BadRequest(response);
            }

            _logger.LogInformation("Payment successful for order {OrderId} with transaction {TransactionId}",
                request.OrderId, response.TransactionId);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid payment method: {PaymentMethod}", paymentMethod);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", request.OrderId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while processing the payment" });
        }
    }

    /// <summary>
    /// Create a payment profile for recurring charges
    /// </summary>
    [HttpPost("profile/create/{paymentMethod}")]
    public async Task<ActionResult<PaymentResponse>> CreatePaymentProfile(
        string paymentMethod,
        [FromBody] CreateProfileRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating {PaymentMethod} payment profile for {CustomerEmail}", 
                paymentMethod, request.PaymentRequest.CustomerEmail);

            var paymentService = _paymentServiceFactory.GetPaymentService(paymentMethod);
            var response = await paymentService.CreatePaymentProfileAsync(
                request.PaymentRequest,
                request.CustomerName,
                request.CustomerEmail,
                cancellationToken);

            if (!response.IsSuccessful)
            {
                _logger.LogWarning("Profile creation failed: {ErrorMessage}", response.ErrorMessage);
                return BadRequest(response);
            }

            _logger.LogInformation("Payment profile created: {CustomerProfileId}", response.CustomerProfileId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid payment method: {PaymentMethod}", paymentMethod);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment profile");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while creating the payment profile" });
        }
    }

    /// <summary>
    /// Charge a previously created payment profile
    /// </summary>
    [HttpPost("profile/charge/{paymentMethod}")]
    public async Task<ActionResult<PaymentResponse>> ChargePaymentProfile(
        string paymentMethod,
        [FromBody] ChargeProfileRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Charging {PaymentMethod} profile {CustomerProfileId} for ${Amount}",
                paymentMethod, request.CustomerProfileId, request.Amount);

            var paymentService = _paymentServiceFactory.GetPaymentService(paymentMethod);
            var response = await paymentService.ChargePaymentProfileAsync(
                request.CustomerProfileId,
                request.PaymentProfileId,
                request.Amount,
                request.OrderId,
                cancellationToken);

            if (!response.IsSuccessful)
            {
                _logger.LogWarning("Profile charge failed: {ErrorMessage}", response.ErrorMessage);
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid payment method: {PaymentMethod}", paymentMethod);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error charging payment profile");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while charging the payment profile" });
        }
    }

    /// <summary>
    /// Refund a transaction
    /// </summary>
    [HttpPost("refund/{paymentMethod}/{transactionId}")]
    public async Task<ActionResult<PaymentResponse>> RefundTransaction(
        string paymentMethod,
        string transactionId,
        [FromBody] RefundRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Refunding {PaymentMethod} transaction {TransactionId} for ${Amount}",
                paymentMethod, transactionId, request.Amount);

            var paymentService = _paymentServiceFactory.GetPaymentService(paymentMethod);
            var response = await paymentService.RefundTransactionAsync(transactionId, request.Amount, cancellationToken);

            if (!response.IsSuccessful)
            {
                _logger.LogWarning("Refund failed: {ErrorMessage}", response.ErrorMessage);
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid payment method: {PaymentMethod}", paymentMethod);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding transaction");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while refunding the transaction" });
        }
    }

    /// <summary>
    /// Void a transaction
    /// </summary>
    [HttpPost("void/{paymentMethod}/{transactionId}")]
    public async Task<ActionResult<PaymentResponse>> VoidTransaction(
        string paymentMethod,
        string transactionId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Voiding {PaymentMethod} transaction {TransactionId}",
                paymentMethod, transactionId);

            var paymentService = _paymentServiceFactory.GetPaymentService(paymentMethod);
            var response = await paymentService.VoidTransactionAsync(transactionId, cancellationToken);

            if (!response.IsSuccessful)
            {
                _logger.LogWarning("Void failed: {ErrorMessage}", response.ErrorMessage);
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid payment method: {PaymentMethod}", paymentMethod);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding transaction");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while voiding the transaction" });
        }
    }

    /// <summary>
    /// Get transaction details
    /// </summary>
    [HttpGet("transaction/{paymentMethod}/{transactionId}")]
    public async Task<ActionResult<PaymentResponse>> GetTransactionDetails(
        string paymentMethod,
        string transactionId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting {PaymentMethod} transaction details for {TransactionId}",
                paymentMethod, transactionId);

            var paymentService = _paymentServiceFactory.GetPaymentService(paymentMethod);
            var response = await paymentService.GetTransactionDetailsAsync(transactionId, cancellationToken);

            if (!response.IsSuccessful)
            {
                _logger.LogWarning("Failed to get transaction details: {ErrorMessage}", response.ErrorMessage);
                return NotFound(response);
            }

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid payment method: {PaymentMethod}", paymentMethod);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction details");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving transaction details" });
        }
    }

    /// <summary>
    /// Create a recurring subscription
    /// </summary>
    [HttpPost("subscription/create/{paymentMethod}")]
    public async Task<ActionResult<PaymentResponse>> CreateSubscription(
        string paymentMethod,
        [FromBody] CreateSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating {PaymentMethod} subscription for {OrderId}",
                paymentMethod, request.PaymentRequest.OrderId);

            var paymentService = _paymentServiceFactory.GetPaymentService(paymentMethod);
            var response = await paymentService.CreateSubscriptionAsync(
                request.PaymentRequest,
                request.SubscriptionName,
                request.IntervalLength,
                request.IntervalUnit,
                request.TotalOccurrences,
                cancellationToken);

            if (!response.IsSuccessful)
            {
                _logger.LogWarning("Subscription creation failed: {ErrorMessage}", response.ErrorMessage);
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid payment method: {PaymentMethod}", paymentMethod);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while creating the subscription" });
        }
    }

    /// <summary>
    /// Cancel a subscription
    /// </summary>
    [HttpPost("subscription/cancel/{paymentMethod}/{subscriptionId}")]
    public async Task<ActionResult<PaymentResponse>> CancelSubscription(
        string paymentMethod,
        string subscriptionId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Canceling {PaymentMethod} subscription {SubscriptionId}",
                paymentMethod, subscriptionId);

            var paymentService = _paymentServiceFactory.GetPaymentService(paymentMethod);
            var response = await paymentService.CancelSubscriptionAsync(subscriptionId, cancellationToken);

            if (!response.IsSuccessful)
            {
                _logger.LogWarning("Subscription cancellation failed: {ErrorMessage}", response.ErrorMessage);
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid payment method: {PaymentMethod}", paymentMethod);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling subscription");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while canceling the subscription" });
        }
    }

    /// <summary>
    /// Validate webhook signature
    /// </summary>
    [HttpPost("webhook/validate/{paymentMethod}")]
    public ActionResult<bool> ValidateWebhook(
        string paymentMethod,
        [FromBody] WebhookValidationRequest request)
    {
        try
        {
            _logger.LogInformation("Validating {PaymentMethod} webhook signature", paymentMethod);

            var paymentService = _paymentServiceFactory.GetPaymentService(paymentMethod);
            var isValid = paymentService.ValidateWebhookSignature(request.Payload, request.Signature);

            if (!isValid)
            {
                _logger.LogWarning("Webhook signature validation failed for {PaymentMethod}", paymentMethod);
            }

            return Ok(new { valid = isValid });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid payment method: {PaymentMethod}", paymentMethod);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating webhook");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while validating the webhook" });
        }
    }
}

/// <summary>
/// Request models for payment operations
/// </summary>
public class CreateProfileRequest
{
    public PaymentRequest PaymentRequest { get; set; } = new();
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
}

public class ChargeProfileRequest
{
    public string CustomerProfileId { get; set; } = "";
    public string PaymentProfileId { get; set; } = "";
    public decimal Amount { get; set; }
    public string OrderId { get; set; } = "";
}

public class RefundRequest
{
    public decimal Amount { get; set; }
}

public class CreateSubscriptionRequest
{
    public PaymentRequest PaymentRequest { get; set; } = new();
    public string SubscriptionName { get; set; } = "";
    public int IntervalLength { get; set; }
    public string IntervalUnit { get; set; } = "MONTH";
    public int TotalOccurrences { get; set; }
}

public class WebhookValidationRequest
{
    public string Payload { get; set; } = "";
    public string Signature { get; set; } = "";
}

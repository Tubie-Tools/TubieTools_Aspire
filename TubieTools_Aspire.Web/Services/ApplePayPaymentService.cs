using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using TubieTools_Aspire.Web.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TubieTools_Aspire.Web.Services;

/// <summary>
/// Service for processing payments through Apple Pay
/// </summary>
public class ApplePayPaymentService : IPaymentService
{
    private readonly PaymentSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApplePayPaymentService> _logger;
    private const string ApplePayValidationUrl = "https://apple-pay-gateway.apple.com/paymentservices";

    public ApplePayPaymentService(
        IOptions<PaymentSettings> settings,
        IHttpClientFactory httpClientFactory,
        ILogger<ApplePayPaymentService> logger)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(
        PaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = "Payment processing is not enabled",
                OrderId = request.OrderId
            };
        }

        try
        {
            // Apple Pay provides a payment token that needs to be processed
            var paymentToken = DecodeApplePayToken(request.DataValue);

            var payload = new
            {
                version = "EC_v1",
                data = paymentToken.Data,
                signature = paymentToken.Signature,
                header = new
                {
                    ephemeralPublicKey = paymentToken.Header?.EphemeralPublicKey,
                    publicKeyHash = paymentToken.Header?.PublicKeyHash,
                    transactionId = paymentToken.Header?.TransactionId
                },
                orderId = request.OrderId,
                amount = (long)(request.Amount * 100), // Apple Pay uses cents
                currency = "USD",
                description = request.Description,
                customerEmail = request.CustomerEmail
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var response = await SendApplePayRequestAsync(jsonPayload, cancellationToken);

            return ParseApplePayResponse(response, request.OrderId, request.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Apple Pay payment for order {OrderId}", request.OrderId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Apple Pay processing failed: {ex.Message}",
                OrderId = request.OrderId,
                Amount = request.Amount
            };
        }
    }

    public async Task<PaymentResponse> CreatePaymentProfileAsync(
        PaymentRequest request,
        string customerName,
        string customerEmail,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = "Payment processing is not enabled",
                OrderId = request.OrderId
            };
        }

        try
        {
            var paymentToken = DecodeApplePayToken(request.DataValue);

            var payload = new
            {
                method = "createPaymentMethod",
                tokenData = new
                {
                    version = "EC_v1",
                    data = paymentToken.Data,
                    signature = paymentToken.Signature,
                    header = new
                    {
                        ephemeralPublicKey = paymentToken.Header?.EphemeralPublicKey,
                        publicKeyHash = paymentToken.Header?.PublicKeyHash
                    }
                },
                customerId = request.OrderId,
                customerEmail = customerEmail,
                customerName = customerName,
                billingAddress = new
                {
                    street = request.BillingAddress,
                    city = request.BillingCity,
                    state = request.BillingState,
                    postalCode = request.BillingZip,
                    country = request.BillingCountry
                }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var response = await SendApplePayRequestAsync(jsonPayload, cancellationToken);

            return ParseApplePayProfileResponse(response, request.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Apple Pay profile for {CustomerEmail}", customerEmail);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to create Apple Pay profile: {ex.Message}",
                OrderId = request.OrderId
            };
        }
    }

    public async Task<PaymentResponse> ChargePaymentProfileAsync(
        string customerProfileId,
        string paymentProfileId,
        decimal amount,
        string orderId,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = "Payment processing is not enabled",
                OrderId = orderId
            };
        }

        try
        {
            var payload = new
            {
                method = "chargePaymentMethod",
                customerId = customerProfileId,
                paymentMethodId = paymentProfileId,
                amount = (long)(amount * 100), // Apple Pay uses cents
                currency = "USD",
                orderId = orderId
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var response = await SendApplePayRequestAsync(jsonPayload, cancellationToken);

            return ParseApplePayTransactionResponse(response, orderId, amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error charging Apple Pay profile {CustomerProfileId}", customerProfileId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to charge Apple Pay profile: {ex.Message}",
                OrderId = orderId,
                Amount = amount
            };
        }
    }

    public async Task<PaymentResponse> RefundTransactionAsync(
        string transactionId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = "Payment processing is not enabled",
                OrderId = transactionId
            };
        }

        try
        {
            var payload = new
            {
                method = "refundTransaction",
                transactionId = transactionId,
                amount = (long)(amount * 100), // Apple Pay uses cents
                reason = "Customer requested refund"
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var response = await SendApplePayRequestAsync(jsonPayload, cancellationToken);

            return ParseApplePayTransactionResponse(response, transactionId, amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding Apple Pay transaction {TransactionId}", transactionId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Apple Pay refund failed: {ex.Message}",
                TransactionId = transactionId,
                Amount = amount
            };
        }
    }

    public async Task<PaymentResponse> VoidTransactionAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = "Payment processing is not enabled",
                TransactionId = transactionId
            };
        }

        try
        {
            var payload = new
            {
                method = "voidTransaction",
                transactionId = transactionId
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var response = await SendApplePayRequestAsync(jsonPayload, cancellationToken);

            return ParseApplePayTransactionResponse(response, transactionId, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding Apple Pay transaction {TransactionId}", transactionId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Apple Pay void failed: {ex.Message}",
                TransactionId = transactionId
            };
        }
    }

    public bool ValidateWebhookSignature(string payload, string signature)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.AuthorizeNetSignatureKey))
            {
                _logger.LogWarning("Signature key not configured for Apple Pay webhook validation");
                return false;
            }

            // Apple Pay uses HMAC-SHA256 for webhook signature validation
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.AuthorizeNetSignatureKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var expectedSignature = Convert.ToBase64String(hash);

                return CryptographicEquals(signature, expectedSignature);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Apple Pay webhook signature");
            return false;
        }
    }

    public async Task<PaymentResponse> GetTransactionDetailsAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = "Payment processing is not enabled",
                TransactionId = transactionId
            };
        }

        try
        {
            var payload = new
            {
                method = "getTransactionDetails",
                transactionId = transactionId
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var response = await SendApplePayRequestAsync(jsonPayload, cancellationToken);

            return ParseApplePayTransactionDetailsResponse(response, transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Apple Pay transaction details for {TransactionId}", transactionId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to retrieve transaction details: {ex.Message}",
                TransactionId = transactionId
            };
        }
    }

    public async Task<PaymentResponse> CreateSubscriptionAsync(
        PaymentRequest request,
        string subscriptionName,
        int intervalLength,
        string intervalUnit,
        int totalOccurrences,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = "Payment processing is not enabled",
                OrderId = request.OrderId
            };
        }

        try
        {
            var paymentToken = DecodeApplePayToken(request.DataValue);

            var payload = new
            {
                method = "createSubscription",
                tokenData = new
                {
                    version = "EC_v1",
                    data = paymentToken.Data,
                    signature = paymentToken.Signature,
                    header = new
                    {
                        ephemeralPublicKey = paymentToken.Header?.EphemeralPublicKey,
                        publicKeyHash = paymentToken.Header?.PublicKeyHash
                    }
                },
                subscriptionName = subscriptionName,
                customerId = request.OrderId,
                amount = (long)(request.Amount * 100),
                currency = "USD",
                billingCycle = new
                {
                    intervalLength = intervalLength,
                    intervalUnit = intervalUnit,
                    totalOccurrences = totalOccurrences
                }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var response = await SendApplePayRequestAsync(jsonPayload, cancellationToken);

            return ParseApplePaySubscriptionResponse(response, request.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Apple Pay subscription for order {OrderId}", request.OrderId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to create Apple Pay subscription: {ex.Message}",
                OrderId = request.OrderId,
                Amount = request.Amount
            };
        }
    }

    public async Task<PaymentResponse> CancelSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = "Payment processing is not enabled",
                OrderId = subscriptionId
            };
        }

        try
        {
            var payload = new
            {
                method = "cancelSubscription",
                subscriptionId = subscriptionId
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var response = await SendApplePayRequestAsync(jsonPayload, cancellationToken);

            return ParseApplePayBasicResponse(response, subscriptionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling Apple Pay subscription {SubscriptionId}", subscriptionId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to cancel Apple Pay subscription: {ex.Message}",
                OrderId = subscriptionId
            };
        }
    }

    #region Private Helper Methods

    private ApplePayToken DecodeApplePayToken(string tokenJson)
    {
        try
        {
            var doc = JsonDocument.Parse(tokenJson);
            var root = doc.RootElement;

            var token = new ApplePayToken
            {
                Version = root.GetProperty("version").GetString() ?? "",
                Data = root.GetProperty("data").GetString() ?? "",
                Signature = root.GetProperty("signature").GetString() ?? ""
            };

            if (root.TryGetProperty("header", out var headerElement))
            {
                token.Header = new ApplePayTokenHeader
                {
                    EphemeralPublicKey = headerElement.GetProperty("ephemeralPublicKey").GetString() ?? "",
                    PublicKeyHash = headerElement.GetProperty("publicKeyHash").GetString() ?? "",
                    TransactionId = headerElement.GetProperty("transactionId").GetString() ?? ""
                };
            }

            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decoding Apple Pay token");
            return new ApplePayToken { Data = tokenJson };
        }
    }

    private async Task<string> SendApplePayRequestAsync(string payload, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(ApplePayValidationUrl, content, cancellationToken);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error sending Apple Pay request");
            throw;
        }
    }

    private PaymentResponse ParseApplePayResponse(string jsonResponse, string orderId, decimal amount)
    {
        try
        {
            var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            var response = new PaymentResponse { OrderId = orderId, Amount = amount };

            if (root.TryGetProperty("success", out var successElement) && successElement.GetBoolean())
            {
                response.IsSuccessful = true;

                if (root.TryGetProperty("transactionId", out var transElement))
                {
                    response.TransactionId = transElement.GetString() ?? "";
                }

                if (root.TryGetProperty("status", out var statusElement))
                {
                    response.ResponseCode = statusElement.GetString() ?? "";
                }
            }
            else if (root.TryGetProperty("error", out var errorElement))
            {
                response.IsSuccessful = false;
                response.ErrorMessage = errorElement.GetString() ?? "Unknown error";
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Apple Pay response");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, OrderId = orderId };
        }
    }

    private PaymentResponse ParseApplePayProfileResponse(string jsonResponse, string orderId)
    {
        try
        {
            var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            var response = new PaymentResponse { OrderId = orderId };

            if (root.TryGetProperty("success", out var successElement) && successElement.GetBoolean())
            {
                response.IsSuccessful = true;

                if (root.TryGetProperty("paymentMethodId", out var methodElement))
                {
                    response.CustomerPaymentProfileId = methodElement.GetString() ?? "";
                }

                if (root.TryGetProperty("customerId", out var customerElement))
                {
                    response.CustomerProfileId = customerElement.GetString() ?? "";
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Apple Pay profile response");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, OrderId = orderId };
        }
    }

    private PaymentResponse ParseApplePayTransactionResponse(string jsonResponse, string orderId, decimal amount)
    {
        try
        {
            var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            var response = new PaymentResponse { OrderId = orderId, Amount = amount };

            if (root.TryGetProperty("success", out var successElement) && successElement.GetBoolean())
            {
                response.IsSuccessful = true;

                if (root.TryGetProperty("transactionId", out var transElement))
                {
                    response.TransactionId = transElement.GetString() ?? "";
                }

                if (root.TryGetProperty("status", out var statusElement))
                {
                    response.ResponseCode = statusElement.GetString() ?? "";
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Apple Pay transaction response");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, OrderId = orderId };
        }
    }

    private PaymentResponse ParseApplePayTransactionDetailsResponse(string jsonResponse, string transactionId)
    {
        try
        {
            var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            var response = new PaymentResponse { TransactionId = transactionId };

            if (root.TryGetProperty("transaction", out var transElement))
            {
                response.IsSuccessful = true;

                if (transElement.TryGetProperty("status", out var statusElement))
                {
                    response.ResponseCode = statusElement.GetString() ?? "";
                }

                if (transElement.TryGetProperty("amount", out var amountElement))
                {
                    if (long.TryParse(amountElement.GetString(), out var amountCents))
                    {
                        response.Amount = amountCents / 100m;
                    }
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Apple Pay transaction details");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, TransactionId = transactionId };
        }
    }

    private PaymentResponse ParseApplePaySubscriptionResponse(string jsonResponse, string orderId)
    {
        try
        {
            var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            var response = new PaymentResponse { OrderId = orderId };

            if (root.TryGetProperty("success", out var successElement) && successElement.GetBoolean())
            {
                response.IsSuccessful = true;

                if (root.TryGetProperty("subscriptionId", out var subElement))
                {
                    response.TransactionId = subElement.GetString() ?? "";
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Apple Pay subscription response");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, OrderId = orderId };
        }
    }

    private PaymentResponse ParseApplePayBasicResponse(string jsonResponse, string referenceId)
    {
        try
        {
            var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            var response = new PaymentResponse { OrderId = referenceId };

            if (root.TryGetProperty("success", out var successElement))
            {
                response.IsSuccessful = successElement.GetBoolean();
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Apple Pay basic response");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, OrderId = referenceId };
        }
    }

    private static bool CryptographicEquals(string a, string b)
    {
        if (a == null || b == null)
            return a == b;

        if (a.Length != b.Length)
            return false;

        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] != b[i] ? 1 : 0;
        }

        return result == 0;
    }

    #endregion

    private class ApplePayToken
    {
        public string Version { get; set; } = "";
        public string Data { get; set; } = "";
        public string Signature { get; set; } = "";
        public ApplePayTokenHeader? Header { get; set; }
    }

    private class ApplePayTokenHeader
    {
        public string EphemeralPublicKey { get; set; } = "";
        public string PublicKeyHash { get; set; } = "";
        public string TransactionId { get; set; } = "";
    }
}

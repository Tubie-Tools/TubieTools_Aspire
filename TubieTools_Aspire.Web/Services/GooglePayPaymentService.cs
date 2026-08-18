using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using TubieTools_Aspire.Web.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TubieTools_Aspire.Web.Services;

/// <summary>
/// Service for processing payments through Google Pay
/// </summary>
public class GooglePayPaymentService : IPaymentService
{
    private readonly PaymentSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GooglePayPaymentService> _logger;
    private const string GooglePayApiUrl = "https://pay.google.com/payment";

    public GooglePayPaymentService(
        IOptions<PaymentSettings> settings,
        IHttpClientFactory httpClientFactory,
        ILogger<GooglePayPaymentService> logger)
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
            // Google Pay provides a token from the client which we decrypt and process
            var decryptedPaymentData = DecryptGooglePayToken(request.DataValue);

            var payload = new
            {
                merchantInfo = new
                {
                    merchantName = "TubieTools",
                    merchantId = _settings.AuthorizeNetApiLoginId
                },
                transactionInfo = new
                {
                    currencyCode = "USD",
                    countryCode = "US",
                    transactionId = request.OrderId,
                    totalPriceStatus = "FINAL",
                    totalPrice = request.Amount.ToString("F2")
                },
                paymentMethodData = decryptedPaymentData,
                emailAddress = request.CustomerEmail
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var response = await SendGooglePayRequestAsync(jsonPayload, cancellationToken);

            return ParseGooglePayResponse(response, request.OrderId, request.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Google Pay payment for order {OrderId}", request.OrderId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Google Pay processing failed: {ex.Message}",
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
            var payload = new
            {
                method = "createPaymentMethod",
                paymentMethodType = "CARD",
                customerInfo = new
                {
                    customerId = request.OrderId,
                    email = customerEmail,
                    name = customerName
                },
                paymentData = request.DataValue,
                billingAddress = new
                {
                    address1 = request.BillingAddress,
                    city = request.BillingCity,
                    state = request.BillingState,
                    postalCode = request.BillingZip,
                    country = request.BillingCountry
                }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var response = await SendGooglePayRequestAsync(jsonPayload, cancellationToken);

            return ParseGooglePayProfileResponse(response, request.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Google Pay profile for {CustomerEmail}", customerEmail);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to create Google Pay profile: {ex.Message}",
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
                paymentMethodId = paymentProfileId,
                customerId = customerProfileId,
                amount = amount.ToString("F2"),
                orderId = orderId,
                currency = "USD"
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var response = await SendGooglePayRequestAsync(jsonPayload, cancellationToken);

            return ParseGooglePayTransactionResponse(response, orderId, amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error charging Google Pay profile {CustomerProfileId}", customerProfileId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to charge Google Pay profile: {ex.Message}",
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
                amount = amount.ToString("F2"),
                reason = "Customer requested refund"
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var response = await SendGooglePayRequestAsync(jsonPayload, cancellationToken);

            return ParseGooglePayTransactionResponse(response, transactionId, amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding Google Pay transaction {TransactionId}", transactionId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Google Pay refund failed: {ex.Message}",
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
                transactionId = transactionId,
                reason = "Void requested"
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var response = await SendGooglePayRequestAsync(jsonPayload, cancellationToken);

            return ParseGooglePayTransactionResponse(response, transactionId, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding Google Pay transaction {TransactionId}", transactionId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Google Pay void failed: {ex.Message}",
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
                _logger.LogWarning("Signature key not configured for Google Pay webhook validation");
                return false;
            }

            // Verify the signature using HMAC-SHA256
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.AuthorizeNetSignatureKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var expectedSignature = Convert.ToBase64String(hash);

                return CryptographicEquals(signature, expectedSignature);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google Pay webhook signature");
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
            var response = await SendGooglePayRequestAsync(jsonPayload, cancellationToken);

            return ParseGooglePayTransactionDetailsResponse(response, transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Google Pay transaction details for {TransactionId}", transactionId);
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
            var payload = new
            {
                method = "createSubscription",
                subscriptionName = subscriptionName,
                paymentData = request.DataValue,
                billingCycle = new
                {
                    intervalLength = intervalLength,
                    intervalUnit = intervalUnit,
                    occurrences = totalOccurrences
                },
                amount = request.Amount.ToString("F2"),
                currency = "USD",
                customerId = request.OrderId,
                customerEmail = request.CustomerEmail
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var response = await SendGooglePayRequestAsync(jsonPayload, cancellationToken);

            return ParseGooglePaySubscriptionResponse(response, request.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Google Pay subscription for order {OrderId}", request.OrderId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to create subscription: {ex.Message}",
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
                subscriptionId = subscriptionId,
                reason = "Customer requested cancellation"
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var response = await SendGooglePayRequestAsync(jsonPayload, cancellationToken);

            return ParseGooglePayBasicResponse(response, subscriptionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling Google Pay subscription {SubscriptionId}", subscriptionId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to cancel subscription: {ex.Message}",
                OrderId = subscriptionId
            };
        }
    }

    #region Private Helper Methods

    private string DecryptGooglePayToken(string encryptedToken)
    {
        try
        {
            // Google Pay sends encrypted payment data that needs to be decrypted
            // This is a simplified implementation; production should use Google's libraries
            var decodedToken = Convert.FromBase64String(encryptedToken);

            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_settings.AuthorizeNetTransactionKey.PadRight(32).Substring(0, 32));
                aes.IV = decodedToken.Take(16).ToArray();

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(decodedToken.Skip(16).ToArray()))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting Google Pay token");
            return encryptedToken; // Return original if decryption fails
        }
    }

    private async Task<string> SendGooglePayRequestAsync(string payload, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(GooglePayApiUrl, content, cancellationToken);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error sending Google Pay request");
            throw;
        }
    }

    private PaymentResponse ParseGooglePayResponse(string jsonResponse, string orderId, decimal amount)
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

                if (root.TryGetProperty("authCode", out var authElement))
                {
                    response.AuthCode = authElement.GetString() ?? "";
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
            _logger.LogError(ex, "Error parsing Google Pay response");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, OrderId = orderId };
        }
    }

    private PaymentResponse ParseGooglePayProfileResponse(string jsonResponse, string orderId)
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
            _logger.LogError(ex, "Error parsing Google Pay profile response");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, OrderId = orderId };
        }
    }

    private PaymentResponse ParseGooglePayTransactionResponse(string jsonResponse, string orderId, decimal amount)
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
            _logger.LogError(ex, "Error parsing Google Pay transaction response");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, OrderId = orderId };
        }
    }

    private PaymentResponse ParseGooglePayTransactionDetailsResponse(string jsonResponse, string transactionId)
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
                    decimal.TryParse(amountElement.GetString(), out var amount);
                    response.Amount = amount;
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Google Pay transaction details");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, TransactionId = transactionId };
        }
    }

    private PaymentResponse ParseGooglePaySubscriptionResponse(string jsonResponse, string orderId)
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
            _logger.LogError(ex, "Error parsing Google Pay subscription response");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, OrderId = orderId };
        }
    }

    private PaymentResponse ParseGooglePayBasicResponse(string jsonResponse, string referenceId)
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
            _logger.LogError(ex, "Error parsing Google Pay basic response");
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
}

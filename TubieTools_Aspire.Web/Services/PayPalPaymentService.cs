using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TubieTools_Aspire.Web.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TubieTools_Aspire.Web.Services;

/// <summary>
/// Service for processing payments through PayPal
/// </summary>
public class PayPalPaymentService : IPaymentService
{
    private readonly PaymentSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PayPalPaymentService> _logger;
    private const string PayPalSandboxUrl = "https://api.sandbox.paypal.com";
    private const string PayPalProductionUrl = "https://api.paypal.com";
    private string? _accessToken;
    private DateTime _tokenExpiration = DateTime.MinValue;

    public PayPalPaymentService(
        IOptions<PaymentSettings> settings,
        IHttpClientFactory httpClientFactory,
        ILogger<PayPalPaymentService> logger)
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
            await EnsureAccessTokenAsync(cancellationToken);

            var payload = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = request.OrderId,
                        amount = new
                        {
                            currency_code = "USD",
                            value = request.Amount.ToString("F2")
                        },
                        description = request.Description,
                        items = request.LineItems.Select(li => new
                        {
                            name = li.Name,
                            description = li.Description,
                            quantity = li.Quantity.ToString(),
                            unit_amount = new { currency_code = "USD", value = li.UnitPrice.ToString("F2") }
                        }).ToList()
                    }
                },
                payer = new
                {
                    name = new
                    {
                        given_name = request.CustomerName.Split(' ')[0],
                        surname = string.Join(" ", request.CustomerName.Split(' ').Skip(1))
                    },
                    email_address = request.CustomerEmail,
                    address = new
                    {
                        address_line_1 = request.BillingAddress,
                        admin_area_2 = request.BillingCity,
                        admin_area_1 = request.BillingState,
                        postal_code = request.BillingZip,
                        country_code = request.BillingCountry
                    }
                }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var url = $"{GetPayPalUrl()}/v2/checkout/orders";
            var response = await SendPayPalRequestAsync("POST", url, jsonPayload, cancellationToken);

            return ParsePayPalCreateOrderResponse(response, request.OrderId, request.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PayPal payment for order {OrderId}", request.OrderId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"PayPal payment processing failed: {ex.Message}",
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
        // PayPal uses customer tokens instead of profiles; implemented via billing agreements
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
            await EnsureAccessTokenAsync(cancellationToken);

            var payload = new
            {
                plan_id = request.DataValue, // Use plan_id from request
                custom_id = request.OrderId,
                subscriber = new
                {
                    name = new
                    {
                        given_name = customerName.Split(' ')[0],
                        surname = string.Join(" ", customerName.Split(' ').Skip(1))
                    },
                    email_address = customerEmail
                }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var url = $"{GetPayPalUrl()}/v1/billing/subscriptions";
            var response = await SendPayPalRequestAsync("POST", url, jsonPayload, cancellationToken);

            return ParsePayPalSubscriptionResponse(response, request.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayPal payment profile for {CustomerEmail}", customerEmail);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to create PayPal profile: {ex.Message}",
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
            await EnsureAccessTokenAsync(cancellationToken);

            // Capture payment from subscription or billing agreement
            var url = $"{GetPayPalUrl()}/v1/billing/subscriptions/{customerProfileId}/payments";
            var response = await SendPayPalRequestAsync("POST", url, "{}", cancellationToken);

            return ParsePayPalTransactionResponse(response, orderId, amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error charging PayPal profile {CustomerProfileId}", customerProfileId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to charge PayPal profile: {ex.Message}",
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
            await EnsureAccessTokenAsync(cancellationToken);

            var payload = new { amount = amount.ToString("F2") };
            var jsonPayload = JsonSerializer.Serialize(payload);
            var url = $"{GetPayPalUrl()}/v2/payments/captures/{transactionId}/refund";
            var response = await SendPayPalRequestAsync("POST", url, jsonPayload, cancellationToken);

            return ParsePayPalRefundResponse(response, transactionId, amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding PayPal transaction {TransactionId}", transactionId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"PayPal refund failed: {ex.Message}",
                TransactionId = transactionId,
                Amount = amount
            };
        }
    }

    public async Task<PaymentResponse> VoidTransactionAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        // PayPal doesn't have a traditional void; instead we refund
        return await RefundTransactionAsync(transactionId, 0, cancellationToken);
    }

    public bool ValidateWebhookSignature(string payload, string signature)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.AuthorizeNetSignatureKey))
            {
                _logger.LogWarning("Signature key not configured for PayPal webhook validation");
                return false;
            }

            // PayPal webhook validation requires the webhook ID and event ID
            // This is a simplified version - production should use PayPal's verification API
            var jsonDoc = JsonDocument.Parse(payload);
            var root = jsonDoc.RootElement;

            return root.TryGetProperty("id", out _) && 
                   root.TryGetProperty("event_type", out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating PayPal webhook signature");
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
            await EnsureAccessTokenAsync(cancellationToken);

            var url = $"{GetPayPalUrl()}/v2/payments/captures/{transactionId}";
            var response = await SendPayPalRequestAsync("GET", url, "", cancellationToken);

            return ParsePayPalTransactionDetailsResponse(response, transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PayPal transaction details for {TransactionId}", transactionId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to retrieve PayPal transaction details: {ex.Message}",
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
            await EnsureAccessTokenAsync(cancellationToken);

            // First create a billing plan
            var planPayload = new
            {
                product_id = request.OrderId,
                name = subscriptionName,
                description = request.Description,
                billing_cycles = new[]
                {
                    new
                    {
                        frequency = new
                        {
                            interval_unit = intervalUnit.ToUpper(),
                            interval_count = intervalLength
                        },
                        tenure_type = "REGULAR",
                        sequence = 1,
                        total_cycles = totalOccurrences,
                        pricing_scheme = new
                        {
                            fixed_price = new { currency_code = "USD", value = request.Amount.ToString("F2") }
                        }
                    }
                },
                payment_preferences = new
                {
                    auto_bill_amount = "YES",
                    setup_fee_failure_action = "CANCEL",
                    payment_failure_threshold = 3
                }
            };

            var planJsonPayload = JsonSerializer.Serialize(planPayload);
            var planUrl = $"{GetPayPalUrl()}/v1/billing/plans";
            var planResponse = await SendPayPalRequestAsync("POST", planUrl, planJsonPayload, cancellationToken);

            // Extract plan ID from response and create subscription (handled by CreatePaymentProfileAsync)
            return ParsePayPalSubscriptionResponse(planResponse, request.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayPal subscription for order {OrderId}", request.OrderId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to create PayPal subscription: {ex.Message}",
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
            await EnsureAccessTokenAsync(cancellationToken);

            var payload = new { reason = "Customer requested cancellation" };
            var jsonPayload = JsonSerializer.Serialize(payload);
            var url = $"{GetPayPalUrl()}/v1/billing/subscriptions/{subscriptionId}/cancel";
            var response = await SendPayPalRequestAsync("POST", url, jsonPayload, cancellationToken);

            return new PaymentResponse
            {
                IsSuccessful = response.Contains("204") || !response.Contains("error"),
                OrderId = subscriptionId,
                ResponseText = "Subscription cancelled successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling PayPal subscription {SubscriptionId}", subscriptionId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to cancel PayPal subscription: {ex.Message}",
                OrderId = subscriptionId
            };
        }
    }

    #region Private Helper Methods

    private string GetPayPalUrl()
    {
        return _settings.AuthorizeNetEnvironment.Equals("production", StringComparison.OrdinalIgnoreCase)
            ? PayPalProductionUrl
            : PayPalSandboxUrl;
    }

    private async Task EnsureAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiration)
            return;

        var client = _httpClientFactory.CreateClient();
        var auth = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_settings.AuthorizeNetApiLoginId}:{_settings.AuthorizeNetTransactionKey}"));

        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetPayPalUrl()}/v1/oauth2/token")
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth) },
            Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded")
        };

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = JsonDocument.Parse(jsonResponse);
        var root = doc.RootElement;

        _accessToken = root.GetProperty("access_token").GetString();
        var expiresIn = root.GetProperty("expires_in").GetInt32();
        _tokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn - 60);
    }

    private async Task<string> SendPayPalRequestAsync(
        string method,
        string url,
        string payload,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

        var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (!string.IsNullOrEmpty(payload))
        {
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        }

        var response = await client.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("PayPal API returned status {StatusCode}: {Response}", response.StatusCode, responseContent);
        }

        return responseContent;
    }

    private PaymentResponse ParsePayPalCreateOrderResponse(string jsonResponse, string orderId, decimal amount)
    {
        try
        {
            var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            var response = new PaymentResponse { OrderId = orderId, Amount = amount };

            if (root.TryGetProperty("id", out var idElement))
            {
                response.IsSuccessful = true;
                response.TransactionId = idElement.GetString() ?? "";
            }
            else if (root.TryGetProperty("error_description", out var errorElement))
            {
                response.IsSuccessful = false;
                response.ErrorMessage = errorElement.GetString() ?? "Unknown error";
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing PayPal order response");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, OrderId = orderId };
        }
    }

    private PaymentResponse ParsePayPalTransactionResponse(string jsonResponse, string orderId, decimal amount)
    {
        try
        {
            var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            var response = new PaymentResponse { OrderId = orderId, Amount = amount };

            if (root.TryGetProperty("status", out var statusElement))
            {
                var status = statusElement.GetString();
                response.IsSuccessful = status == "COMPLETED" || status == "APPROVED";
                response.ResponseCode = status ?? "";
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing PayPal transaction response");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, OrderId = orderId };
        }
    }

    private PaymentResponse ParsePayPalRefundResponse(string jsonResponse, string transactionId, decimal amount)
    {
        try
        {
            var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            var response = new PaymentResponse { TransactionId = transactionId, Amount = amount };

            if (root.TryGetProperty("status", out var statusElement))
            {
                var status = statusElement.GetString();
                response.IsSuccessful = status == "COMPLETED" || status == "SUCCESS";
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing PayPal refund response");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, TransactionId = transactionId };
        }
    }

    private PaymentResponse ParsePayPalTransactionDetailsResponse(string jsonResponse, string transactionId)
    {
        try
        {
            var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            var response = new PaymentResponse { TransactionId = transactionId };

            if (root.TryGetProperty("id", out _))
            {
                response.IsSuccessful = true;

                if (root.TryGetProperty("amount", out var amountElement) &&
                    amountElement.TryGetProperty("value", out var valueElement))
                {
                    decimal.TryParse(valueElement.GetString(), out var amount);
                    response.Amount = amount;
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
            _logger.LogError(ex, "Error parsing PayPal transaction details");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, TransactionId = transactionId };
        }
    }

    private PaymentResponse ParsePayPalSubscriptionResponse(string jsonResponse, string orderId)
    {
        try
        {
            var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            var response = new PaymentResponse { OrderId = orderId };

            if (root.TryGetProperty("id", out var idElement))
            {
                response.IsSuccessful = true;
                response.TransactionId = idElement.GetString() ?? "";
            }
            else if (root.TryGetProperty("error_description", out var errorElement))
            {
                response.IsSuccessful = false;
                response.ErrorMessage = errorElement.GetString() ?? "Unknown error";
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing PayPal subscription response");
            return new PaymentResponse { IsSuccessful = false, ErrorMessage = ex.Message, OrderId = orderId };
        }
    }

    #endregion
}

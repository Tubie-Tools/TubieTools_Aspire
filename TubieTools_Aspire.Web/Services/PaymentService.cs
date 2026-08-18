using System.Text;
using System.Security.Cryptography;
using TubieTools_Aspire.Web.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace TubieTools_Aspire.Web.Services;

/// <summary>
/// Service for processing payments through Authorize.Net
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly PaymentSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PaymentService> _logger;
    private const string AuthorizeNetSandboxUrl = "https://apitest.authorize.net/xml/v1/request.api";
    private const string AuthorizeNetProductionUrl = "https://api.authorize.net/xml/v1/request.api";

    public PaymentService(
        IOptions<PaymentSettings> settings,
        IHttpClientFactory httpClientFactory,
        ILogger<PaymentService> logger)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Process a payment transaction using Authorize.Net
    /// </summary>
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
            var payload = BuildCreateTransactionPayload(request);
            var response = await SendAuthorizenetRequestAsync(payload, cancellationToken);
            return ParseCreateTransactionResponse(response, request.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", request.OrderId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Payment processing failed: {ex.Message}",
                OrderId = request.OrderId,
                Amount = request.Amount
            };
        }
    }

    /// <summary>
    /// Create a customer payment profile for recurring billing
    /// </summary>
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
            var payload = BuildCreateCustomerProfilePayload(request, customerName, customerEmail);
            var response = await SendAuthorizenetRequestAsync(payload, cancellationToken);
            return ParseCreateCustomerProfileResponse(response, request.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment profile for {CustomerEmail}", customerEmail);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to create payment profile: {ex.Message}",
                OrderId = request.OrderId
            };
        }
    }

    /// <summary>
    /// Charge a previously created payment profile
    /// </summary>
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
            var payload = BuildChargeProfilePayload(customerProfileId, paymentProfileId, amount, orderId);
            var response = await SendAuthorizenetRequestAsync(payload, cancellationToken);
            return ParseCreateTransactionResponse(response, orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error charging payment profile {CustomerProfileId}", customerProfileId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to charge payment profile: {ex.Message}",
                OrderId = orderId,
                Amount = amount
            };
        }
    }

    /// <summary>
    /// Refund a transaction
    /// </summary>
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
            var payload = BuildRefundTransactionPayload(transactionId, amount);
            var response = await SendAuthorizenetRequestAsync(payload, cancellationToken);
            return ParseTransactionResponse(response, "refund", transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding transaction {TransactionId}", transactionId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Refund failed: {ex.Message}",
                TransactionId = transactionId,
                Amount = amount
            };
        }
    }

    /// <summary>
    /// Void an authorized transaction
    /// </summary>
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
            var payload = BuildVoidTransactionPayload(transactionId);
            var response = await SendAuthorizenetRequestAsync(payload, cancellationToken);
            return ParseTransactionResponse(response, "void", transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding transaction {TransactionId}", transactionId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Void failed: {ex.Message}",
                TransactionId = transactionId
            };
        }
    }

    /// <summary>
    /// Validate webhook signature from Authorize.Net
    /// </summary>
    public bool ValidateWebhookSignature(string payload, string signature)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.AuthorizeNetSignatureKey))
            {
                _logger.LogWarning("Signature key not configured for webhook validation");
                return false;
            }

            // Extract the timestamp and hash from the signature
            var signatureParts = signature.Split(",");
            if (signatureParts.Length < 2)
                return false;

            var provider = signatureParts[0];
            var hash = signatureParts[1];

            if (!provider.Equals("sha512", StringComparison.OrdinalIgnoreCase))
                return false;

            // Compute the expected hash
            var expectedHash = ComputeWebhookHash(payload);

            // Compare hashes (constant-time comparison to prevent timing attacks)
            return CryptographicEquals(hash.ToUpperInvariant(), expectedHash.ToUpperInvariant());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating webhook signature");
            return false;
        }
    }

    /// <summary>
    /// Get transaction details
    /// </summary>
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
            var payload = BuildGetTransactionDetailsPayload(transactionId);
            var response = await SendAuthorizenetRequestAsync(payload, cancellationToken);
            return ParseTransactionDetailsResponse(response, transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction details for {TransactionId}", transactionId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to retrieve transaction details: {ex.Message}",
                TransactionId = transactionId
            };
        }
    }

    /// <summary>
    /// Create a recurring billing subscription
    /// </summary>
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
            var payload = BuildCreateSubscriptionPayload(
                request, subscriptionName, intervalLength, intervalUnit, totalOccurrences);
            var response = await SendAuthorizenetRequestAsync(payload, cancellationToken);
            return ParseCreateSubscriptionResponse(response, request.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription for order {OrderId}", request.OrderId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to create subscription: {ex.Message}",
                OrderId = request.OrderId,
                Amount = request.Amount
            };
        }
    }

    /// <summary>
    /// Cancel a subscription
    /// </summary>
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
            var payload = BuildCancelSubscriptionPayload(subscriptionId);
            var response = await SendAuthorizenetRequestAsync(payload, cancellationToken);
            return ParseBasicResponse(response, "cancel subscription", subscriptionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling subscription {SubscriptionId}", subscriptionId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Failed to cancel subscription: {ex.Message}",
                OrderId = subscriptionId
            };
        }
    }

    #region Private Helper Methods

    private string GetAuthorizenetUrl()
    {
        return _settings.AuthorizeNetEnvironment.Equals("production", StringComparison.OrdinalIgnoreCase)
            ? AuthorizeNetProductionUrl
            : AuthorizeNetSandboxUrl;
    }

    private async Task<string> SendAuthorizenetRequestAsync(string payload, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var content = new StringContent(payload, Encoding.UTF8, "application/xml");

        var response = await client.PostAsync(GetAuthorizenetUrl(), content, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private string BuildCreateTransactionPayload(PaymentRequest request)
    {
        var payload = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<createTransactionRequest xmlns=""AnetApi/xml/v1/schema/AnetApiSchema.xsd"">
    <merchantAuthentication>
        <name>{XmlEscape(_settings.AuthorizeNetApiLoginId)}</name>
        <transactionKey>{XmlEscape(_settings.AuthorizeNetTransactionKey)}</transactionKey>
    </merchantAuthentication>
    <refId>{XmlEscape(request.OrderId)}</refId>
    <transactionRequest>
        <transactionType>authCaptureTransaction</transactionType>
        <amount>{request.Amount:F2}</amount>
        <payment>
            <opaqueData>
                <dataDescriptor>{XmlEscape(request.DataDescriptor)}</dataDescriptor>
                <dataValue>{XmlEscape(request.DataValue)}</dataValue>
            </opaqueData>
        </payment>
        <order>
            <invoiceNumber>{XmlEscape(request.InvoiceNumber)}</invoiceNumber>
            <description>{XmlEscape(request.Description)}</description>
            <poNumber>{XmlEscape(request.PurchaseOrderNumber)}</poNumber>
        </order>
        <lineItems>";

        foreach (var item in request.LineItems)
        {
            payload += $@"
            <lineItem>
                <itemId>{XmlEscape(item.ItemId)}</itemId>
                <name>{XmlEscape(item.Name)}</name>
                <description>{XmlEscape(item.Description)}</description>
                <quantity>{item.Quantity}</quantity>
                <unitPrice>{item.UnitPrice:F2}</unitPrice>
            </lineItem>";
        }

        payload += $@"
        </lineItems>
        <customer>
            <id>{XmlEscape(request.OrderId)}</id>
            <email>{XmlEscape(request.CustomerEmail)}</email>
        </customer>
        <billTo>
            <firstName>{XmlEscape(request.CustomerName.Split(' ')[0])}</firstName>
            <lastName>{XmlEscape(string.Join(" ", request.CustomerName.Split(' ').Skip(1)))}</lastName>
            <address>{XmlEscape(request.BillingAddress)}</address>
            <city>{XmlEscape(request.BillingCity)}</city>
            <state>{XmlEscape(request.BillingState)}</state>
            <zip>{XmlEscape(request.BillingZip)}</zip>
            <country>{XmlEscape(request.BillingCountry)}</country>
            <phoneNumber>{XmlEscape(request.CustomerPhone)}</phoneNumber>
        </billTo>
        <userFields>
            <userField>
                <name>customer_ip</name>
                <value>{XmlEscape(request.CustomerIPAddress)}</value>
            </userField>
        </userFields>
        <customerProfileCreateRequest>
            <createProfile>{(request.CreatePaymentProfile ? "true" : "false")}</createProfile>
        </customerProfileCreateRequest>
    </transactionRequest>
</createTransactionRequest>";

        return payload;
    }

    private string BuildCreateCustomerProfilePayload(PaymentRequest request, string customerName, string customerEmail)
    {
        var payload = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<createCustomerProfileRequest xmlns=""AnetApi/xml/v1/schema/AnetApiSchema.xsd"">
    <merchantAuthentication>
        <name>{XmlEscape(_settings.AuthorizeNetApiLoginId)}</name>
        <transactionKey>{XmlEscape(_settings.AuthorizeNetTransactionKey)}</transactionKey>
    </merchantAuthentication>
    <profile>
        <merchantCustomerId>{XmlEscape(request.OrderId)}</merchantCustomerId>
        <description>{XmlEscape(customerName)}</description>
        <email>{XmlEscape(customerEmail)}</email>
        <paymentProfiles>
            <payment>
                <opaqueData>
                    <dataDescriptor>{XmlEscape(request.DataDescriptor)}</dataDescriptor>
                    <dataValue>{XmlEscape(request.DataValue)}</dataValue>
                </opaqueData>
            </payment>
            <billTo>
                <firstName>{XmlEscape(customerName.Split(' ')[0])}</firstName>
                <lastName>{XmlEscape(string.Join(" ", customerName.Split(' ').Skip(1)))}</lastName>
                <address>{XmlEscape(request.BillingAddress)}</address>
                <city>{XmlEscape(request.BillingCity)}</city>
                <state>{XmlEscape(request.BillingState)}</state>
                <zip>{XmlEscape(request.BillingZip)}</zip>
                <country>{XmlEscape(request.BillingCountry)}</country>
            </billTo>
        </paymentProfiles>
    </profile>
    <validationMode>testMode</validationMode>
</createCustomerProfileRequest>";

        return payload;
    }

    private string BuildChargeProfilePayload(string customerProfileId, string paymentProfileId, decimal amount, string orderId)
    {
        var payload = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<createTransactionRequest xmlns=""AnetApi/xml/v1/schema/AnetApiSchema.xsd"">
    <merchantAuthentication>
        <name>{XmlEscape(_settings.AuthorizeNetApiLoginId)}</name>
        <transactionKey>{XmlEscape(_settings.AuthorizeNetTransactionKey)}</transactionKey>
    </merchantAuthentication>
    <refId>{XmlEscape(orderId)}</refId>
    <transactionRequest>
        <transactionType>authCaptureTransaction</transactionType>
        <amount>{amount:F2}</amount>
        <profile>
            <customerProfileId>{XmlEscape(customerProfileId)}</customerProfileId>
            <paymentProfile>
                <paymentProfileId>{XmlEscape(paymentProfileId)}</paymentProfileId>
            </paymentProfile>
        </profile>
    </transactionRequest>
</createTransactionRequest>";

        return payload;
    }

    private string BuildRefundTransactionPayload(string transactionId, decimal amount)
    {
        var payload = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<createTransactionRequest xmlns=""AnetApi/xml/v1/schema/AnetApiSchema.xsd"">
    <merchantAuthentication>
        <name>{XmlEscape(_settings.AuthorizeNetApiLoginId)}</name>
        <transactionKey>{XmlEscape(_settings.AuthorizeNetTransactionKey)}</transactionKey>
    </merchantAuthentication>
    <refId>{XmlEscape(transactionId)}</refId>
    <transactionRequest>
        <transactionType>refundTransaction</transactionType>
        <amount>{amount:F2}</amount>
        <refTransId>{XmlEscape(transactionId)}</refTransId>
    </transactionRequest>
</createTransactionRequest>";

        return payload;
    }

    private string BuildVoidTransactionPayload(string transactionId)
    {
        var payload = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<createTransactionRequest xmlns=""AnetApi/xml/v1/schema/AnetApiSchema.xsd"">
    <merchantAuthentication>
        <name>{XmlEscape(_settings.AuthorizeNetApiLoginId)}</name>
        <transactionKey>{XmlEscape(_settings.AuthorizeNetTransactionKey)}</transactionKey>
    </merchantAuthentication>
    <refId>{XmlEscape(transactionId)}</refId>
    <transactionRequest>
        <transactionType>voidTransaction</transactionType>
        <refTransId>{XmlEscape(transactionId)}</refTransId>
    </transactionRequest>
</createTransactionRequest>";

        return payload;
    }

    private string BuildGetTransactionDetailsPayload(string transactionId)
    {
        var payload = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<getTransactionDetailsRequest xmlns=""AnetApi/xml/v1/schema/AnetApiSchema.xsd"">
    <merchantAuthentication>
        <name>{XmlEscape(_settings.AuthorizeNetApiLoginId)}</name>
        <transactionKey>{XmlEscape(_settings.AuthorizeNetTransactionKey)}</transactionKey>
    </merchantAuthentication>
    <transId>{XmlEscape(transactionId)}</transId>
</getTransactionDetailsRequest>";

        return payload;
    }

    private string BuildCreateSubscriptionPayload(
        PaymentRequest request,
        string subscriptionName,
        int intervalLength,
        string intervalUnit,
        int totalOccurrences)
    {
        var payload = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ARBCreateSubscriptionRequest xmlns=""AnetApi/xml/v1/schema/AnetApiSchema.xsd"">
    <merchantAuthentication>
        <name>{XmlEscape(_settings.AuthorizeNetApiLoginId)}</name>
        <transactionKey>{XmlEscape(_settings.AuthorizeNetTransactionKey)}</transactionKey>
    </merchantAuthentication>
    <refId>{XmlEscape(request.OrderId)}</refId>
    <subscription>
        <name>{XmlEscape(subscriptionName)}</name>
        <paymentSchedule>
            <interval>
                <length>{intervalLength}</length>
                <unit>{XmlEscape(intervalUnit)}</unit>
            </interval>
            <startDate>{DateTime.UtcNow:yyyy-MM-dd}</startDate>
            <totalOccurrences>{totalOccurrences}</totalOccurrences>
        </paymentSchedule>
        <amount>{request.Amount:F2}</amount>
        <payment>
            <opaqueData>
                <dataDescriptor>{XmlEscape(request.DataDescriptor)}</dataDescriptor>
                <dataValue>{XmlEscape(request.DataValue)}</dataValue>
            </opaqueData>
        </payment>
        <billTo>
            <firstName>{XmlEscape(request.CustomerName.Split(' ')[0])}</firstName>
            <lastName>{XmlEscape(string.Join(" ", request.CustomerName.Split(' ').Skip(1)))}</lastName>
            <address>{XmlEscape(request.BillingAddress)}</address>
            <city>{XmlEscape(request.BillingCity)}</city>
            <state>{XmlEscape(request.BillingState)}</state>
            <zip>{XmlEscape(request.BillingZip)}</zip>
            <country>{XmlEscape(request.BillingCountry)}</country>
        </billTo>
    </subscription>
</ARBCreateSubscriptionRequest>";

        return payload;
    }

    private string BuildCancelSubscriptionPayload(string subscriptionId)
    {
        var payload = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ARBCancelSubscriptionRequest xmlns=""AnetApi/xml/v1/schema/AnetApiSchema.xsd"">
    <merchantAuthentication>
        <name>{XmlEscape(_settings.AuthorizeNetApiLoginId)}</name>
        <transactionKey>{XmlEscape(_settings.AuthorizeNetTransactionKey)}</transactionKey>
    </merchantAuthentication>
    <refId>{XmlEscape(subscriptionId)}</refId>
    <subscriptionId>{XmlEscape(subscriptionId)}</subscriptionId>
</ARBCancelSubscriptionRequest>";

        return payload;
    }

    private PaymentResponse ParseCreateTransactionResponse(string xmlResponse, string orderId)
    {
        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(xmlResponse);
            var root = doc.Root;
            var ns = root?.Name.NamespaceName ?? "";

            var resultCode = root?.Element(System.Xml.Linq.XName.Get("messages", ns))
                ?.Element(System.Xml.Linq.XName.Get("resultCode", ns))?.Value ?? "Error";

            var response = new PaymentResponse { OrderId = orderId };

            if (resultCode == "Ok")
            {
                var transResult = root?.Element(System.Xml.Linq.XName.Get("transactionResponse", ns));
                response.IsSuccessful = transResult?.Element(System.Xml.Linq.XName.Get("responseCode", ns))?.Value == "1";
                response.TransactionId = transResult?.Element(System.Xml.Linq.XName.Get("transId", ns))?.Value ?? "";
                response.ResponseCode = transResult?.Element(System.Xml.Linq.XName.Get("responseCode", ns))?.Value ?? "";
                response.ResponseReasonCode = transResult?.Element(System.Xml.Linq.XName.Get("responseReasonCode", ns))?.Value ?? "";
                response.ResponseText = transResult?.Element(System.Xml.Linq.XName.Get("responseReasonDescription", ns))?.Value ?? "";
                response.AuthCode = transResult?.Element(System.Xml.Linq.XName.Get("authCode", ns))?.Value ?? "";
                response.AvsResponse = transResult?.Element(System.Xml.Linq.XName.Get("avsResultCode", ns))?.Value ?? "";
                response.CvvResponse = transResult?.Element(System.Xml.Linq.XName.Get("cvvResultCode", ns))?.Value ?? "";

                var customerProfile = transResult?.Element(System.Xml.Linq.XName.Get("profile", ns));
                if (customerProfile != null)
                {
                    response.CustomerProfileId = customerProfile.Element(System.Xml.Linq.XName.Get("customerProfileId", ns))?.Value ?? "";
                    response.CustomerPaymentProfileId = customerProfile
                        .Element(System.Xml.Linq.XName.Get("customerPaymentProfileId", ns))?.Value ?? "";
                }
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = root?.Element(System.Xml.Linq.XName.Get("messages", ns))
                    ?.Element(System.Xml.Linq.XName.Get("message", ns))
                    ?.Element(System.Xml.Linq.XName.Get("text", ns))?.Value ?? "Unknown error";
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing transaction response for order {OrderId}", orderId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Error parsing response: {ex.Message}",
                OrderId = orderId
            };
        }
    }

    private PaymentResponse ParseCreateCustomerProfileResponse(string xmlResponse, string orderId)
    {
        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(xmlResponse);
            var root = doc.Root;
            var ns = root?.Name.NamespaceName ?? "";

            var resultCode = root?.Element(System.Xml.Linq.XName.Get("messages", ns))
                ?.Element(System.Xml.Linq.XName.Get("resultCode", ns))?.Value ?? "Error";

            var response = new PaymentResponse { OrderId = orderId };

            if (resultCode == "Ok")
            {
                response.IsSuccessful = true;
                response.CustomerProfileId = root?.Element(System.Xml.Linq.XName.Get("customerProfileId", ns))?.Value ?? "";
                response.CustomerPaymentProfileId = root?.Element(System.Xml.Linq.XName.Get("customerPaymentProfileIdList", ns))
                    ?.Element(System.Xml.Linq.XName.Get("numericString", ns))?.Value ?? "";
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = root?.Element(System.Xml.Linq.XName.Get("messages", ns))
                    ?.Element(System.Xml.Linq.XName.Get("message", ns))
                    ?.Element(System.Xml.Linq.XName.Get("text", ns))?.Value ?? "Unknown error";
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing customer profile response");
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Error parsing response: {ex.Message}",
                OrderId = orderId
            };
        }
    }

    private PaymentResponse ParseTransactionResponse(string xmlResponse, string operationType, string referenceId)
    {
        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(xmlResponse);
            var root = doc.Root;
            var ns = root?.Name.NamespaceName ?? "";

            var resultCode = root?.Element(System.Xml.Linq.XName.Get("messages", ns))
                ?.Element(System.Xml.Linq.XName.Get("resultCode", ns))?.Value ?? "Error";

            var response = new PaymentResponse { TransactionId = referenceId };

            if (resultCode == "Ok")
            {
                response.IsSuccessful = true;
                response.ResponseText = $"{operationType} successful";
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = root?.Element(System.Xml.Linq.XName.Get("messages", ns))
                    ?.Element(System.Xml.Linq.XName.Get("message", ns))
                    ?.Element(System.Xml.Linq.XName.Get("text", ns))?.Value ?? "Unknown error";
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing {OperationType} response", operationType);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Error parsing response: {ex.Message}",
                TransactionId = referenceId
            };
        }
    }

    private PaymentResponse ParseTransactionDetailsResponse(string xmlResponse, string transactionId)
    {
        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(xmlResponse);
            var root = doc.Root;
            var ns = root?.Name.NamespaceName ?? "";

            var resultCode = root?.Element(System.Xml.Linq.XName.Get("messages", ns))
                ?.Element(System.Xml.Linq.XName.Get("resultCode", ns))?.Value ?? "Error";

            var response = new PaymentResponse { TransactionId = transactionId };

            if (resultCode == "Ok")
            {
                var txn = root?.Element(System.Xml.Linq.XName.Get("transaction", ns));
                response.IsSuccessful = true;
                response.OrderId = txn?.Element(System.Xml.Linq.XName.Get("order", ns))
                    ?.Element(System.Xml.Linq.XName.Get("invoiceNumber", ns))?.Value ?? "";
                response.Amount = decimal.TryParse(
                    txn?.Element(System.Xml.Linq.XName.Get("authAmount", ns))?.Value ?? "0",
                    out var amount) ? amount : 0;
                response.ResponseCode = txn?.Element(System.Xml.Linq.XName.Get("responseCode", ns))?.Value ?? "";
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = root?.Element(System.Xml.Linq.XName.Get("messages", ns))
                    ?.Element(System.Xml.Linq.XName.Get("message", ns))
                    ?.Element(System.Xml.Linq.XName.Get("text", ns))?.Value ?? "Unknown error";
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing transaction details response");
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Error parsing response: {ex.Message}",
                TransactionId = transactionId
            };
        }
    }

    private PaymentResponse ParseCreateSubscriptionResponse(string xmlResponse, string orderId)
    {
        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(xmlResponse);
            var root = doc.Root;
            var ns = root?.Name.NamespaceName ?? "";

            var resultCode = root?.Element(System.Xml.Linq.XName.Get("messages", ns))
                ?.Element(System.Xml.Linq.XName.Get("resultCode", ns))?.Value ?? "Error";

            var response = new PaymentResponse { OrderId = orderId };

            if (resultCode == "Ok")
            {
                response.IsSuccessful = true;
                response.TransactionId = root?.Element(System.Xml.Linq.XName.Get("subscriptionId", ns))?.Value ?? "";
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = root?.Element(System.Xml.Linq.XName.Get("messages", ns))
                    ?.Element(System.Xml.Linq.XName.Get("message", ns))
                    ?.Element(System.Xml.Linq.XName.Get("text", ns))?.Value ?? "Unknown error";
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing subscription response");
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Error parsing response: {ex.Message}",
                OrderId = orderId
            };
        }
    }

    private PaymentResponse ParseBasicResponse(string xmlResponse, string operationType, string referenceId)
    {
        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(xmlResponse);
            var root = doc.Root;
            var ns = root?.Name.NamespaceName ?? "";

            var resultCode = root?.Element(System.Xml.Linq.XName.Get("messages", ns))
                ?.Element(System.Xml.Linq.XName.Get("resultCode", ns))?.Value ?? "Error";

            var response = new PaymentResponse { OrderId = referenceId };

            if (resultCode == "Ok")
            {
                response.IsSuccessful = true;
                response.ResponseText = $"{operationType} successful";
            }
            else
            {
                response.IsSuccessful = false;
                response.ErrorMessage = root?.Element(System.Xml.Linq.XName.Get("messages", ns))
                    ?.Element(System.Xml.Linq.XName.Get("message", ns))
                    ?.Element(System.Xml.Linq.XName.Get("text", ns))?.Value ?? "Unknown error";
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing basic response for {OperationType}", operationType);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Error parsing response: {ex.Message}",
                OrderId = referenceId
            };
        }
    }

    private string ComputeWebhookHash(string payload)
    {
        using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_settings.AuthorizeNetSignatureKey)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return BitConverter.ToString(hash).Replace("-", "");
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

    private static string XmlEscape(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return System.Security.SecurityElement.Escape(text);
    }

    #endregion
}

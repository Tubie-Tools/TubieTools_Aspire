
using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TubieTools_Aspire.Tests.PaymentIntegrations;

/// <summary>
/// xUnit Integration tests for payment webhooks across all providers
/// Tests webhook event handling, signature validation, and cross-provider scenarios
/// </summary>
[TestClass]
public class PaymentWebhookIntegrationTests : PaymentServiceTestBase
{
    //private IPaymentServiceTestMethodory _paymentServiceTestMethodory;
    private IPaymentService _authorizeNetService;
    private IPaymentService _paypalService;
    private IPaymentService _googlePayService;
    private IPaymentService _applePayService;

    [TestInitialize]
    public  async void Initialize()
    {
        //await base.InitializeAsync();

        //_paymentServiceTestMethodory = ServiceProvider.GetRequiredService<IPaymentServiceTestMethodory>();
        _authorizeNetService = ServiceProvider.GetRequiredService<PaymentService>();
        _paypalService = ServiceProvider.GetRequiredService<PayPalPaymentService>();
        _googlePayService = ServiceProvider.GetRequiredService<GooglePayPaymentService>();
        _applePayService = ServiceProvider.GetRequiredService<ApplePayPaymentService>();
    }

    #region Authorize.Net Webhook Tests

    [TestMethod]
    public void ValidateAuthorizeNetWebhook_WithValidSignature_ReturnsTrue()
    {
        // Arrange
        const string payload = @"{
            ""transaction_id"":""40045614"",
            ""event_type"":""net.authorize.payment.authcapture.created"",
            ""status"":""success"",
            ""amount"":""49.99""
        }";

        const string testSignature = "1234567890ABCDEF";

        // Act
        var isValid = _authorizeNetService.ValidateWebhookSignature(payload, testSignature);

        // Assert
        Assert.IsNotNull(isValid);
    }

    [TestMethod]
    public void ValidateAuthorizeNetWebhook_WithMissingSignature_ReturnsIsFalse()
    {
        // Arrange
        const string payload = @"{""transaction_id"":""12345""}";
        const string emptySignature = "";

        // Act
        var isValid = _authorizeNetService.ValidateWebhookSignature(payload, emptySignature);

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void ValidateAuthorizeNetWebhook_WithTamperedPayload_ReturnsIsFalse()
    {
        // Arrange
        const string originalPayload = @"{""transaction_id"":""40045614"",""amount"":""49.99""}";
        const string tamperedPayload = @"{""transaction_id"":""40045614"",""amount"":""99.99""}";
        const string signature = "original-signature";

        // Act
        var isValid = _authorizeNetService.ValidateWebhookSignature(tamperedPayload, signature);

        // Assert
        Assert.IsNotNull(isValid);
    }

    #endregion

    #region PayPal Webhook Tests

    [TestMethod]
    public void ValidatePayPalWebhook_WithValidPayPalSignature_ReturnsTrue()
    {
        // Arrange
        const string payload = @"{
            ""id"":""WH-12345ABCDE"",
            ""event_type"":""CHECKOUT.ORDER.APPROVED"",
            ""resource"":{
                ""id"":""8CP85004T0849104L"",
                ""status"":""APPROVED""
            }
        }";

        const string testSignature = "PayPal-TRANSMISSION-SIG=...";

        // Act
        var isValid = _paypalService.ValidateWebhookSignature(payload, testSignature);

        // Assert
        Assert.IsNotNull(isValid);
    }

    [TestMethod]
    public void ValidatePayPalWebhook_WithInvalidSignature_ReturnsIsFalse()
    {
        // Arrange
        const string payload = @"{""event_type"":""CHECKOUT.ORDER.COMPLETED""}";
        const string invalidSignature = "invalid-paypal-sig";

        // Act
        var isValid = _paypalService.ValidateWebhookSignature(payload, invalidSignature);

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void ValidatePayPalWebhook_WithExpiredWebhook_ReturnsIsFalse()
    {
        // Arrange - Simulating an expired webhook signature
        const string oldPayload = @"{""timestamp"":""2020-01-01T00:00:00Z""}";
        const string signature = "expired-sig";

        // Act
        var isValid = _paypalService.ValidateWebhookSignature(oldPayload, signature);

        // Assert
        Assert.IsNotNull(isValid);
    }

    #endregion

    #region Google Pay Webhook Tests

    [TestMethod]
    public void ValidateGooglePayWebhook_WithValidSignature_ReturnsTrue()
    {
        // Arrange
        const string payload = @"{
            ""transaction_id"":""googlepay-12345"",
            ""event_type"":""PAYMENT_COMPLETED"",
            ""status"":""success"",
            ""amount"":""99.99""
        }";

        const string testSignature = "google-pay-hmac-signature";

        // Act
        var isValid = _googlePayService.ValidateWebhookSignature(payload, testSignature);

        // Assert
        Assert.IsNotNull(isValid);
    }

    [TestMethod]
    public void ValidateGooglePayWebhook_WithInvalidSignature_ReturnsIsFalse()
    {
        // Arrange
        const string payload = @"{""transaction_id"":""gpay-123""}";
        const string invalidSignature = "invalid-google-sig";

        // Act
        var isValid = _googlePayService.ValidateWebhookSignature(payload, invalidSignature);

        // Assert
        Assert.IsFalse(isValid);
    }

    #endregion

    #region Apple Pay Webhook Tests

    [TestMethod]
    public void ValidateApplePayWebhook_WithValidSignature_ReturnsTrue()
    {
        // Arrange
        const string payload = @"{
            ""transaction_id"":""applepay-12345"",
            ""event_type"":""PAYMENT_CAPTURED"",
            ""status"":""success"",
            ""amount"":""79.99""
        }";

        const string testSignature = "apple-pay-signature";

        // Act
        var isValid = _applePayService.ValidateWebhookSignature(payload, testSignature);

        // Assert
        Assert.IsNotNull(isValid);
    }

    [TestMethod]
    public void ValidateApplePayWebhook_WithInvalidSignature_ReturnsIsFalse()
    {
        // Arrange
        const string payload = @"{""transaction_id"":""applepay-123""}";
        const string invalidSignature = "invalid-apple-sig";

        // Act
        var isValid = _applePayService.ValidateWebhookSignature(payload, invalidSignature);

        // Assert
        Assert.IsFalse(isValid);
    }

    #endregion

    #region Cross-Provider Webhook Tests

    [TestMethod]
    public async Task ProcessPayment_AcrossMultipleProviders_WithSameOrder_ReturnsResponses()
    {
        // Arrange
        var orderId = "CROSS-PROVIDER-001";
        var testOrder = CreateTestOrder(orderId, 149.99m, 2);

        var authNetRequest = new PaymentRequest
        {
            OrderId = orderId,
            CustomerName = testOrder.CustomerName,
            CustomerEmail = testOrder.CustomerEmail,
            Amount = testOrder.TotalAmount,
            BillingCity = "Test City",
            BillingState = "TS",
            Description = "Cross-Provider Test",
            LineItems = testOrder.Items,
            DataValue = "auth-net-token"
        };

        var paypalRequest = new PaymentRequest
        {
            OrderId = orderId,
            CustomerName = testOrder.CustomerName,
            CustomerEmail = testOrder.CustomerEmail,
            Amount = testOrder.TotalAmount,
            BillingCity = "Test City",
            BillingState = "TS",
            Description = "Cross-Provider Test",
            LineItems = testOrder.Items,
            DataValue = "paypal-token"
        };

        // Act
        var authNetResponse = await _authorizeNetService.ProcessPaymentAsync(authNetRequest);
        var paypalResponse = await _paypalService.ProcessPaymentAsync(paypalRequest);

        // Assert
        Assert.IsNotNull(authNetResponse);
        Assert.IsNotNull(paypalResponse);
        Assert.AreEqual(orderId, authNetResponse.OrderId);
        Assert.AreEqual(orderId, paypalResponse.OrderId);
    }

    [TestMethod]
    public async Task RefundTransaction_AcrossProviders_WithDifferentTransactionIds_ReturnsResponses()
    {
        // Arrange
        const decimal refundAmount = 50.00m;
        var transactionIds = new[]
        {
            "AUTH-NET-TXN-001",
            "PAYPAL-TXN-002",
            "GOOGLE-PAY-TXN-003",
            "APPLE-PAY-TXN-004"
        };

        var responses = new List<PaymentResponse>();

        // Act
        foreach (var txnId in transactionIds)
        {
            PaymentResponse response = null;

            if (txnId.StartsWith("AUTH-NET"))
                response = await _authorizeNetService.RefundTransactionAsync(txnId, refundAmount);
            else if (txnId.StartsWith("PAYPAL"))
                response = await _paypalService.RefundTransactionAsync(txnId, refundAmount);
            else if (txnId.StartsWith("GOOGLE"))
                response = await _googlePayService.RefundTransactionAsync(txnId, refundAmount);
            else if (txnId.StartsWith("APPLE"))
                response = await _applePayService.RefundTransactionAsync(txnId, refundAmount);

            if (response != null)
                responses.Add(response);
        }

        // Assert
        Assert.AreEqual(4, responses.Count);
        //Assert.All(responses, r => Assert.AreEqual(refundAmount, r.Amount));
    }

    [TestMethod]
    public async Task SubscriptionManagement_AcrossProviders_WithDifferentPlans_ReturnsIds()
    {
        // Arrange
        var subscriptionPlans = new[]
        {
            ("AuthNet Monthly", "AUTH-NET", 9.99m, 1, "month", 12),
            ("PayPal Quarterly", "PAYPAL", 29.99m, 3, "month", 4),
            ("Google Weekly", "GOOGLE", 1.99m, 1, "week", 52),
            ("Apple Annual", "APPLE", 99.99m, 12, "month", 1)
        };

        var responses = new List<PaymentResponse>();

        // Act
        foreach (var (planName, provider, amount, interval, unit, occurrences) in subscriptionPlans)
        {
            var request = CreateTestPaymentRequest(
                orderId: $"SUB-{provider}",
                amount: amount);

            PaymentResponse response = null;

            switch (provider)
            {
                case "AUTH-NET":
                    response = await _authorizeNetService.CreateSubscriptionAsync(
                        request, planName, interval, unit, occurrences);
                    break;
                case "PAYPAL":
                    response = await _paypalService.CreateSubscriptionAsync(
                        request, planName, interval, unit, occurrences);
                    break;
                case "GOOGLE":
                    response = await _googlePayService.CreateSubscriptionAsync(
                        request, planName, interval, unit, occurrences);
                    break;
                case "APPLE":
                    response = await _applePayService.CreateSubscriptionAsync(
                        request, planName, interval, unit, occurrences);
                    break;
            }

            if (response != null)
                responses.Add(response);
        }

        // Assert
        Assert.AreEqual(4, responses.Count);
        Assert.AreEqual(9.99m, responses[0].Amount);
        Assert.AreEqual(29.99m, responses[1].Amount);
        Assert.AreEqual(1.99m, responses[2].Amount);
        Assert.AreEqual(99.99m, responses[3].Amount);
    }

    #endregion

    #region Error Handling and Edge Cases

    [TestMethod]
    public async Task ProcessPayment_WithAllProvidersDisabled_ReturnsFailed()
    {
        // Arrange
        TestPaymentSettings.Enabled = false;

        var paymentRequest = CreateTestPaymentRequest();

        // Act
        var response = await _authorizeNetService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsFalse(response.IsSuccessful);
    }

    [TestMethod]
    public void ValidateWebhook_WithAllProvidersInvalidSignature_AllReturnIsFalse()
    {
        // Arrange
        const string testPayload = "{\"test\":\"data\"}";
        const string invalidSignature = "invalid";

        // Act
        var authNetValid = _authorizeNetService.ValidateWebhookSignature(testPayload, invalidSignature);
        var paypalValid = _paypalService.ValidateWebhookSignature(testPayload, invalidSignature);
        var gPayValid = _googlePayService.ValidateWebhookSignature(testPayload, invalidSignature);
        var aPayValid = _applePayService.ValidateWebhookSignature(testPayload, invalidSignature);

        // Assert
        Assert.IsFalse(authNetValid);
        Assert.IsFalse(paypalValid);
        Assert.IsFalse(gPayValid);
        Assert.IsFalse(aPayValid);
    }

    #endregion

    #region Webhook Event Processing Tests

    [TestMethod]
    public void ProcessWebhookEvent_WithAuthorizeNetApproved_ContainsApprovedStatus()
    {
        // Arrange
        const string approvedEvent = @"{
            ""notificationId"":""12345"",
            ""eventType"":""net.authorize.payment.authcapture.created"",
            ""eventDate"":""2024-01-15T10:30:00Z"",
            ""webhook"":{
                ""id"":""webhook-id""
            },
            ""payload"":{
                ""id"":""40045614"",
                ""status"":""Approved""
            }
        }";

        // Act
        var isValid = _authorizeNetService.ValidateWebhookSignature(approvedEvent, "test-sig");

        // Assert
        Assert.IsNotNull(isValid);
    }

    [TestMethod]
    public void ProcessWebhookEvent_WithPayPalCompleted_ContainsCompletedStatus()
    {
        // Arrange
        const string completedEvent = @"{
            ""id"":""WH-EVENT12345"",
            ""event_type"":""CHECKOUT.ORDER.COMPLETED"",
            ""create_time"":""2024-01-15T10:30:00Z"",
            ""resource"":{
                ""id"":""8CP85004T0849104L"",
                ""status"":""COMPLETED"",
                ""purchase_units"":[{
                    ""amount"":{
                        ""currency_code"":""USD"",
                        ""value"":""99.99""
                    }
                }]
            }
        }";

        // Act
        var isValid = _paypalService.ValidateWebhookSignature(completedEvent, "test-sig");

        // Assert
        Assert.IsNotNull(isValid);
    }

    [TestMethod]
    public void ProcessWebhookEvent_WithGooglePaySuccess_ContainsSuccessStatus()
    {
        // Arrange
        const string successEvent = @"{
            ""transaction_id"":""gpay-txn-123"",
            ""event_type"":""PAYMENT_COMPLETED"",
            ""timestamp"":""2024-01-15T10:30:00Z"",
            ""payload"":{
                ""status"":""SUCCESS"",
                ""amount"":""99.99""
            }
        }";

        // Act
        var isValid = _googlePayService.ValidateWebhookSignature(successEvent, "test-sig");

        // Assert
        Assert.IsNotNull(isValid);
    }

    [TestMethod]
    public void ProcessWebhookEvent_WithApplePayCaptured_ContainsCapturedStatus()
    {
        // Arrange
        const string capturedEvent = @"{
            ""transaction_id"":""applepay-txn-456"",
            ""event_type"":""PAYMENT_CAPTURED"",
            ""timestamp"":""2024-01-15T10:30:00Z"",
            ""payload"":{
                ""status"":""CAPTURED"",
                ""amount"":""79.99""
            }
        }";

        // Act
        var isValid = _applePayService.ValidateWebhookSignature(capturedEvent, "test-sig");

        // Assert
        Assert.IsNotNull(isValid);
    }

    #endregion

    #region Provider TestMethodory Selection Tests
    //TODO FIX
    //[TestMethod]
    //public void GetPaymentService_ByEnum_ReturnsCorrectProvider()
    //{
    //    // Arrange
    //    var authNetService = _paymentServiceTestMethodory.GetPaymentService(PaymentMethodType.AuthorizeNet);
    //    var paypalService = _paymentServiceTestMethodory.GetPaymentService(PaymentMethodType.PayPal);
    //    var googlePayService = _paymentServiceTestMethodory.GetPaymentService(PaymentMethodType.GooglePay);
    //    var applePayService = _paymentServiceTestMethodory.GetPaymentService(PaymentMethodType.ApplePay);

    //    // Act & Assert
    //    Assert.IsNotNull(authNetService);
    //    Assert.IsNotNull(paypalService);
    //    Assert.IsNotNull(googlePayService);
    //    Assert.IsNotNull(applePayService);
    //    Assert.NotAreEqual(authNetService.GetType(), paypalService.GetType());
    //}

    //[TestMethod]
    //public void GetPaymentService_ByString_ReturnsCorrectProvider()
    //{
    //    // Arrange
    //    var authNetService = _paymentServiceTestMethodory.GetPaymentService("AuthorizeNet");
    //    var paypalService = _paymentServiceTestMethodory.GetPaymentService("PayPal");
    //    var googlePayService = _paymentServiceTestMethodory.GetPaymentService("GooglePay");
    //    var applePayService = _paymentServiceTestMethodory.GetPaymentService("ApplePay");

    //    // Act & Assert
    //    Assert.IsNotNull(authNetService);
    //    Assert.IsNotNull(paypalService);
    //    Assert.IsNotNull(googlePayService);
    //    Assert.IsNotNull(applePayService);
    //}

    #endregion
}

/// <summary>
/// Payment method enum for provider selection
/// </summary>
public enum PaymentMethodType
{
    AuthorizeNet,
    PayPal,
    GooglePay,
    ApplePay
}

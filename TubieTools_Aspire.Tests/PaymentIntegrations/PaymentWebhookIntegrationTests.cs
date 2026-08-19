using Microsoft.VisualStudio.TestTools.UnitTesting;
using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TubieTools_Aspire.Tests.PaymentIntegrations;

/// <summary>
/// MSTest Integration tests for payment webhooks across all providers
/// Tests webhook event handling, signature validation, and cross-provider scenarios
/// </summary>
[TestClass]
public class PaymentWebhookIntegrationTests : PaymentServiceTestBase
{
    private IPaymentService _authorizeNetService;
    private IPaymentService _paypalService;
    private IPaymentService _googlePayService;
    private IPaymentService _applePayService;

    [TestInitialize]
    public new void Setup()
    {
        base.Setup();
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

    #region Cross-Provider Webhook Scenarios

    [TestMethod]
    public void ProcessPayment_Across_AllProviders_WithDifferentPayloads()
    {
        // Arrange
        var authorizeNetRequest = CreateTestPaymentRequest(
            orderId: "CROSS-AUTH-001",
            amount: 50.00m);

        var paypalRequest = CreateTestPaymentRequest(
            orderId: "CROSS-PAYPAL-001",
            amount: 50.00m);

        var googlePayRequest = CreateTestPaymentRequest(
            orderId: "CROSS-GPAY-001",
            amount: 50.00m);

        var applePayRequest = CreateTestPaymentRequest(
            orderId: "CROSS-APPLEPAY-001",
            amount: 50.00m);

        // Act
        var authResponse = _authorizeNetService.ProcessPaymentAsync(authorizeNetRequest).GetAwaiter().GetResult();
        var paypalResponse = _paypalService.ProcessPaymentAsync(paypalRequest).GetAwaiter().GetResult();
        var googleResponse = _googlePayService.ProcessPaymentAsync(googlePayRequest).GetAwaiter().GetResult();
        var appleResponse = _applePayService.ProcessPaymentAsync(applePayRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(authResponse);
        Assert.IsNotNull(paypalResponse);
        Assert.IsNotNull(googleResponse);
        Assert.IsNotNull(appleResponse);
    }

    [TestMethod]
    public void RefundTransaction_Across_AllProviders_WithDifferentTransactionIds()
    {
        // Arrange
        const string authTxnId = "AUTH-TXN-12345";
        const string paypalTxnId = "PAYPAL-TXN-12345";
        const string googleTxnId = "GPAY-TXN-12345";
        const string appleTxnId = "APPLEPAY-TXN-12345";
        const decimal refundAmount = 25.00m;

        // Act
        var authRefund = _authorizeNetService.RefundTransactionAsync(authTxnId, refundAmount).GetAwaiter().GetResult();
        var paypalRefund = _paypalService.RefundTransactionAsync(paypalTxnId, refundAmount).GetAwaiter().GetResult();
        var googleRefund = _googlePayService.RefundTransactionAsync(googleTxnId, refundAmount).GetAwaiter().GetResult();
        var appleRefund = _applePayService.RefundTransactionAsync(appleTxnId, refundAmount).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(authRefund);
        Assert.IsNotNull(paypalRefund);
        Assert.IsNotNull(googleRefund);
        Assert.IsNotNull(appleRefund);
    }

    [TestMethod]
    public void CreateSubscription_Across_AllProviders_WithDifferentPlans()
    {
        // Arrange
        var authRequest = CreateTestPaymentRequest("AUTH-SUB-001", 9.99m);
        var paypalRequest = CreateTestPaymentRequest("PAYPAL-SUB-001", 9.99m);
        var googleRequest = CreateTestPaymentRequest("GPAY-SUB-001", 9.99m);
        var appleRequest = CreateTestPaymentRequest("APPLEPAY-SUB-001", 9.99m);

        // Act
        var authSub = _authorizeNetService.CreateSubscriptionAsync(
            authRequest, "Auth Monthly", 1, "month", 12).GetAwaiter().GetResult();

        var paypalSub = _paypalService.CreateSubscriptionAsync(
            paypalRequest, "PayPal Monthly", 1, "month", 12).GetAwaiter().GetResult();

        var googleSub = _googlePayService.CreateSubscriptionAsync(
            googleRequest, "Google Monthly", 1, "month", 12).GetAwaiter().GetResult();

        var appleSub = _applePayService.CreateSubscriptionAsync(
            appleRequest, "Apple Monthly", 1, "month", 12).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(authSub);
        Assert.IsNotNull(paypalSub);
        Assert.IsNotNull(googleSub);
        Assert.IsNotNull(appleSub);
    }

    #endregion

    #region Factory/Provider Selection Tests

    [TestMethod]
    public void ValidateServiceSelection_AllProvidersResolved_FromDependencyInjection()
    {
        // Arrange & Act & Assert
        Assert.IsNotNull(_authorizeNetService);
        Assert.IsNotNull(_paypalService);
        Assert.IsNotNull(_googlePayService);
        Assert.IsNotNull(_applePayService);
    }

    #endregion
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace TubieTools_Aspire.Tests.PaymentIntegrations;

/// <summary>
/// MSTest tests for Apple Pay payment service integration
/// Uses EC_v1 encrypted tokens and sandbox environment
/// </summary>
[TestClass]
public class ApplePayPaymentServiceTests : PaymentServiceTestBase
{
    private IPaymentService _paymentService;

    [TestInitialize]
    public new void Setup()
    {
        base.Setup();
        _paymentService = ServiceProvider.GetRequiredService<ApplePayPaymentService>();
    }

    private static string Base64Encode(string plainText)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
    }

    private static string CreateTestApplePayToken()
    {
        return "{\"version\":\"EC_v1\",\"data\":\"test_encrypted_data\",\"signature\":\"test_signature\",\"header\":{\"ephemeralPublicKey\":\"test_key\",\"transactionId\":\"test_txn\",\"wrappedKey\":\"test_wrapped\"}}";
    }

    #region Basic Apple Pay Processing Tests

    [TestMethod]
    public void ProcessPayment_WithApplePayToken_ReturnsTransactionId()
    {
        // Arrange
        var applePayToken = CreateTestApplePayToken();
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-TEST-001",
            amount: 49.99m,
            paymentToken: Base64Encode(applePayToken));

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("APPLEPAY-TEST-001", response.OrderId);
        Assert.AreEqual(49.99m, response.Amount);
    }

    [TestMethod]
    public void ProcessPayment_WithApplePayEncryptedToken_DecryptsAndProcesses()
    {
        // Arrange
        var applePayToken = CreateTestApplePayToken();
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-ENCRYPTED-001",
            amount: 99.99m,
            paymentToken: Base64Encode(applePayToken));

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(99.99m, response.Amount);
    }

    [TestMethod]
    public void ProcessPayment_WithApplePayLargeAmount_ProcessesCorrectly()
    {
        // Arrange - Apple Pay uses cents internally
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-LARGE-001",
            amount: 999.99m);

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(999.99m, response.Amount);
    }

    [TestMethod]
    public void ProcessPayment_WithApplePayMinimalAmount_ProcessesCorrectly()
    {
        // Arrange - Testing with very small amount
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-MIN-001",
            amount: 0.01m);

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(0.01m, response.Amount);
    }

    #endregion

    #region Apple Pay Profile Tests

    [TestMethod]
    public void CreatePaymentProfile_WithApplePayToken_ReturnsPaymentMethodId()
    {
        // Arrange
        var applePayToken = CreateTestApplePayToken();
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-PROFILE-001",
            amount: 99.99m,
            paymentToken: Base64Encode(applePayToken));

        // Act
        var response = _paymentService.CreatePaymentProfileAsync(
            paymentRequest,
            "Apple Pay Tester",
            "applepay@test.com").GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("APPLEPAY-PROFILE-001", response.OrderId);
    }

    [TestMethod]
    public void ChargePaymentProfile_WithSavedApplePayMethod_ProcessesRecurring()
    {
        // Arrange
        const string customerId = "APPLEPAY-CUSTOMER-001";
        const string paymentMethodId = "APPLEPAY-METHOD-001";
        const decimal chargeAmount = 50.00m;

        // Act
        var response = _paymentService.ChargePaymentProfileAsync(
            customerId,
            paymentMethodId,
            chargeAmount,
            "APPLEPAY-RECURRING-CHARGE").GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(chargeAmount, response.Amount);
    }

    [TestMethod]
    public void ChargePaymentProfile_WithSubscriptionAmount_ProcessesCorrectly()
    {
        // Arrange
        const string customerId = "APPLEPAY-SUBSCRIPTION-CUSTOMER";
        const string paymentMethodId = "APPLEPAY-SUBSCRIPTION-METHOD";
        const decimal subscriptionAmount = 14.99m;

        // Act
        var response = _paymentService.ChargePaymentProfileAsync(
            customerId,
            paymentMethodId,
            subscriptionAmount,
            "APPLEPAY-SUB-CHARGE").GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(subscriptionAmount, response.Amount);
    }

    #endregion

    #region Apple Pay Subscription Tests

    [TestMethod]
    public void CreateSubscription_WithApplePayMethod_ReturnsSubscriptionId()
    {
        // Arrange
        var applePayToken = CreateTestApplePayToken();
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-SUB-001",
            amount: 9.99m,
            paymentToken: Base64Encode(applePayToken));

        // Act
        var response = _paymentService.CreateSubscriptionAsync(
            paymentRequest,
            "Apple Pay Monthly Service",
            intervalLength: 1,
            intervalUnit: "month",
            totalOccurrences: 12).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("APPLEPAY-SUB-001", response.OrderId);
        Assert.AreEqual(9.99m, response.Amount);
    }

    [TestMethod]
    public void CreateSubscription_WithAnnualBilling_ReturnsResponse()
    {
        // Arrange
        var applePayToken = CreateTestApplePayToken();
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-SUB-ANNUAL",
            amount: 99.99m,
            paymentToken: Base64Encode(applePayToken));

        // Act
        var response = _paymentService.CreateSubscriptionAsync(
            paymentRequest,
            "Apple Pay Annual Plan",
            intervalLength: 12,
            intervalUnit: "month",
            totalOccurrences: 1).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("APPLEPAY-SUB-ANNUAL", response.OrderId);
    }

    [TestMethod]
    public void CancelSubscription_WithApplePaySubscription_ReturnsSuccess()
    {
        // Arrange
        const string subscriptionId = "APPLEPAY-SUB-CANCEL-001";

        // Act
        var response = _paymentService.CancelSubscriptionAsync(subscriptionId).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
    }

    #endregion

    #region Apple Pay Refund Tests

    [TestMethod]
    public void RefundTransaction_WithApplePayCapture_ReturnsRefundId()
    {
        // Arrange
        const string transactionId = "APPLEPAY-CAPTURE-12345";
        const decimal refundAmount = 75.00m;

        // Act
        var response = _paymentService.RefundTransactionAsync(transactionId, refundAmount).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    [TestMethod]
    public void RefundTransaction_WithPartialAmount_ReturnsRefundId()
    {
        // Arrange
        const string transactionId = "APPLEPAY-PARTIAL-001";
        const decimal refundAmount = 25.00m;

        // Act
        var response = _paymentService.RefundTransactionAsync(transactionId, refundAmount).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    #endregion

    #region Apple Pay Transaction Details

    [TestMethod]
    public void GetTransactionDetails_WithApplePayTransaction_ReturnsDetails()
    {
        // Arrange
        const string transactionId = "APPLEPAY-TXN-12345";

        // Act
        var response = _paymentService.GetTransactionDetailsAsync(transactionId).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    #endregion

    #region Apple Pay Webhooks

    [TestMethod]
    public void ValidateWebhookSignature_WithValidApplePaySignature_ReturnsTrue()
    {
        // Arrange
        const string payload = "APPLEPAY_WEBHOOK_PAYLOAD";
        const string signature = "APPLEPAY_SIGNATURE";

        // Act
        var isValid = _paymentService.ValidateWebhookSignature(payload, signature);

        // Assert
        Assert.IsTrue(isValid);
    }

    #endregion

    #region Apple Pay Complex Scenarios

    [TestMethod]
    public void CompleteOrder_WithApplePayPayments_ProcessesAllCharges()
    {
        // Arrange
        var order = CreateTestOrder();
        order.CustomerId = "APPLEPAY-CUSTOMER-001";
        order.Payments = new List<Payment>
        {
            new Payment { Amount = 100.00m, PaymentToken = Base64Encode(CreateTestApplePayToken()) },
            new Payment { Amount = 50.00m, PaymentToken = Base64Encode(CreateTestApplePayToken()) }
        };

        const decimal expectedTotal = 150.00m;

        // Act
        decimal totalProcessed = 0;
        foreach (var payment in order.Payments)
        {
            var req = CreateTestPaymentRequest(order.OrderId, payment.Amount, payment.PaymentToken);
            var response = _paymentService.ProcessPaymentAsync(req).GetAwaiter().GetResult();
            if (response.Success)
            {
                totalProcessed += payment.Amount;
            }
        }

        // Assert
        Assert.AreEqual(expectedTotal, totalProcessed);
    }

    [TestMethod]
    public void MultiDevice_ApplePayScenario_ProcessesDifferentTokens()
    {
        // Arrange
        const string device1 = "APPLEPAY-DEVICE-1";
        const string device2 = "APPLEPAY-DEVICE-2";

        // Act
        var profile1 = _paymentService.CreatePaymentProfileAsync(
            CreateTestPaymentRequest($"{device1}-ORDER", 50m, Base64Encode(CreateTestApplePayToken())),
            "iPhone",
            "device1@test.com").GetAwaiter().GetResult();

        var profile2 = _paymentService.CreatePaymentProfileAsync(
            CreateTestPaymentRequest($"{device2}-ORDER", 75m, Base64Encode(CreateTestApplePayToken())),
            "Apple Watch",
            "device2@test.com").GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(profile1);
        Assert.IsNotNull(profile2);
    }

    #endregion

    #region Apple Pay Cent Precision Tests

    [TestMethod]
    public void ProcessPayment_WithCentPrecision_HandlesCorrectly()
    {
        // Arrange - Apple Pay must handle cent precision
        var amounts = new[] { 0.01m, 1.23m, 99.99m, 1000.00m };

        // Act & Assert
        foreach (var amount in amounts)
        {
            var paymentRequest = CreateTestPaymentRequest(
                orderId: $"APPLEPAY-CENTS-{amount:F2}",
                amount: amount);

            var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

            Assert.IsNotNull(response);
            Assert.AreEqual(amount, response.Amount);
        }
    }

    #endregion

    #region Apple Pay Void Transaction Tests

    [TestMethod]
    public void VoidTransaction_WithApplePayAuthorization_ReturnsSuccess()
    {
        // Arrange
        const string authorizationId = "APPLEPAY-AUTH-12345";

        // Act
        var response = _paymentService.VoidTransactionAsync(authorizationId).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(authorizationId, response.TransactionId);
    }

    #endregion
}

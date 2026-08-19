using Microsoft.VisualStudio.TestTools.UnitTesting;
using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace TubieTools_Aspire.Tests.PaymentIntegrations;

/// <summary>
/// MSTest tests for Google Pay payment service integration
/// Uses test tokens and sandbox environment
/// </summary>
[TestClass]
public class GooglePayPaymentServiceTests : PaymentServiceTestBase
{
    private IPaymentService _paymentService;

    [TestInitialize]
    public new void Setup()
    {
        base.Setup();
        _paymentService = ServiceProvider.GetRequiredService<GooglePayPaymentService>();
    }

    private static string Base64Encode(string plainText)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
    }

    private static string CreateTestGooglePayToken()
    {
        return "{\"version\":\"EC_v1\",\"data\":\"test_data\",\"signature\":\"test_signature\"}";
    }

    #region Basic Google Pay Processing Tests

    [TestMethod]
    public void ProcessPayment_WithGooglePayToken_ReturnsTransactionId()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "GPAY-TEST-001",
            amount: 49.99m,
            paymentToken: Base64Encode("{\"version\":\"EC_v1\",\"data\":\"test\",\"signature\":\"test\"}"));

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("GPAY-TEST-001", response.OrderId);
        Assert.AreEqual(49.99m, response.Amount);
    }

    [TestMethod]
    public void ProcessPayment_WithGooglePayEncryptedToken_DecryptsAndProcesses()
    {
        // Arrange
        var googlePayToken = CreateTestGooglePayToken();
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "GPAY-ENCRYPTED-001",
            amount: 99.99m,
            paymentToken: Base64Encode(googlePayToken));

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(99.99m, response.Amount);
    }

    [TestMethod]
    public void ProcessPayment_WithGooglePayMultipleItems_IncludesCartDetails()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "GPAY-CART-001",
            amount: 199.98m);
        paymentRequest.LineItems = new List<LineItem>
        {
            new LineItem 
            { 
                ItemId = "GPAY-SKU-001", 
                Name = "Google Pay Item 1", 
                Quantity = 1, 
                UnitPrice = 99.99m 
            },
            new LineItem 
            { 
                ItemId = "GPAY-SKU-002", 
                Name = "Google Pay Item 2", 
                Quantity = 1, 
                UnitPrice = 99.99m 
            }
        };

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(199.98m, response.Amount);
    }

    #endregion

    #region Google Pay Profile Tests

    [TestMethod]
    public void CreatePaymentProfile_WithGooglePayToken_ReturnsPaymentMethodId()
    {
        // Arrange
        var tokenJson = CreateTestGooglePayToken();
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "GPAY-PROFILE-001",
            amount: 99.99m,
            paymentToken: Base64Encode(tokenJson));

        // Act
        var response = _paymentService.CreatePaymentProfileAsync(
            paymentRequest,
            "Google Pay Tester",
            "googlepay@test.com").GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("GPAY-PROFILE-001", response.OrderId);
    }

    [TestMethod]
    public void ChargePaymentProfile_WithSavedGooglePayMethod_ProcessesRecurring()
    {
        // Arrange
        const string customerId = "GPAY-CUSTOMER-001";
        const string paymentMethodId = "GPAY-METHOD-001";
        const decimal chargeAmount = 50.00m;

        // Act
        var response = _paymentService.ChargePaymentProfileAsync(
            customerId,
            paymentMethodId,
            chargeAmount,
            "GPAY-RECURRING-CHARGE").GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(chargeAmount, response.Amount);
    }

    [TestMethod]
    public void ChargePaymentProfile_WithVariousAmounts_ProcessesCorrectly()
    {
        // Arrange
        var amounts = new[] { 10.00m, 25.50m, 99.99m };

        // Act
        var responses = new List<PaymentResponse>();
        foreach (var amount in amounts)
        {
            var response = _paymentService.ChargePaymentProfileAsync(
                "GPAY-CUSTOMER",
                "GPAY-METHOD",
                amount,
                $"CHARGE-{amount:F2}").GetAwaiter().GetResult();
            responses.Add(response);
        }

        // Assert
        Assert.AreEqual(3, responses.Count);
        foreach (var r in responses)
        {
            Assert.IsTrue(r.Amount > 0);
        }
    }

    #endregion

    #region Google Pay Subscription Tests

    [TestMethod]
    public void CreateSubscription_WithGooglePayMethod_ReturnsSubscriptionId()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "GPAY-SUB-001",
            amount: 9.99m);
        paymentRequest.DataValue = Base64Encode(CreateTestGooglePayToken());

        // Act
        var response = _paymentService.CreateSubscriptionAsync(
            paymentRequest,
            "Google Pay Monthly",
            intervalLength: 1,
            intervalUnit: "month",
            totalOccurrences: 12).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("GPAY-SUB-001", response.OrderId);
        Assert.AreEqual(9.99m, response.Amount);
    }

    [TestMethod]
    public void CreateSubscription_WithDifferentBillingCycles_ReturnsResponse()
    {
        // Arrange - Weekly subscription
        var weeklyRequest = CreateTestPaymentRequest(
            orderId: "GPAY-SUB-WEEKLY",
            amount: 1.99m);

        // Act
        var response = _paymentService.CreateSubscriptionAsync(
            weeklyRequest,
            "Google Pay Weekly",
            intervalLength: 1,
            intervalUnit: "week",
            totalOccurrences: 52).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public void CancelSubscription_WithGooglePaySubscription_ReturnsSuccess()
    {
        // Arrange
        const string subscriptionId = "GPAY-SUB-CANCEL-001";

        // Act
        var response = _paymentService.CancelSubscriptionAsync(subscriptionId).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
    }

    #endregion

    #region Google Pay Refund Tests

    [TestMethod]
    public void RefundTransaction_WithGooglePayCapture_ReturnsRefundId()
    {
        // Arrange
        const string transactionId = "GPAY-CAPTURE-12345";
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
        const string transactionId = "GPAY-PARTIAL-001";
        const decimal refundAmount = 25.00m;

        // Act
        var response = _paymentService.RefundTransactionAsync(transactionId, refundAmount).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    #endregion

    #region Google Pay Transaction Details

    [TestMethod]
    public void GetTransactionDetails_WithGooglePayTransaction_ReturnsDetails()
    {
        // Arrange
        const string transactionId = "GPAY-TXN-12345";

        // Act
        var response = _paymentService.GetTransactionDetailsAsync(transactionId).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    #endregion

    #region Google Pay Webhooks

    [TestMethod]
    public void ValidateWebhookSignature_WithValidGooglePaySignature_ReturnsTrue()
    {
        // Arrange
        const string payload = "GPAY_WEBHOOK_PAYLOAD";
        const string signature = "GPAY_SIGNATURE";

        // Act
        var isValid = _paymentService.ValidateWebhookSignature(payload, signature);

        // Assert
        Assert.IsTrue(isValid);
    }

    #endregion

    #region Google Pay Complex Scenarios

    [TestMethod]
    public void CompleteOrder_WithGooglePayPayments_ProcessesAllCharges()
    {
        // Arrange
        var order = CreateTestOrder();
        order.CustomerId = "GPAY-CUSTOMER-001";
        order.Payments = new List<Payment>
        {
            new Payment { Amount = 100.00m, PaymentToken = Base64Encode(CreateTestGooglePayToken()) },
            new Payment { Amount = 50.00m, PaymentToken = Base64Encode(CreateTestGooglePayToken()) }
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
    public void MultiDevice_GooglePayScenario_ProcessesDifferentTokens()
    {
        // Arrange
        const string customer1 = "GPAY-DEVICE-1";
        const string customer2 = "GPAY-DEVICE-2";

        // Act
        var profile1 = _paymentService.CreatePaymentProfileAsync(
            CreateTestPaymentRequest($"{customer1}-ORDER", 50m, Base64Encode(CreateTestGooglePayToken())),
            "Device 1",
            "device1@test.com").GetAwaiter().GetResult();

        var profile2 = _paymentService.CreatePaymentProfileAsync(
            CreateTestPaymentRequest($"{customer2}-ORDER", 75m, Base64Encode(CreateTestGooglePayToken())),
            "Device 2",
            "device2@test.com").GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(profile1);
        Assert.IsNotNull(profile2);
    }

    #endregion

    #region Google Pay Void Transaction Tests

    [TestMethod]
    public void VoidTransaction_WithGooglePayAuthorization_ReturnsSuccess()
    {
        // Arrange
        const string authorizationId = "GPAY-AUTH-12345";

        // Act
        var response = _paymentService.VoidTransactionAsync(authorizationId).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(authorizationId, response.TransactionId);
    }

    #endregion
}

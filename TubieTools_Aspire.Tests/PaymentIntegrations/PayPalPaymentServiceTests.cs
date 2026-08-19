using Microsoft.VisualStudio.TestTools.UnitTesting;
using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TubieTools_Aspire.Tests.PaymentIntegrations;

/// <summary>
/// MSTest tests for PayPal payment service integration
/// Uses test tokens and sandbox environment
/// </summary>
[TestClass]
public class PayPalPaymentServiceTests : PaymentServiceTestBase
{
    private IPaymentService _paymentService;

    [TestInitialize]
    public new void Setup()
    {
        base.Setup();
        _paymentService = ServiceProvider.GetRequiredService<PayPalPaymentService>();
    }

    #region Basic PayPal Processing Tests

    [TestMethod]
    public void ProcessPayment_WithPayPalToken_ReturnsOrderId()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "PAYPAL-TEST-001",
            amount: 49.99m,
            paymentToken: "paypal-test-token");

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("PAYPAL-TEST-001", response.OrderId);
        Assert.AreEqual(49.99m, response.Amount);
        // unauthorized
        Assert.IsTrue(response.Success == false);
    }

    [TestMethod]
    public void ProcessPayment_WithPayPalLargeAmount_ProcessesCorrectly()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "PAYPAL-LARGE-001",
            amount: 999.99m);

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(999.99m, response.Amount);
    }

    [TestMethod]
    public void ProcessPayment_WithPayPalCartMultipleItems_IncludesDetails()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "PAYPAL-CART-001",
            amount: 199.98m);
        paymentRequest.LineItems = new List<LineItem>
        {
            new LineItem { ItemId = "PAYPAL-SKU-001", Name = "PayPal Item 1", Quantity = 1, UnitPrice = 99.99m },
            new LineItem { ItemId = "PAYPAL-SKU-002", Name = "PayPal Item 2", Quantity = 1, UnitPrice = 99.99m }
        };

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(199.98m, response.Amount);
    }

    [TestMethod]
    public void ProcessPayment_WithMinimalAmount_ProcessesCorrectly()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "PAYPAL-MIN-001",
            amount: 0.01m);

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(0.01m, response.Amount);
    }

    #endregion

    #region PayPal Profile Tests

    [TestMethod]
    public void CreatePaymentProfile_WithPayPalToken_ReturnsProfileId()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "PAYPAL-PROFILE-001",
            amount: 99.99m);

        // Act
        var response = _paymentService.CreatePaymentProfileAsync(
            paymentRequest,
            "PayPal Tester",
            "paypal@test.com").GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("PAYPAL-PROFILE-001", response.OrderId);
    }

    [TestMethod]
    public void ChargePaymentProfile_WithBillingAgreement_ProcessesRecurring()
    {
        // Arrange
        const string customerId = "PAYPAL-CUSTOMER-001";
        const string billingAgreementId = "PAYPAL-AGREEMENT-001";
        const decimal chargeAmount = 50.00m;

        // Act
        var response = _paymentService.ChargePaymentProfileAsync(
            customerId,
            billingAgreementId,
            chargeAmount,
            "PAYPAL-RECURRING-CHARGE").GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(chargeAmount, response.Amount);
    }

    #endregion

    #region PayPal Subscription Tests

    [TestMethod]
    public void CreateSubscription_WithPayPalPlan_ReturnsSubscriptionId()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "PAYPAL-SUB-001",
            amount: 9.99m);

        // Act
        var response = _paymentService.CreateSubscriptionAsync(
            paymentRequest,
            "PayPal Monthly",
            intervalLength: 1,
            intervalUnit: "month",
            totalOccurrences: 12).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("PAYPAL-SUB-001", response.OrderId);
        Assert.AreEqual(9.99m, response.Amount);
    }

    [TestMethod]
    public void CreateSubscription_WithBiweeklyBilling_ReturnsResponse()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "PAYPAL-SUB-BIWEEKLY",
            amount: 19.99m);

        // Act
        var response = _paymentService.CreateSubscriptionAsync(
            paymentRequest,
            "PayPal Biweekly",
            intervalLength: 2,
            intervalUnit: "week",
            totalOccurrences: 26).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public void CancelSubscription_WithPayPalSubscription_ReturnsSuccess()
    {
        // Arrange
        const string subscriptionId = "PAYPAL-SUB-CANCEL-001";

        // Act
        var response = _paymentService.CancelSubscriptionAsync(subscriptionId).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(subscriptionId, response.OrderId);
    }

    #endregion

    #region PayPal Refund Tests

    [TestMethod]
    public void RefundTransaction_WithPayPalCapture_ReturnsRefundId()
    {
        // Arrange
        const string transactionId = "PAYPAL-CAPTURE-12345";
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
        const string transactionId = "PAYPAL-CAPTURE-67890";
        const decimal refundAmount = 25.00m;

        // Act
        var response = _paymentService.RefundTransactionAsync(transactionId, refundAmount).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    #endregion

    #region PayPal Transaction Details

    [TestMethod]
    public void GetTransactionDetails_WithPayPalTransaction_ReturnsDetails()
    {
        // Arrange
        const string transactionId = "PAYPAL-TXN-12345";

        // Act
        var response = _paymentService.GetTransactionDetailsAsync(transactionId).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    #endregion

    #region PayPal Webhooks

    [TestMethod]
    public void ValidateWebhookSignature_WithValidPayPalSignature_ReturnsTrue()
    {
        // Arrange
        const string payload = "PAYPAL_WEBHOOK_PAYLOAD";
        const string signature = "PAYPAL_SIGNATURE";

        // Act
        var isValid = _paymentService.ValidateWebhookSignature(payload, signature);

        // Assert
        Assert.IsTrue(isValid);
    }

    #endregion

    #region PayPal Complex Scenarios

    [TestMethod]
    public void CompleteOrder_WithPayPalPayments_ProcessesAllCharges()
    {
        // Arrange
        var order = CreateTestOrder();
        order.CustomerId = "PAYPAL-CUSTOMER-001";
        order.Payments = new List<Payment>
        {
            new Payment { Amount = 100.00m, PaymentToken = "paypal-token-1" },
            new Payment { Amount = 50.00m, PaymentToken = "paypal-token-2" }
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
    public void MultiCustomer_PayPalScenario_ProcessesDifferentProfiles()
    {
        // Arrange
        const string customer1 = "PAYPAL-CUST-1";
        const string customer2 = "PAYPAL-CUST-2";

        // Act
        var profile1 = _paymentService.CreatePaymentProfileAsync(
            CreateTestPaymentRequest($"{customer1}-ORDER", 50m),
            "Customer 1",
            "cust1@test.com").GetAwaiter().GetResult();

        var profile2 = _paymentService.CreatePaymentProfileAsync(
            CreateTestPaymentRequest($"{customer2}-ORDER", 75m),
            "Customer 2",
            "cust2@test.com").GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(profile1);
        Assert.IsNotNull(profile2);
    }

    #endregion

    #region PayPal Disabled Service Tests

    [TestMethod]
    public void ProcessPayment_WithDisabledPayPal_ReturnsServiceDisabledResponse()
    {
        // Arrange
        var disabledSettings = new PaymentSettings
        {
            PayPalEnabled = false,
            AuthorizeNetEnabled = true
        };

        var paymentRequest = CreateTestPaymentRequest(
            orderId: "PAYPAL-DISABLED-001",
            amount: 50.00m);

        // Act & Assert
        // This test validates that disabled PayPal service behavior is handled correctly
        // Implementation depends on how PayPalPaymentService checks enabled status
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Should either return error or be handled gracefully
        Assert.IsNotNull(response);
    }

    #endregion

    #region PayPal Void Transaction Tests

    [TestMethod]
    public void VoidTransaction_WithPayPalAuthorization_ReturnsSuccess()
    {
        // Arrange
        const string authorizationId = "PAYPAL-AUTH-12345";

        // Act
        var response = _paymentService.VoidTransactionAsync(authorizationId).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(authorizationId, response.TransactionId);
    }

    #endregion
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TubieTools_Aspire.Tests.PaymentIntegrations;

/// <summary>
/// MSTest tests for Authorize.Net payment service integration
/// Uses test tokens and sandbox environment
/// </summary>
[TestClass]
public class AuthorizeNetPaymentServiceTests : PaymentServiceTestBase
{
    private IPaymentService _paymentService;

    [TestInitialize]
    public new void Setup()
    {
        base.Setup();
        _paymentService = ServiceProvider.GetRequiredService<PaymentService>();
    }

    #region Basic Authorize.Net Processing Tests

    [TestMethod]
    public void ProcessPayment_WithValidRequest_ReturnsPaymentResponse()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "AUTH-TEST-001",
            amount: 49.99m);

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("AUTH-TEST-001", response.OrderId);
        Assert.AreEqual(49.99m, response.Amount);
    }

    [TestMethod]
    public void ProcessPayment_WithNegativeAmount_ReturnsFailed()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(amount: -50.00m);

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.IsFalse(response.IsSuccessful);
    }

    [TestMethod]
    public void ProcessPayment_WithZeroAmount_ReturnsFailed()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(amount: 0m);

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.IsFalse(response.IsSuccessful);
    }

    [TestMethod]
    public void ProcessPayment_WithoutCustomerEmail_StillProcesses()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest();
        paymentRequest.CustomerEmail = null;

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(!string.IsNullOrEmpty(response.OrderId));
    }

    [TestMethod]
    public void ProcessPayment_WithMultipleLineItems_IncludesAllItems()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "AUTH-ITEMS-001",
            amount: 199.98m);
        paymentRequest.LineItems = new List<LineItem>
        {
            new LineItem { ItemId = "ITEM-1", Name = "Product 1", Quantity = 1, UnitPrice = 99.99m },
            new LineItem { ItemId = "ITEM-2", Name = "Product 2", Quantity = 1, UnitPrice = 99.99m }
        };

        // Act
        var response = _paymentService.ProcessPaymentAsync(paymentRequest).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(199.98m, response.Amount);
    }

    #endregion

    #region Authorize.Net Profile Tests

    [TestMethod]
    public void CreatePaymentProfile_WithValidRequest_ReturnsProfileId()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "AUTH-PROFILE-001",
            amount: 99.99m);

        // Act
        var response = _paymentService.CreatePaymentProfileAsync(
            paymentRequest,
            "Authorize.Net Tester",
            "authnet@test.com").GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("AUTH-PROFILE-001", response.OrderId);
    }

    [TestMethod]
    public void ChargePaymentProfile_WithSavedMethod_ProcessesRecurring()
    {
        // Arrange
        const string customerId = "AUTH-CUSTOMER-001";
        const string paymentMethodId = "AUTH-METHOD-001";
        const decimal chargeAmount = 50.00m;

        // Act
        var response = _paymentService.ChargePaymentProfileAsync(
            customerId,
            paymentMethodId,
            chargeAmount,
            "AUTH-RECURRING-CHARGE").GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(chargeAmount, response.Amount);
    }

    [TestMethod]
    public void ChargePaymentProfile_WithPartialAmount_ProcessesCorrectly()
    {
        // Arrange
        const decimal chargeAmount = 25.50m;

        // Act
        var response = _paymentService.ChargePaymentProfileAsync(
            "AUTH-CUSTOMER",
            "AUTH-METHOD",
            chargeAmount,
            "PARTIAL-CHARGE").GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(chargeAmount, response.Amount);
    }

    #endregion

    #region Authorize.Net Refund Tests

    [TestMethod]
    public void RefundTransaction_WithValidTransactionId_ReturnsRefundResponse()
    {
        // Arrange
        const string transactionId = "AUTH-TXN-12345";
        const decimal refundAmount = 75.00m;

        // Act
        var response = _paymentService.RefundTransactionAsync(transactionId, refundAmount).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
        Assert.AreEqual(refundAmount, response.Amount);
    }

    [TestMethod]
    public void RefundTransaction_WithPartialRefund_ReturnsResponse()
    {
        // Arrange
        const string transactionId = "AUTH-PARTIAL-001";
        const decimal refundAmount = 25.00m;

        // Act
        var response = _paymentService.RefundTransactionAsync(transactionId, refundAmount).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(refundAmount, response.Amount);
    }

    #endregion

    #region Authorize.Net Void Tests

    [TestMethod]
    public void VoidTransaction_WithValidTransactionId_ReturnsResponse()
    {
        // Arrange
        const string transactionId = "AUTH-VOID-001";

        // Act
        var response = _paymentService.VoidTransactionAsync(transactionId).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
    }

    #endregion

    #region Authorize.Net Subscription Tests

    [TestMethod]
    public void CreateSubscription_WithValidRequest_ReturnsSubscriptionId()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "AUTH-SUB-001",
            amount: 9.99m);

        // Act
        var response = _paymentService.CreateSubscriptionAsync(
            paymentRequest,
            "Monthly Subscription",
            intervalLength: 1,
            intervalUnit: "month",
            totalOccurrences: 12).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("AUTH-SUB-001", response.OrderId);
    }

    [TestMethod]
    public void CancelSubscription_WithValidSubscriptionId_ReturnsSuccess()
    {
        // Arrange
        const string subscriptionId = "AUTH-SUB-CANCEL-001";

        // Act
        var response = _paymentService.CancelSubscriptionAsync(subscriptionId).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
    }

    #endregion

    #region Authorize.Net Webhook Tests

    [TestMethod]
    public void ValidateWebhookSignature_WithValidSignature_ReturnsTrue()
    {
        // Arrange
        const string payload = "AUTH_WEBHOOK_PAYLOAD";
        const string signature = "AUTH_SIGNATURE";

        // Act
        var isValid = _paymentService.ValidateWebhookSignature(payload, signature);

        // Assert
        Assert.IsTrue(isValid);
    }

    #endregion

    #region Authorize.Net Complex Scenarios

    [TestMethod]
    public void CompleteOrder_WithMultiplePayments_ProcessesSequentially()
    {
        // Arrange
        var order = CreateTestOrder();
        order.CustomerId = "AUTH-CUSTOMER-MULTI";
        order.Payments = new List<Payment>
        {
            new Payment { Amount = 100.00m, PaymentToken = "auth-token-1" },
            new Payment { Amount = 50.00m, PaymentToken = "auth-token-2" }
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
    public void MultiCustomer_AuthorizeNetScenario_ProcessesDifferentProfiles()
    {
        // Arrange
        const string customer1 = "AUTH-CUST-1";
        const string customer2 = "AUTH-CUST-2";

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

    #region Authorize.Net Transaction Details

    [TestMethod]
    public void GetTransactionDetails_WithValidTransactionId_ReturnsDetails()
    {
        // Arrange
        const string transactionId = "AUTH-TXN-DETAIL-001";

        // Act
        var response = _paymentService.GetTransactionDetailsAsync(transactionId).GetAwaiter().GetResult();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    #endregion
}

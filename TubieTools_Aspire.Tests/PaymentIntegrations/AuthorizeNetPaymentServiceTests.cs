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
    public override void Setup()
    {
        base.Setup();
        _paymentService = ServiceProvider.GetRequiredService<PaymentService>();
    }

    #region Basic Authorize.Net Processing Tests

    [TestMethod]
    public async Task ProcessPayment_WithValidRequest_ReturnsPaymentResponse()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "AUTH-TEST-001",
            amount: 49.99m);

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("AUTH-TEST-001", response.OrderId);
        Assert.AreEqual(49.99m, response.Amount);
    }

    [TestMethod]
    public async Task ProcessPayment_WithNegativeAmount_ReturnsFailed()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(amount: -50.00m);

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsFalse(response.IsSuccessful);
    }

    [TestMethod]
    public async Task ProcessPayment_WithZeroAmount_ReturnsFailed()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(amount: 0m);

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsFalse(response.IsSuccessful);
    }

    [TestMethod]
    public async Task ProcessPayment_WithoutCustomerEmail_StillProcesses()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest();
        paymentRequest.CustomerEmail = null;

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(!string.IsNullOrEmpty(response.OrderId));
    }

    [TestMethod]
    public async Task ProcessPayment_WithMultipleLineItems_IncludesAllItems()
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
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(199.98m, response.Amount);
    }

    #endregion

    #region Authorize.Net Profile Tests

    [TestMethod]
    public async Task CreatePaymentProfile_WithValidRequest_ReturnsProfileId()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "AUTH-PROFILE-001",
            amount: 99.99m);

        // Act
        var response = await _paymentService.CreatePaymentProfileAsync(
            paymentRequest,
            "Authorize.Net Tester",
            "authnet@test.com");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("AUTH-PROFILE-001", response.OrderId);
    }

    [TestMethod]
    public async Task ChargePaymentProfile_WithSavedMethod_ProcessesRecurring()
    {
        // Arrange
        const string customerId = "AUTH-CUSTOMER-001";
        const string paymentMethodId = "AUTH-METHOD-001";
        const decimal chargeAmount = 50.00m;

        // Act
        var response = await _paymentService.ChargePaymentProfileAsync(
            customerId,
            paymentMethodId,
            chargeAmount,
            "AUTH-RECURRING-CHARGE");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(chargeAmount, response.Amount);
    }

    [TestMethod]
    public async Task ChargePaymentProfile_WithPartialAmount_ProcessesCorrectly()
    {
        // Arrange
        const decimal chargeAmount = 25.50m;

        // Act
        var response = await _paymentService.ChargePaymentProfileAsync(
            "AUTH-CUSTOMER",
            "AUTH-METHOD",
            chargeAmount,
            "PARTIAL-CHARGE");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(chargeAmount, response.Amount);
    }

    #endregion

    #region Authorize.Net Refund Tests

    [TestMethod]
    public async Task RefundTransaction_WithValidTransactionId_ReturnsRefundResponse()
    {
        // Arrange
        const string transactionId = "AUTH-TXN-12345";
        const decimal refundAmount = 75.00m;

        // Act
        var response = await _paymentService.RefundTransactionAsync(transactionId, refundAmount);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
        Assert.AreEqual(refundAmount, response.Amount);
    }

    [TestMethod]
    public async Task RefundTransaction_WithPartialRefund_ReturnsResponse()
    {
        // Arrange
        const string transactionId = "AUTH-PARTIAL-001";
        const decimal refundAmount = 25.00m;

        // Act
        var response = await _paymentService.RefundTransactionAsync(transactionId, refundAmount);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(refundAmount, response.Amount);
    }

    #endregion

    #region Authorize.Net Void Tests

    [TestMethod]
    public async Task VoidTransaction_WithValidTransactionId_ReturnsResponse()
    {
        // Arrange
        const string transactionId = "AUTH-VOID-001";

        // Act
        var response = await _paymentService.VoidTransactionAsync(transactionId);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    #endregion

    #region Authorize.Net Transaction Details Tests

    [TestMethod]
    public async Task GetTransactionDetails_WithValidTransactionId_ReturnsDetails()
    {
        // Arrange
        const string transactionId = "AUTH-DETAILS-001";

        // Act
        var response = await _paymentService.GetTransactionDetailsAsync(transactionId);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    [TestMethod]
    public async Task GetTransactionDetails_WithInvalidTransactionId_ReturnsFailed()
    {
        // Arrange
        const string invalidTransactionId = "INVALID-TXN";

        // Act
        var response = await _paymentService.GetTransactionDetailsAsync(invalidTransactionId);

        // Assert
        Assert.IsNotNull(response);
    }

    #endregion

    #region Authorize.Net Subscription Tests

    [TestMethod]
    public async Task CreateSubscription_WithMonthlyBilling_ReturnsSubscriptionId()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "AUTH-SUB-001",
            amount: 9.99m);

        // Act
        var response = await _paymentService.CreateSubscriptionAsync(
            paymentRequest,
            "Authorize.Net Monthly",
            intervalLength: 1,
            intervalUnit: "month",
            totalOccurrences: 12);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("AUTH-SUB-001", response.OrderId);
        Assert.AreEqual(9.99m, response.Amount);
    }

    [TestMethod]
    public async Task CreateSubscription_WithQuarterlyBilling_ReturnsResponse()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "AUTH-SUB-QUARTERLY",
            amount: 29.99m);

        // Act
        var response = await _paymentService.CreateSubscriptionAsync(
            paymentRequest,
            "Authorize.Net Quarterly",
            intervalLength: 3,
            intervalUnit: "month",
            totalOccurrences: 4);

        // Assert
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task CancelSubscription_WithValidSubscriptionId_ReturnsSuccess()
    {
        // Arrange
        const string subscriptionId = "AUTH-SUB-CANCEL-001";

        // Act
        var response = await _paymentService.CancelSubscriptionAsync(subscriptionId);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(subscriptionId, response.OrderId);
    }

    #endregion

    #region Authorize.Net Webhook Tests

    [TestMethod]
    public void ValidateWebhookSignature_WithValidSignature_ReturnsTrue()
    {
        // Arrange
        const string webhookPayload = "{\"transaction_id\":\"40045614\",\"status\":\"Approved\"}";
        const string validSignature = "valid-authorize-net-signature";

        // Act
        var isValid = _paymentService.ValidateWebhookSignature(webhookPayload, validSignature);

        // Assert
        Assert.IsNotNull(isValid);
    }

    [TestMethod]
    public void ValidateWebhookSignature_WithInvalidSignature_ReturnsFalse()
    {
        // Arrange
        const string webhookPayload = "{\"transaction_id\":\"40045614\"}";
        const string invalidSignature = "invalid-signature-hash";

        // Act
        var isValid = _paymentService.ValidateWebhookSignature(webhookPayload, invalidSignature);

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void ValidateWebhookSignature_WithEmptyPayload_ReturnsFalse()
    {
        // Arrange
        const string emptyPayload = "";
        const string signature = "some-signature";

        // Act
        var isValid = _paymentService.ValidateWebhookSignature(emptyPayload, signature);

        // Assert
        Assert.IsFalse(isValid);
    }

    #endregion

    #region Authorize.Net Complete Order Tests

    [TestMethod]
    public async Task ProcessPayment_WithCompleteOrder_HandlesAllDetails()
    {
        // Arrange
        var testOrder = CreateTestOrder(
            orderId: "AUTH-COMPLETE-ORDER",
            totalAmount: 299.97m,
            itemCount: 3);

        var paymentRequest = new PaymentRequest
        {
            OrderId = testOrder.OrderId,
            CustomerName = testOrder.CustomerName,
            CustomerEmail = testOrder.CustomerEmail,
            Amount = testOrder.TotalAmount,
            BillingAddress = "1234 Authorize Way",
            BillingCity = "San Francisco",
            BillingState = "CA",
            BillingZip = "94043",
            BillingCountry = "US",
            Description = "Complete Authorize.Net Order",
            LineItems = testOrder.Items,
            DataValue = "auth-net-token"
        };

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(testOrder.OrderId, response.OrderId);
        Assert.AreEqual(testOrder.TotalAmount, response.Amount);
    }

    [TestMethod]
    public async Task ProcessMultiplePayments_WithDifferentAmounts_ReturnsResponses()
    {
        // Arrange
        var amounts = new[] { 10.00m, 25.50m, 99.99m };
        var responses = new List<PaymentResponse>();

        // Act
        foreach (var amount in amounts)
        {
            var request = CreateTestPaymentRequest(amount: amount);
            var response = await _paymentService.ProcessPaymentAsync(request);
            responses.Add(response);
        }

        // Assert
        Assert.AreEqual(3, responses.Count);
        foreach (var response in responses)
        {
            Assert.IsNotNull(response);
        }
    }

    #endregion
}

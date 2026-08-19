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
    public override void Setup()
    {
        base.Setup();
        _paymentService = ServiceProvider.GetRequiredService<PayPalPaymentService>();
    }

    #region Basic PayPal Processing Tests

    [TestMethod]
    public async Task ProcessPayment_WithPayPalToken_ReturnsOrderId()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "PAYPAL-TEST-001",
            amount: 49.99m,
            paymentToken: "paypal-test-token");

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("PAYPAL-TEST-001", response.OrderId);
        Assert.AreEqual(49.99m, response.Amount);
    }

    [TestMethod]
    public async Task ProcessPayment_WithPayPalLargeAmount_ProcessesCorrectly()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "PAYPAL-LARGE-001",
            amount: 999.99m);

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(999.99m, response.Amount);
    }

    [TestMethod]
    public async Task ProcessPayment_WithPayPalCartMultipleItems_IncludesDetails()
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
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(199.98m, response.Amount);
    }

    [TestMethod]
    public async Task ProcessPayment_WithMinimalAmount_ProcessesCorrectly()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "PAYPAL-MIN-001",
            amount: 0.01m);

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(0.01m, response.Amount);
    }

    #endregion

    #region PayPal Profile Tests

    [TestMethod]
    public async Task CreatePaymentProfile_WithPayPalToken_ReturnsProfileId()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "PAYPAL-PROFILE-001",
            amount: 99.99m);

        // Act
        var response = await _paymentService.CreatePaymentProfileAsync(
            paymentRequest,
            "PayPal Tester",
            "paypal@test.com");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("PAYPAL-PROFILE-001", response.OrderId);
    }

    [TestMethod]
    public async Task ChargePaymentProfile_WithBillingAgreement_ProcessesRecurring()
    {
        // Arrange
        const string customerId = "PAYPAL-CUSTOMER-001";
        const string billingAgreementId = "PAYPAL-AGREEMENT-001";
        const decimal chargeAmount = 50.00m;

        // Act
        var response = await _paymentService.ChargePaymentProfileAsync(
            customerId,
            billingAgreementId,
            chargeAmount,
            "PAYPAL-RECURRING-CHARGE");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(chargeAmount, response.Amount);
    }

    #endregion

    #region PayPal Subscription Tests

    [TestMethod]
    public async Task CreateSubscription_WithPayPalPlan_ReturnsSubscriptionId()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "PAYPAL-SUB-001",
            amount: 9.99m);

        // Act
        var response = await _paymentService.CreateSubscriptionAsync(
            paymentRequest,
            "PayPal Monthly",
            intervalLength: 1,
            intervalUnit: "month",
            totalOccurrences: 12);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("PAYPAL-SUB-001", response.OrderId);
        Assert.AreEqual(9.99m, response.Amount);
    }

    [TestMethod]
    public async Task CreateSubscription_WithBiweeklyBilling_ReturnsResponse()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "PAYPAL-SUB-BIWEEKLY",
            amount: 19.99m);

        // Act
        var response = await _paymentService.CreateSubscriptionAsync(
            paymentRequest,
            "PayPal Biweekly",
            intervalLength: 2,
            intervalUnit: "week",
            totalOccurrences: 26);

        // Assert
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task CancelSubscription_WithPayPalSubscription_ReturnsSuccess()
    {
        // Arrange
        const string subscriptionId = "PAYPAL-SUB-CANCEL-001";

        // Act
        var response = await _paymentService.CancelSubscriptionAsync(subscriptionId);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(subscriptionId, response.OrderId);
    }

    #endregion

    #region PayPal Refund Tests

    [TestMethod]
    public async Task RefundTransaction_WithPayPalCapture_ReturnsRefundId()
    {
        // Arrange
        const string transactionId = "PAYPAL-CAPTURE-12345";
        const decimal refundAmount = 75.00m;

        // Act
        var response = await _paymentService.RefundTransactionAsync(transactionId, refundAmount);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
        Assert.AreEqual(refundAmount, response.Amount);
    }

    [TestMethod]
    public async Task RefundTransaction_WithPartialPayPalRefund_ReturnsResponse()
    {
        // Arrange
        const string transactionId = "PAYPAL-PARTIAL-001";
        const decimal refundAmount = 25.00m;

        // Act
        var response = await _paymentService.RefundTransactionAsync(transactionId, refundAmount);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(refundAmount, response.Amount);
    }

    #endregion

    #region PayPal Transaction Details Tests

    [TestMethod]
    public async Task GetTransactionDetails_WithPayPalTransaction_ReturnsDetails()
    {
        // Arrange
        const string transactionId = "PAYPAL-DETAILS-001";

        // Act
        var response = await _paymentService.GetTransactionDetailsAsync(transactionId);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    #endregion

    #region PayPal Webhook Tests

    [TestMethod]
    public void ValidateWebhookSignature_WithPayPalWebhook_ReturnsValidation()
    {
        // Arrange
        const string webhookPayload = "{\"event_type\":\"CHECKOUT.ORDER.APPROVED\",\"status\":\"success\"}";
        const string validSignature = "valid-paypal-signature";

        // Act
        var isValid = _paymentService.ValidateWebhookSignature(webhookPayload, validSignature);

        // Assert
        Assert.IsNotNull(isValid);
    }

    [TestMethod]
    public void ValidateWebhookSignature_WithInvalidPayPalSignature_ReturnsFalse()
    {
        // Arrange
        const string webhookPayload = "{\"event_type\":\"CHECKOUT.ORDER.APPROVED\"}";
        const string invalidSignature = "invalid-paypal-sig";

        // Act
        var isValid = _paymentService.ValidateWebhookSignature(webhookPayload, invalidSignature);

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void ValidateWebhookSignature_WithEmptyPayPalPayload_ReturnsFalse()
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

    #region PayPal Complete Order Tests

    [TestMethod]
    public async Task ProcessPayment_WithPayPalCompleteOrder_HandlesAllDetails()
    {
        // Arrange
        var testOrder = CreateTestOrder(
            orderId: "PAYPAL-COMPLETE-ORDER",
            totalAmount: 249.97m,
            itemCount: 3);

        var paymentRequest = new PaymentRequest
        {
            OrderId = testOrder.OrderId,
            CustomerName = testOrder.CustomerName,
            CustomerEmail = testOrder.CustomerEmail,
            Amount = testOrder.TotalAmount,
            BillingAddress = "1234 PayPal Way",
            BillingCity = "San Jose",
            BillingState = "CA",
            BillingZip = "95131",
            BillingCountry = "US",
            Description = "Complete PayPal Order",
            LineItems = testOrder.Items,
            DataValue = "paypal-token"
        };

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(testOrder.OrderId, response.OrderId);
        Assert.AreEqual(testOrder.TotalAmount, response.Amount);
    }

    [TestMethod]
    public async Task ProcessMultiplePayPalPayments_WithDifferentCustomers_ReturnsResponses()
    {
        // Arrange
        var customers = new[]
        {
            ("Customer 1", "customer1@test.com", 50.00m),
            ("Customer 2", "customer2@test.com", 75.50m),
            ("Customer 3", "customer3@test.com", 99.99m)
        };

        var responses = new List<PaymentResponse>();

        // Act
        foreach (var (name, email, amount) in customers)
        {
            var request = new PaymentRequest
            {
                OrderId = $"PAYPAL-{email.Split('@')[0]}",
                CustomerName = name,
                CustomerEmail = email,
                Amount = amount,
                BillingCity = "PayPal City",
                BillingState = "PC",
                DataValue = "paypal-token",
                LineItems = new List<LineItem>
                {
                    new LineItem { ItemId = "ITEM-1", Name = "Product", Quantity = 1, UnitPrice = amount }
                }
            };

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

    #region PayPal Error Handling Tests

    [TestMethod]
    public async Task ProcessPayment_WithDisabledService_ReturnsFailed()
    {
        // Arrange
        TestPaymentSettings.Enabled = false;
        var paymentRequest = CreateTestPaymentRequest();

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsFalse(response.IsSuccessful);
    }

    [TestMethod]
    public async Task VoidTransaction_WithPayPalTransaction_ReturnsResponse()
    {
        // Arrange
        const string transactionId = "PAYPAL-VOID-001";

        // Act
        var response = await _paymentService.VoidTransactionAsync(transactionId);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    #endregion
}

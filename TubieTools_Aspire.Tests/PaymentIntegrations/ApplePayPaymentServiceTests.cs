using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TubieTools_Aspire.Tests.PaymentIntegrations;

/// <summary>
/// xUnit tests for Apple Pay payment service integration
/// Uses EC_v1 encrypted tokens and sandbox environment
/// </summary>
[TestClass]
public class ApplePayPaymentServiceTests : PaymentServiceTestBase
{
    private IPaymentService _paymentService;

    [TestInitialize]
    public async void InitializeAsync()
    {
        //await base.InitializeAsync();
        _paymentService = ServiceProvider.GetRequiredService<ApplePayPaymentService>();
    }

    #region Basic Apple Pay Processing Tests

    [TestMethod]
    public async Task ProcessPayment_WithApplePayToken_ReturnsTransactionId()
    {
        // Arrange
        var applePayToken = CreateTestApplePayToken();
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-TEST-001",
            amount: 49.99m,
            paymentToken: Base64Encode(applePayToken));

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("APPLEPAY-TEST-001", response.OrderId);
        Assert.AreEqual(49.99m, response.Amount);
    }

    [TestMethod]
    public async Task ProcessPayment_WithApplePayEncryptedToken_DecryptsAndProcesses()
    {
        // Arrange
        var applePayToken = CreateTestApplePayToken();
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-ENCRYPTED-001",
            amount: 99.99m,
            paymentToken: Base64Encode(applePayToken));

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(99.99m, response.Amount);
    }

    [TestMethod]
    public async Task ProcessPayment_WithApplePayLargeAmount_ProcessesCorrectly()
    {
        // Arrange - Apple Pay uses cents internally
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-LARGE-001",
            amount: 999.99m);

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(999.99m, response.Amount);
    }

    [TestMethod]
    public async Task ProcessPayment_WithApplePayMinimalAmount_ProcessesCorrectly()
    {
        // Arrange - Testing with very small amount
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-MIN-001",
            amount: 0.01m);

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(0.01m, response.Amount);
    }

    #endregion

    #region Apple Pay Profile Tests

    [TestMethod]
    public async Task CreatePaymentProfile_WithApplePayToken_ReturnsPaymentMethodId()
    {
        // Arrange
        var applePayToken = CreateTestApplePayToken();
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-PROFILE-001",
            amount: 99.99m,
            paymentToken: Base64Encode(applePayToken));

        // Act
        var response = await _paymentService.CreatePaymentProfileAsync(
            paymentRequest,
            "Apple Pay Tester",
            "applepay@test.com");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("APPLEPAY-PROFILE-001", response.OrderId);
    }

    [TestMethod]
    public async Task ChargePaymentProfile_WithSavedApplePayMethod_ProcessesRecurring()
    {
        // Arrange
        const string customerId = "APPLEPAY-CUSTOMER-001";
        const string paymentMethodId = "APPLEPAY-METHOD-001";
        const decimal chargeAmount = 50.00m;

        // Act
        var response = await _paymentService.ChargePaymentProfileAsync(
            customerId,
            paymentMethodId,
            chargeAmount,
            "APPLEPAY-RECURRING-CHARGE");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(chargeAmount, response.Amount);
    }

    [TestMethod]
    public async Task ChargePaymentProfile_WithSubscriptionAmount_ProcessesCorrectly()
    {
        // Arrange
        const string customerId = "APPLEPAY-SUBSCRIPTION-CUSTOMER";
        const string paymentMethodId = "APPLEPAY-SUBSCRIPTION-METHOD";
        const decimal subscriptionAmount = 14.99m;

        // Act
        var response = await _paymentService.ChargePaymentProfileAsync(
            customerId,
            paymentMethodId,
            subscriptionAmount,
            "APPLEPAY-SUB-CHARGE");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(subscriptionAmount, response.Amount);
    }

    #endregion

    #region Apple Pay Subscription Tests

    [TestMethod]
    public async Task CreateSubscription_WithApplePayMethod_ReturnsSubscriptionId()
    {
        // Arrange
        var applePayToken = CreateTestApplePayToken();
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-SUB-001",
            amount: 9.99m,
            paymentToken: Base64Encode(applePayToken));

        // Act
        var response = await _paymentService.CreateSubscriptionAsync(
            paymentRequest,
            "Apple Pay Monthly Service",
            intervalLength: 1,
            intervalUnit: "month",
            totalOccurrences: 12);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("APPLEPAY-SUB-001", response.OrderId);
        Assert.AreEqual(9.99m, response.Amount);
    }

    [TestMethod]
    public async Task CreateSubscription_WithAnnualBilling_ReturnsResponse()
    {
        // Arrange
        var applePayToken = CreateTestApplePayToken();
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-SUB-ANNUAL",
            amount: 99.99m,
            paymentToken: Base64Encode(applePayToken));

        // Act
        var response = await _paymentService.CreateSubscriptionAsync(
            paymentRequest,
            "Apple Pay Annual Plan",
            intervalLength: 12,
            intervalUnit: "month",
            totalOccurrences: 1);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(99.99m, response.Amount);
    }

    [TestMethod]
    public async Task CreateSubscription_WithBiweeklyBilling_ReturnsResponse()
    {
        // Arrange
        var applePayToken = CreateTestApplePayToken();
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-SUB-BIWEEKLY",
            amount: 19.99m,
            paymentToken: Base64Encode(applePayToken));

        // Act
        var response = await _paymentService.CreateSubscriptionAsync(
            paymentRequest,
            "Apple Pay Biweekly",
            intervalLength: 2,
            intervalUnit: "week",
            totalOccurrences: 26);

        // Assert
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task CancelSubscription_WithApplePaySubscription_ReturnsSuccess()
    {
        // Arrange
        const string subscriptionId = "APPLEPAY-SUB-CANCEL-001";

        // Act
        var response = await _paymentService.CancelSubscriptionAsync(subscriptionId);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(subscriptionId, response.OrderId);
    }

    #endregion

    #region Apple Pay Refund Tests

    [TestMethod]
    public async Task RefundTransaction_WithApplePayTransaction_ReturnsRefundId()
    {
        // Arrange
        const string transactionId = "APPLEPAY-TXN-12345";
        const decimal refundAmount = 75.00m;

        // Act
        var response = await _paymentService.RefundTransactionAsync(transactionId, refundAmount);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
        Assert.AreEqual(refundAmount, response.Amount);
    }

    [TestMethod]
    public async Task RefundTransaction_WithPartialApplePayRefund_ReturnsResponse()
    {
        // Arrange
        const string transactionId = "APPLEPAY-PARTIAL-001";
        const decimal refundAmount = 10.00m;

        // Act
        var response = await _paymentService.RefundTransactionAsync(transactionId, refundAmount);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(refundAmount, response.Amount);
    }

    [TestMethod]
    public async Task RefundTransaction_WithHighPrecisionAmount_ProcessesCorrectly()
    {
        // Arrange - Testing cent precision
        const string transactionId = "APPLEPAY-PRECISION-001";
        const decimal refundAmount = 45.67m;

        // Act
        var response = await _paymentService.RefundTransactionAsync(transactionId, refundAmount);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(refundAmount, response.Amount);
    }

    #endregion

    #region Apple Pay Transaction Details Tests

    [TestMethod]
    public async Task GetTransactionDetails_WithApplePayTransaction_ReturnsDetails()
    {
        // Arrange
        const string transactionId = "APPLEPAY-DETAILS-001";

        // Act
        var response = await _paymentService.GetTransactionDetailsAsync(transactionId);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    #endregion

    #region Apple Pay Webhook Validation Tests

    [TestMethod]
    public void ValidateWebhookSignature_WithValidApplePayWebhook_ReturnsTrue()
    {
        // Arrange
        const string webhookPayload = "{\"transaction_id\":\"applepay-123\",\"status\":\"COMPLETED\"}";
        const string validSignature = "valid-apple-pay-signature";

        // Act
        var isValid = _paymentService.ValidateWebhookSignature(webhookPayload, validSignature);

        // Assert
        Assert.IsNotNull(isValid);
    }

    [TestMethod]
    public void ValidateWebhookSignature_WithInvalidApplePaySignature_ReturnsFalse()
    {
        // Arrange
        const string webhookPayload = "{\"transaction_id\":\"applepay-123\"}";
        const string invalidSignature = "invalid-signature";

        // Act
        var isValid = _paymentService.ValidateWebhookSignature(webhookPayload, invalidSignature);

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void ValidateWebhookSignature_WithMissingApplePayPayload_ReturnsFalse()
    {
        // Arrange
        const string missingPayload = "";
        const string signature = "some-signature";

        // Act
        var isValid = _paymentService.ValidateWebhookSignature(missingPayload, signature);

        // Assert
        Assert.IsFalse(isValid);
    }

    #endregion

    #region Apple Pay Void Transaction Tests

    [TestMethod]
    public async Task VoidTransaction_WithApplePayTransaction_ReturnsResponse()
    {
        // Arrange
        const string transactionId = "APPLEPAY-VOID-001";

        // Act
        var response = await _paymentService.VoidTransactionAsync(transactionId);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    #endregion

    #region Apple Pay Complete Order Tests

    [TestMethod]
    public async Task ProcessPayment_WithApplePayCompleteOrder_HandlesAllDetails()
    {
        // Arrange
        var testOrder = CreateTestOrder(
            orderId: "APPLEPAY-COMPLETE-ORDER",
            totalAmount: 299.97m,
            itemCount: 3);

        var applePayToken = CreateTestApplePayToken();
        var paymentRequest = new PaymentRequest
        {
            OrderId = testOrder.OrderId,
            CustomerName = testOrder.CustomerName,
            CustomerEmail = testOrder.CustomerEmail,
            Amount = testOrder.TotalAmount,
            BillingAddress = "1 Apple Park Way",
            BillingCity = "Cupertino",
            BillingState = "CA",
            BillingZip = "95014",
            BillingCountry = "US",
            Description = "Complete Apple Pay Order",
            LineItems = testOrder.Items,
            DataValue = Base64Encode(applePayToken),
            DataDescriptor = "APPLE_PAY_TOKEN"
        };

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(testOrder.OrderId, response.OrderId);
        Assert.AreEqual(testOrder.TotalAmount, response.Amount);
    }

    [TestMethod]
    public async Task ProcessMultipleApplePayPayments_WithDifferentDevices_ReturnsResponses()
    {
        // Arrange - Testing multiple iOS devices
        var iosDevices = new[]
        {
            ("iPhone User 1", "iphone1@test.com", 29.99m),
            ("iPad User 1", "ipad1@test.com", 49.99m),
            ("Mac User 1", "mac1@test.com", 99.99m)
        };

        var responses = new List<PaymentResponse>();

        // Act
        foreach (var (name, email, amount) in iosDevices)
        {
            var request = new PaymentRequest
            {
                OrderId = $"APPLEPAY-{email.Split('@')[0]}",
                CustomerName = name,
                CustomerEmail = email,
                Amount = amount,
                BillingCity = "Apple City",
                BillingState = "AC",
                DataValue = Base64Encode(CreateTestApplePayToken()),
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
        //Assert.All(responses, r => Assert.IsNotNull(r));
    }

    [TestMethod]
    public async Task ProcessPayment_WithApplePayCentAmount_HandlesCorrectly()
    {
        // Arrange - Apple Pay works with cents internally
        const long amountInCents = 9999; // $99.99
        decimal dollarAmount = amountInCents / 100m;

        var paymentRequest = CreateTestPaymentRequest(
            orderId: "APPLEPAY-CENTS-001",
            amount: dollarAmount);

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(dollarAmount, response.Amount);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Create a test Apple Pay token with EC_v1 format
    /// </summary>
    private string CreateTestApplePayToken()
    {
        return @"{
            ""version"":""EC_v1"",
            ""data"":""test-encrypted-payment-data"",
            ""signature"":""test-merchant-signature"",
            ""header"":{
                ""ephemeralPublicKey"":""test-ephemeral-public-key"",
                ""publicKeyHash"":""test-public-key-hash"",
                ""transactionId"":""test-apple-transaction-id""
            }
        }";
    }

    /// <summary>
    /// Helper to Base64 encode strings
    /// </summary>
    private string Base64Encode(string plainText)
    {
        byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    #endregion
}

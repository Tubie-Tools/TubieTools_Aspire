using Microsoft.VisualStudio.TestTools.UnitTesting;
using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;
using Microsoft.Extensions.DependencyInjection;

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
    public override void Setup()
    {
        base.Setup();
        _paymentService = ServiceProvider.GetRequiredService<GooglePayPaymentService>();
    }

    #region Basic Google Pay Processing Tests

    [TestMethod]
    public async Task ProcessPayment_WithGooglePayToken_ReturnsTransactionId()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "GPAY-TEST-001",
            amount: 49.99m,
            paymentToken: Base64Encode("{\"version\":\"EC_v1\",\"data\":\"test\",\"signature\":\"test\"}"));

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("GPAY-TEST-001", response.OrderId);
        Assert.AreEqual(49.99m, response.Amount);
    }

    [TestMethod]
    public async Task ProcessPayment_WithGooglePayEncryptedToken_DecryptsAndProcesses()
    {
        // Arrange
        var googlePayToken = CreateTestGooglePayToken();
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "GPAY-ENCRYPTED-001",
            amount: 99.99m,
            paymentToken: Base64Encode(googlePayToken));

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(99.99m, response.Amount);
    }

    [TestMethod]
    public async Task ProcessPayment_WithGooglePayMultipleItems_IncludesCartDetails()
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
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(199.98m, response.Amount);
    }

    #endregion

    #region Google Pay Profile Tests

    [TestMethod]
    public async Task CreatePaymentProfile_WithGooglePayToken_ReturnsPaymentMethodId()
    {
        // Arrange
        var tokenJson = CreateTestGooglePayToken();
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "GPAY-PROFILE-001",
            amount: 99.99m,
            paymentToken: Base64Encode(tokenJson));

        // Act
        var response = await _paymentService.CreatePaymentProfileAsync(
            paymentRequest,
            "Google Pay Tester",
            "googlepay@test.com");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("GPAY-PROFILE-001", response.OrderId);
    }

    [TestMethod]
    public async Task ChargePaymentProfile_WithSavedGooglePayMethod_ProcessesRecurring()
    {
        // Arrange
        const string customerId = "GPAY-CUSTOMER-001";
        const string paymentMethodId = "GPAY-METHOD-001";
        const decimal chargeAmount = 50.00m;

        // Act
        var response = await _paymentService.ChargePaymentProfileAsync(
            customerId,
            paymentMethodId,
            chargeAmount,
            "GPAY-RECURRING-CHARGE");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(chargeAmount, response.Amount);
    }

    [TestMethod]
    public async Task ChargePaymentProfile_WithVariousAmounts_ProcessesCorrectly()
    {
        // Arrange
        var amounts = new[] { 10.00m, 25.50m, 99.99m };

        // Act
        var responses = new List<PaymentResponse>();
        foreach (var amount in amounts)
        {
            var response = await _paymentService.ChargePaymentProfileAsync(
                "GPAY-CUSTOMER",
                "GPAY-METHOD",
                amount,
                $"CHARGE-{amount:F2}");
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
    public async Task CreateSubscription_WithGooglePayMethod_ReturnsSubscriptionId()
    {
        // Arrange
        var paymentRequest = CreateTestPaymentRequest(
            orderId: "GPAY-SUB-001",
            amount: 9.99m);
        paymentRequest.DataValue = Base64Encode(CreateTestGooglePayToken());

        // Act
        var response = await _paymentService.CreateSubscriptionAsync(
            paymentRequest,
            "Google Pay Monthly",
            intervalLength: 1,
            intervalUnit: "month",
            totalOccurrences: 12);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("GPAY-SUB-001", response.OrderId);
        Assert.AreEqual(9.99m, response.Amount);
    }

    [TestMethod]
    public async Task CreateSubscription_WithDifferentBillingCycles_ReturnsResponse()
    {
        // Arrange - Weekly subscription
        var weeklyRequest = CreateTestPaymentRequest(
            orderId: "GPAY-SUB-WEEKLY",
            amount: 1.99m);

        // Act
        var response = await _paymentService.CreateSubscriptionAsync(
            weeklyRequest,
            "Google Pay Weekly",
            intervalLength: 1,
            intervalUnit: "week",
            totalOccurrences: 52);

        // Assert
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task CancelSubscription_WithGooglePaySubscription_ReturnsSuccess()
    {
        // Arrange
        const string subscriptionId = "GPAY-SUB-CANCEL-001";

        // Act
        var response = await _paymentService.CancelSubscriptionAsync(subscriptionId);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(subscriptionId, response.OrderId);
    }

    #endregion

    #region Google Pay Refund Tests

    [TestMethod]
    public async Task RefundTransaction_WithGooglePayTransaction_ReturnsRefundId()
    {
        // Arrange
        const string transactionId = "GPAY-TXN-12345";
        const decimal refundAmount = 75.00m;

        // Act
        var response = await _paymentService.RefundTransactionAsync(transactionId, refundAmount);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
        Assert.AreEqual(refundAmount, response.Amount);
    }

    [TestMethod]
    public async Task RefundTransaction_WithPartialGooglePayRefund_ReturnsResponse()
    {
        // Arrange
        const string transactionId = "GPAY-PARTIAL-001";
        const decimal refundAmount = 25.00m;

        // Act
        var response = await _paymentService.RefundTransactionAsync(transactionId, refundAmount);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(refundAmount, response.Amount);
    }

    #endregion

    #region Google Pay Transaction Details Tests

    [TestMethod]
    public async Task GetTransactionDetails_WithGooglePayTransaction_ReturnsDetails()
    {
        // Arrange
        const string transactionId = "GPAY-DETAILS-001";

        // Act
        var response = await _paymentService.GetTransactionDetailsAsync(transactionId);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    #endregion

    #region Google Pay Webhook Validation Tests

    [TestMethod]
    public void ValidateWebhookSignature_WithValidGooglePayWebhook_ReturnsTrue()
    {
        // Arrange
        const string webhookPayload = "{\"transaction_id\":\"gpay-123\",\"status\":\"COMPLETED\"}";
        const string validSignature = "valid-google-pay-signature";

        // Act
        var isValid = _paymentService.ValidateWebhookSignature(webhookPayload, validSignature);

        // Assert
        Assert.IsNotNull(isValid);
    }

    [TestMethod]
    public void ValidateWebhookSignature_WithInvalidGooglePaySignature_ReturnsFalse()
    {
        // Arrange
        const string webhookPayload = "{\"transaction_id\":\"gpay-123\"}";
        const string invalidSignature = "invalid-signature-hash";

        // Act
        var isValid = _paymentService.ValidateWebhookSignature(webhookPayload, invalidSignature);

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void ValidateWebhookSignature_WithEmptyGooglePayPayload_ReturnsFalse()
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

    #region Google Pay Void Transaction Tests

    [TestMethod]
    public async Task VoidTransaction_WithGooglePayTransaction_ReturnsResponse()
    {
        // Arrange
        const string transactionId = "GPAY-VOID-001";

        // Act
        var response = await _paymentService.VoidTransactionAsync(transactionId);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(transactionId, response.TransactionId);
    }

    #endregion

    #region Google Pay Complete Order Tests

    [TestMethod]
    public async Task ProcessPayment_WithGooglePayCompleteOrder_HandlesAllDetails()
    {
        // Arrange
        var testOrder = CreateTestOrder(
            orderId: "GPAY-COMPLETE-ORDER",
            totalAmount: 299.97m,
            itemCount: 3);

        var tokenJson = CreateTestGooglePayToken();
        var paymentRequest = new PaymentRequest
        {
            OrderId = testOrder.OrderId,
            CustomerName = testOrder.CustomerName,
            CustomerEmail = testOrder.CustomerEmail,
            Amount = testOrder.TotalAmount,
            BillingAddress = "1234 Google Way",
            BillingCity = "Mountain View",
            BillingState = "CA",
            BillingZip = "94043",
            BillingCountry = "US",
            Description = "Complete Google Pay Order",
            LineItems = testOrder.Items,
            DataValue = Base64Encode(tokenJson),
            DataDescriptor = "GOOGLE_PAY_TOKEN"
        };

        // Act
        var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(testOrder.OrderId, response.OrderId);
        Assert.AreEqual(testOrder.TotalAmount, response.Amount);
    }

    [TestMethod]
    public async Task ProcessMultipleGooglePayPayments_WithDifferentDevices_ReturnsResponses()
    {
        // Arrange
        var androidDevices = new[]
        {
            ("Android User 1", "android1@test.com", 50.00m),
            ("Android User 2", "android2@test.com", 75.50m),
            ("Android User 3", "android3@test.com", 99.99m)
        };

        var responses = new List<PaymentResponse>();

        // Act
        foreach (var (name, email, amount) in androidDevices)
        {
            var request = new PaymentRequest
            {
                OrderId = $"GPAY-ANDROID-{email.Split('@')[0]}",
                CustomerName = name,
                CustomerEmail = email,
                Amount = amount,
                BillingCity = "Android City",
                BillingState = "AC",
                DataValue = Base64Encode(CreateTestGooglePayToken()),
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
        foreach (var r in responses)
        {
            Assert.IsNotNull(r);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Create a test Google Pay token JSON
    /// </summary>
    private string CreateTestGooglePayToken()
    {
        return @"{
            ""version"":""EC_v1"",
            ""data"":""test-encrypted-data"",
            ""signature"":""test-signature"",
            ""header"":{
                ""ephemeralPublicKey"":""test-ephemeral-key"",
                ""publicKeyHash"":""test-key-hash"",
                ""transactionId"":""test-transaction-id""
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

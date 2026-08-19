using Microsoft.VisualStudio.TestTools.UnitTesting;
using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TubieTools_Aspire.Tests.PaymentIntegrations;

/// <summary>
/// Base test class with common setup for all payment service tests
/// Uses MSTest framework with [TestInitialize] and [TestCleanup]
/// </summary>
[TestClass]
public abstract class PaymentServiceTestBase
{
    protected IServiceCollection Services { get; set; }
    protected ServiceProvider ServiceProvider { get; set; }
    protected ILogger<PaymentServiceTestBase> Logger { get; set; }
    protected PaymentSettings TestPaymentSettings { get; set; }

    [TestInitialize]
    public void Setup()
    {
        Services = new ServiceCollection();

        // Add logging
        Services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Add HTTP client factory
        Services.AddHttpClient();

        // Configure test payment settings (sandbox environment)
        TestPaymentSettings = new PaymentSettings
        {
            AuthorizeNetApiLoginId = "SANDBOX_API_LOGIN_ID",
            AuthorizeNetTransactionKey = "SANDBOX_TRANSACTION_KEY",
            AuthorizeNetSignatureKey = "SANDBOX_SIGNATURE_KEY",
            AuthorizeNetMerchantHash = "SANDBOX_MERCHANT_HASH",
            AuthorizeNetClientKey = "SANDBOX_CLIENT_KEY",
            AuthorizeNetEnvironment = "sandbox",
            Enabled = true
        };

        Services.Configure<PaymentSettings>(options =>
        {
            options.AuthorizeNetApiLoginId = TestPaymentSettings.AuthorizeNetApiLoginId;
            options.AuthorizeNetTransactionKey = TestPaymentSettings.AuthorizeNetTransactionKey;
            options.AuthorizeNetSignatureKey = TestPaymentSettings.AuthorizeNetSignatureKey;
            options.AuthorizeNetMerchantHash = TestPaymentSettings.AuthorizeNetMerchantHash;
            options.AuthorizeNetClientKey = TestPaymentSettings.AuthorizeNetClientKey;
            options.AuthorizeNetEnvironment = TestPaymentSettings.AuthorizeNetEnvironment;
            options.Enabled = TestPaymentSettings.Enabled;
        });

        // Register payment services
        Services.AddScoped<PaymentService>();
        Services.AddScoped<PayPalPaymentService>();
        Services.AddScoped<GooglePayPaymentService>();
        Services.AddScoped<ApplePayPaymentService>();
        Services.AddScoped<IPaymentServiceFactory, PaymentServiceFactory>();

        ServiceProvider = Services.BuildServiceProvider();
        Logger = ServiceProvider.GetRequiredService<ILogger<PaymentServiceTestBase>>();
    }

    [TestCleanup]
    public void TearDown()
    {
        ServiceProvider?.Dispose();
    }

    /// <summary>
    /// Create a test payment request with standard order data
    /// </summary>
    protected PaymentRequest CreateTestPaymentRequest(
        string orderId = null,
        decimal amount = 99.99m,
        string paymentToken = "test-payment-token")
    {
        orderId ??= $"TEST-ORDER-{Guid.NewGuid():N}".Substring(0, 20);

        return new PaymentRequest
        {
            OrderId = orderId,
            CustomerName = "John Test Doe",
            CustomerEmail = "test@example.com",
            CustomerPhone = "555-0123",
            Amount = amount,
            Description = "Test Product Purchase",
            BillingAddress = "123 Test Street",
            BillingCity = "Test City",
            BillingState = "TS",
            BillingZip = "12345",
            BillingCountry = "US",
            DataValue = paymentToken,
            DataDescriptor = "COMMON.ACCEPT.INAPP.PAYMENT",
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
            PurchaseOrderNumber = "PO-001",
            CustomerIPAddress = "127.0.0.1",
            CreatePaymentProfile = false,
            LineItems = new List<LineItem>
            {
                new LineItem
                {
                    ItemId = "ITEM-001",
                    Name = "Test Product",
                    Description = "A test product for payment processing",
                    Quantity = 1,
                    UnitPrice = amount
                }
            }
        };
    }

    /// <summary>
    /// Create a test order for payment testing
    /// </summary>
    protected Order CreateTestOrder(
        string orderId = null,
        decimal totalAmount = 99.99m,
        int itemCount = 1)
    {
        orderId ??= $"ORDER-{Guid.NewGuid():N}".Substring(0, 15);

        var items = new List<OrderItem>();
        var itemAmount = totalAmount / itemCount;

        for (int i = 0; i < itemCount; i++)
        {
            items.Add(new OrderItem
            {
                ProductId = i + 1,
                ProductName = $"Test Product {i + 1}",
                ProductDescription = $"Test product number {i + 1}",
                Quantity = 1,
                UnitPrice = itemAmount
            });
        }

        return new Order
        {
            OrderId = orderId,
            CustomerName = "Test Customer",
            CustomerEmail = "customer@test.com",
            TotalAmount = totalAmount,
            OrderItems = items,
            CreatedDate = DateTime.UtcNow,
            PaymentStatus = PaymentStatus.Pending
        };
    }

    /// <summary>
    /// Assert payment response indicates success
    /// </summary>
    protected void AssertPaymentSuccess(
        PaymentResponse response,
        string orderId = null,
        decimal expectedAmount = 0)
    {
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessful, $"Payment should be successful. Error: {response.ErrorMessage}");
        Assert.IsNotNull(response.TransactionId);
        Assert.IsTrue(!string.IsNullOrEmpty(response.TransactionId));

        if (!string.IsNullOrEmpty(orderId))
        {
            Assert.AreEqual(orderId, response.OrderId);
        }

        if (expectedAmount > 0)
        {
            Assert.AreEqual(expectedAmount, response.Amount);
        }
    }

    /// <summary>
    /// Assert payment response indicates failure
    /// </summary>
    protected void AssertPaymentFailure(PaymentResponse response)
    {
        Assert.IsNotNull(response);
        Assert.IsFalse(response.IsSuccessful);
        Assert.IsNotNull(response.ErrorMessage);
        Assert.IsTrue(!string.IsNullOrEmpty(response.ErrorMessage));
    }
}

/// <summary>
/// Test order model for testing purposes
/// </summary>
public class TestOrder
{
    public string OrderId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public decimal TotalAmount { get; set; }
    public List<LineItem> Items { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public OrderStatus Status { get; set; }
    public string CustomerId { get; internal set; }
    public List<Payment> Payments { get; internal set; }
}

/// <summary>
/// Order status enum for testing
/// </summary>
public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded
}

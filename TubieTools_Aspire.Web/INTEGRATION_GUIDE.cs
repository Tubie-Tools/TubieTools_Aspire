/**
 * INTEGRATION GUIDE: Multi-Provider Payment Processing System
 * 
 * This file shows how to integrate the payment system into your ASP.NET Core application.
 */

// ==========================================
// STEP 1: Add to Program.cs - Service Registration
// ==========================================

/*
Add these lines to your Program.cs after other service registrations:

using Microsoft.Extensions.Options;
using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;

// Configure payment settings from appsettings.json
builder.Services.Configure<PaymentSettings>(
    builder.Configuration.GetSection("PaymentSettings"));

// Register all payment services and factory
builder.Services.AddPaymentServices();

// Optional: Add PaymentServiceFactory as a singleton if you prefer
// builder.Services.AddSingleton<IPaymentServiceFactory, PaymentServiceFactory>();

// Important: Register HttpClientFactory (usually already done)
builder.Services.AddHttpClient();
*/

// ==========================================
// STEP 2: Configuration - appsettings.Development.json
// ==========================================

/*
{
  "PaymentSettings": {
    "AuthorizeNetApiLoginId": "your-authorize-net-api-id",
    "AuthorizeNetTransactionKey": "your-authorize-net-transaction-key",
    "AuthorizeNetSignatureKey": "your-authorize-net-signature-key",
    "AuthorizeNetMerchantHash": "your-merchant-hash",
    "AuthorizeNetClientKey": "your-client-key",
    "AuthorizeNetEnvironment": "sandbox",
    "Enabled": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "TubieTools_Aspire.Web.Services": "Debug",
      "Microsoft": "Warning"
    }
  }
}
*/

// ==========================================
// STEP 3: Environment Variables (Production)
// ==========================================

/*
On your production server, set these environment variables:

# ASP.NET Core Configuration
ASPNETCORE_ENVIRONMENT=Production

# Payment Service Configuration
PaymentSettings__AuthorizeNetApiLoginId=your-prod-api-id
PaymentSettings__AuthorizeNetTransactionKey=your-prod-transaction-key
PaymentSettings__AuthorizeNetSignatureKey=your-prod-signature-key
PaymentSettings__AuthorizeNetEnvironment=production
PaymentSettings__Enabled=true
*/

// ==========================================
// STEP 4: Inject into a Service
// ==========================================

using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;
using Microsoft.Extensions.Logging;

public class CheckoutService
{
    private readonly IPaymentServiceFactory _paymentServiceFactory;
    private readonly ILogger<CheckoutService> _logger;

    // Inject the factory
    public CheckoutService(
        IPaymentServiceFactory paymentServiceFactory,
        ILogger<CheckoutService> logger)
    {
        _paymentServiceFactory = paymentServiceFactory;
        _logger = logger;
    }

    /// <summary>
    /// Process a checkout with the customer's preferred payment method
    /// </summary>
    public async Task<PaymentResponse> ProcessCheckoutAsync(
        string paymentMethod,
        Order order,
        PaymentRequest paymentRequest,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Processing {PaymentMethod} payment for order {OrderId} with amount ${Amount}",
                paymentMethod,
                order.OrderId,
                order.TotalAmount);

            // Get the appropriate payment service
            var paymentService = _paymentServiceFactory.GetPaymentService(paymentMethod);

            // Process the payment
            var response = await paymentService.ProcessPaymentAsync(paymentRequest, cancellationToken);

            if (!response.IsSuccessful)
            {
                _logger.LogWarning(
                    "Payment processing failed for order {OrderId}: {ErrorMessage}",
                    order.OrderId,
                    response.ErrorMessage);
                return response;
            }

            _logger.LogInformation(
                "Payment successful for order {OrderId} with transaction ID {TransactionId}",
                order.OrderId,
                response.TransactionId);

            // Save transaction details to database
            await SaveTransactionAsync(order, response);

            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid payment method: {PaymentMethod}", paymentMethod);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Invalid payment method: {paymentMethod}",
                OrderId = order.OrderId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing payment for order {OrderId}", order.OrderId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = "An unexpected error occurred while processing your payment",
                OrderId = order.OrderId
            };
        }
    }

    /// <summary>
    /// Create a payment profile for recurring charges
    /// </summary>
    public async Task<PaymentResponse> CreateRecurringPaymentAsync(
        string paymentMethod,
        string customerId,
        string customerName,
        string customerEmail,
        PaymentRequest paymentRequest,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Creating recurring {PaymentMethod} profile for customer {CustomerId}",
                paymentMethod,
                customerId);

            var paymentService = _paymentServiceFactory.GetPaymentService(paymentMethod);

            var response = await paymentService.CreatePaymentProfileAsync(
                paymentRequest,
                customerName,
                customerEmail,
                cancellationToken);

            if (response.IsSuccessful)
            {
                _logger.LogInformation(
                    "Payment profile created for customer {CustomerId}: {CustomerProfileId}",
                    customerId,
                    response.CustomerProfileId);

                // Save profile IDs to database
                await SavePaymentProfileAsync(customerId, response);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recurring payment profile for customer {CustomerId}", customerId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = "Failed to create recurring payment profile"
            };
        }
    }

    /// <summary>
    /// Process a refund
    /// </summary>
    public async Task<PaymentResponse> RefundTransactionAsync(
        string paymentMethod,
        string transactionId,
        decimal refundAmount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Processing {PaymentMethod} refund for transaction {TransactionId} with amount ${RefundAmount}",
                paymentMethod,
                transactionId,
                refundAmount);

            var paymentService = _paymentServiceFactory.GetPaymentService(paymentMethod);
            var response = await paymentService.RefundTransactionAsync(transactionId, refundAmount, cancellationToken);

            if (response.IsSuccessful)
            {
                _logger.LogInformation(
                    "Refund successful for transaction {TransactionId}",
                    transactionId);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for transaction {TransactionId}", transactionId);
            return new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = "Failed to process refund"
            };
        }
    }

    private async Task SaveTransactionAsync(Order order, PaymentResponse response)
    {
        // TODO: Implement database save for transaction
        await Task.CompletedTask;
    }

    private async Task SavePaymentProfileAsync(string customerId, PaymentResponse response)
    {
        // TODO: Implement database save for payment profile
        await Task.CompletedTask;
    }
}

// ==========================================
// STEP 5: Use in a Blazor Component
// ==========================================

/*
@page "/checkout"
@using TubieTools_Aspire.Web.Models
@using TubieTools_Aspire.Web.Services
@inject CheckoutService CheckoutService
@inject ILogger<Checkout> Logger

<div class="checkout">
    <h1>Checkout</h1>

    <div class="payment-methods">
        <button @onclick="() => ProcessPayment('AuthorizeNet')">Authorize.Net</button>
        <button @onclick="() => ProcessPayment('PayPal')">PayPal</button>
        <button @onclick="() => ProcessPayment('GooglePay')">Google Pay</button>
        <button @onclick="() => ProcessPayment('ApplePay')">Apple Pay</button>
    </div>

    @if (!string.IsNullOrEmpty(message))
    {
        <div class="alert alert-info">@message</div>
    }
</div>

@code {
    private string message = "";

    private async Task ProcessPayment(string paymentMethod)
    {
        try
        {
            var paymentRequest = new PaymentRequest
            {
                OrderId = "ORDER-123",
                CustomerName = "Customer Name",
                CustomerEmail = "customer@example.com",
                Amount = 99.99m,
                BillingAddress = "123 Main St",
                BillingCity = "New York",
                BillingState = "NY",
                BillingZip = "10001",
                DataValue = "token-from-frontend",
                DataDescriptor = "COMMON.ACCEPT.INAPP.PAYMENT"
            };

            var order = new Order { OrderId = "ORDER-123", TotalAmount = 99.99m };

            var response = await CheckoutService.ProcessCheckoutAsync(
                paymentMethod,
                order,
                paymentRequest);

            if (response.IsSuccessful)
            {
                message = $"Payment successful! Transaction ID: {response.TransactionId}";
            }
            else
            {
                message = $"Payment failed: {response.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing payment");
            message = "An error occurred while processing your payment";
        }
    }
}
*/

// ==========================================
// STEP 6: Test with Dependent Service
// ==========================================

/*
[Test]
public class PaymentServiceFactoryTests
{
    private IServiceCollection _services;
    private ServiceProvider _serviceProvider;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();

        _services.AddLogging();
        _services.AddHttpClient();
        _services.Configure<PaymentSettings>(options =>
        {
            options.AuthorizeNetApiLoginId = "test-id";
            options.AuthorizeNetTransactionKey = "test-key";
            options.AuthorizeNetEnvironment = "sandbox";
            options.Enabled = true;
        });

        _services.AddPaymentServices();

        _serviceProvider = _services.BuildServiceProvider();
    }

    [Test]
    public void GetPaymentService_WithValidMethod_ReturnsService()
    {
        var factory = _serviceProvider.GetRequiredService<IPaymentServiceFactory>();

        var service = factory.GetPaymentService(PaymentMethod.AuthorizeNet);

        Assert.IsNotNull(service);
        Assert.IsInstanceOf<PaymentService>(service);
    }

    [Test]
    public void GetPaymentService_WithInvalidMethod_ThrowsException()
    {
        var factory = _serviceProvider.GetRequiredService<IPaymentServiceFactory>();

        Assert.Throws<ArgumentException>(() => 
            factory.GetPaymentService("InvalidMethod"));
    }
}
*/

// ==========================================
// STEP 7: Docker Configuration (Optional)
// ==========================================

/*
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TubieTools_Aspire.Web/TubieTools_Aspire.Web.csproj", "TubieTools_Aspire.Web/"]
RUN dotnet restore "TubieTools_Aspire.Web/TubieTools_Aspire.Web.csproj"
COPY . .
WORKDIR "/src/TubieTools_Aspire.Web"
RUN dotnet build "TubieTools_Aspire.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TubieTools_Aspire.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PaymentSettings__AuthorizeNetEnvironment=production

ENTRYPOINT ["dotnet", "TubieTools_Aspire.Web.dll"]
*/


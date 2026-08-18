/**
 * PROGRAM.CS INTEGRATION EXAMPLE
 * 
 * This shows how to integrate the payment system into your existing Program.cs
 */

// Add these using statements
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using TubieTools_Aspire.Web.Data;
using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// EXISTING SERVICES (keep all of these)
// ==========================================

// Add services to the container
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

// Add Identity services
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
	.AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

// Add Razor Pages
builder.Services.AddRazorPages();

// Add EF Core DbContext if needed
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==========================================
// NEW: PAYMENT SYSTEM SERVICES
// ==========================================

// Configure payment settings from appsettings.json
builder.Services.Configure<PaymentSettings>(
	builder.Configuration.GetSection("PaymentSettings"));

// Register HTTP client factory (required by payment services)
builder.Services.AddHttpClient();

// Register all payment services (Authorize.Net, PayPal, Google Pay, Apple Pay)
builder.Services.AddPaymentServices();

// ==========================================
// EXISTING MIDDLEWARE (keep all of these)
// ==========================================

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// ==========================================
// NEW: MAP PAYMENT API ENDPOINTS
// ==========================================

// Map the payments API controller
app.MapControllers();

// ==========================================
// EXISTING COMPONENT MAPPING (keep this)
// ==========================================

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.MapRazorPages();

// ==========================================
// RUN THE APPLICATION
// ==========================================

app.Run();

/**
 * ===========================================
 * CONFIGURATION TEMPLATE (appsettings.json)
 * ===========================================
 * 
 * Copy this section into your appsettings.json:
 */

/*
{
  "AzureAd": {
	"Instance": "https://login.microsoftonline.com/",
	"TenantId": "common",
	"ClientId": "your-client-id",
	"CallbackPath": "/signin-oidc"
  },
  "PaymentSettings": {
	"AuthorizeNetApiLoginId": "YOUR_AUTHORIZE_NET_API_LOGIN_ID",
	"AuthorizeNetTransactionKey": "YOUR_AUTHORIZE_NET_TRANSACTION_KEY",
	"AuthorizeNetSignatureKey": "YOUR_AUTHORIZE_NET_SIGNATURE_KEY",
	"AuthorizeNetMerchantHash": "YOUR_MERCHANT_HASH",
	"AuthorizeNetClientKey": "YOUR_CLIENT_KEY",
	"AuthorizeNetEnvironment": "sandbox",
	"Enabled": true
  },
  "ConnectionStrings": {
	"DefaultConnection": "Data Source=.\\sqlexpress;Initial Catalog=TubieToolsDb;Integrated Security=true;Encrypt=false"
  },
  "Logging": {
	"LogLevel": {
	  "Default": "Information",
	  "Microsoft": "Warning",
	  "Microsoft.AspNetCore": "Warning",
	  "TubieTools_Aspire.Web.Services": "Debug"
	}
  },
  "AllowedHosts": "*"
}
*/

/**
 * ===========================================
 * ENVIRONMENT VARIABLES FOR PRODUCTION
 * ===========================================
 * 
 * Set these on your production server:
 */

/*
# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80

# Payment Settings
PaymentSettings__AuthorizeNetApiLoginId=prod-api-id
PaymentSettings__AuthorizeNetTransactionKey=prod-transaction-key
PaymentSettings__AuthorizeNetSignatureKey=prod-signature-key
PaymentSettings__AuthorizeNetMerchantHash=prod-merchant-hash
PaymentSettings__AuthorizeNetClientKey=prod-client-key
PaymentSettings__AuthorizeNetEnvironment=production
PaymentSettings__Enabled=true

# Azure AD
AzureAd__Instance=https://login.microsoftonline.com/
AzureAd__TenantId=your-tenant-id
AzureAd__ClientId=your-client-id

# Database
ConnectionStrings__DefaultConnection=Server=your-sql-server;Database=TubieToolsDb;User Id=sa;Password=your-password;Encrypt=true;Connection Timeout=30;
*/

/**
 * ===========================================
 * USAGE IN A RAZOR COMPONENT
 * ===========================================
 */

/*
@page "/checkout"
@using TubieTools_Aspire.Web.Models
@using TubieTools_Aspire.Web.Services
@inject IPaymentServiceFactory PaymentServiceFactory
@inject ILogger<Checkout> Logger

<div class="checkout-container">
	<h1>Checkout</h1>

	<div class="payment-method-selector">
		<button @onclick="() => SelectedPaymentMethod = 'AuthorizeNet'" 
				class="@(SelectedPaymentMethod == "AuthorizeNet" ? "selected" : "")">
			Authorize.Net
		</button>
		<button @onclick="() => SelectedPaymentMethod = 'PayPal'" 
				class="@(SelectedPaymentMethod == "PayPal" ? "selected" : "")">
			PayPal
		</button>
		<button @onclick="() => SelectedPaymentMethod = 'GooglePay'" 
				class="@(SelectedPaymentMethod == "GooglePay" ? "selected" : "")">
			Google Pay
		</button>
		<button @onclick="() => SelectedPaymentMethod = 'ApplePay'" 
				class="@(SelectedPaymentMethod == "ApplePay" ? "selected" : "")">
			Apple Pay
		</button>
	</div>

	@if (!string.IsNullOrEmpty(Message))
	{
		<div class="alert @MessageClass">
			@Message
		</div>
	}

	<button @onclick="ProcessPayment" disabled="@IsProcessing">
		@if (IsProcessing)
		{
			<span>Processing...</span>
		}
		else
		{
			<span>Complete Purchase ($@Amount.ToString("F2"))</span>
		}
	</button>
</div>

@code {
	private string SelectedPaymentMethod = "PayPal";
	private string Message = "";
	private string MessageClass = "";
	private bool IsProcessing = false;
	private decimal Amount = 99.99m;

	private async Task ProcessPayment()
	{
		IsProcessing = true;
		Message = "";

		try
		{
			var paymentService = PaymentServiceFactory.GetPaymentService(SelectedPaymentMethod);

			var paymentRequest = new PaymentRequest
			{
				OrderId = Guid.NewGuid().ToString(),
				CustomerName = "Customer Name",
				CustomerEmail = "customer@example.com",
				Amount = Amount,
				BillingAddress = "123 Main St",
				BillingCity = "New York",
				BillingState = "NY",
				BillingZip = "10001",
				Billin Country = "US",
				Description = "Your Order",
				// These would come from your payment processor's frontend library
				DataValue = "encrypted-token-from-frontend",
				DataDescriptor = "COMMON.ACCEPT.INAPP.PAYMENT"
			};

			var response = await paymentService.ProcessPaymentAsync(paymentRequest);

			if (response.IsSuccessful)
			{
				Message = $"Payment successful! Transaction ID: {response.TransactionId}";
				MessageClass = "alert-success";
				Logger.LogInformation("Payment successful: {TransactionId}", response.TransactionId);
			}
			else
			{
				Message = $"Payment failed: {response.ErrorMessage}";
				MessageClass = "alert-danger";
				Logger.LogWarning("Payment failed: {ErrorMessage}", response.ErrorMessage);
			}
		}
		catch (Exception ex)
		{
			Message = "An error occurred while processing your payment. Please try again.";
			MessageClass = "alert-danger";
			Logger.LogError(ex, "Error processing payment");
		}
		finally
		{
			IsProcessing = false;
		}
	}
}
*/

/**
 * ===========================================
 * DEPENDENCY INJECTION IN A SERVICE
 * ===========================================
 */

/*
using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;
using Microsoft.Extensions.Logging;

public class OrderService
{
	private readonly IPaymentServiceFactory _paymentServiceFactory;
	private readonly ILogger<OrderService> _logger;

	public OrderService(
		IPaymentServiceFactory paymentServiceFactory,
		ILogger<OrderService> logger)
	{
		_paymentServiceFactory = paymentServiceFactory;
		_logger = logger;
	}

	public async Task<bool> CompleteOrderAsync(
		Order order,
		string paymentMethod,
		PaymentRequest paymentRequest,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var paymentService = _paymentServiceFactory.GetPaymentService(paymentMethod);
			var response = await paymentService.ProcessPaymentAsync(paymentRequest, cancellationToken);

			if (response.IsSuccessful)
			{
				order.Status = OrderStatus.Completed;
				order.TransactionId = response.TransactionId;
				order.AuthCode = response.AuthCode;
				order.ProcessedDate = DateTime.UtcNow;

				// Save to database
				await SaveOrderAsync(order);

				_logger.LogInformation("Order {OrderId} completed with transaction {TransactionId}",
					order.OrderId, response.TransactionId);

				return true;
			}
			else
			{
				order.Status = OrderStatus.Failed;
				order.ErrorMessage = response.ErrorMessage;

				await SaveOrderAsync(order);

				_logger.LogWarning("Order {OrderId} payment failed: {ErrorMessage}",
					order.OrderId, response.ErrorMessage);

				return false;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error completing order {OrderId}", order.OrderId);
			throw;
		}
	}

	private async Task SaveOrderAsync(Order order)
	{
		// TODO: Save to database
		await Task.CompletedTask;
	}
}
*/

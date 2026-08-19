# Payment Integration Tests

This folder contains comprehensive NUnit integration tests for the TubieTools Aspire payment processing system. The tests validate all payment providers (Authorize.Net, PayPal, Google Pay, and Apple Pay) with test credentials and sandbox environments.

## Test Files Overview

### 1. **PaymentServiceTestBase.cs**
Base test fixture providing shared setup and utilities for all payment integration tests.

**Responsibilities:**
- DI container configuration with logging and HTTP client factory
- Sandbox payment settings initialization with test credentials
- Helper methods for creating test payment requests and orders
- Common assertion methods for payment success/failure validation
- Service provider lifecycle management

**Key Methods:**
```csharp
Setup()                                    // Initializes DI and sandbox settings
CreateTestPaymentRequest()                 // Creates payment request with defaults
CreateTestOrder()                          // Creates test order with line items
AssertPaymentSuccess()                     // Validates successful payment response
AssertPaymentFailure()                     // Validates failed payment response
```

**Test Models:**
- `TestOrder` - Business object representing an order for testing
- `OrderStatus` - Enum for order states (Pending, Processing, Completed, Failed, Refunded)

---

### 2. **AuthorizeNetPaymentServiceTests.cs**
Tests for the Authorize.Net payment provider.

**Coverage Areas:**

#### Basic Payment Processing (5 tests)
- Valid payment request processing
- Negative/zero amount handling
- Missing customer email scenarios
- Multiple line items inclusion

#### Payment Profiles (3 tests)
- Profile creation with valid tokens
- Profile charging for recurring payments
- Partial amount charging

#### Refunds & Voids (3 tests)
- Full and partial refund processing
- Transaction voiding

#### Transaction Details (2 tests)
- Valid transaction details retrieval
- Invalid transaction ID handling

#### Subscriptions (3 tests)
- Subscription creation with intervals
- Different billing frequencies
- Subscription cancellation

#### Webhook Validation (3 tests)
- Valid signature verification
- Invalid signature detection
- Empty signature handling

#### Batch Operations (2 tests)
- Multiple payments with different amounts
- Complete order with all details
- Cancellation handling

**Sample Test:**
```csharp
[Test]
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
```

---

### 3. **PayPalPaymentServiceTests.cs**
Tests for the PayPal payment provider.

**Coverage Areas:**

#### Basic Payment Processing (4 tests)
- PayPal token processing
- Cart details with multiple line items
- Large amount transactions
- Minimal amount handling

#### PayPal Profiles (2 tests)
- Billing agreement creation
- Saved payment method charging

#### PayPal Subscriptions (3 tests)
- Plan-based subscription creation
- Quarterly billing cycles
- Subscription cancellation

#### PayPal Refunds (2 tests)
- Capture refunds
- Partial refunds

#### PayPal Webhooks (2 tests)
- Webhook signature validation
- Invalid webhook handling

#### PayPal Orders (2 tests)
- Complete order processing
- Multiple customer payments

#### Error Handling (2 tests)
- Disabled service behavior
- Transaction voiding

**Sample Test:**
```csharp
[Test]
public async Task ProcessPayment_WithPayPalCompleteOrder_HandlesAllDetails()
{
	// Arrange
	var testOrder = CreateTestOrder(
		orderId: "PAYPAL-COMPLETE-ORDER",
		totalAmount: 249.97m,
		itemCount: 3);

	var paymentRequest = new PaymentRequest { /* ... */ };

	// Act
	var response = await _paymentService.ProcessPaymentAsync(paymentRequest);

	// Assert
	AssertPaymentSuccess(response, testOrder.OrderId, testOrder.TotalAmount);
}
```

---

### 4. **GooglePayPaymentServiceTests.cs**
Tests for the Google Pay payment provider.

**Coverage Areas:**

#### Basic Google Pay Processing (4 tests)
- Payment token processing
- Encrypted token decryption
- Multiple line items
- Cart management

#### Google Pay Profiles (3 tests)
- Payment method creation
- Saved method recurring charges
- Multiple charge amounts

#### Google Pay Subscriptions (3 tests)
- Subscription creation with different intervals
- Billing cycle management
- Cancellation

#### Google Pay Refunds (2 tests)
- Transaction refunds
- Partial refund processing

#### Google Pay Webhooks (3 tests)
- Valid signature validation
- Invalid signature detection
- Empty payload handling

#### Google Pay Operations (2 tests)
- Complete order processing
- Multi-device scenarios (Android emulation)

#### Helper Utilities
```csharp
CreateTestGooglePayToken()  // Generates test Google Pay token JSON
Base64Encode()              // Encodes token data
```

---

### 5. **ApplePayPaymentServiceTests.cs**
Tests for the Apple Pay payment provider.

**Coverage Areas:**

#### Basic Apple Pay Processing (4 tests)
- Apple Pay token processing
- Encrypted token decryption
- Large amount handling
- Minimal amount precision

#### Apple Pay Profiles (3 tests)
- Payment method creation
- Saved method recurring charges
- Subscription amount processing

#### Apple Pay Subscriptions (4 tests)
- Monthly subscriptions
- Annual billing
- Biweekly billing cycles
- Cancellation

#### Apple Pay Refunds (3 tests)
- Transaction refunds
- Partial refunds
- High-precision cent handling

#### Apple Pay Webhooks (3 tests)
- Valid signature validation
- Invalid signature detection
- Missing payload handling

#### Apple Pay Operations (2 tests)
- Complete order processing
- Multiple device types (iPhone, iPad, Mac)
- Cent amount accuracy

#### Helper Utilities
```csharp
CreateTestApplePayToken()   // Generates EC_v1 Apple Pay token JSON
Base64Encode()              // Encodes token data
```

---

### 6. **PaymentWebhookIntegrationTests.cs**
Cross-provider webhook and integration tests.

**Coverage Areas:**

#### Authorize.Net Webhooks (3 tests)
- Valid signature validation
- Missing signature handling
- Tampered payload detection

#### PayPal Webhooks (3 tests)
- PayPal-specific signature format
- Invalid signature detection
- Expired webhook handling

#### Google Pay Webhooks (2 tests)
- Google Pay signature validation
- Invalid signature detection

#### Apple Pay Webhooks (2 tests)
- Apple Pay signature validation
- Invalid signature detection

#### Cross-Provider Scenarios (3 tests)
- Same order across multiple providers
- Refunds across different providers
- Subscription management with different plans

#### Error Handling & Edge Cases (3 tests)
- Disabled provider handling
- Null payload validation
- All providers with invalid signatures

#### Webhook Event Processing (4 tests)
- Authorize.Net approved events
- PayPal completed events
- Google Pay success events
- Apple Pay captured events

#### Factory Pattern Tests (2 tests)
- Enum-based provider selection
- String-based provider selection

---

## Running the Tests

### Prerequisites
- .NET 8 or higher
- xUnit 2.6+
- Visual Studio 2022+ or VS Code with C# extension

### Via Command Line
```bash
# Run all payment integration tests
dotnet test TubieTools_Aspire.Tests --filter "FullyQualifiedName~PaymentIntegrations"

# Run specific test class
dotnet test TubieTools_Aspire.Tests --filter "FullyQualifiedName~AuthorizeNetPaymentServiceTests"

# Run specific test method
dotnet test TubieTools_Aspire.Tests --filter "Name=ProcessPayment_WithValidRequest_ReturnsPaymentResponse"

# Run with verbose output
dotnet test TubieTools_Aspire.Tests -v d

# Run with detailed output and tracing
dotnet test TubieTools_Aspire.Tests --logger "console;verbosity=detailed"
```

### Via Visual Studio
1. Open Test Explorer (Test > Test Explorer)
2. Search for "PaymentIntegrations"
3. Right-click test class or method and select "Run"

### Via VS Code
1. Install "C# Dev Kit" and "Test Explorer" extensions
2. Open Test Explorer in the sidebar
3. Expand PaymentIntegrations folder
4. Click the Run icon next to tests

### Via TestDriven.Net
1. Install TestDriven.Net extension
2. Right-click test class or method
3. Select "Run Test"

---

## Test Credentials

All tests use **sandbox credentials** configured in `PaymentServiceTestBase.cs`:

```csharp
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
```

**No real charges are made.** Tests use:
- Fake token values
- Sandbox payment processors
- Mock transaction IDs
- Test credit card numbers (if needed)

---

## Test Data

### Standard Test Payment Request
```csharp
CreateTestPaymentRequest(
	orderId: "TEST-ORDER-{GUID}",
	amount: 99.99m,
	paymentToken: "test-payment-token"
)
```

Includes:
- Customer name: "John Test Doe"
- Email: "test@example.com"
- Phone: "555-0123"
- Billing address: 123 Test Street, Test City, TS 12345
- Line items with unit pricing
- Invoice/PO numbers
- Customer IP address

### Standard Test Order
```csharp
CreateTestOrder(
	orderId: "ORDER-{GUID}",
	totalAmount: 99.99m,
	itemCount: 1
)
```

Includes:
- Customer details
- Multiple line items
- Total amount calculation
- Order status (Pending)
- Creation timestamp

---

## Assertion Helpers

### AssertPaymentSuccess()
Validates that a payment was processed successfully:
```csharp
AssertPaymentSuccess(
	response,
	orderId: "ABC-123",
	expectedAmount: 99.99m
);
```

Checks:
- Response is not null
- IsSuccessful == true
- TransactionId is populated
- OrderId matches (if provided)
- Amount matches (if provided)

### AssertPaymentFailure()
Validates that a payment failed appropriately:
```csharp
AssertPaymentFailure(response);
```

Checks:
- Response is not null
- IsSuccessful == false
- ErrorMessage is populated

---

## Common Test Patterns

### Testing Payment Processing
```csharp
[Fact]
public async Task ProcessPayment_WithValidRequest_ReturnsTransactionId()
{
	// Arrange
	var request = CreateTestPaymentRequest(amount: 50.00m);

	// Act
	var response = await paymentService.ProcessPaymentAsync(request);

	// Assert
	AssertPaymentSuccess(response);
	Assert.Equal(50.00m, response.Amount);
}
```

### Testing Error Scenarios
```csharp
[Fact]
public async Task ProcessPayment_WithNegativeAmount_ReturnsFailed()
{
	// Arrange
	var request = CreateTestPaymentRequest(amount: -50.00m);

	// Act
	var response = await paymentService.ProcessPaymentAsync(request);

	// Assert
	AssertPaymentFailure(response);
}
```

### Testing Subscriptions
```csharp
[Fact]
public async Task CreateSubscription_WithMonthlyBilling_ReturnsSubscriptionId()
{
	// Arrange
	var request = CreateTestPaymentRequest(amount: 29.99m);

	// Act
	var response = await paymentService.CreateSubscriptionAsync(
		request,
		"Monthly Plan",
		intervalLength: 1,
		intervalUnit: "month",
		totalOccurrences: 12);

	// Assert
	AssertPaymentSuccess(response);
}
```

### Testing Webhooks
```csharp
[Fact]
public void ValidateWebhook_WithValidSignature_ReturnsTrue()
{
	// Arrange
	const string payload = "{\"transaction_id\":\"12345\"}";
	const string validSignature = "signature-value";

	// Act
	var isValid = paymentService.ValidateWebhookSignature(payload, validSignature);

	// Assert
	Assert.True(isValid);
}
```

---

## Extending Tests

### Adding a New Payment Provider Test Class
1. Create `YourPaymentServiceTests.cs` in the `PaymentIntegrations` folder
2. Inherit from `PaymentServiceTestBase`
3. Override `InitializeAsync()` to initialize the payment service
4. Implement provider-specific test methods with `[Fact]` attributes
5. Use helper methods for consistency

### Example Template
```csharp
public class YourPaymentServiceTests : PaymentServiceTestBase
{
	private IPaymentService _paymentService;

	public override async Task InitializeAsync()
	{
		await base.InitializeAsync();
		_paymentService = ServiceProvider.GetRequiredService<YourPaymentService>();
	}

	[Fact]
	public async Task ProcessPayment_WithValidRequest_ReturnsSuccess()
	{
		// Your test code here
	}
}
```

---

## Best Practices

### ✅ DO:
- Use the base fixture `PaymentServiceTestBase` for shared setup
- Name tests descriptively: `MethodName_Condition_ExpectedResult`
- Create test data using helper methods
- Use `AssertPaymentSuccess()` and `AssertPaymentFailure()`
- Test both success and failure scenarios
- Validate webhook signatures thoroughly
- Use sandbox/test credentials only

### ❌ DON'T:
- Make actual network calls to real payment processors
- Use production API keys
- Create hard-coded test data
- Test multiple behaviors in a single test
- Ignore error responses
- Leave tests with side effects

---

## Continuous Integration

These tests are designed to run in CI/CD pipelines:

```yaml
# Example GitHub Actions workflow
- name: Run Payment Integration Tests
  run: dotnet test TubieTools_Aspire.Tests --filter "PaymentIntegrations" --logger "trx;LogFileName=test-results.trx"
```

Tests should:
- Not depend on external services
- Complete within reasonable time
- Provide clear failure messages
- Run consistently across environments

---

## Support

For issues or questions:
1. Check test logs for detailed failure messages
2. Review the payment service implementations in `TubieTools_Aspire.Web/Services`
3. Consult the payment models in `TubieTools_Aspire.Web/Models`
4. Review payment integration documentation

---

## Related Files

- `TubieTools_Aspire.Web/Services/PaymentService.cs` - Authorize.Net implementation
- `TubieTools_Aspire.Web/Services/PayPalPaymentService.cs` - PayPal implementation
- `TubieTools_Aspire.Web/Services/GooglePayPaymentService.cs` - Google Pay implementation
- `TubieTools_Aspire.Web/Services/ApplePayPaymentService.cs` - Apple Pay implementation
- `TubieTools_Aspire.Web/Services/PaymentServiceFactory.cs` - Provider factory
- `TubieTools_Aspire.Web/Models/PaymentRequest.cs` - Payment input model
- `TubieTools_Aspire.Web/Models/PaymentResponse.cs` - Payment response model
- `TubieTools_Aspire.Web/Models/PaymentSettings.cs` - Configuration model

---

**Last Updated:** 2024
**Test Framework:** xUnit 2.6+
**Target Framework:** .NET 8+

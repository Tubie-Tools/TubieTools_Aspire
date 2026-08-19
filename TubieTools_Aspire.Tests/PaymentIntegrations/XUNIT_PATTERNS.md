# xUnit Test Patterns and Examples

This document provides xUnit-specific patterns, best practices, and examples for the payment integration tests.

## Table of Contents
1. [Basic Patterns](#basic-patterns)
2. [Async Testing](#async-testing)
3. [Theory Tests](#theory-tests)
4. [Fixture Patterns](#fixture-patterns)
5. [Custom Assertions](#custom-assertions)
6. [Error Handling](#error-handling)
7. [Best Practices](#best-practices)

## Basic Patterns

### Simple Fact Test
```csharp
[Fact]
public async Task ProcessPayment_WithValidRequest_ReturnsSuccess()
{
	// Arrange
	var request = CreateTestPaymentRequest(amount: 50.00m);

	// Act
	var response = await _paymentService.ProcessPaymentAsync(request);

	// Assert
	Assert.NotNull(response);
	Assert.True(response.IsSuccessful);
}
```

### Multiple Assertions
```csharp
[Fact]
public async Task ProcessPayment_WithValidRequest_ReturnCompleteResponse()
{
	// Arrange
	var request = CreateTestPaymentRequest(orderId: "TEST-001", amount: 99.99m);

	// Act
	var response = await _paymentService.ProcessPaymentAsync(request);

	// Assert - Multiple related assertions
	Assert.NotNull(response);
	Assert.True(response.IsSuccessful);
	Assert.Equal("TEST-001", response.OrderId);
	Assert.Equal(99.99m, response.Amount);
	Assert.NotEmpty(response.TransactionId);
}
```

### Assert.All() for Collections
```csharp
[Fact]
public async Task ProcessMultiplePayments_AllSucceed()
{
	// Arrange
	var amounts = new[] { 10.00m, 25.00m, 50.00m };
	var responses = new List<PaymentResponse>();

	// Act
	foreach (var amount in amounts)
	{
		var request = CreateTestPaymentRequest(amount: amount);
		responses.Add(await _paymentService.ProcessPaymentAsync(request));
	}

	// Assert - All items must meet the condition
	Assert.All(responses, response =>
	{
		Assert.NotNull(response);
		Assert.True(response.IsSuccessful);
	});
}
```

## Async Testing

### Async Fact with Proper Await
```csharp
[Fact]
public async Task ProcessPayment_WithAsyncService_Completes()
{
	// ✅ DO: Use async/await properly
	var request = CreateTestPaymentRequest();
	var response = await _paymentService.ProcessPaymentAsync(request);
	Assert.NotNull(response);
}
```

### Async Initialization
```csharp
public class PaymentServiceTests : PaymentServiceTestBase
{
	private IPaymentService _paymentService;

	public override async Task InitializeAsync()
	{
		// ✅ DO: Call base async init
		await base.InitializeAsync();

		// ✅ DO: Perform async operations
		_paymentService = ServiceProvider.GetRequiredService<PaymentService>();
	}

	[Fact]
	public async Task TestMethod_RunsWithProperInitialization()
	{
		Assert.NotNull(_paymentService);
	}
}
```

### Task Vs Task<T>
```csharp
// ✅ DO: Return Task for async operations
public override async Task InitializeAsync()
{
	await SomeAsyncOperation();
	return Task.CompletedTask;
}

// ✅ DO: Return Task for async cleanup
public override async Task DisposeAsync()
{
	await SomeAsyncCleanup();
	return Task.CompletedTask;
}
```

## Theory Tests

### Theory with Inline Data
```csharp
[Theory]
[InlineData(10.00)]
[InlineData(50.00)]
[InlineData(100.00)]
public async Task ProcessPayment_WithVariousAmounts_AllSucceed(decimal amount)
{
	// Arrange
	var request = CreateTestPaymentRequest(amount: amount);

	// Act
	var response = await _paymentService.ProcessPaymentAsync(request);

	// Assert
	Assert.NotNull(response);
	Assert.Equal(amount, response.Amount);
}
```

### Theory with Member Data
```csharp
public static IEnumerable<object[]> ValidPaymentAmounts =>
	new List<object[]>
	{
		new object[] { 0.01m },
		new object[] { 50.00m },
		new object[] { 999.99m }
	};

[Theory]
[MemberData(nameof(ValidPaymentAmounts))]
public async Task ProcessPayment_WithVariousAmounts_Succeeds(decimal amount)
{
	var request = CreateTestPaymentRequest(amount: amount);
	var response = await _paymentService.ProcessPaymentAsync(request);
	Assert.NotNull(response);
}
```

### Theory with Class Data
```csharp
public class PaymentAmountData : IEnumerable<object[]>
{
	public IEnumerator<object[]> GetEnumerator()
	{
		yield return new object[] { 10.00m, "small" };
		yield return new object[] { 500.00m, "medium" };
		yield return new object[] { 5000.00m, "large" };
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		=> GetEnumerator();
}

[Theory]
[ClassData(typeof(PaymentAmountData))]
public async Task ProcessPayment_WithClassData_Succeeds(decimal amount, string size)
{
	var request = CreateTestPaymentRequest(amount: amount);
	var response = await _paymentService.ProcessPaymentAsync(request);
	Assert.NotNull(response);
}
```

## Fixture Patterns

### Implicit Fixture Injection
```csharp
public class PaymentServiceTests : PaymentServiceTestBase
{
	// Fixture from base class available through inheritance
	// Use in tests via protected properties

	[Fact]
	public void TestUsingFixture()
	{
		Assert.NotNull(ServiceProvider);
		Assert.NotNull(TestPaymentSettings);
	}
}
```

### Custom Fixture
```csharp
public class PaymentFixture : IAsyncLifetime
{
	public IPaymentService PaymentService { get; private set; }
	public PaymentSettings Settings { get; private set; }

	public async Task InitializeAsync()
	{
		var services = new ServiceCollection();
		services.AddHttpClient();
		services.AddLogging();

		Settings = new PaymentSettings { /* ... */ };
		services.Configure<PaymentSettings>(_ => Settings);
		services.AddScoped<PaymentService>();

		var provider = services.BuildServiceProvider();
		PaymentService = provider.GetRequiredService<PaymentService>();

		await Task.CompletedTask;
	}

	public Task DisposeAsync()
	{
		return Task.CompletedTask;
	}
}

[CollectionDefinition("Payment Collection")]
public class PaymentCollection : ICollectionFixture<PaymentFixture>
{
}

[Collection("Payment Collection")]
public class PaymentServiceTests
{
	private readonly PaymentFixture _fixture;

	public PaymentServiceTests(PaymentFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task TestWithCustomFixture()
	{
		var response = await _fixture.PaymentService.ProcessPaymentAsync(
			new PaymentRequest { Amount = 50.00m });
		Assert.NotNull(response);
	}
}
```

## Custom Assertions

### Extension Methods for Assertions
```csharp
public static class PaymentAssertions
{
	public static void AssertIsSuccessful(this PaymentResponse response)
	{
		Assert.NotNull(response);
		Assert.True(response.IsSuccessful, $"Payment failed: {response.ErrorMessage}");
	}

	public static void AssertHasTransaction(this PaymentResponse response)
	{
		response.AssertIsSuccessful();
		Assert.NotEmpty(response.TransactionId);
	}

	public static void AssertAmountEquals(this PaymentResponse response, decimal expectedAmount)
	{
		Assert.Equal(expectedAmount, response.Amount);
	}

	public static void AssertOrderIdEquals(this PaymentResponse response, string expectedOrderId)
	{
		Assert.Equal(expectedOrderId, response.OrderId);
	}

	public static void AssertRefundAmount(this PaymentResponse response, decimal refundAmount)
	{
		response.AssertIsSuccessful();
		Assert.True(response.Amount == refundAmount || response.Amount == -refundAmount);
	}
}

// Usage
[Fact]
public async Task ProcessPayment_WithCustomAssertions()
{
	var response = await _paymentService.ProcessPaymentAsync(request);

	response.AssertIsSuccessful();
	response.AssertHasTransaction();
	response.AssertAmountEquals(99.99m);
	response.AssertOrderIdEquals("TEST-001");
}
```

### Custom Assertion Class
```csharp
public class PaymentAssert
{
	private readonly PaymentResponse _response;

	public PaymentAssert(PaymentResponse response)
	{
		_response = response;
	}

	public PaymentAssert IsSuccessful()
	{
		Assert.True(_response.IsSuccessful, 
			$"Expected successful payment, but got error: {_response.ErrorMessage}");
		return this;
	}

	public PaymentAssert HasTransactionId()
	{
		Assert.NotEmpty(_response.TransactionId);
		return this;
	}

	public PaymentAssert AmountEquals(decimal expected)
	{
		Assert.Equal(expected, _response.Amount);
		return this;
	}

	public PaymentAssert OrderIdEquals(string expected)
	{
		Assert.Equal(expected, _response.OrderId);
		return this;
	}
}

// Fluent Usage
[Fact]
public async Task ProcessPayment_WithFluentAssertions()
{
	var response = await _paymentService.ProcessPaymentAsync(request);

	new PaymentAssert(response)
		.IsSuccessful()
		.HasTransactionId()
		.AmountEquals(99.99m)
		.OrderIdEquals("TEST-001");
}
```

## Error Handling

### Testing Exceptions
```csharp
[Fact]
public async Task ProcessPayment_WithNullRequest_ThrowsArgumentNullException()
{
	// Act & Assert
	await Assert.ThrowsAsync<ArgumentNullException>(
		() => _paymentService.ProcessPaymentAsync(null)
	);
}
```

### Testing Specific Exception Properties
```csharp
[Fact]
public async Task ProcessPayment_WithInvalidAmount_ThrowsArgumentException()
{
	// Act
	var exception = await Assert.ThrowsAsync<ArgumentException>(
		() => _paymentService.ProcessPaymentAsync(
			new PaymentRequest { Amount = -50.00m }
		)
	);

	// Assert
	Assert.Contains("Amount", exception.Message);
	Assert.Contains("negative", exception.Message);
}
```

### Testing Async Exceptions
```csharp
[Fact]
public async Task ProcessPayment_WhenTimeoutOccurs_ThrowsOperationCanceledException()
{
	// Arrange
	var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

	// Act & Assert
	await Assert.ThrowsAsync<OperationCanceledException>(
		() => _paymentService.ProcessPaymentAsync(request, cts.Token)
	);
}
```

## Best Practices

### ✅ DO: Clear Test Names
```csharp
// ✅ Clear and descriptive
[Fact]
public async Task ProcessPayment_WithValidRequest_ReturnsSuccessfulResponse()

// ❌ Vague
[Fact]
public async Task TestPayment()
```

### ✅ DO: Single Responsibility
```csharp
// ✅ Tests one thing
[Fact]
public async Task ProcessPayment_WithValidAmount_Succeeds()

// ❌ Tests multiple things
[Fact]
public async Task ProcessPayment_WithValidRequestAndRefundAndVoid_Works()
```

### ✅ DO: Arrange-Act-Assert
```csharp
[Fact]
public async Task MyTest()
{
	// Arrange - Setup
	var request = CreateTestPaymentRequest();

	// Act - Execute
	var response = await _paymentService.ProcessPaymentAsync(request);

	// Assert - Verify
	Assert.NotNull(response);
}
```

### ✅ DO: Async/Await Properly
```csharp
// ✅ Use async/await
[Fact]
public async Task TestAsync()
{
	var result = await SomeAsyncMethod();
	Assert.NotNull(result);
}

// ❌ Blocking
[Fact]
public void TestSync()
{
	var result = SomeAsyncMethod().Result; // BAD!
}
```

### ✅ DO: Meaningful Assertions
```csharp
// ✅ Specific and meaningful
Assert.Equal(99.99m, response.Amount);
Assert.True(response.IsSuccessful, $"Expected success but got: {response.ErrorMessage}");

// ❌ Vague
Assert.NotNull(response);
Assert.IsTrue(status);
```

### ✅ DO: Test Isolation
```csharp
// ✅ Each test is independent
[Fact]
public async Task Test1() { }

[Fact]
public async Task Test2() { }

// ❌ Dependent tests
[Fact]
public async Task Test1_CreatesResource() { }

[Fact]
public async Task Test2_UsesResourceFromTest1() { } // BAD!
```

### ✅ DO: Use Fixtures for Shared State
```csharp
// ✅ Shared setup via fixture
public class PaymentServiceTests : PaymentServiceTestBase
{
	public override async Task InitializeAsync()
	{
		await base.InitializeAsync();
		// Setup once for all tests in class
	}
}

// ❌ Repeated setup in each test
[Fact]
public async Task Test1()
{
	var services = new ServiceCollection();
	// Setup...
}

[Fact]
public async Task Test2()
{
	var services = new ServiceCollection();
	// Setup...
}
```

## Running Tests

### By Trait
```bash
# xUnit doesn't use [Category] but you can create custom attributes
[Trait("Category", "Payment")]
[Fact]
public async Task TestPayment() { }

# Run by trait
dotnet test --filter "@Trait=Category&Payment"
```

### By Name Pattern
```bash
# Run all tests containing "ProcessPayment"
dotnet test --filter "Name~ProcessPayment"

# Run all tests in a class
dotnet test --filter "FullyQualifiedName~PaymentServiceTests"
```

### With Output
```bash
# Verbose output
dotnet test -v d

# Include console output
dotnet test --no-build -- RunConfiguration.LogMessageFormat=json
```

## Performance Considerations

### Parallel Execution
```csharp
// xUnit runs tests in parallel by default within each assembly
// To disable parallelization:

[DisableParallelization]
public class SlowTests
{
	[Fact]
	public async Task SlowTest() { }
}

// Or configure in xunit.runner.json:
{
	"$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
	"parallelizeAssembly": false,
	"parallelizeTestCollections": false
}
```

### Timeout Tests
```csharp
[Fact(Timeout = 5000)] // 5 second timeout
public async Task ProcessPayment_WithTimeout()
{
	var response = await _paymentService.ProcessPaymentAsync(request);
	Assert.NotNull(response);
}
```

## Resources

- **xUnit Official Docs:** https://xunit.net/
- **Assertion Methods:** https://github.com/xunit/assert.xunit
- **Running Tests:** https://xunit.net/docs/configuration
- **Best Practices:** https://stackoverflow.com/questions/tagged/xunit.net

---

**Version:** 1.0
**Framework:** xUnit 2.6+
**Last Updated:** January 2024

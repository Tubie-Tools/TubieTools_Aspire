# xUnit Quick Reference Guide

## Test Attributes

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `[Fact]` | Basic test method | `[Fact] public async Task MyTest()` |
| `[Theory]` | Parameterized test | `[Theory] [InlineData(5)]` |
| `[InlineData(...)]` | Inline parameters | `[InlineData(10, 20)]` |
| `[MemberData(...)]` | Member property data | `[MemberData(nameof(TestData))]` |
| `[ClassData(...)]` | Class providing data | `[ClassData(typeof(MyData))]` |
| `[Trait(...)` | Test categorization | `[Trait("Category", "Payment")]` |
| `[Skip(...)]` | Skip test with reason | `[Skip("Not implemented")]` |
| `[Timeout(...)]` | Test timeout in ms | `[Fact(Timeout = 5000)]` |

## Fixture Lifecycle

```csharp
// Implement IAsyncLifetime
public abstract class PaymentServiceTestBase : IAsyncLifetime
{
	// Runs BEFORE each test
	public virtual Task InitializeAsync()
	{
		// Setup code here
		return Task.CompletedTask;
	}

	// Runs AFTER each test
	public virtual Task DisposeAsync()
	{
		// Cleanup code here
		return Task.CompletedTask;
	}
}
```

## Common Assertions

### Null/Empty
| Method | Usage |
|--------|-------|
| `Assert.Null(obj)` | Assert object is null |
| `Assert.NotNull(obj)` | Assert object is not null |
| `Assert.Empty(collection)` | Assert collection is empty |
| `Assert.NotEmpty(collection)` | Assert collection is not empty |
| `Assert.Empty(string)` | Assert string is empty |
| `Assert.NotEmpty(string)` | Assert string is not empty |

### Boolean
| Method | Usage |
|--------|-------|
| `Assert.True(condition)` | Assert condition is true |
| `Assert.False(condition)` | Assert condition is false |

### Equality
| Method | Usage |
|--------|-------|
| `Assert.Equal(expected, actual)` | Assert values are equal |
| `Assert.NotEqual(expected, actual)` | Assert values are not equal |
| `Assert.Same(obj1, obj2)` | Assert same reference |
| `Assert.NotSame(obj1, obj2)` | Assert different references |

### Collections
| Method | Usage |
|--------|-------|
| `Assert.All(collection, assert)` | Assert all items match |
| `Assert.Contains(item, collection)` | Assert item in collection |
| `Assert.DoesNotContain(item, collection)` | Assert item not in collection |
| `Assert.Single(collection)` | Assert exactly one item |
| `Assert.Empty(collection)` | Assert no items |

### Type & String
| Method | Usage |
|--------|-------|
| `Assert.IsType<T>(obj)` | Assert exact type |
| `Assert.IsAssignableFrom<T>(obj)` | Assert assignable type |
| `Assert.StartsWith(prefix, str)` | Assert string prefix |
| `Assert.EndsWith(suffix, str)` | Assert string suffix |
| `Assert.Contains(substring, str)` | Assert substring match |
| `Assert.Matches(pattern, str)` | Assert regex pattern |

### Numeric
| Method | Usage |
|--------|-------|
| `Assert.InRange(value, min, max)` | Assert value in range |
| `Assert.NotInRange(value, min, max)` | Assert value out of range |

### Exceptions
| Method | Usage |
|--------|-------|
| `Assert.Throws<T>(() => ...)` | Assert specific exception |
| `Assert.ThrowsAsync<T>(async () => ...)` | Assert async exception |
| `Assert.ThrowsAny<T>(() => ...)` | Assert exception or derived |

## Theory Test Patterns

### Inline Data
```csharp
[Theory]
[InlineData(5)]
[InlineData(10)]
[InlineData(15)]
public void Add_WithData(int value)
{
	// Test runs 3 times with different values
}
```

### Multiple Inline Values
```csharp
[Theory]
[InlineData(2, 4, 6)]
[InlineData(3, 5, 8)]
public void Add_WithMultipleData(int a, int b, int expected)
{
	Assert.Equal(expected, a + b);
}
```

### Member Data
```csharp
public static IEnumerable<object[]> TestData =>
	new List<object[]>
	{
		new object[] { 1, 2, 3 },
		new object[] { 4, 5, 9 }
	};

[Theory]
[MemberData(nameof(TestData))]
public void Calculate(int a, int b, int expected)
{
	Assert.Equal(expected, a + b);
}
```

## Async Testing

### Async Fact
```csharp
[Fact]
public async Task AsyncTest()
{
	var result = await SomeAsyncMethod();
	Assert.NotNull(result);
}
```

### Async Initialization
```csharp
public override async Task InitializeAsync()
{
	await base.InitializeAsync();
	await SomeAsyncSetup();
}
```

### Async Cleanup
```csharp
public override async Task DisposeAsync()
{
	await SomeAsyncCleanup();
	await base.DisposeAsync();
}
```

### Exception in Async
```csharp
[Fact]
public async Task AsyncMethod_ThrowsException()
{
	await Assert.ThrowsAsync<InvalidOperationException>(
		() => MethodThatThrows()
	);
}
```

## Running Tests

### Command Line
```bash
# All tests
dotnet test

# Specific class
dotnet test --filter "FullyQualifiedName~PaymentServiceTests"

# Specific test
dotnet test --filter "Name=ProcessPayment_WithValidRequest"

# By trait
dotnet test --filter "Trait=Category&Payment"

# Verbose
dotnet test -v d

# No build
dotnet test --no-build
```

### Test Explorer (Visual Studio)
1. Open Test > Test Explorer
2. Search for test name
3. Right-click and select "Run"

### Command Shortcuts
```bash
cd TubieTools_Aspire.Tests
dotnet test

# Watch mode (requires dotnet watch)
dotnet watch test

# With logging
dotnet test -- --reporter json
```

## NUnit to xUnit Migration Cheat Sheet

| NUnit | xUnit | Notes |
|-------|-------|-------|
| `[Test]` | `[Fact]` | Basic test |
| `[TestCase(...)]` | `[Theory] [InlineData(...)]` | Parameterized |
| `[TestFixture]` | (removed) | Class is fixture |
| `[SetUp]` | `InitializeAsync()` | Setup before test |
| `[TearDown]` | `DisposeAsync()` | Cleanup after test |
| `[Category]` | `[Trait(...)]` | Test category |
| `Assert.AreEqual()` | `Assert.Equal()` | Equality |
| `Assert.IsNull()` | `Assert.Null()` | Null check |
| `Assert.IsTrue()` | `Assert.True()` | Boolean |
| `Assert.Throws<T>()` | `Assert.Throws<T>()` | Exception (same) |

## Test Organization

```
PaymentIntegrations/
├── PaymentServiceTestBase.cs          (Base fixture)
├── AuthorizeNetPaymentServiceTests.cs (Specific tests)
├── PayPalPaymentServiceTests.cs
├── GooglePayPaymentServiceTests.cs
├── ApplePayPaymentServiceTests.cs
├── PaymentWebhookIntegrationTests.cs
├── README.md                          (Documentation)
├── XUNIT_MIGRATION.md                 (Migration guide)
└── XUNIT_PATTERNS.md                  (Pattern examples)
```

## File Template

```csharp
using Xunit;
using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TubieTools_Aspire.Tests.PaymentIntegrations;

/// <summary>
/// Description of test class
/// </summary>
public class MyPaymentServiceTests : PaymentServiceTestBase
{
	private IPaymentService _paymentService;

	public override async Task InitializeAsync()
	{
		await base.InitializeAsync();
		_paymentService = ServiceProvider.GetRequiredService<MyPaymentService>();
	}

	#region Section Name

	[Fact]
	public async Task MethodName_Condition_ExpectedResult()
	{
		// Arrange
		var request = CreateTestPaymentRequest();

		// Act
		var response = await _paymentService.ProcessPaymentAsync(request);

		// Assert
		Assert.NotNull(response);
	}

	#endregion
}
```

## Debugging Tests

### Visual Studio Debugger
1. Click on test line number to set breakpoint
2. Right-click test > "Debug"
3. Step through code

### VS Code Debugger
1. Set breakpoint (F9)
2. Open Debug menu
3. Select "Run and Debug"
4. Choose ".NET: Launch"

### Console Output
```csharp
// Use ITestOutputHelper
public class MyTests
{
	private readonly ITestOutputHelper _output;

	public MyTests(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public void Test()
	{
		_output.WriteLine("Debug message");
	}
}
```

## Performance Tips

### Parallel Execution
```csharp
// Run tests in parallel (default)
// Configure in xunit.runner.json to disable:
{
	"parallelizeAssembly": false,
	"parallelizeTestCollections": false
}
```

### Timeouts
```csharp
[Fact(Timeout = 5000)] // 5 seconds
public async Task SlowTest() { }
```

### Skip Slow Tests
```csharp
[Fact(Skip = "Slow integration test")]
public async Task SlowTest() { }
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Test not discovered | Ensure `[Fact]` attribute and public method |
| Async not working | Use `async Task` not `async void` |
| Fixture not initialized | Override `InitializeAsync()` and call `await base.InitializeAsync()` |
| Service not found | Register in `InitializeAsync()` via DI |
| Test times out | Increase timeout or check for blocking calls |
| Tests don't run in parallel | Check xunit.runner.json settings |

## References

- **xUnit Home:** https://xunit.net/
- **Assertions:** https://github.com/xunit/assert.xunit
- **Documentation:** https://xunit.net/docs/getting-started
- **GitHub:** https://github.com/xunit/xunit

---

**Version:** 1.0
**Framework:** xUnit 2.6+
**Last Updated:** January 2024

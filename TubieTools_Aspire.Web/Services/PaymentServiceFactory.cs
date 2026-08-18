using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TubieTools_Aspire.Web.Services;

/// <summary>
/// Enum representing supported payment methods
/// </summary>
public enum PaymentMethod
{
    AuthorizeNet,
    PayPal,
    GooglePay,
    ApplePay
}

/// <summary>
/// Factory for creating and managing payment service instances
/// </summary>
public interface IPaymentServiceFactory
{
    /// <summary>
    /// Get a payment service for the specified payment method
    /// </summary>
    IPaymentService GetPaymentService(PaymentMethod method);

    /// <summary>
    /// Get a payment service by string name (useful for API parameters)
    /// </summary>
    IPaymentService GetPaymentService(string paymentMethodName);
}

/// <summary>
/// Implementation of payment service factory
/// </summary>
public class PaymentServiceFactory : IPaymentServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentServiceFactory> _logger;
    private readonly Dictionary<PaymentMethod, Type> _paymentServiceMap;

    public PaymentServiceFactory(IServiceProvider serviceProvider, ILogger<PaymentServiceFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Map payment methods to their service implementations
        _paymentServiceMap = new Dictionary<PaymentMethod, Type>
        {
            { PaymentMethod.AuthorizeNet, typeof(PaymentService) },
            { PaymentMethod.PayPal, typeof(PayPalPaymentService) },
            { PaymentMethod.GooglePay, typeof(GooglePayPaymentService) },
            { PaymentMethod.ApplePay, typeof(ApplePayPaymentService) }
        };
    }

    /// <summary>
    /// Get a payment service for the specified payment method
    /// </summary>
    public IPaymentService GetPaymentService(PaymentMethod method)
    {
        if (!_paymentServiceMap.TryGetValue(method, out var serviceType))
        {
            _logger.LogWarning("Unsupported payment method: {PaymentMethod}", method);
            throw new NotSupportedException($"Payment method '{method}' is not supported");
        }

        try
        {
            return _serviceProvider.GetRequiredService(serviceType) as IPaymentService
                ?? throw new InvalidOperationException($"Failed to resolve {serviceType.Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment service for method {PaymentMethod}", method);
            throw;
        }
    }

    /// <summary>
    /// Get a payment service by string name
    /// </summary>
    public IPaymentService GetPaymentService(string paymentMethodName)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodName))
        {
            _logger.LogWarning("Empty payment method name provided");
            throw new ArgumentException("Payment method name cannot be null or empty", nameof(paymentMethodName));
        }

        // Try to parse the payment method name
        if (Enum.TryParse<PaymentMethod>(paymentMethodName, ignoreCase: true, out var method))
        {
            return GetPaymentService(method);
        }

        _logger.LogWarning("Invalid payment method name: {PaymentMethodName}", paymentMethodName);
        throw new ArgumentException($"Payment method '{paymentMethodName}' is not recognized", nameof(paymentMethodName));
    }
}

/// <summary>
/// Extension methods for dependency injection
/// </summary>
public static class PaymentServiceExtensions
{
    /// <summary>
    /// Register all payment services with the dependency injection container
    /// </summary>
    public static IServiceCollection AddPaymentServices(this IServiceCollection services)
    {
        // Register individual payment services
        services.AddScoped<PaymentService>();
        services.AddScoped<PayPalPaymentService>();
        services.AddScoped<GooglePayPaymentService>();
        services.AddScoped<ApplePayPaymentService>();

        // Register the factory
        services.AddScoped<IPaymentServiceFactory, PaymentServiceFactory>();

        return services;
    }

    /// <summary>
    /// Get payment service by name from DI container
    /// </summary>
    public static IPaymentService GetPaymentService(this IServiceProvider provider, PaymentMethod method)
    {
        return provider.GetRequiredService<IPaymentServiceFactory>().GetPaymentService(method);
    }

    /// <summary>
    /// Get payment service by string name from DI container
    /// </summary>
    public static IPaymentService GetPaymentService(this IServiceProvider provider, string paymentMethodName)
    {
        return provider.GetRequiredService<IPaymentServiceFactory>().GetPaymentService(paymentMethodName);
    }
}

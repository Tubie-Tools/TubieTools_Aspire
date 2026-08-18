using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TubieTools_Aspire.Web.Models;

namespace TubieTools_Aspire.Web.Services;

/// <summary>
/// Helper utility for validating and debugging payment system configuration
/// </summary>
public static class PaymentConfigurationValidator
{
    /// <summary>
    /// Validate payment settings and log any issues
    /// </summary>
    public static bool ValidatePaymentSettings(
        IServiceProvider serviceProvider,
        ILogger logger)
    {
        try
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PaymentSettings>>();
            var settings = options.Value;

            logger.LogInformation("=== Payment Configuration Validation ===");

            var isValid = true;

            // Check if payment processing is enabled
            if (!settings.Enabled)
            {
                logger.LogWarning("Payment processing is DISABLED");
                return false;
            }

            logger.LogInformation("Payment processing is ENABLED");

            // Validate Authorize.Net settings
            logger.LogInformation("Validating Authorize.Net configuration...");
            if (string.IsNullOrWhiteSpace(settings.AuthorizeNetApiLoginId))
            {
                logger.LogError("Missing: AuthorizeNetApiLoginId");
                isValid = false;
            }
            else
            {
                logger.LogInformation("✓ AuthorizeNetApiLoginId is configured");
            }

            if (string.IsNullOrWhiteSpace(settings.AuthorizeNetTransactionKey))
            {
                logger.LogError("Missing: AuthorizeNetTransactionKey");
                isValid = false;
            }
            else
            {
                logger.LogInformation("✓ AuthorizeNetTransactionKey is configured");
            }

            if (string.IsNullOrWhiteSpace(settings.AuthorizeNetSignatureKey))
            {
                logger.LogWarning("Missing: AuthorizeNetSignatureKey (required for webhooks)");
            }
            else
            {
                logger.LogInformation("✓ AuthorizeNetSignatureKey is configured");
            }

            // Validate environment setting
            if (string.IsNullOrWhiteSpace(settings.AuthorizeNetEnvironment))
            {
                logger.LogWarning("AuthorizeNetEnvironment not set, defaulting to 'sandbox'");
            }
            else
            {
                logger.LogInformation($"Environment: {settings.AuthorizeNetEnvironment}");

                if (!settings.AuthorizeNetEnvironment.Equals("sandbox", StringComparison.OrdinalIgnoreCase) &&
                    !settings.AuthorizeNetEnvironment.Equals("production", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogWarning("Invalid AuthorizeNetEnvironment value. Should be 'sandbox' or 'production'");
                    isValid = false;
                }
            }

            logger.LogInformation("=== Validation Complete ===");
            logger.LogInformation($"Configuration Status: {(isValid ? "✓ VALID" : "✗ INVALID")}");

            return isValid;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating payment settings");
            return false;
        }
    }

    /// <summary>
    /// Get a summary of payment configuration (safe for logging)
    /// </summary>
    public static string GetConfigurationSummary(IServiceProvider serviceProvider)
    {
        try
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PaymentSettings>>();
            var settings = options.Value;

            var summary = new System.Text.StringBuilder();
            summary.AppendLine("=== Payment Configuration Summary ===");
            summary.AppendLine($"Enabled: {settings.Enabled}");
            summary.AppendLine($"Environment: {settings.AuthorizeNetEnvironment}");
            summary.AppendLine($"API Login ID: {MaskSensitiveData(settings.AuthorizeNetApiLoginId)}");
            summary.AppendLine($"Transaction Key Configured: {!string.IsNullOrEmpty(settings.AuthorizeNetTransactionKey)}");
            summary.AppendLine($"Signature Key Configured: {!string.IsNullOrEmpty(settings.AuthorizeNetSignatureKey)}");
            summary.AppendLine("======================================");

            return summary.ToString();
        }
        catch (Exception ex)
        {
            return $"Error getting configuration summary: {ex.Message}";
        }
    }

    /// <summary>
    /// Mask sensitive data for safe logging
    /// </summary>
    private static string MaskSensitiveData(string data)
    {
        if (string.IsNullOrEmpty(data))
            return "[not configured]";

        if (data.Length <= 4)
            return "****";

        return data.Substring(0, 4) + "...";
    }

    /// <summary>
    /// Test payment gateway connectivity
    /// </summary>
    public static async Task<bool> TestPaymentGatewayConnectivityAsync(
        IHttpClientFactory httpClientFactory,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Testing payment gateway connectivity...");

            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            // Test Authorize.Net connectivity
            logger.LogInformation("Testing Authorize.Net sandbox connectivity...");
            try
            {
                var authNetResponse = await client.GetAsync(
                    "https://apitest.authorize.net/",
                    cancellationToken);

                if (authNetResponse.IsSuccessStatusCode || authNetResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    logger.LogInformation("✓ Authorize.Net sandbox is reachable");
                }
                else
                {
                    logger.LogWarning($"Authorize.Net returned status: {authNetResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to reach Authorize.Net sandbox");
                return false;
            }

            // Test PayPal connectivity
            logger.LogInformation("Testing PayPal sandbox connectivity...");
            try
            {
                var paypalResponse = await client.GetAsync(
                    "https://api.sandbox.paypal.com/",
                    cancellationToken);

                if (paypalResponse.IsSuccessStatusCode || paypalResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    logger.LogInformation("✓ PayPal sandbox is reachable");
                }
                else
                {
                    logger.LogWarning($"PayPal returned status: {paypalResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to reach PayPal sandbox");
            }

            logger.LogInformation("Connectivity tests complete");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error testing payment gateway connectivity");
            return false;
        }
    }

    /// <summary>
    /// Generate a test payment report
    /// </summary>
    public static void GeneratePaymentServiceReport(
        IServiceProvider serviceProvider,
        ILogger logger)
    {
        logger.LogInformation("=== Payment Service Report ===");

        try
        {
            var factory = serviceProvider.GetRequiredService<IPaymentServiceFactory>();

            logger.LogInformation("Available Payment Services:");

            var methods = new[] 
            { 
                PaymentMethod.AuthorizeNet,
                PaymentMethod.PayPal,
                PaymentMethod.GooglePay,
                PaymentMethod.ApplePay
            };

            foreach (var method in methods)
            {
                try
                {
                    var service = factory.GetPaymentService(method);
                    logger.LogInformation($"✓ {method}: {service.GetType().Name}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"✗ {method}: Failed to load - {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating payment service report");
        }

        logger.LogInformation("==============================");
    }
}

/// <summary>
/// Extension method for easy startup diagnostic
/// </summary>
public static class PaymentDiagnosticsExtensions
{
    /// <summary>
    /// Run payment system diagnostics on application startup
    /// </summary>
    public static IApplicationBuilder UsePaymentDiagnostics(
        this IApplicationBuilder app,
        bool enableDetailedLogging = false)
    {
        var serviceProvider = app.ApplicationServices;
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("PaymentDiagnostics");

        logger.LogInformation("Running Payment System Diagnostics...");

        // Validate configuration
        var isConfigValid = PaymentConfigurationValidator.ValidatePaymentSettings(
            serviceProvider,
            logger);

        if (!isConfigValid)
        {
            logger.LogWarning("⚠️ Payment system configuration issues detected!");
        }

        // Log configuration summary
        if (enableDetailedLogging)
        {
            var summary = PaymentConfigurationValidator.GetConfigurationSummary(serviceProvider);
            logger.LogInformation(summary);
        }

        // Generate service report
        PaymentConfigurationValidator.GeneratePaymentServiceReport(serviceProvider, logger);

        logger.LogInformation("Diagnostics Complete");

        return app;
    }

    /// <summary>
    /// Async diagnostic with connectivity test
    /// </summary>
    public static async Task UsePaymentDiagnosticsAsync(
        this IApplicationBuilder app,
        bool testConnectivity = true)
    {
        var serviceProvider = app.ApplicationServices;
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("PaymentDiagnostics");

        logger.LogInformation("Running Payment System Diagnostics...");

        // Validate configuration
        PaymentConfigurationValidator.ValidatePaymentSettings(serviceProvider, logger);

        // Test connectivity if requested
        if (testConnectivity)
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            await PaymentConfigurationValidator.TestPaymentGatewayConnectivityAsync(
                httpClientFactory,
                logger);
        }

        // Generate service report
        PaymentConfigurationValidator.GeneratePaymentServiceReport(serviceProvider, logger);

        logger.LogInformation("Diagnostics Complete");
    }
}

/// <summary>
/// Example usage in Program.cs:
/// 
/// var app = builder.Build();
/// 
/// // Run diagnostics before pipeline
/// if (app.Environment.IsDevelopment())
/// {
///     app.UsePaymentDiagnostics(enableDetailedLogging: true);
/// }
/// 
/// // Or async version with connectivity test
/// // await app.UsePaymentDiagnosticsAsync(testConnectivity: true);
/// 
/// // Configure the rest of the pipeline
/// app.UseHttpsRedirection();
/// // ... rest of configuration
/// </summary>

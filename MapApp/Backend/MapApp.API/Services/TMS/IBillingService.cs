using MapApp.API.Models.TMS;

namespace MapApp.API.Services.TMS;

/// <summary>
/// Code-to-Cash billing service
/// Manages the revenue recognition and billing process
/// </summary>
public interface IBillingService
{
    /// <summary>
    /// Calculate total revenue for shipment
    /// </summary>
    Task<decimal> CalculateTotalRevenueAsync(Shipment shipment);

    /// <summary>
    /// Generate billing record from completed shipment
    /// </summary>
    Task<ShipmentBillingRecord> GenerateBillingRecordAsync(Shipment shipment);

    /// <summary>
    /// Validate billing record accuracy
    /// </summary>
    Task<BillingValidation> ValidateBillingRecordAsync(BillingRecord record);

    /// <summary>
    /// Calculate revenue recognition for accounting
    /// </summary>
    Task<RevenueRecognition> CalculateRevenueRecognitionAsync(ShipmentBillingRecord record);

    /// <summary>
    /// Process payment and mark billing as complete
    /// </summary>
    Task<PaymentProcessResult> ProcessPaymentAsync(string billingId, decimal amountReceived);
}

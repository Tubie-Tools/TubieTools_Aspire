namespace ModelLayer.Models;

/// <summary>
/// Cost management policies.
/// </summary>
public class CostManagementPolicy
{
    /// <summary>Budget tracking enabled</summary>
    public bool BudgetTrackingEnabled { get; set; }

    /// <summary>Monthly budget limit</summary>
    public decimal? MonthlyBudgetLimit { get; set; }

    /// <summary>Cost alerts threshold (%)</summary>
    public decimal CostAlertThreshold { get; set; } = 80m;

    /// <summary>Cost allocation tags required</summary>
    public bool CostAllocationTagsRequired { get; set; }

    /// <summary>Rate limiting to control costs</summary>
    public Dictionary<string, int> RateLimits { get; set; } = new();

    /// <summary>Reserved capacity/commitments recommended</summary>
    public bool ReservedCapacityRecommended { get; set; }

    /// <summary>Cost optimization reviews</summary>
    public string CostOptimizationReviewFrequency { get; set; }

    /// <summary>Chargeback/showback model</summary>
    public string ChargebackModel { get; set; } // None, Chargeback, Showback
}

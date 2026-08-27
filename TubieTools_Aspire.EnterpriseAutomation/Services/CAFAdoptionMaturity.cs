namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

/// <summary>
/// CAF Adoption maturity across all phases
/// </summary>
public class CAFAdoptionMaturity
{
    public int StrategyMaturity { get; set; } // 0-100%
    public int PlanMaturity { get; set; }
    public int ReadyMaturity { get; set; }
    public int GovernMaturity { get; set; }
    public int SecureMaturity { get; set; }
    public int ManageMaturity { get; set; }
    public int OverallMaturity { get; set; } // Average
    public DateTime AssessmentDate { get; set; }
}

#endregion

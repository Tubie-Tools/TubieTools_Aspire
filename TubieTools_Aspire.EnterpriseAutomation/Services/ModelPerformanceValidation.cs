namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

/// <summary>
/// Model performance validation against success criteria
/// </summary>
public class ModelPerformanceValidation
{
    public string AgentId { get; set; }
    public bool PassedValidation { get; set; }
    public List<KPIValidationResult> KPIResults { get; set; } = new();
    public int PassedCriteria { get; set; }
    public int TotalCriteria { get; set; }
    public DateTime ValidationDate { get; set; }
}

#endregion

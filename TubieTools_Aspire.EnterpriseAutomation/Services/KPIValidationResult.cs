namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

public class KPIValidationResult
{
    public string KPIName { get; set; }
    public string TargetValue { get; set; }
    public string ActualValue { get; set; }
    public bool Met { get; set; }
    public string Unit { get; set; }
}

#endregion

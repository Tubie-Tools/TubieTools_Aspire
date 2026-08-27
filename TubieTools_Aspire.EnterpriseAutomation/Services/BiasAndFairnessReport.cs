namespace TubieTools_Aspire.EnterpriseAutomation.Services;

using TubieTools_Aspire.EnterpriseAutomation.Models;
#region Supporting Models for Service Operations

/// <summary>
/// Bias and fairness assessment report
/// </summary>
public class BiasAndFairnessReport
{
    public string AgentId { get; set; }
    public bool BiasesIdentified { get; set; }
    public List<string> IdentifiedBiases { get; set; } = new();
    public List<BiasItem> BiasDetails { get; set; } = new();
    public List<string> MitigationMeasures { get; set; } = new();
    public int FairnessScore { get; set; }
}

#endregion

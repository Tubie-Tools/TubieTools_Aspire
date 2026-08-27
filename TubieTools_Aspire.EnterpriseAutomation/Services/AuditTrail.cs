namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

/// <summary>
/// Audit trail export
/// </summary>
public class AuditTrail
{
    public string AgentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<AuditEvent> Events { get; set; } = new();
}

#endregion

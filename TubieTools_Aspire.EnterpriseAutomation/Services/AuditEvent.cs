namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

public class AuditEvent
{
    public DateTime EventTime { get; set; }
    public string EventType { get; set; }
    public string Actor { get; set; }
    public string Action { get; set; }
    public string ResourceAffected { get; set; }
    public string Details { get; set; }
}

#endregion

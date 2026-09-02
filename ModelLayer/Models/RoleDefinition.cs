namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

public class RoleDefinition
{
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public string Description { get; set; }
    public List<string> Permissions { get; set; } = new();
}

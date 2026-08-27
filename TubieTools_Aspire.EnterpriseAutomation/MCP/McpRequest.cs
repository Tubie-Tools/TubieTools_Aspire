namespace TubieTools_Aspire.EnterpriseAutomation.MCP;

public class McpRequest
{
    public string Method { get; set; }
    public string Resource { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
}

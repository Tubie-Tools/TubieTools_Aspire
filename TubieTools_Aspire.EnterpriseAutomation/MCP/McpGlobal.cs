namespace TubieTools_Aspire.EnterpriseAutomation.MCP;

public interface IMcpProtocolHandler
{
    Task<McpResponse> HandleRequestAsync(McpRequest request);
}

public class McpRequest
{
    public string Method { get; set; }
    public string Resource { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
}

public class McpResponse
{
    public bool Success { get; set; }
    public object Data { get; set; }
    public string Error { get; set; }
}
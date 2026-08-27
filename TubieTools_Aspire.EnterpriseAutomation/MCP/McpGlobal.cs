namespace TubieTools_Aspire.EnterpriseAutomation.MCP;

public interface IMcpProtocolHandler
{
    Task<McpResponse> HandleRequestAsync(McpRequest request);
}

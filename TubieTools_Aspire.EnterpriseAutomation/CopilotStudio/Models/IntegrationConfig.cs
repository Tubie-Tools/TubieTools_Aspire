namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Integration configuration for action tools.
/// </summary>
public class IntegrationConfig
{
    public string ConfigId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Integration framework (Power Platform, Logic Apps, Custom, etc.)</summary>
    public string Framework { get; set; }

    /// <summary>Endpoint/URL</summary>
    public string Endpoint { get; set; }

    /// <summary>HTTP method (GET, POST, PUT, DELETE, PATCH)</summary>
    public string HttpMethod { get; set; } = "POST";

    /// <summary>Authentication type</summary>
    public string AuthType { get; set; } // Bearer, APIKey, ManagedIdentity, BasicAuth, OAuth

    /// <summary>Authentication details (template with placeholders)</summary>
    public string AuthTemplate { get; set; }

    /// <summary>Request body template</summary>
    public string RequestTemplate { get; set; }

    /// <summary>Response mapping template</summary>
    public string ResponseTemplate { get; set; }

    /// <summary>Content type</summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>Required headers</summary>
    public Dictionary<string, string> RequiredHeaders { get; set; } = new();

    /// <summary>Rate limiting (requests per second)</summary>
    public decimal RateLimitPerSecond { get; set; } = 10;

    /// <summary>Connection test status</summary>
    public string ConnectionStatus { get; set; } = "Unknown"; // Connected, Failed, Disabled
}

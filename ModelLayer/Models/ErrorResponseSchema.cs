namespace ModelLayer.Models;

public class ErrorResponseSchema
{
    /// <summary>Error code field name</summary>
    public string ErrorCodeField { get; set; }

    /// <summary>Error message field name</summary>
    public string ErrorMessageField { get; set; }

    /// <summary>Common error codes and meanings</summary>
    public Dictionary<string, string> ErrorCodeMappings { get; set; } = new();
}

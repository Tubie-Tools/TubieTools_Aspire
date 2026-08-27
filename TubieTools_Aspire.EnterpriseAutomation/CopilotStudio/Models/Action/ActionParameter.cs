namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models.Action;

/// <summary>
/// Action parameter definition.
/// </summary>
public class ActionParameter
{
    public string ParamId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Parameter name</summary>
    public string Name { get; set; }

    /// <summary>Parameter description</summary>
    public string Description { get; set; }

    /// <summary>Data type</summary>
    public string DataType { get; set; } // string, integer, boolean, object, array

    /// <summary>Is required</summary>
    public bool IsRequired { get; set; }

    /// <summary>Default value</summary>
    public string DefaultValue { get; set; }

    /// <summary>Valid values/enum</summary>
    public List<string> ValidValues { get; set; } = new();

    /// <summary>Validation regex</summary>
    public string ValidationRegex { get; set; }

    /// <summary>Example value</summary>
    public string ExampleValue { get; set; }

    /// <summary>Parameter location in request (header, body, query, path)</summary>
    public string Location { get; set; } = "body";
}

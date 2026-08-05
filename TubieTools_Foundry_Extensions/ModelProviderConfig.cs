namespace TubieTools_Foundry_Extensions.Configuration;

public class ModelProviderConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public Dictionary<string, object> Options { get; set; } = [];
}
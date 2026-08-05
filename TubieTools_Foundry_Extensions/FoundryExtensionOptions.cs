namespace TubieTools_Foundry_Extensions.Configuration;

/// <summary>
/// Configuration options for the Foundry extension.
/// </summary>
public class FoundryExtensionOptions
{
    public const string SectionName = "FoundryExtension";

    public string RepositoryPath { get; set; } = "./models";
    public int MaxConcurrentModels { get; set; } = 5;
    public int ModelTimeoutSeconds { get; set; } = 300;
    public bool EnableCaching { get; set; } = true;
    public int CacheDurationMinutes { get; set; } = 60;
    public Dictionary<string, ModelProviderConfig> Providers { get; set; } = [];
}

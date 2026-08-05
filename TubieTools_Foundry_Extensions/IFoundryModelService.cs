namespace TubieTools_Foundry_Extensions.Services;

using TubieTools_Foundry_Extensions.Models;

/// <summary>
/// Service interface for managing custom AI models in Foundry.
/// </summary>
public interface IFoundryModelService
{
    Task<CustomAiModel> RegisterModelAsync(CustomAiModel model);
    Task<CustomAiModel?> GetModelAsync(string modelId);
    Task<IEnumerable<CustomAiModel>> ListModelsAsync();
    Task<IEnumerable<CustomAiModel>> ListActiveModelsAsync();
    Task UpdateModelAsync(string modelId, CustomAiModel updates);
    Task EnableModelAsync(string modelId);
    Task DisableModelAsync(string modelId);
    Task DeleteModelAsync(string modelId);
    Task<ModelCapability[]> GetModelCapabilitiesAsync(string modelId);
    Task ValidateModelAsync(string modelId);
}
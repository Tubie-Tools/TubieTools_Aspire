namespace TubieTools_Foundry_Extensions.Repository;

using TubieTools_Foundry_Extensions.Models;

/// <summary>
/// Repository interface for managing custom AI models.
/// </summary>
public interface IModelRepository
{
    Task<CustomAiModel?> GetModelByIdAsync(string modelId);
    Task<IEnumerable<CustomAiModel>> GetAllModelsAsync();
    Task<IEnumerable<CustomAiModel>> GetActiveModelsAsync();
    Task<CustomAiModel> CreateModelAsync(CustomAiModel model);
    Task UpdateModelAsync(CustomAiModel model);
    Task DeleteModelAsync(string modelId);
    Task<bool> ModelExistsAsync(string modelId);
    Task<IEnumerable<CustomAiModel>> GetModelsByProviderAsync(ModelProvider provider);
    Task<IEnumerable<CustomAiModel>> GetModelsByTypeAsync(string modelType);
}
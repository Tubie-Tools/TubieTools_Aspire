namespace TubieTools_Foundry_Extensions.Repository;

using TubieTools_Foundry_Extensions.Models;

/// <summary>
/// In-memory implementation of the model repository.
/// Replace with database-backed implementation as needed.
/// </summary>
public class InMemoryModelRepository : IModelRepository
{
    private readonly Dictionary<string, CustomAiModel> _models = [];

    public Task<CustomAiModel?> GetModelByIdAsync(string modelId)
    {
        _models.TryGetValue(modelId, out var model);
        return Task.FromResult(model);
    }

    public Task<IEnumerable<CustomAiModel>> GetAllModelsAsync()
    {
        return Task.FromResult(_models.Values.AsEnumerable());
    }

    public Task<IEnumerable<CustomAiModel>> GetActiveModelsAsync()
    {
        return Task.FromResult(_models.Values.Where(m => m.IsEnabled && m.Status == ModelStatus.Active).AsEnumerable());
    }

    public Task<CustomAiModel> CreateModelAsync(CustomAiModel model)
    {
        if (_models.ContainsKey(model.Id))
            throw new InvalidOperationException($"Model with ID {model.Id} already exists.");

        _models[model.Id] = model;
        return Task.FromResult(model);
    }

    public Task UpdateModelAsync(CustomAiModel model)
    {
        if (!_models.ContainsKey(model.Id))
            throw new KeyNotFoundException($"Model with ID {model.Id} not found.");

        model.UpdatedAt = DateTime.UtcNow;
        _models[model.Id] = model;
        return Task.CompletedTask;
    }

    public Task DeleteModelAsync(string modelId)
    {
        _models.Remove(modelId);
        return Task.CompletedTask;
    }

    public Task<bool> ModelExistsAsync(string modelId)
    {
        return Task.FromResult(_models.ContainsKey(modelId));
    }

    public Task<IEnumerable<CustomAiModel>> GetModelsByProviderAsync(ModelProvider provider)
    {
        return Task.FromResult(_models.Values.Where(m => m.Provider == provider).AsEnumerable());
    }

    public Task<IEnumerable<CustomAiModel>> GetModelsByTypeAsync(string modelType)
    {
        return Task.FromResult(_models.Values.Where(m => m.ModelType == modelType).AsEnumerable());
    }
}
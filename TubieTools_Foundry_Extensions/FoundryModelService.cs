using Microsoft.Extensions.Logging;
using TubieTools_Foundry_Extensions.Models;
using TubieTools_Foundry_Extensions.Repository;

namespace TubieTools_Foundry_Extensions.Services;

/// <summary>
/// Service for managing custom AI models in the Foundry extension.
/// </summary>
public class FoundryModelService : IFoundryModelService
{
    private readonly IModelRepository _repository;
    private readonly ILogger<FoundryModelService> _logger;

    public FoundryModelService(IModelRepository repository, ILogger<FoundryModelService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CustomAiModel> RegisterModelAsync(CustomAiModel model)
    {
        _logger.LogInformation("Registering AI model: {ModelName}", model.Name);

        if (string.IsNullOrWhiteSpace(model.Id))
            model.Id = Guid.NewGuid().ToString();

        model.CreatedAt = DateTime.UtcNow;
        model.Status = ModelStatus.Inactive;

        return await _repository.CreateModelAsync(model);
    }

    public async Task<CustomAiModel?> GetModelAsync(string modelId)
    {
        return await _repository.GetModelByIdAsync(modelId);
    }

    public async Task<IEnumerable<CustomAiModel>> ListModelsAsync()
    {
        return await _repository.GetAllModelsAsync();
    }

    public async Task<IEnumerable<CustomAiModel>> ListActiveModelsAsync()
    {
        return await _repository.GetActiveModelsAsync();
    }

    public async Task UpdateModelAsync(string modelId, CustomAiModel updates)
    {
        _logger.LogInformation("Updating AI model: {ModelId}", modelId);

        var existingModel = await _repository.GetModelByIdAsync(modelId);
        if (existingModel == null)
            throw new KeyNotFoundException($"Model {modelId} not found.");

        existingModel.Name = updates.Name;
        existingModel.Description = updates.Description;
        existingModel.Configuration = updates.Configuration;
        existingModel.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateModelAsync(existingModel);
    }

    public async Task EnableModelAsync(string modelId)
    {
        _logger.LogInformation("Enabling AI model: {ModelId}", modelId);

        var model = await _repository.GetModelByIdAsync(modelId);
        if (model == null)
            throw new KeyNotFoundException($"Model {modelId} not found.");

        model.IsEnabled = true;
        model.Status = ModelStatus.Active;
        await _repository.UpdateModelAsync(model);
    }

    public async Task DisableModelAsync(string modelId)
    {
        _logger.LogInformation("Disabling AI model: {ModelId}", modelId);

        var model = await _repository.GetModelByIdAsync(modelId);
        if (model == null)
            throw new KeyNotFoundException($"Model {modelId} not found.");

        model.IsEnabled = false;
        model.Status = ModelStatus.Inactive;
        await _repository.UpdateModelAsync(model);
    }

    public async Task DeleteModelAsync(string modelId)
    {
        _logger.LogInformation("Deleting AI model: {ModelId}", modelId);
        await _repository.DeleteModelAsync(modelId);
    }

    public Task<ModelCapability[]> GetModelCapabilitiesAsync(string modelId)
    {
        // TODO: Implement capability extraction based on model metadata
        return Task.FromResult(Array.Empty<ModelCapability>());
    }

    public async Task ValidateModelAsync(string modelId)
    {
        _logger.LogInformation("Validating AI model: {ModelId}", modelId);

        var model = await _repository.GetModelByIdAsync(modelId);
        if (model == null)
            throw new KeyNotFoundException($"Model {modelId} not found.");

        // Validate model structure and configuration
        if (string.IsNullOrWhiteSpace(model.Name))
            throw new InvalidOperationException("Model name is required.");

        if (model.Configuration == null || model.Configuration.Count == 0)
            throw new InvalidOperationException("Model configuration is required.");

        _logger.LogInformation("Model {ModelId} validation passed.", modelId);
    }
}
using TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;
using TubieTools_CopilotStudio_API.Data.Repositories;
using TubieTools_CopilotStudio_API.Services.DTOs;

namespace TubieTools_CopilotStudio_API.Services;

/// <summary>
/// Service for Copilot Application business logic.
/// </summary>
public interface ICopilotApplicationService
{
    Task<CopilotApplicationDto> CreateAsync(CreateCopilotRequest request, CancellationToken cancellationToken = default);
    Task<CopilotApplicationDto?> GetByIdAsync(string copilotId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CopilotApplicationDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CopilotApplicationDto>> GetByLandingZoneAsync(string landingZone, CancellationToken cancellationToken = default);
    Task<CopilotApplicationDto> UpdateAsync(string copilotId, UpdateCopilotRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string copilotId, CancellationToken cancellationToken = default);
}

public class CopilotApplicationService : ICopilotApplicationService
{
    private readonly ICopilotApplicationRepository _repository;
    private readonly ILogger<CopilotApplicationService> _logger;

    public CopilotApplicationService(
        ICopilotApplicationRepository repository,
        ILogger<CopilotApplicationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CopilotApplicationDto> CreateAsync(CreateCopilotRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new copilot: {Name}", request.Name);

        var copilot = new CopilotApplication
        {
            CopilotId = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            BusinessObjective = request.BusinessObjective,
            LandingZone = request.LandingZone,
            Owner = request.Owner,
            ContactEmail = request.ContactEmail,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(copilot, cancellationToken);
        return MapToDto(created);
    }

    public async Task<CopilotApplicationDto?> GetByIdAsync(string copilotId, CancellationToken cancellationToken = default)
    {
        var copilot = await _repository.GetByIdAsync(copilotId, cancellationToken);
        return copilot != null ? MapToDto(copilot) : null;
    }

    public async Task<IEnumerable<CopilotApplicationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var copilots = await _repository.GetAllAsync(cancellationToken);
        return copilots.Select(MapToDto);
    }

    public async Task<IEnumerable<CopilotApplicationDto>> GetByLandingZoneAsync(string landingZone, CancellationToken cancellationToken = default)
    {
        var copilots = await _repository.GetByLandingZoneAsync(landingZone, cancellationToken);
        return copilots.Select(MapToDto);
    }

    public async Task<CopilotApplicationDto> UpdateAsync(string copilotId, UpdateCopilotRequest request, CancellationToken cancellationToken = default)
    {
        var copilot = await _repository.GetByIdAsync(copilotId, cancellationToken)
            ?? throw new KeyNotFoundException($"Copilot not found: {copilotId}");

        if (!string.IsNullOrWhiteSpace(request.Name))
            copilot.Name = request.Name;
        if (!string.IsNullOrWhiteSpace(request.Description))
            copilot.Description = request.Description;
        if (!string.IsNullOrWhiteSpace(request.BusinessObjective))
            copilot.BusinessObjective = request.BusinessObjective;

        copilot.LastModifiedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(copilot, cancellationToken);
        return MapToDto(copilot);
    }

    public async Task DeleteAsync(string copilotId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting copilot: {CopilotId}", copilotId);
        await _repository.DeleteAsync(copilotId, cancellationToken);
    }

    private static CopilotApplicationDto MapToDto(CopilotApplication copilot)
    {
        return new CopilotApplicationDto(
            CopilotId: copilot.CopilotId,
            Name: copilot.Name ?? string.Empty,
            Description: copilot.Description,
            BusinessObjective: copilot.BusinessObjective,
            LandingZone: copilot.LandingZone ?? string.Empty,
            Owner: copilot.Owner,
            ContactEmail: copilot.ContactEmail,
            CurrentVersion: copilot.CurrentVersion ?? "1.0.0",
            IsActive: copilot.IsActive,
            CreatedDate: copilot.CreatedDate,
            LastModifiedDate: copilot.LastModifiedDate);
    }
}

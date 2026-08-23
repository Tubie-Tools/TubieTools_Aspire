using System;

namespace TubieTools_CopilotStudio_API.Services.DTOs;

/// <summary>
/// DTO for Copilot Application responses.
/// </summary>
public record CopilotApplicationDto(
    string CopilotId,
    string Name,
    string? Description,
    string? BusinessObjective,
    string LandingZone,
    string? Owner,
    string? ContactEmail,
    string CurrentVersion,
    bool IsActive,
    DateTime CreatedDate,
    DateTime LastModifiedDate);

/// <summary>
/// Request DTO for creating a new Copilot Application.
/// </summary>
public record CreateCopilotRequest(
    string Name,
    string? Description,
    string? BusinessObjective,
    string LandingZone,
    string? Owner,
    string? ContactEmail);

/// <summary>
/// Request DTO for updating an existing Copilot Application.
/// </summary>
public record UpdateCopilotRequest(
    string? Name,
    string? Description,
    string? BusinessObjective);

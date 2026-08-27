namespace TubieTools_Aspire.EnterpriseAutomation.Services;

/// <summary>
/// Interface for CAF Adoption Phase Tracking
/// </summary>
public interface ICAFAdoptionPhaseService
{
    /// <summary>
    /// Tracks organization's adoption maturity across all CAF phases
    /// </summary>
    Task<CAFAdoptionMaturity> GetAdoptionMaturityAsync();

    /// <summary>
    /// Records progress in a specific adoption phase
    /// </summary>
    Task<bool> RecordAdoptionProgressAsync(string phase, string initiative, int completionPercentage);

    /// <summary>
    /// Generates adoption roadmap
    /// </summary>
    Task<AdoptionRoadmap> GenerateAdoptionRoadmapAsync();

    /// <summary>
    /// Identifies gaps and blockers in adoption
    /// </summary>
    Task<List<AdoptionBlocker>> IdentifyAdoptionGapsAsync();
}
namespace TubieTools_Aspire.EnterpriseAutomation.Services;

/// <summary>
/// Interface for AI Agent Audit and Compliance Reporting
/// </summary>
public interface IAIAgentAuditService
{
    /// <summary>
    /// Generates comprehensive audit report for an agent
    /// </summary>
    Task<AuditReport> GenerateAuditReportAsync(string agentId);

    /// <summary>
    /// Generates compliance report for regulatory requirements
    /// </summary>
    Task<RegulatoryComplianceReport> GenerateComplianceReportAsync(string agentId, string regulationName);

    /// <summary>
    /// Generates security assessment report
    /// </summary>
    Task<SecurityAssessmentReport> GenerateSecurityAssessmentAsync(string agentId);

    /// <summary>
    /// Generates bias and fairness report
    /// </summary>
    Task<BiasAndFairnessReport> GenerateBiasAndFairnessReportAsync(string agentId);

    /// <summary>
    /// Exports audit trail data
    /// </summary>
    Task<AuditTrail> ExportAuditTrailAsync(string agentId, DateTime startDate, DateTime endDate);
}
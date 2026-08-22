namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Copilot governance policy aligned with landing zones and enterprise standards.
/// </summary>
public class CopilotGovernancePolicy
{
    public string PolicyId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Landing zone this policy applies to</summary>
    public string LandingZone { get; set; }

    /// <summary>Policy name</summary>
    public string PolicyName { get; set; }

    /// <summary>Policy description</summary>
    public string Description { get; set; }

    /// <summary>Data residency requirements</summary>
    public DataResidencyRequirements DataResidency { get; set; }

    /// <summary>Security requirements</summary>
    public SecurityRequirements SecurityRequirements { get; set; }

    /// <summary>Compliance requirements</summary>
    public List<ComplianceRequirementPolicy> ComplianceRequirements { get; set; } = new();

    /// <summary>Data handling policies</summary>
    public DataHandlingPolicy DataHandling { get; set; }

    /// <summary>Model and AI governance</summary>
    public ModelGovernance ModelGovernance { get; set; }

    /// <summary>Audit and logging requirements</summary>
    public AuditRequirements AuditRequirements { get; set; }

    /// <summary>Cost management policies</summary>
    public CostManagementPolicy CostManagement { get; set; }

    /// <summary>Escalation and incident response</summary>
    public IncidentResponsePolicy IncidentResponse { get; set; }

    /// <summary>Policy enforcement mode (Strict, Moderate, Advisory)</summary>
    public string EnforcementMode { get; set; } = "Strict";

    /// <summary>Attestation requirement</summary>
    public bool RequiresAttestation { get; set; }

    /// <summary>Last review date</summary>
    public DateTime LastReviewDate { get; set; }

    /// <summary>Next review date</summary>
    public DateTime NextReviewDate { get; set; }
}

/// <summary>
/// Data residency requirements by landing zone.
/// </summary>
public class DataResidencyRequirements
{
    /// <summary>Allowed geographic regions</summary>
    public List<string> AllowedRegions { get; set; } = new();

    /// <summary>Data must remain in country/region</summary>
    public bool DataLocalizationRequired { get; set; }

    /// <summary>Approved data centers</summary>
    public List<string> ApprovedDataCenters { get; set; } = new();

    /// <summary>Backup region requirements</summary>
    public string BackupRegionRequirement { get; set; }

    /// <summary>Latency SLA (milliseconds)</summary>
    public int? LatencySLAMs { get; set; }

    /// <summary>Disaster recovery region restrictions</summary>
    public string DRRegionRestriction { get; set; }
}

/// <summary>
/// Security requirements for the copilot.
/// </summary>
public class SecurityRequirements
{
    /// <summary>Encryption in transit (TLS version minimum)</summary>
    public string EncryptionInTransit { get; set; } = "TLS 1.2";

    /// <summary>Encryption at rest required</summary>
    public bool EncryptionAtRestRequired { get; set; }

    /// <summary>Encryption key management requirements</summary>
    public string KeyManagementService { get; set; } // "Managed", "BYOK", "BYOZK"

    /// <summary>Authentication requirements for users</summary>
    public string UserAuthenticationMethod { get; set; } // MFA, SAML, AD, OAuth

    /// <summary>Multi-factor authentication required</summary>
    public bool MFARequired { get; set; }

    /// <summary>Session timeout minutes</summary>
    public int? SessionTimeoutMinutes { get; set; }

    /// <summary>IP allowlist/blocklist required</summary>
    public bool IPRestrictionRequired { get; set; }

    /// <summary>Allowed IP ranges</summary>
    public List<string> AllowedIPRanges { get; set; } = new();

    /// <summary>VPC/Network requirements</summary>
    public string NetworkIsolationRequirement { get; set; } // "Public", "Private", "VPCOnly"

    /// <summary>Secret management solution required</summary>
    public string SecretManagement { get; set; } // "KeyVault", "SecretsManager", "Custom"

    /// <summary>Vulnerability scanning required</summary>
    public bool VulnerabilityScanningRequired { get; set; }

    /// <summary>Scanning frequency</summary>
    public string ScanningFrequency { get; set; } // "Continuous", "Daily", "Weekly"

    /// <summary>Penetration testing required</summary>
    public bool PenetrationTestingRequired { get; set; }

    /// <summary>Third-party security assessment</summary>
    public bool ThirdPartyAssessmentRequired { get; set; }
}

/// <summary>
/// Compliance requirement for specific regulation.
/// </summary>
public class ComplianceRequirementPolicy
{
    public string RequirementId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Regulation name</summary>
    public string RegulationName { get; set; } // GDPR, HIPAA, SOC2, NIST, etc.

    /// <summary>Jurisdiction</summary>
    public string Jurisdiction { get; set; }

    /// <summary>Specific articles/requirements</summary>
    public List<string> SpecificRequirements { get; set; } = new();

    /// <summary>Required controls</summary>
    public List<string> RequiredControls { get; set; } = new();

    /// <summary>Compliance certification needed</summary>
    public string CertificationNeeded { get; set; }

    /// <summary>Audit frequency</summary>
    public string AuditFrequency { get; set; } // Annual, Semi-Annual, Quarterly

    /// <summary>Last certification date</summary>
    public DateTime? LastCertificationDate { get; set; }

    /// <summary>Certification expiry date</summary>
    public DateTime? CertificationExpiryDate { get; set; }
}

/// <summary>
/// Data handling and privacy policies.
/// </summary>
public class DataHandlingPolicy
{
    /// <summary>PII/Personal data handling</summary>
    public PIIHandlingRequirements PIIHandling { get; set; }

    /// <summary>Data classification levels allowed</summary>
    public List<string> AllowedDataClassifications { get; set; } = new();

    /// <summary>Sensitive data masking required</summary>
    public bool SensitiveDataMaskingRequired { get; set; }

    /// <summary>Data anonymization requirement</summary>
    public string AnonymizationRequirement { get; set; } // Required, Recommended, Optional

    /// <summary>Retention policies</summary>
    public DataRetentionPolicy RetentionPolicy { get; set; }

    /// <summary>Right to be forgotten implementation</summary>
    public bool SupportsRightToBeForgotten { get; set; }

    /// <summary>Data lineage tracking required</summary>
    public bool DataLineageTrackingRequired { get; set; }

    /// <summary>Third-party data sharing restrictions</summary>
    public List<string> ThirdPartyRestrictions { get; set; } = new();
}

/// <summary>
/// PII handling requirements.
/// </summary>
public class PIIHandlingRequirements
{
    /// <summary>Can process PII</summary>
    public bool CanProcessPII { get; set; }

    /// <summary>Allowed PII types</summary>
    public List<string> AllowedPIITypes { get; set; } = new();

    /// <summary>Masking strategy</summary>
    public string MaskingStrategy { get; set; } // Tokenization, Pseudonymization, Encryption, Redaction

    /// <summary>Encryption for PII</summary>
    public bool EncryptPII { get; set; }

    /// <summary>PII access logging</summary>
    public bool LogPIIAccess { get; set; }

    /// <summary>Consent management required</summary>
    public bool ConsentRequired { get; set; }

    /// <summary>Consent tracking and storage</summary>
    public string ConsentStorage { get; set; }
}

/// <summary>
/// Data retention policies.
/// </summary>
public class DataRetentionPolicy
{
    /// <summary>Default retention period (days)</summary>
    public int DefaultRetentionDays { get; set; }

    /// <summary>Maximum retention period (days)</summary>
    public int MaxRetentionDays { get; set; }

    /// <summary>Archive after N days</summary>
    public int ArchiveAfterDays { get; set; }

    /// <summary>Deletion method (Soft, Hard, Shred)</summary>
    public string DeletionMethod { get; set; }

    /// <summary>Deletion confirmation required</summary>
    public bool DeletionConfirmationRequired { get; set; }

    /// <summary>Backup retention</summary>
    public int BackupRetentionDays { get; set; }
}

/// <summary>
/// Model governance for AI components.
/// </summary>
public class ModelGovernance
{
    /// <summary>Approved model providers</summary>
    public List<string> ApprovedProviders { get; set; } = new();

    /// <summary>Approved model names/versions</summary>
    public List<string> ApprovedModels { get; set; } = new();

    /// <summary>Custom models allowed</summary>
    public bool CustomModelsAllowed { get; set; }

    /// <summary>Fine-tuning allowed</summary>
    public bool FineTuningAllowed { get; set; }

    /// <summary>Model training data sourcing requirements</summary>
    public string TrainingDataSourceRequirements { get; set; }

    /// <summary>Bias assessment required</summary>
    public bool BiasAssessmentRequired { get; set; }

    /// <summary>Fairness testing required</summary>
    public bool FairnessTestingRequired { get; set; }

    /// <summary>Explainability requirement</summary>
    public bool ExplainabilityRequired { get; set; }

    /// <summary>Regular model performance monitoring</summary>
    public string PerformanceMonitoringFrequency { get; set; } // Daily, Weekly, Monthly

    /// <summary>Model drift detection required</summary>
    public bool DriftDetectionRequired { get; set; }

    /// <summary>Model retraining SLA</summary>
    public string RetrainingAgreement { get; set; }

    /// <summary>Model versioning required</summary>
    public bool VersioningRequired { get; set; }

    /// <summary>Model rollback capability required</summary>
    public bool RollbackCapabilityRequired { get; set; }

    /// <summary>A/B testing for model updates</summary>
    public bool ABTestingRequired { get; set; }
}

/// <summary>
/// Audit and logging requirements.
/// </summary>
public class AuditRequirements
{
    /// <summary>Audit logging enabled</summary>
    public bool AuditLoggingEnabled { get; set; } = true;

    /// <summary>Events to log (Execution, Configuration, Access, etc.)</summary>
    public List<string> LoggedEvents { get; set; } = new();

    /// <summary>Audit log retention period (days)</summary>
    public int AuditLogRetentionDays { get; set; } = 90;

    /// <summary>Real-time log export to SIEM</summary>
    public bool RealTimeLogExport { get; set; }

    /// <summary>SIEM tool name</summary>
    public string SIEMTool { get; set; }

    /// <summary>Log immutability required</summary>
    public bool LogImmutabilityRequired { get; set; }

    /// <summary>Chain of custody for audit logs</summary>
    public bool ChainOfCustodyRequired { get; set; }

    /// <summary>Regular audit reviews</summary>
    public string AuditReviewFrequency { get; set; } // Weekly, Monthly, Quarterly

    /// <summary>Audit trail for tool modifications</summary>
    public bool ToolModificationAuditRequired { get; set; }

    /// <summary>User action audit trail</summary>
    public bool UserActionAuditRequired { get; set; }

    /// <summary>Data access audit trail</summary>
    public bool DataAccessAuditRequired { get; set; }
}

/// <summary>
/// Cost management policies.
/// </summary>
public class CostManagementPolicy
{
    /// <summary>Budget tracking enabled</summary>
    public bool BudgetTrackingEnabled { get; set; }

    /// <summary>Monthly budget limit</summary>
    public decimal? MonthlyBudgetLimit { get; set; }

    /// <summary>Cost alerts threshold (%)</summary>
    public decimal CostAlertThreshold { get; set; } = 80m;

    /// <summary>Cost allocation tags required</summary>
    public bool CostAllocationTagsRequired { get; set; }

    /// <summary>Rate limiting to control costs</summary>
    public Dictionary<string, int> RateLimits { get; set; } = new();

    /// <summary>Reserved capacity/commitments recommended</summary>
    public bool ReservedCapacityRecommended { get; set; }

    /// <summary>Cost optimization reviews</summary>
    public string CostOptimizationReviewFrequency { get; set; }

    /// <summary>Chargeback/showback model</summary>
    public string ChargebackModel { get; set; } // None, Chargeback, Showback
}

/// <summary>
/// Incident response and escalation policy.
/// </summary>
public class IncidentResponsePolicy
{
    /// <summary>Incident severity levels</summary>
    public List<SeverityLevel> SeverityLevels { get; set; } = new();

    /// <summary>Escalation contacts by severity</summary>
    public Dictionary<string, List<string>> EscalationContacts { get; set; } = new();

    /// <summary>Response time SLA by severity (minutes)</summary>
    public Dictionary<string, int> ResponseTimeSLAs { get; set; } = new();

    /// <summary>Resolution time SLA by severity (hours)</summary>
    public Dictionary<string, int> ResolutionTimeSLAs { get; set; } = new();

    /// <summary>Incident communication template</summary>
    public string CommunicationTemplate { get; set; }

    /// <summary>Post-incident review required</summary>
    public bool PostIncidentReviewRequired { get; set; }

    /// <summary>RCA (Root Cause Analysis) report timing</summary>
    public string RCAReportTiming { get; set; } // Same-Day, NextDay, Within3Days

    /// <summary>Incident tracking system</summary>
    public string IncidentTrackingSystem { get; set; }
}

public class SeverityLevel
{
    public string Level { get; set; } // Critical, High, Medium, Low
    public string Description { get; set; }
    public int ResponseTimeMinutes { get; set; }
}

/// <summary>
/// Landing zone configuration and guardrails.
/// </summary>
public class LandingZoneConfiguration
{
    public string LandingZoneId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Landing zone type</summary>
    public string LandingZoneType { get; set; }

    /// <summary>Landing zone name</summary>
    public string Name { get; set; }

    /// <summary>Landing zone description</summary>
    public string Description { get; set; }

    /// <summary>Business classification</summary>
    public string BusinessClassification { get; set; }

    /// <summary>Data classification level</summary>
    public string DataClassificationLevel { get; set; } // Public, Internal, Confidential, Restricted

    /// <summary>Regulatory requirements applicable</summary>
    public List<string> ApplicableRegulations { get; set; } = new();

    /// <summary>Governance policy</summary>
    public CopilotGovernancePolicy GovernancePolicy { get; set; }

    /// <summary>Network configuration</summary>
    public NetworkConfiguration NetworkConfig { get; set; }

    /// <summary>Identity and access management</summary>
    public IAMConfiguration IAMConfig { get; set; }

    /// <summary>Storage configuration</summary>
    public StorageConfiguration StorageConfig { get; set; }

    /// <summary>Monitoring and logging</summary>
    public MonitoringConfiguration MonitoringConfig { get; set; }

    /// <summary>Disaster recovery and backup</summary>
    public DRConfiguration DRConfig { get; set; }

    /// <summary>Capacity and scaling</summary>
    public CapacityConfiguration CapacityConfig { get; set; }

    /// <summary>Approved services/tools list</summary>
    public List<ApprovedService> ApprovedServices { get; set; } = new();

    /// <summary>Blocked services/tools list</summary>
    public List<string> BlockedServices { get; set; } = new();

    /// <summary>Cost budget for zone</summary>
    public decimal? MonthlyBudget { get; set; }

    /// <summary>Environment type</summary>
    public string EnvironmentType { get; set; } // Development, Testing, Staging, Production

    /// <summary>Created date</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Network configuration for landing zone.
/// </summary>
public class NetworkConfiguration
{
    /// <summary>Virtual network/VPC ID</summary>
    public string VirtualNetworkId { get; set; }

    /// <summary>Subnets</summary>
    public List<string> Subnets { get; set; } = new();

    /// <summary>Network security groups/firewall rules</summary>
    public List<NetworkRule> SecurityRules { get; set; } = new();

    /// <summary>DDoS protection enabled</summary>
    public bool DDoSProtectionEnabled { get; set; }

    /// <summary>WAF (Web Application Firewall) enabled</summary>
    public bool WAFEnabled { get; set; }

    /// <summary>VPN/ExpressRoute required</summary>
    public bool VPNRequired { get; set; }

    /// <summary>Egress filtering/proxy required</summary>
    public bool EgressFilteringRequired { get; set; }
}

public class NetworkRule
{
    public string Direction { get; set; } // Inbound, Outbound
    public string Protocol { get; set; } // TCP, UDP, ICMP
    public string SourceAddress { get; set; }
    public int? SourcePort { get; set; }
    public string DestinationAddress { get; set; }
    public int? DestinationPort { get; set; }
    public string Action { get; set; } // Allow, Deny
    public int Priority { get; set; }
}

/// <summary>
/// Identity and Access Management configuration.
/// </summary>
public class IAMConfiguration
{
    /// <summary>Identity provider (Azure AD, Okta, etc.)</summary>
    public string IdentityProvider { get; set; }

    /// <summary>RBAC enabled</summary>
    public bool RBACEnabled { get; set; }

    /// <summary>Predefined roles</summary>
    public List<RoleDefinition> Roles { get; set; } = new();

    /// <summary>Service principal required</summary>
    public bool ServicePrincipalRequired { get; set; }

    /// <summary>Managed identity preferred</summary>
    public bool ManagedIdentityPreferred { get; set; }

    /// <summary>MFA enforcement</summary>
    public bool MFAEnforced { get; set; }

    /// <summary>Conditional access policies</summary>
    public List<string> ConditionalAccessPolicies { get; set; } = new();

    /// <summary>Privileged access management</summary>
    public bool PAMEnabled { get; set; }

    /// <summary>Session recording for sensitive roles</summary>
    public bool SessionRecordingEnabled { get; set; }
}

public class RoleDefinition
{
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public string Description { get; set; }
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// Storage configuration for landing zone.
/// </summary>
public class StorageConfiguration
{
    /// <summary>Storage account type</summary>
    public string StorageType { get; set; } // Azure, AWS, GCP, On-Premises

    /// <summary>Encryption type</summary>
    public string EncryptionType { get; set; } // ServiceManaged, CustomerManagedKey, BYOK

    /// <summary>Replication strategy</summary>
    public string ReplicationStrategy { get; set; } // LRS, ZRS, GRS, GZRS

    /// <summary>Retention policies</summary>
    public List<string> RetentionPolicies { get; set; } = new();

    /// <summary>Backup configuration</summary>
    public BackupConfiguration BackupConfig { get; set; }

    /// <summary>Data classification by storage tier</summary>
    public Dictionary<string, string> DataClassificationMapping { get; set; } = new();
}

public class BackupConfiguration
{
    public bool BackupEnabled { get; set; }
    public string BackupFrequency { get; set; }
    public int RetentionDays { get; set; }
    public string BackupStorage { get; set; }
}

/// <summary>
/// Monitoring and logging configuration.
/// </summary>
public class MonitoringConfiguration
{
    /// <summary>Monitoring platform</summary>
    public string MonitoringPlatform { get; set; }

    /// <summary>Log aggregation enabled</summary>
    public bool LogAggregationEnabled { get; set; }

    /// <summary>Metrics collection frequency</summary>
    public string MetricsFrequency { get; set; }

    /// <summary>Alert thresholds</summary>
    public List<AlertThreshold> AlertThresholds { get; set; } = new();

    /// <summary>Incident response automation</summary>
    public bool IncidentAutomationEnabled { get; set; }
}

public class AlertThreshold
{
    public string MetricName { get; set; }
    public string Operator { get; set; } // GreaterThan, LessThan, Equals
    public decimal Threshold { get; set; }
    public string Severity { get; set; }
}

/// <summary>
/// Disaster recovery configuration.
/// </summary>
public class DRConfiguration
{
    /// <summary>Recovery time objective (hours)</summary>
    public int RTOHours { get; set; }

    /// <summary>Recovery point objective (minutes)</summary>
    public int RPOMinutes { get; set; }

    /// <summary>DR region</summary>
    public string DRRegion { get; set; }

    /// <summary>Backup frequency</summary>
    public string BackupFrequency { get; set; }

    /// <summary>Failover automation enabled</summary>
    public bool FailoverAutomationEnabled { get; set; }

    /// <summary>DR testing frequency</summary>
    public string DRTestingFrequency { get; set; }
}

/// <summary>
/// Capacity and scaling configuration.
/// </summary>
public class CapacityConfiguration
{
    /// <summary>Auto-scaling enabled</summary>
    public bool AutoScalingEnabled { get; set; }

    /// <summary>Min capacity</summary>
    public int MinCapacity { get; set; }

    /// <summary>Max capacity</summary>
    public int MaxCapacity { get; set; }

    /// <summary>Scale-up threshold (%)</summary>
    public decimal ScaleUpThreshold { get; set; }

    /// <summary>Scale-down threshold (%)</summary>
    public decimal ScaleDownThreshold { get; set; }

    /// <summary>Scale cooldown period (minutes)</summary>
    public int ScaleCooldownMinutes { get; set; }
}

/// <summary>
/// Approved service in landing zone.
/// </summary>
public class ApprovedService
{
    public string ServiceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Service name</summary>
    public string ServiceName { get; set; }

    /// <summary>Service provider</summary>
    public string Provider { get; set; }

    /// <summary>Version approved</summary>
    public string ApprovedVersion { get; set; }

    /// <summary>Approval date</summary>
    public DateTime ApprovalDate { get; set; }

    /// <summary>Expiration date</summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>Service level agreement</summary>
    public string SLAAgreement { get; set; }

    /// <summary>Cost per unit/month</summary>
    public decimal? CostPerUnit { get; set; }
}

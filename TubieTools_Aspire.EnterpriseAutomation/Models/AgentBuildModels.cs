namespace TubieTools_Aspire.EnterpriseAutomation.Models;

/// <summary>
/// Represents the build and deployment phase of an AI agent.
/// Aligns with CAF "Build Agents" lifecycle phase.
/// </summary>
public class AgentBuild
{
    public string BuildId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Development status</summary>
    public string DevelopmentStatus { get; set; } = "NotStarted";

    /// <summary>Build pipeline configuration</summary>
    public BuildPipeline BuildPipeline { get; set; }

    /// <summary>Model training configuration</summary>
    public ModelTrainingConfig ModelTraining { get; set; }

    /// <summary>Testing strategy and results</summary>
    public TestingStrategy TestingStrategy { get; set; }

    /// <summary>Deployment configuration</summary>
    public DeploymentConfig Deployment { get; set; }

    /// <summary>Integration testing results</summary>
    public List<IntegrationTestResult> IntegrationTestResults { get; set; } = new();

    /// <summary>Performance benchmarks</summary>
    public PerformanceBenchmark PerformanceBenchmark { get; set; }

    /// <summary>Security testing results (SAST, DAST, penetration testing)</summary>
    public SecurityTestingResults SecurityTesting { get; set; }

    /// <summary>Deployment history and versions</summary>
    public List<DeploymentRecord> DeploymentHistory { get; set; } = new();

    /// <summary>Current production version</summary>
    public string CurrentProductionVersion { get; set; }

    /// <summary>Build completion date</summary>
    public DateTime? CompletionDate { get; set; }

    /// <summary>Build notes and known issues</summary>
    public string BuildNotes { get; set; }
}

/// <summary>
/// Build pipeline configuration for CI/CD.
/// </summary>
public class BuildPipeline
{
    public string PipelineId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Source code repository URL</summary>
    public string RepositoryUrl { get; set; }

    /// <summary>Main branch for deployments</summary>
    public string MainBranch { get; set; }

    /// <summary>Build automation tool (GitHub Actions, Azure DevOps, Jenkins, etc.)</summary>
    public string BuildTool { get; set; }

    /// <summary>Build triggers (on commit, scheduled, manual)</summary>
    public List<string> BuildTriggers { get; set; } = new();

    /// <summary>Build job steps/stages</summary>
    public List<BuildStage> BuildStages { get; set; } = new();

    /// <summary>Artifact storage location</summary>
    public string ArtifactStorage { get; set; }

    /// <summary>Docker container configuration</summary>
    public ContainerConfig ContainerConfig { get; set; }

    /// <summary>Dependency vulnerability scanning enabled</summary>
    public bool EnablesDependencyScanning { get; set; }

    /// <summary>Code quality gates enabled</summary>
    public bool EnablesCodeQualityGates { get; set; }

    /// <summary>Minimum code quality score required</summary>
    public decimal MinimumQualityScore { get; set; }
}

/// <summary>
/// Represents a single build stage/step.
/// </summary>
public class BuildStage
{
    public string StageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Stage order</summary>
    public int StageOrder { get; set; }

    /// <summary>Stage name</summary>
    public string StageName { get; set; }

    /// <summary>Description of what this stage does</summary>
    public string Description { get; set; }

    /// <summary>Commands/script to execute</summary>
    public string Script { get; set; }

    /// <summary>Timeout for this stage (minutes)</summary>
    public int TimeoutMinutes { get; set; }

    /// <summary>Retry policy if stage fails</summary>
    public int MaxRetries { get; set; }

    /// <summary>Continue on error flag</summary>
    public bool ContinueOnError { get; set; }
}

/// <summary>
/// Container/Docker configuration for agent deployment.
/// </summary>
public class ContainerConfig
{
    public string ConfigId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Base image (Python, Node.js, etc.)</summary>
    public string BaseImage { get; set; }

    /// <summary>Base image version/tag</summary>
    public string BaseImageVersion { get; set; }

    /// <summary>Dockerfile path</summary>
    public string DockerfilePath { get; set; }

    /// <summary>Container registry (Docker Hub, ACR, ECR)</summary>
    public string ContainerRegistry { get; set; }

    /// <summary>Container image name</summary>
    public string ImageName { get; set; }

    /// <summary>Image tag</summary>
    public string ImageTag { get; set; }

    /// <summary>Image scanning for vulnerabilities</summary>
    public bool ImageScanningEnabled { get; set; }

    /// <summary>Resource limits (CPU, memory)</summary>
    public ResourceLimits ResourceLimits { get; set; }
}

/// <summary>
/// Resource limits for container execution.
/// </summary>
public class ResourceLimits
{
    /// <summary>CPU limit (e.g., "1" = 1 core, "500m" = half core)</summary>
    public string CPULimit { get; set; }

    /// <summary>Memory limit (e.g., "512Mi", "2Gi")</summary>
    public string MemoryLimit { get; set; }

    /// <summary>GPU requirement if needed</summary>
    public string GPURequirements { get; set; }

    /// <summary>Disk storage requirement</summary>
    public string StorageRequirement { get; set; }
}

/// <summary>
/// Model training configuration.
/// </summary>
public class ModelTrainingConfig
{
    public string ConfigId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Training approach (FineTuning, Custom, Transfer, etc.)</summary>
    public string TrainingApproach { get; set; }

    /// <summary>Base model to start from</summary>
    public string BaseModel { get; set; }

    /// <summary>Training dataset size (number of samples)</summary>
    public int DatasetSize { get; set; }

    /// <summary>Train/validation/test split ratio</summary>
    public string DataSplitRatio { get; set; }

    /// <summary>Training hyperparameters</summary>
    public List<HyperParameter> HyperParameters { get; set; } = new();

    /// <summary>Training hardware requirements (GPU, TPU, etc.)</summary>
    public string HardwareRequirements { get; set; }

    /// <summary>Estimated training time</summary>
    public string EstimatedTrainingTime { get; set; }

    /// <summary>Training cost estimate</summary>
    public decimal EstimatedCost { get; set; }

    /// <summary>Convergence criteria</summary>
    public string ConvergenceCriteria { get; set; }

    /// <summary>Model evaluation metrics</summary>
    public List<EvaluationMetric> EvaluationMetrics { get; set; } = new();

    /// <summary>Early stopping enabled</summary>
    public bool EnablesEarlyStopping { get; set; }

    /// <summary>Cross-validation enabled</summary>
    public bool EnablesCrossValidation { get; set; }
}

/// <summary>
/// Hyperparameter for model training.
/// </summary>
public class HyperParameter
{
    public string ParamId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Parameter name</summary>
    public string Name { get; set; }

    /// <summary>Parameter value</summary>
    public string Value { get; set; }

    /// <summary>Parameter type (float, int, string)</summary>
    public string Type { get; set; }

    /// <summary>Valid range or options</summary>
    public string ValidRange { get; set; }

    /// <summary>Justification for this value</summary>
    public string Justification { get; set; }
}

/// <summary>
/// Model evaluation metric.
/// </summary>
public class EvaluationMetric
{
    public string MetricId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Metric name (Accuracy, F1, Precision, Recall, RMSE, etc.)</summary>
    public string MetricName { get; set; }

    /// <summary>Target value for this metric</summary>
    public decimal TargetValue { get; set; }

    /// <summary>Actual achieved value</summary>
    public decimal ActualValue { get; set; }

    /// <summary>Metric calculation method</summary>
    public string CalculationMethod { get; set; }

    /// <summary>Whether metric passed the target</summary>
    public bool Passed { get; set; }
}

/// <summary>
/// Testing strategy and configuration.
/// </summary>
public class TestingStrategy
{
    public string StrategyId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Unit testing configuration</summary>
    public TestConfig UnitTesting { get; set; }

    /// <summary>Integration testing configuration</summary>
    public TestConfig IntegrationTesting { get; set; }

    /// <summary>End-to-end testing configuration</summary>
    public TestConfig E2ETesting { get; set; }

    /// <summary>Performance testing configuration</summary>
    public PerformanceTesting PerformanceTesting { get; set; }

    /// <summary>Adversarial/robustness testing</summary>
    public AdversarialTesting AdversarialTesting { get; set; }

    /// <summary>Regression testing for model behavior</summary>
    public RegressionTesting RegressionTesting { get; set; }

    /// <summary>Test coverage requirement (%)</summary>
    public int CoverageRequirement { get; set; }

    /// <summary>Current test coverage achievement</summary>
    public int CurrentCoverage { get; set; }

    /// <summary>Test automation level (Full, Partial, Manual)</summary>
    public string AutomationLevel { get; set; }
}

/// <summary>
/// Generic test configuration.
/// </summary>
public class TestConfig
{
    /// <summary>Tests enabled</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Test framework used (xUnit, NUnit, pytest, etc.)</summary>
    public string TestFramework { get; set; }

    /// <summary>Number of test cases</summary>
    public int TestCaseCount { get; set; }

    /// <summary>Passing test count</summary>
    public int PassingTestCount { get; set; }

    /// <summary>Failing test count</summary>
    public int FailingTestCount { get; set; }

    /// <summary>Test execution time</summary>
    public string ExecutionTime { get; set; }

    /// <summary>Last test execution date</summary>
    public DateTime? LastExecutionDate { get; set; }

    /// <summary>Overall pass rate (%)</summary>
    public decimal PassRate { get; set; }
}

/// <summary>
/// Performance testing configuration.
/// </summary>
public class PerformanceTesting
{
    /// <summary>Tests enabled</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Load testing tool (JMeter, Gatling, k6, etc.)</summary>
    public string LoadTestingTool { get; set; }

    /// <summary>Target throughput (requests/second)</summary>
    public int TargetThroughput { get; set; }

    /// <summary>Target latency (milliseconds)</summary>
    public int TargetLatencyMs { get; set; }

    /// <summary>Stress testing configuration</summary>
    public string StressTestingConfig { get; set; }

    /// <summary>Results from last performance test</summary>
    public PerformanceTestResult LastTestResult { get; set; }
}

/// <summary>
/// Adversarial/robustness testing configuration.
/// </summary>
public class AdversarialTesting
{
    /// <summary>Tests enabled</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Types of adversarial attacks tested</summary>
    public List<string> AttackTypes { get; set; } = new();

    /// <summary>Input perturbation testing</summary>
    public bool TestsInputPerturbation { get; set; }

    /// <summary>Out-of-distribution detection testing</summary>
    public bool TestsOutOfDistribution { get; set; }

    /// <summary>Adversarial robustness score (0-100)</summary>
    public int RobustnessScore { get; set; }

    /// <summary>Identified vulnerabilities</summary>
    public List<string> IdentifiedVulnerabilities { get; set; } = new();
}

/// <summary>
/// Regression testing configuration for model behavior.
/// </summary>
public class RegressionTesting
{
    /// <summary>Tests enabled</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Test dataset for regression comparison</summary>
    public string TestDatasetPath { get; set; }

    /// <summary>Acceptable deviation from baseline (%)</summary>
    public decimal AcceptableDeviation { get; set; }

    /// <summary>Baseline model version for comparison</summary>
    public string BaselineVersion { get; set; }

    /// <summary>Models passed regression testing</summary>
    public bool PassedRegressionTesting { get; set; }

    /// <summary>Regression test results detail</summary>
    public List<RegressionTestDetailResult> DetailResults { get; set; } = new();
}

/// <summary>
/// Detailed regression test result.
/// </summary>
public class RegressionTestDetailResult
{
    public string ResultId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Test case name</summary>
    public string TestCaseName { get; set; }

    /// <summary>Baseline result</summary>
    public string BaselineResult { get; set; }

    /// <summary>Current result</summary>
    public string CurrentResult { get; set; }

    /// <summary>Deviation percentage</summary>
    public decimal DeviationPercentage { get; set; }

    /// <summary>Status (Passed, Failed)</summary>
    public string Status { get; set; }
}

/// <summary>
/// Deployment configuration.
/// </summary>
public class DeploymentConfig
{
    public string ConfigId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Deployment environment (Development, Staging, Production)</summary>
    public string Environment { get; set; }

    /// <summary>Deployment platform (Azure, AWS, On-Premises, Hybrid)</summary>
    public string DeploymentPlatform { get; set; }

    /// <summary>Deployment strategy (BlueGreen, Canary, RollingUpdate, etc.)</summary>
    public string DeploymentStrategy { get; set; }

    /// <summary>Canary deployment percentage (for canary strategy)</summary>
    public int CanaryPercentage { get; set; }

    /// <summary>Canary duration before full rollout</summary>
    public int CanaryDurationMinutes { get; set; }

    /// <summary>Rollback capability</summary>
    public bool SupportsRollback { get; set; }

    /// <summary>Infrastructure as Code configuration (Terraform, ARM, CloudFormation)</summary>
    public string IaCTemplate { get; set; }

    /// <summary>Service mesh configuration (if applicable)</summary>
    public string ServiceMeshConfig { get; set; }

    /// <summary>Load balancer configuration</summary>
    public string LoadBalancerConfig { get; set; }

    /// <summary>High availability enabled</summary>
    public bool HighAvailabilityEnabled { get; set; }

    /// <summary>Disaster recovery plan</summary>
    public string DisasterRecoveryPlan { get; set; }

    /// <summary>Recovery time objective (hours)</summary>
    public int RTOHours { get; set; }

    /// <summary>Recovery point objective (minutes)</summary>
    public int RPOMinutes { get; set; }
}

/// <summary>
/// Performance benchmark results.
/// </summary>
public class PerformanceBenchmark
{
    public string BenchmarkId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Benchmark execution date</summary>
    public DateTime ExecutionDate { get; set; }

    /// <summary>Average response time (milliseconds)</summary>
    public decimal AvgResponseTimeMs { get; set; }

    /// <summary>P95 response time (milliseconds)</summary>
    public decimal P95ResponseTimeMs { get; set; }

    /// <summary>P99 response time (milliseconds)</summary>
    public decimal P99ResponseTimeMs { get; set; }

    /// <summary>Maximum response time (milliseconds)</summary>
    public decimal MaxResponseTimeMs { get; set; }

    /// <summary>Throughput (requests/second)</summary>
    public decimal ThroughputRPS { get; set; }

    /// <summary>Error rate (%)</summary>
    public decimal ErrorRate { get; set; }

    /// <summary>Resource utilization (CPU, Memory)</summary>
    public ResourceUtilization ResourceUtilization { get; set; }

    /// <summary>Benchmark environment description</summary>
    public string EnvironmentDescription { get; set; }

    /// <summary>Passed performance threshold</summary>
    public bool PassedThreshold { get; set; }
}

/// <summary>
/// Resource utilization metrics.
/// </summary>
public class ResourceUtilization
{
    /// <summary>CPU utilization (%)</summary>
    public decimal CPUUtilizationPercent { get; set; }

    /// <summary>Memory utilization (%)</summary>
    public decimal MemoryUtilizationPercent { get; set; }

    /// <summary>Disk I/O utilization (%)</summary>
    public decimal DiskIOUtilizationPercent { get; set; }

    /// <summary>Network bandwidth utilization (%)</summary>
    public decimal NetworkUtilizationPercent { get; set; }
}

/// <summary>
/// Security testing results.
/// </summary>
public class SecurityTestingResults
{
    public string ResultsId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>SAST (Static Application Security Testing) results</summary>
    public SecurityScanResult SASTResults { get; set; }

    /// <summary>DAST (Dynamic Application Security Testing) results</summary>
    public SecurityScanResult DASTResults { get; set; }

    /// <summary>Dependency vulnerability scanning results</summary>
    public SecurityScanResult DependencyScanResults { get; set; }

    /// <summary>Container image security scan results</summary>
    public SecurityScanResult ContainerScanResults { get; set; }

    /// <summary>Penetration testing results</summary>
    public PenetrationTestingResult PenetrationTestResults { get; set; }

    /// <summary>Overall security score (0-100)</summary>
    public int OverallSecurityScore { get; set; }

    /// <summary>Security testing passed</summary>
    public bool PassedSecurityTesting { get; set; }
}

/// <summary>
/// Security scan results.
/// </summary>
public class SecurityScanResult
{
    /// <summary>Scan type</summary>
    public string ScanType { get; set; }

    /// <summary>Scan date</summary>
    public DateTime ScanDate { get; set; }

    /// <summary>Tool used</summary>
    public string Tool { get; set; }

    /// <summary>Critical issues found</summary>
    public int CriticalIssues { get; set; }

    /// <summary>High severity issues</summary>
    public int HighIssues { get; set; }

    /// <summary>Medium severity issues</summary>
    public int MediumIssues { get; set; }

    /// <summary>Low severity issues</summary>
    public int LowIssues { get; set; }

    /// <summary>Issues requiring immediate remediation</summary>
    public List<string> RemediationRequired { get; set; } = new();

    /// <summary>Overall scan result (Pass/Fail)</summary>
    public string ScanResult { get; set; }
}

/// <summary>
/// Penetration testing results.
/// </summary>
public class PenetrationTestingResult
{
    /// <summary>Test date</summary>
    public DateTime TestDate { get; set; }

    /// <summary>Tester name/organization</summary>
    public string TesterName { get; set; }

    /// <summary>Vulnerabilities found</summary>
    public List<string> VulnerabilitiesFound { get; set; } = new();

    /// <summary>Exploitation successful</summary>
    public bool ExploitationSuccessful { get; set; }

    /// <summary>Risk rating</summary>
    public string RiskRating { get; set; }

    /// <summary>Recommendations</summary>
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Deployment record history.
/// </summary>
public class DeploymentRecord
{
    public string RecordId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Version number</summary>
    public string Version { get; set; }

    /// <summary>Target environment</summary>
    public string Environment { get; set; }

    /// <summary>Deployment date and time</summary>
    public DateTime DeploymentDate { get; set; }

    /// <summary>Deployed by (user/service)</summary>
    public string DeployedBy { get; set; }

    /// <summary>Deployment status</summary>
    public string Status { get; set; }

    /// <summary>Commit/build ID</summary>
    public string BuildCommitId { get; set; }

    /// <summary>Deployment duration (minutes)</summary>
    public int DurationMinutes { get; set; }

    /// <summary>Release notes</summary>
    public string ReleaseNotes { get; set; }

    /// <summary>Rollback performed</summary>
    public bool WasRolledBack { get; set; }

    /// <summary>Rollback reason (if applicable)</summary>
    public string RollbackReason { get; set; }

    /// <summary>Deployment monitoring period (hours after deployment to monitor)</summary>
    public int MonitoringHours { get; set; }
}

/// <summary>
/// Performance test result.
/// </summary>
public class PerformanceTestResult
{
    public string ResultId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Test date</summary>
    public DateTime TestDate { get; set; }

    /// <summary>Benchmark referenced</summary>
    public PerformanceBenchmark Benchmark { get; set; }

    /// <summary>Test passed</summary>
    public bool Passed { get; set; }

    /// <summary>Issues found</summary>
    public List<string> Issues { get; set; } = new();
}

/// <summary>
/// Integration test result.
/// </summary>
public class IntegrationTestResult
{
    public string ResultId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Test case name</summary>
    public string TestCaseName { get; set; }

    /// <summary>System being integrated with</summary>
    public string SystemIntegrated { get; set; }

    /// <summary>Test status</summary>
    public string Status { get; set; }

    /// <summary>Test execution date</summary>
    public DateTime ExecutionDate { get; set; }

    /// <summary>Duration (milliseconds)</summary>
    public int DurationMs { get; set; }

    /// <summary>Error message if test failed</summary>
    public string ErrorMessage { get; set; }
}

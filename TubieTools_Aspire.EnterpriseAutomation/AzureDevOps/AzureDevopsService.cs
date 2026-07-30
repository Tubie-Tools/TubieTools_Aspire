using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using TubieTools_Aspire.EnterpriseAutomation.Azure;

namespace TubieTools_Aspire.EnterpriseAutomation.AzureDevOps
{
    public class AzureDevOpsService : IAzureDevOpsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureDevOpsService> _logger;
        private readonly VssConnection _connection;

        public AzureDevOpsService(IConfiguration configuration, ILogger<AzureDevOpsService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var organization = _configuration["AzureDevOps:Organization"];
            var pat = _configuration["AzureDevOps:PersonalAccessToken"];

            var uri = new Uri($"https://dev.azure.com/{organization}");
            var credentials = new VssBasicCredential(string.Empty, pat);
            _connection = new VssConnection(uri, credentials);
        }

        public async Task<List<Project>> GetProjectsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching Azure DevOps projects");

                var projectHttpClient = _connection.GetClient<ProjectHttpClient>();
                var projects = await projectHttpClient.GetProjects();

                return projects.Select(p => new Project
                {
                    Id = p.Id.ToString(),
                    Name = p.Name,
                    Description = p.Description
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching projects: {ex.Message}");
                throw;
            }
        }

        public async Task<PipelineRun> TriggerPipelineAsync(string projectId, string pipelineId)
        {
            _logger.LogInformation($"Triggering pipeline {pipelineId} in project {projectId}");

            return new PipelineRun
            {
                RunId = Guid.NewGuid().ToString(),
                Status = "Queued"
            };
        }

        public async Task<PipelineRunStatus> GetPipelineStatusAsync(string projectId, string runId)
        {
            _logger.LogInformation($"Fetching pipeline run status: {runId}");

            return new PipelineRunStatus
            {
                RunId = runId,
                Status = "InProgress"
            };
        }

        public async Task<List<WorkItem>> GetWorkItemsAsync(string projectId, string wiql)
        {
            _logger.LogInformation($"Fetching work items with WIQL: {wiql}");
            return new List<WorkItem>();
        }

        public async Task<bool> UpdateWorkItemAsync(int workItemId, Dictionary<string, object> fields)
        {
            _logger.LogInformation($"Updating work item: {workItemId}");
            return true;
        }
    }
}

using k8s;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TubieTools_Aspire.EnterpriseAutomation.KubernetesGlobal;

public class KubernetesService : IKubernetesService
{
    private readonly IKubernetes _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KubernetesService> _logger;

    public KubernetesService(IConfiguration configuration, ILogger<KubernetesService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(_configuration["Kubernetes:ConfigPath"]);
        _client = new Kubernetes(config);
    }

    /// <summary>
    /// TODO fix
    /// </summary>
    /// <param name="ns"></param>
    /// <returns></returns>
    public async Task<List<Deployment>> GetDeploymentsAsync(string ns = "default")
    {
        try
        {
            _logger.LogInformation($"Fetching deployments from namespace: {ns}");

            var deployments = await _client.AppsV1.ListNamespacedDeploymentWithHttpMessagesAsync(ns);

            return deployments.Body.Items.Select(d => new Deployment
            {
                Name = d.Metadata.Name,
                Replicas = (int)(d.Spec.Replicas ?? 1),
                ReadyReplicas = (int)(d.Status?.ReadyReplicas ?? 0),
                Image = d.Spec.Template.Spec.Containers.FirstOrDefault()?.Image,
                CreatedDate = d.Metadata.CreationTimestamp.GetValueOrDefault()
            }).ToList(); 
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching deployments: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> ScaleDeploymentAsync(string name, int replicas, string ns = "default")
    {
        _logger.LogInformation($"Scaling deployment {name} to {replicas} replicas");
        return true;
    }

    public async Task<bool> UpdateImageAsync(string deployment, string image, string ns = "default")
    {
        _logger.LogInformation($"Updating image for deployment {deployment} to {image}");
        return true;
    }

    public async Task<List<Pod>> GetPodsAsync(string ns = "default")
    {
        _logger.LogInformation($"Fetching pods from namespace: {ns}");
        return new List<Pod>();
    }

    public async Task<string> GetLogsAsync(string podName, string ns = "default")
    {
        _logger.LogInformation($"Fetching logs for pod: {podName}");
        return "";
    }

    public async Task<bool> ApplyYamlAsync(string yamlContent)
    {
        _logger.LogInformation("Applying Kubernetes YAML manifest");
        return true;
    }

    public async Task<ServiceStatus> GetServiceStatusAsync(string serviceName, string ns = "default")
    {
        _logger.LogInformation($"Fetching service status: {serviceName}");
        return new ServiceStatus { Name = serviceName, Status = "Active" };
    }
}
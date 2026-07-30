namespace TubieTools_Aspire.EnterpriseAutomation.KubernetesGlobal;

public interface IKubernetesService
{
    Task<List<Deployment>> GetDeploymentsAsync(string ns = "default");
    Task<bool> ScaleDeploymentAsync(string name, int replicas, string ns = "default");
    Task<bool> UpdateImageAsync(string deployment, string image, string ns = "default");
    Task<List<Pod>> GetPodsAsync(string ns = "default");
    Task<string> GetLogsAsync(string podName, string ns = "default");
    Task<bool> ApplyYamlAsync(string yamlContent);
    Task<ServiceStatus> GetServiceStatusAsync(string serviceName, string ns = "default");
}

public class Deployment
{
    public string Name { get; set; }
    public int Replicas { get; set; }
    public int ReadyReplicas { get; set; }
    public string Image { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class Pod
{
    public string Name { get; set; }
    public string Status { get; set; }
    public string Node { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class ServiceStatus
{
    public string Name { get; set; }
    public string Status { get; set; }
    public string ClusterIP { get; set; }
    public List<string> ExternalIPs { get; set; }
}


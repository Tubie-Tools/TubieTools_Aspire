namespace TubieTools_Aspire.EnterpriseAutomation.Terraform;

public interface ITerraformService
{
    Task<TerraformPlanResult> PlanAsync(string workspacePath, Dictionary<string, string> variables);
    Task<TerraformApplyResult> ApplyAsync(string workspacePath, Dictionary<string, string> variables);
    Task<bool> DestroyAsync(string workspacePath);
    Task<TerraformState> GetStateAsync(string workspacePath);
    Task<bool> ValidateAsync(string workspacePath);
    Task<string> InitAsync(string workspacePath);
}

public class TerraformPlanResult
{
    public string PlanId { get; set; }
    public List<ResourceChange> Changes { get; set; }
    public string Summary { get; set; }
    public bool HasChanges { get; set; }
}

public class TerraformApplyResult
{
    public string ApplyId { get; set; }
    public string Status { get; set; }
    public List<ResourceChange> AppliedChanges { get; set; }
    public DateTime CompletedAt { get; set; }
}

public class ResourceChange
{
    public string Action { get; set; } // create, update, delete
    public string ResourceType { get; set; }
    public string ResourceName { get; set; }
    public Dictionary<string, object> Changes { get; set; }
}

public class TerraformState
{
    public string Version { get; set; }
    public List<Resource> Resources { get; set; }
}

public class Resource
{
    public string Type { get; set; }
    public string Name { get; set; }
    public Dictionary<string, object> Instances { get; set; }
}


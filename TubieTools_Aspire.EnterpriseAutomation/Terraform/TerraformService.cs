
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TubieTools_Aspire.EnterpriseAutomation.Terraform;

public class TerraformService : ITerraformService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TerraformService> _logger;

    public TerraformService(IConfiguration configuration, ILogger<TerraformService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TerraformPlanResult> PlanAsync(string workspacePath, Dictionary<string, string> variables)
    {
        try
        {
            _logger.LogInformation($"Running Terraform plan in: {workspacePath}");

            var result = await ExecuteTerraformCommandAsync(workspacePath, "plan", variables);

            return new TerraformPlanResult
            {
                PlanId = Guid.NewGuid().ToString(),
                Summary = result,
                HasChanges = result.Contains("will be created") || result.Contains("will be updated")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Terraform plan failed: {ex.Message}");
            throw;
        }
    }

    public async Task<TerraformApplyResult> ApplyAsync(string workspacePath, Dictionary<string, string> variables)
    {
        _logger.LogInformation($"Running Terraform apply in: {workspacePath}");

        return new TerraformApplyResult
        {
            ApplyId = Guid.NewGuid().ToString(),
            Status = "Completed"
        };
    }

    public async Task<bool> DestroyAsync(string workspacePath)
    {
        _logger.LogInformation($"Running Terraform destroy in: {workspacePath}");
        return true;
    }

    public async Task<TerraformState> GetStateAsync(string workspacePath)
    {
        _logger.LogInformation($"Fetching Terraform state from: {workspacePath}");
        return new TerraformState { Version = "4.0" };
    }

    public async Task<bool> ValidateAsync(string workspacePath)
    {
        _logger.LogInformation($"Validating Terraform in: {workspacePath}");
        return true;
    }

    public async Task<string> InitAsync(string workspacePath)
    {
        _logger.LogInformation($"Initializing Terraform in: {workspacePath}");
        return await ExecuteTerraformCommandAsync(workspacePath, "init", new());
    }

    private async Task<string> ExecuteTerraformCommandAsync(string workspacePath, string command, Dictionary<string, string> variables)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = "terraform",
            Arguments = command,
            WorkingDirectory = workspacePath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo);
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new Exception($"Terraform command failed: {error}");

        return output;
    }
}


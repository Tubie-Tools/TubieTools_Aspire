namespace TubieTools_Aspire.EnterpriseAutomation.Security;

public interface ISecurityService
{
    Task<bool> ValidateTokenAsync(string token);
    Task<string> EncryptAsync(string plaintext);
    Task<string> DecryptAsync(string ciphertext);
    Task<List<AuditLog>> GetAuditLogsAsync(int limit = 100);
    Task LogActionAsync(string userId, string action, string resource);
    Task<bool> CheckPermissionAsync(string userId, string permission);
}

public class AuditLog
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Action { get; set; }
    public string Resource { get; set; }
    public string Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; }
}
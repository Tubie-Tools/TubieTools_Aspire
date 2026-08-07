namespace TubieTools_Aspire.EnterpriseAutomation.Extensions;

using Microsoft.AspNetCore.Mvc;

public static class ControllerExtensions
{
    public static IActionResult HandleAccessDenial(this ControllerBase controller, string? errorMessage = null)
    {
        return controller.StatusCode(403, new { error = errorMessage ?? "Access denied", statusCode = 403 });
    }

    public static IActionResult HandleQuotaExceeded(this ControllerBase controller, string? customMessage = null)
    {
        return controller.StatusCode(403, new { error = customMessage ?? "Quota exceeded", statusCode = 403 });
    }

    public static IActionResult HandlePermissionDenied(this ControllerBase controller, string? resource = null)
    {
        var message = resource != null ? $"Permission denied for resource: {resource}" : "Permission denied";
        return controller.StatusCode(403, new { error = message, statusCode = 403 });
    }

    public static IActionResult HandleUnauthorizedAccess(this ControllerBase controller, string? reason = null)
    {
        return controller.StatusCode(403, new { error = reason ?? "Unauthorized access", statusCode = 403 });
    }

    public static bool IsAccessDenialResponse(this ControllerBase controller, string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return false;
        var lowerMessage = message.ToLower();
        return lowerMessage.Contains("access denied") || lowerMessage.Contains("quota exceeded") || lowerMessage.Contains("not authorized") || lowerMessage.Contains("permission denied") || lowerMessage.Contains("insufficient permissions") || lowerMessage.Contains("unauthorized") || lowerMessage.Contains("forbidden");
    }
}

namespace MapApp.API.Services.TMS;

public class BillingValidation
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public List<string> Validations { get; set; } = new();
    public object BillingId { get; internal set; }
    public List<string> ValidationMessages { get; internal set; }
    public List<string> Issues { get; internal set; }
    public string Severity { get; internal set; }
    public string Recommendation { get; internal set; }
}

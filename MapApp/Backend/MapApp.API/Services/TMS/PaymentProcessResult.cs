namespace MapApp.API.Services.TMS;

public class PaymentProcessResult
{
    public string BillingId { get; set; } = string.Empty;
    public decimal AmountReceived { get; set; }
    public DateTime ProcessDate { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ConfirmationNumber { get; set; }
    public string? ErrorMessage { get; set; }
}

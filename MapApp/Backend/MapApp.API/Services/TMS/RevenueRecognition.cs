namespace MapApp.API.Services.TMS;

public class RevenueRecognition
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime RecognitionDate { get; set; }
    public decimal RevenueAmount { get; set; }
    public string RevenueRecognitionMethod { get; set; } = string.Empty;
    public decimal LineHaulRevenue { get; set; }
    public decimal FuelSurchargeRevenue { get; set; }
    public decimal AccessorialRevenue { get; set; }
}

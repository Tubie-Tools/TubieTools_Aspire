namespace MapApp.API.Models;

/// <summary>
/// Represents a US state capital with geographic and sales data
/// </summary>
public class StateCapital
{
    public string StateCode { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public string CapitalName { get; set; } = string.Empty;

    // Geographic coordinates (latitude, longitude)
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Sales information
    public bool HasSoldProducts { get; set; }
    public DateTime? LastSaleDate { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public int ProductsSold { get; set; }

    // Additional metadata
    public string Region { get; set; } = string.Empty; // Northeast, Southeast, Midwest, Southwest, West
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

namespace MapApp.API.Services.TMS;

public class FuelEfficiencyMetrics
{
    public string TruckId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double ActualMPG { get; set; }
    public double TotalMiles { get; set; }
    public double TotalGallonsUsed { get; set; }
    public double TotalFuelCost { get; set; }
    public double CostPerMile { get; set; }
    public double EfficiencyVariance { get; set; } // % above/below benchmark
    public bool IsBelowBenchmark { get; set; }
}

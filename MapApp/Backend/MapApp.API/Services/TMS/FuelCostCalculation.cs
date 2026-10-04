namespace MapApp.API.Services.TMS;

/// <summary>
/// Enumeration of fuel types for vehicle fuel cost calculations.
/// </summary>
public enum FuelType
{
    /// <summary>
    /// Diesel fuel (commonly used in commercial vehicles and trucks)
    /// </summary>
    Diesel = 0,

    /// <summary>
    /// Regular gasoline (unleaded 87 octane)
    /// </summary>
    Regular = 1,

    /// <summary>
    /// Premium unleaded gasoline (91-93 octane)
    /// </summary>
    Unleaded = 2,

    /// <summary>
    /// Electric power (zero emissions, battery-powered vehicles)
    /// </summary>
    Electric = 3
}

/// <summary>
/// Does this matter if it is DISEL or GAS?  Should we have a fuel type enum?  For now, we will assume all fuel is diesel.
/// </summary>
public class FuelCostCalculation
{
    public FuelType FuelType { get; set; }
    public string ShipmentId { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public decimal FuelPrice { get; set; }
    public decimal GallonsRequired { get; set; }
    public decimal TotalFuelCost { get; set; }
    public decimal CostPerMile { get; set; }
    public decimal MilesPerGallon { get; set; } // For combustion engines
    public decimal EfficiencyRating { get; set; } // For electric vehicles (miles per kWh)   


    /// <summary>
    /// Calculates the fuel cost for a given distance traveled.
    /// </summary>
    /// <returns>Total fuel cost for the trip</returns>
    public decimal CalculateFuelCost()
    {
        return FuelType switch
        {
            FuelType.Diesel or FuelType.Regular or FuelType.Unleaded
                => (Distance / MilesPerGallon) * FuelPrice,

            FuelType.Electric
                => (Distance / EfficiencyRating) * FuelPrice,

            _ => throw new ArgumentException($"Unknown fuel type: {FuelType}")
        };
    }

    /// <summary>
    /// Gets the friendly display name for the fuel type.
    /// </summary>
    public string GetFuelTypeName()
    {
        return FuelType switch
        {
            FuelType.Diesel => "Diesel",
            FuelType.Regular => "Regular Gasoline",
            FuelType.Unleaded => "Premium Unleaded",
            FuelType.Electric => "Electric",
            _ => "Unknown"
        };
    }
}

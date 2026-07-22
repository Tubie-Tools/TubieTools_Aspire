using Microsoft.ML.Data;

internal class HousingData
{
    public float Size { get; internal set; }
    [VectorType(3)]  // Specify fixed size of 3
    public float[] HistoricalPrices { get; internal set; }
    public float CurrentPrice { get; internal set; }
}
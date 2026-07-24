using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms.Onnx;

HousingData[] housingData = new HousingData[]
{
    new HousingData
    {
        Size = 600f,
        HistoricalPrices = new float[] { 100000f, 125000f, 122000f },
        CurrentPrice = 170000f
    },
    new HousingData
    {
        Size = 1000f,
        HistoricalPrices = new float[] { 200000f, 250000f, 230000f },
        CurrentPrice = 225000f
    },
    new HousingData
    {
        Size = 1000f,
        HistoricalPrices = new float[] { 126000f, 130000f, 200000f },
        CurrentPrice = 195000f
    }
};

// Create MLContext
MLContext mlContext = new MLContext();

// Load Data
IDataView data = mlContext.Data.LoadFromEnumerable<HousingData>(housingData);

// Define data preparation estimator
EstimatorChain<RegressionPredictionTransformer<LinearRegressionModelParameters>> pipelineEstimator =
    mlContext.Transforms.Concatenate("Features", new string[] { "Size", "HistoricalPrices" })
        .Append(mlContext.Transforms.NormalizeMinMax("Features"))
        .Append(mlContext.Transforms.CopyColumns("Label", "CurrentPrice"))
        .Append(mlContext.Regression.Trainers.Sdca());

// Train model
ITransformer trainedModel = pipelineEstimator.Fit(data);

// Save model
mlContext.Model.Save(trainedModel, data.Schema, "model.zip");

using (FileStream stream = File.Create("onnx_model.onnx"))
{
    mlContext.Model.ConvertToOnnx(trainedModel, data, stream);
    stream.Flush();
    stream.Dispose(); // Explicit dispose
}
string modelPath = Path.GetFullPath("onnx_model.onnx");
System.Threading.Thread.Sleep(100); // Brief delay to ensure file is synced

OnnxScoringEstimator estimator = mlContext.Transforms.ApplyOnnxModel(modelPath);

HousingData[] newHousingData = new HousingData[]
{
    new()
    {
        Size = 1000f,
        HistoricalPrices = new[] { 300_000f, 350_000f, 450_000f },
        CurrentPrice = 550_00f
    }
};

IDataView newHousingDataView = mlContext.Data.LoadFromEnumerable(newHousingData);


var result = estimator.Fit(newHousingDataView);

Console.WriteLine("WTF SHUT UP EVERYONE");
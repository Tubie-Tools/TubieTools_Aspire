# ONNX Price Forecasting Implementation Guide

This guide covers the ONNX implementation for making predictions using AutoML models in your TubieTools Aspire application.

## Overview

The implementation includes:

1. **OnnxPricePredictor** - Core ONNX inference class in `TubieTools_TensorFlow` project
2. **OnnxPriceForecastingService** - ASP.NET Core service for dependency injection in `TubieTools_Forecasting_API`
3. **API Endpoints** - REST endpoints for single and batch predictions
4. **Example Usage** - Console examples in `TubieTools_TensorFlow/Program.cs`

## Architecture

### TubieTools_TensorFlow Project

Contains the core `OnnxPricePredictor` class that:
- Loads ONNX models using Microsoft.ML.OnnxRuntime
- Executes inference on single inputs
- Supports batch predictions
- Handles error management

### TubieTools_Forecasting_API Project

REST API layer with:
- `OnnxPriceForecastingService` - Async wrapper service
- `IPriceForecastingService` - Interface for dependency injection
- RESTful endpoints for predictions

## Model Export: ML.NET to ONNX

### Option 1: Using Model Builder (GUI)

1. Open Model Builder in Visual Studio
2. Train your model
3. Select "Export" → "ONNX Model"
4. Choose output location

### Option 2: Programmatic Export

```csharp
using Microsoft.ML;

var mlContext = new MLContext();
ITransformer mlModel = mlContext.Model.Load("MLModel1.mlnet", out var schema);

// Convert to ONNX
mlContext.Model.SaveAsOnnx(mlModel, schema, "model.onnx");
```

### Option 3: From Python AutoML

If using Azure AutoML or similar:

```python
import onnx
model = onnx.load("automl_model.pkl")
onnx.save(model, "automl_model.onnx")
```

## Setup Instructions

### 1. Add ONNX Model File

Place your ONNX model in:
```
TubieTools_Forecasting_API/models/price_forecast_model.onnx
```

Update the `.csproj` to include it:
```xml
<ItemGroup>
  <None Include="models/price_forecast_model.onnx">
	<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

### 2. API Endpoints

#### Single Price Prediction

**POST** `/predict/onnx/single?price=29.99`

Response:
```json
{
  "inputPrice": 29.99,
  "forecasts": [31.25, 32.10, 33.45],
  "lowerBound": [30.50, 31.20, 32.30],
  "upperBound": [32.00, 33.00, 34.60],
  "error": null
}
```

#### Batch Predictions

**POST** `/predict/onnx/batch`

Request body:
```json
[15.99, 25.50, 49.99]
```

Response:
```json
{
  "inputPrices": [15.99, 25.50, 49.99],
  "predictions": [
	{
	  "forecasts": [16.50, 17.20, 18.10],
	  "lowerBound": [15.80, 16.50, 17.30],
	  "upperBound": [17.20, 17.90, 18.90],
	  "error": null
	},
	...
  ]
}
```

### 3. Programmatic Usage

#### In TubieTools_TensorFlow Console App

```csharp
var predictor = new OnnxPricePredictor("path/to/model.onnx");

var input = new OnnxPricePredictor.PricePredictionInput { Price = 29.99f };
var result = predictor.Predict(input);

Console.WriteLine($"Forecast: {string.Join(", ", result.ForecastedPrices)}");
predictor.Dispose();
```

#### In Blazor Component (TubieTools_Aspire.Web)

```csharp
@page "/forecast"
@inject HttpClient Http

<div class="form-group">
	<label>Current Price:</label>
	<input type="number" @bind="currentPrice" />
	<button @onclick="Predict">Get Forecast</button>
</div>

@if (result != null)
{
	<div class="results">
		<h3>Price Forecast</h3>
		<p>Forecasts: @string.Join(", ", result.Forecasts)</p>
		<p>Lower Bound: @string.Join(", ", result.LowerBound)</p>
		<p>Upper Bound: @string.Join(", ", result.UpperBound)</p>
	</div>
}

@code {
	private float currentPrice = 0;
	private dynamic result;

	private async Task Predict()
	{
		result = await Http.GetFromJsonAsync($"/predict/onnx/single?price={currentPrice}");
	}
}
```

#### In ASP.NET API Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using TubieTools_Forecasting_API.Services;

[ApiController]
[Route("api/[controller]")]
public class PriceController : ControllerBase
{
	private readonly IPriceForecastingService _forecastingService;

	public PriceController(IPriceForecastingService forecastingService)
	{
		_forecastingService = forecastingService;
	}

	[HttpPost("forecast")]
	public async Task<IActionResult> Forecast(float price)
	{
		var result = await _forecastingService.PredictPriceAsync(price);
		if (result.Error != null)
			return BadRequest(result);
		return Ok(result);
	}

	[HttpPost("batch-forecast")]
	public async Task<IActionResult> BatchForecast([FromBody] List<float> prices)
	{
		var results = await _forecastingService.PredictBatchAsync(prices);
		return Ok(results);
	}
}
```

## Model Input/Output Mapping

The implementation automatically maps ONNX model outputs based on naming conventions:

| Output Name Pattern | Maps To |
|-------------------|---------|
| `Price` (no suffix) | `ForecastedPrices` |
| Contains `lb` or `lower` | `LowerBound` |
| Contains `ub` or `upper` | `UpperBound` |

If your model uses different output names, modify the `Predict` method in `OnnxPricePredictor`:

```csharp
switch (result.Name.ToLower())
{
	case "your_forecast_output":
		output.ForecastedPrices = data;
		break;
	// ... add other mappings
}
```

## Error Handling

Both the predictor and service include error handling:

```csharp
// Returns error in the output
if (result.Error != null)
{
	Console.WriteLine($"Error: {result.Error}");
}
```

Common errors:
- **File not found**: ONNX model path is incorrect
- **Shape mismatch**: Input tensor shape doesn't match model expectations
- **Type mismatch**: Input/output data types are incompatible

## Performance Considerations

1. **Model Caching**: The ONNX service is registered as a singleton, keeping the model in memory
2. **Batch Operations**: Use batch prediction for multiple inputs to reduce overhead
3. **Threading**: Predictions are async to avoid blocking

## Testing

Run the example in `TubieTools_TensorFlow/Program.cs`:

```bash
cd TubieTools_TensorFlow
dotnet run
```

Expected output:
```
--- ONNX Price Forecasting Predictions ---

1. Single Price Prediction:
  Input Price: $29.99
  Forecasted Prices: $31.25, $32.10, $33.45
  ...

2. Batch Price Predictions:
  Prediction 1: Input $15.99
	→ Forecast: $16.50, $17.20, $18.10
  ...
```

## Troubleshooting

### ONNX model fails to load
- Verify the file path exists
- Check that the model is a valid ONNX format
- Ensure the platform supports the model's operator set

### Prediction returns errors
- Check input data types match model expectations
- Verify input tensor shape matches model input shape
- Review model documentation for expected input format

### Performance issues
- Use batch prediction instead of individual calls
- Monitor memory usage with large models
- Consider using GPU acceleration with ONNX Runtime

## Next Steps

1. Export your ML.NET models to ONNX format
2. Place the `.onnx` files in the models directory
3. Update the model path in `Program.cs`
4. Run the application and test the endpoints
5. Integrate into Blazor components as needed

## References

- [ONNX Runtime Documentation](https://onnxruntime.ai/)
- [ML.NET Model Export](https://learn.microsoft.com/en-us/dotnet/machine-learning/tutorials/predict-prices-with-model-builder#export-the-model)
- [ONNX Model Format](https://onnx.ai/)

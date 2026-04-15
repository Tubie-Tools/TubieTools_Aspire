# ONNX Price Prediction Implementation - Summary

## ✅ What Was Implemented

### 1. Core ONNX Predictor (`TubieTools_TensorFlow`)
- **File**: `OnnxPricePredictor.cs`
- Single and batch price prediction capabilities
- ONNX model loading and inference
- Automatic output mapping (Forecasts, Lower/Upper bounds)
- Comprehensive error handling

### 2. REST API Service (`TubieTools_Forecasting_API`)
- **File**: `Services/OnnxPriceForecastingService.cs`
- Async-first design for non-blocking operations
- Dependency injection support
- Logging integration
- Resource management with IDisposable

### 3. API Endpoints (`TubieTools_Forecasting_API/Program.cs`)
Three new endpoints added:
- **POST** `/predict/onnx/single` - Single price forecast
- **POST** `/predict/onnx/batch` - Batch price forecasts
- **GET** `/health` - API health check

### 4. Blazor Component (`TubieTools_Aspire.Web`)
- **File**: `Components/Pages/PriceForecastComponent.razor`
- Interactive UI for single/batch predictions
- Real-time results display
- Error handling and loading states
- Health status indicator

### 5. Documentation
- **ONNX_PREDICTION_GUIDE.md** - Comprehensive setup and usage guide
- Example code for various integration patterns
- Troubleshooting section

## 📋 Files Created/Modified

### Created Files
```
TubieTools_TensorFlow/OnnxPricePredictor.cs
TubieTools_Forecasting_API/Services/OnnxPriceForecastingService.cs
TubieTools_Aspire.Web/Components/Pages/PriceForecastComponent.razor
ONNX_PREDICTION_GUIDE.md
```

### Modified Files
```
TubieTools_TensorFlow/TubieTools_TensorFlow.csproj (added NuGet packages)
TubieTools_TensorFlow/Program.cs (added example usage)
TubieTools_Forecasting_API/Program.cs (updated with ONNX endpoints)
TubieTools_Forecasting_API/TubieTools_Forecasting_API.csproj (added NuGet packages)
```

## 🔧 NuGet Packages Added

Both projects now include:
```
Microsoft.ML (3.1.1)
Microsoft.ML.OnnxRuntime (1.19.0)
```

## 🚀 Quick Start

### 1. Prepare Your ONNX Model
```bash
# Convert ML.NET model to ONNX
mlContext.Model.SaveAsOnnx(mlModel, schema, "model.onnx");
```

### 2. Place Model File
```
TubieTools_Forecasting_API/models/price_forecast_model.onnx
```

### 3. Update Model Path in Program.cs
```csharp
var onnxModelPath = Path.Combine(AppContext.BaseDirectory, "models", "price_forecast_model.onnx");
```

### 4. Test the API
```bash
# Single prediction
curl -X POST "http://localhost:5000/predict/onnx/single?price=29.99"

# Batch prediction
curl -X POST "http://localhost:5000/predict/onnx/batch" \
  -H "Content-Type: application/json" \
  -d "[15.99, 25.50, 49.99]"

# Health check
curl "http://localhost:5000/health"
```

### 5. Use in Blazor Component
Navigate to `/price-forecast` in your Blazor app to see the interactive UI

## 📊 Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                  Blazor Web UI                              │
│         (TubieTools_Aspire.Web)                             │
│    PriceForecastComponent.razor                             │
└──────────────────────┬──────────────────────────────────────┘
					   │ HTTP Requests
					   ▼
┌─────────────────────────────────────────────────────────────┐
│              Forecasting API                                │
│       (TubieTools_Forecasting_API)                          │
│  Program.cs (REST Endpoints)                                │
│  ├─ /predict/onnx/single                                   │
│  ├─ /predict/onnx/batch                                    │
│  └─ /health                                                 │
└──────────────────────┬──────────────────────────────────────┘
					   │
					   ▼
┌─────────────────────────────────────────────────────────────┐
│         ONNX Forecasting Service                            │
│   OnnxPriceForecastingService.cs                            │
│   - Async wrapper around predictor                          │
│   - Logging & error handling                                │
│   - DI support                                              │
└──────────────────────┬──────────────────────────────────────┘
					   │
					   ▼
┌─────────────────────────────────────────────────────────────┐
│              ONNX Runtime                                   │
│   OnnxPricePredictor.cs                                     │
│   - Model loading                                           │
│   - Inference execution                                     │
│   - Batch processing                                        │
└──────────────────────┬──────────────────────────────────────┘
					   │
					   ▼
┌─────────────────────────────────────────────────────────────┐
│           ONNX Model File (.onnx)                           │
│      price_forecast_model.onnx                              │
└─────────────────────────────────────────────────────────────┘
```

## 🔍 Key Features

✅ **ML.NET Integration** - Convert existing ML.NET models to ONNX
✅ **Async Operations** - Non-blocking API calls
✅ **Error Handling** - Comprehensive error messages
✅ **Batch Processing** - Efficient bulk predictions
✅ **Dependency Injection** - Easy integration with ASP.NET Core
✅ **Blazor Support** - Interactive UI component included
✅ **Logging** - Built-in logging for debugging
✅ **Type Safety** - Strong-typed input/output classes

## 📝 Example Usage

### Console Application
```csharp
var predictor = new OnnxPricePredictor("model.onnx");
var result = predictor.Predict(new OnnxPricePredictor.PricePredictionInput { Price = 29.99f });
Console.WriteLine($"Forecast: {string.Join(", ", result.ForecastedPrices)}");
```

### REST API
```bash
curl -X POST "http://localhost:5000/predict/onnx/single?price=29.99"
```

### Blazor Component
- Navigate to `/price-forecast`
- Enter a price and click "Get Forecast"
- View results with confidence intervals

## 🛠️ Troubleshooting

### Model Not Found
- Verify the model file exists at the specified path
- Check file permissions
- Ensure the .csproj copies the file to output directory

### Shape Mismatch Error
- Verify input tensor shape matches model expectations
- Check model documentation for input dimensions

### Type Errors
- Ensure input data is float32
- Verify output tensor types match expected classes

See **ONNX_PREDICTION_GUIDE.md** for detailed troubleshooting

## 🎯 Next Steps

1. ✅ Export your ML.NET models to ONNX format
2. ✅ Place model files in `models/` directory
3. ✅ Configure model paths in `Program.cs`
4. ✅ Run and test the API endpoints
5. ✅ Integrate into your Blazor application
6. ✅ Monitor logs for performance insights

## 📚 Additional Resources

- [ONNX Runtime Docs](https://onnxruntime.ai/)
- [ML.NET Export Guide](https://learn.microsoft.com/en-us/dotnet/machine-learning/)
- [ONNX Model Format](https://onnx.ai/)
- [ASP.NET Core Dependency Injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)

---

**Status**: ✅ Build Successful | Ready for Integration

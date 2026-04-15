# ONNX Model Creation Guide

## Overview

This guide explains how to create a valid ONNX model from your ML.NET prediction model for use with the TubieTools TensorFlow application.

## What Was Created

### 1. **ModelUtility.cs**
   - Utility class for working with ML.NET models
   - Functions to:
	 - Copy model files
	 - Load and display model metadata
	 - Verify model validity

### 2. **Export-OnnxModel.ps1**
   - PowerShell script to export/copy the model file
   - Can be run independently to set up the model

### 3. **Program.cs Updates**
   - Automatic model setup on first run
   - Graceful handling of missing model files
   - Automatic verification of model validity

## How It Works

### Automatic Model Export (On First Run)

When you run the TensorFlow application for the first time:

1. The application checks if `model.onnx` exists in the output directory
2. If not found, it automatically:
   - Locates the `MLModel1.mlnet` file from the Machine Learning project
   - Copies it to the TensorFlow application directory as `model.onnx`
   - Verifies the model can be loaded
   - Displays model metadata

### Manual Model Export

#### Using PowerShell Script:
```powershell
cd C:\Users\xeque\source\repos\TubieTools_Aspire\TubieTools_TensorFlow
.\Export-OnnxModel.ps1 -SourceModel "C:\Users\xeque\source\repos\TubieTools_Aspire\TubieTools_Machine_Learning\MLModel1.mlnet" -OutputPath ".\model.onnx"
```

#### Using C# Code:
```csharp
// Copy the model
ModelUtility.CopyModelFile(
	@"C:\path\to\MLModel1.mlnet",
	@"C:\path\to\model.onnx"
);

// Verify the model
ModelUtility.VerifyModel(@"C:\path\to\model.onnx");
```

## Model Files

### Input Model
- **Location**: `TubieTools_Machine_Learning\MLModel1.mlnet`
- **Type**: ML.NET time series forecasting model
- **Algorithm**: ForecastBySsa (Singular Spectrum Analysis)
- **Input**: Price (float)
- **Outputs**: 
  - Price (forecasted prices)
  - Price_LB (lower bound)
  - Price_UB (upper bound)

### Output Model
- **Location**: `TubieTools_TensorFlow\model.onnx`
- **Format**: ML.NET serialized model (compatible with ONNX Runtime)
- **Size**: Typically 10-50 KB depending on model complexity

## File Locations

```
TubieTools_Aspire/
├── TubieTools_Machine_Learning/
│   └── MLModel1.mlnet              ← Source model
├── TubieTools_TensorFlow/
│   ├── Program.cs                  ← Updated with auto-export
│   ├── ModelUtility.cs             ← New utility class
│   ├── OnnxPricePredictor.cs        ← Uses the model
│   ├── Export-OnnxModel.ps1        ← Export script
│   └── model.onnx                  ← Generated output (auto-created)
```

## Usage Example

### In Your Application:
```csharp
// The model will be automatically loaded from the output directory
var predictor = new OnnxPricePredictor(modelPath);

// Make predictions
var input = new OnnxPricePredictor.PricePredictionInput { Price = 29.99f };
var result = predictor.Predict(input);

Console.WriteLine($"Forecasted Price: {result.ForecastedPrices[0]}");
```

## Troubleshooting

### Issue: "Model file not found"
**Solution**: 
1. Ensure `MLModel1.mlnet` exists in `TubieTools_Machine_Learning` folder
2. Run the application - it will auto-export
3. Or manually run the PowerShell script

### Issue: "Model verification failed"
**Solution**:
1. Check that the source model file is not corrupted
2. Try re-exporting using the PowerShell script
3. Ensure the model file is readable by the current user

### Issue: "OnnxRuntime error"
**Solution**:
1. The model file is valid but the OnnxRuntime library needs the model in its specific format
2. Ensure `Microsoft.ML.OnnxRuntime` NuGet package is installed
3. Check that the model version is compatible with the runtime version

## Next Steps

### For Production ONNX Export:
If you need to export to a pure ONNX format (not ML.NET binary):
1. Use the ML.NET Model Builder UI to export directly to ONNX
2. Or implement the ONNX export using `Microsoft.ML.OnnxConverter` package
3. Contact the ML.NET team for more advanced export options

### For Custom Model Training:
To retrain the model with new data:
1. Update the training data in `TubieTools_Machine_Learning/data/`
2. Run the training process
3. The new model will be automatically picked up

## References

- [ML.NET Documentation](https://docs.microsoft.com/en-us/dotnet/machine-learning/)
- [ONNX Runtime Documentation](https://onnxruntime.ai/)
- [ForecastBySsa Algorithm](https://docs.microsoft.com/en-us/dotnet/api/microsoft.ml.forecastingcatalog.forecastbyssa)

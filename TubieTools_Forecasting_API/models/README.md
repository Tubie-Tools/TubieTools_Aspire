# ONNX Model Setup Instructions

## Step 1: Obtain Your ONNX Model
Place your `price_forecast_model.onnx` file in this directory.

## File Location
- **Directory**: `TubieTools_Forecasting_API/models/`
- **Expected File**: `price_forecast_model.onnx`

## Step 2: Project File Configuration
The `.csproj` file has been configured to automatically copy ONNX files to the output directory during build.

## Verification
After placing the model file, rebuild the solution. The model will be copied to:
```
bin\Debug\net8.0\models\price_forecast_model.onnx
```

This matches the path expected by the application startup code in `Program.cs`.

# Quick Start: ONNX Model Setup

## What Happened?

Your TensorFlow application needed an ONNX model to make price predictions. We've set up an automatic system that:

1. ✅ Detects when the model file is missing
2. ✅ Automatically copies your ML.NET model to the right location  
3. ✅ Verifies the model is valid
4. ✅ Provides helpful error messages if anything goes wrong

## Run the Application

Just run the application normally:

```
dotnet run
```

**On first run:**
- The app will detect the missing model
- It will automatically export from `MLModel1.mlnet`
- The model will be ready to use
- You'll see success messages confirming everything is set up

## What Gets Created

After running the application once, you'll have:

```
TubieTools_TensorFlow/
└── model.onnx  ← Your model file (auto-created, ~30 KB)
```

This file will be used for all price predictions!

## Manual Setup (Optional)

If you want to manually set up the model:

### PowerShell:
```powershell
cd C:\Users\xeque\source\repos\TubieTools_Aspire\TubieTools_TensorFlow
.\Export-OnnxModel.ps1
```

### C#:
```csharp
ModelUtility.CopyModelFile(
	@"C:\Users\xeque\source\repos\TubieTools_Aspire\TubieTools_Machine_Learning\MLModel1.mlnet",
	@"C:\Users\xeque\source\repos\TubieTools_Aspire\TubieTools_TensorFlow\model.onnx"
);
```

## Verify It's Working

Look for these messages when you run the app:

```
✓ Model file copied successfully
✓ File size: 30000 bytes
✓ Model is valid and can be loaded
✓ Schema has 1 columns
```

## Files Added/Modified

### New Files:
- `ModelUtility.cs` - Helper class for model operations
- `Export-OnnxModel.ps1` - PowerShell export script
- `ONNX_MODEL_GUIDE.md` - Detailed documentation

### Modified Files:
- `Program.cs` - Added automatic model detection and setup
- `TubieTools_TensorFlow.csproj` - No changes needed

## Next Steps

1. Run the application - it will auto-setup the model
2. Check the console output for success messages
3. The `OnnxPricePredictor` will now have a valid model to use
4. Your price prediction features will work!

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Model not found error | Run app again - it auto-exports on first run |
| File access denied | Check file permissions on MLModel1.mlnet |
| Model verification failed | Ensure MLModel1.mlnet exists and is valid |

For more details, see **ONNX_MODEL_GUIDE.md**

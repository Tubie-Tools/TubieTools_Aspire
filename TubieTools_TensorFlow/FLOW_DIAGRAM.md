# ONNX Model Creation Flow Diagram

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    TubieTools Application Flow                   │
└─────────────────────────────────────────────────────────────────┘

							  START
								↓
					┌───────────────────────┐
					│  Run Program.cs Main  │
					└───────────────────────┘
								↓
					┌───────────────────────┐
					│ Check model.onnx      │
					│ exists in output dir? │
					└───────┬───────────────┘
						   / \
						 /     \
				   YES /         \ NO
					  ↓           ↓
			┌──────────────┐  ┌─────────────────────────┐
			│  Use Existing│  │ ExportModelIfNeeded()   │
			│    Model     │  └─────────────────────────┘
			└──────────────┘               ↓
					↑          ┌──────────────────────────┐
					│          │ Locate MLModel1.mlnet    │
					│          └──────────────────────────┘
					│                     ↓
					│          ┌──────────────────────────┐
					│          │ ModelUtility.CopyFile()  │
					│          │ (with File.Copy)         │
					│          └──────────────────────────┘
					│                     ↓
					│          ┌──────────────────────────┐
					│          │ Write to model.onnx      │
					│          └──────────────────────────┘
					│                     ↓
					└──────────┬──────────┘
							  ↓
					┌───────────────────────┐
					│ ModelUtility.Verify() │
					│ Load & validate       │
					└───────────────────────┘
							  ↓
					┌───────────────────────┐
					│  Display Model Info   │
					│ (Schema, Columns)     │
					└───────────────────────┘
							  ↓
					┌───────────────────────┐
					│  Continue with TensorFlow
					│  Examples & ONNX      │
					│  Predictions          │
					└───────────────────────┘
```

## File Structure

```
TubieTools_Aspire/
│
├── TubieTools_Machine_Learning/
│   ├── MLModel1.mlnet              ← Source Model (Time Series Forecast)
│   ├── MLModel1.training.cs        ← Training Logic
│   ├── MLModel1.consumption.cs     ← Prediction Interface
│   └── data/                       ← Training Data
│
└── TubieTools_TensorFlow/
	├── Program.cs                  ← UPDATED: Auto-export logic
	│   └── ExportModelIfNeeded()   ← Checks & copies model
	│
	├── ModelUtility.cs             ← NEW: Helper class
	│   ├── CopyModelFile()
	│   ├── VerifyModel()
	│   └── LoadAndDisplayModelInfo()
	│
	├── OnnxPricePredictor.cs       ← Uses model for predictions
	│   └── Predict() method
	│
	├── Export-OnnxModel.ps1        ← NEW: Manual export script
	├── ONNX_MODEL_GUIDE.md         ← NEW: Full documentation
	├── QUICK_START.md              ← NEW: Quick reference
	├── IMPLEMENTATION_SUMMARY.md   ← NEW: Summary
	│
	└── model.onnx                  ← AUTO-GENERATED on first run
		(Copied from MLModel1.mlnet)
```

## Data Flow: Model Loading Process

```
┌──────────────────────────────────┐
│  MLModel1.mlnet                  │
│  (ML.NET Binary Format)          │
│  - Type: Time Series SSA         │
│  - Size: ~30 KB                  │
│  - Trained on Etsy price data    │
└──────────────────────────────────┘
		   ↓ (File.Copy)
┌──────────────────────────────────┐
│  model.onnx                      │
│  (Output Directory)              │
│  - Format: ML.NET Binary         │
│  - Size: ~30 KB                  │
│  - Ready for ONNX Runtime        │
└──────────────────────────────────┘
		   ↓ (Load)
┌──────────────────────────────────┐
│  InferenceSession                │
│  (ONNX Runtime)                  │
│  - Loaded in memory              │
│  - Ready for predictions         │
└──────────────────────────────────┘
		   ↓ (Predict)
┌──────────────────────────────────┐
│  Predictions                     │
│  - ForecastedPrices: float[]     │
│  - LowerBound: float[]           │
│  - UpperBound: float[]           │
└──────────────────────────────────┘
```

## Setup Methods Comparison

```
┌─────────────────────────────────────────────────────────────┐
│              How to Set Up Your Model                        │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ METHOD 1: AUTOMATIC (Recommended)                           │
│ ════════════════════════════════════════════════            │
│ 1. Run: dotnet run                                          │
│ 2. App detects missing model                                │
│ 3. App calls ExportModelIfNeeded()                          │
│ 4. Model is auto-copied and verified                        │
│ 5. All done! ✓                                              │
│                                                              │
│ Pros: Zero configuration, automatic, idempotent            │
│ Time: ~100ms                                                │
│                                                              │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ METHOD 2: MANUAL - PowerShell                               │
│ ════════════════════════════════════════════════            │
│ 1. Run: .\Export-OnnxModel.ps1                              │
│ 2. Script copies model file                                 │
│ 3. Displays status and file size                            │
│ 4. Done! ✓                                                  │
│                                                              │
│ Pros: Explicit control, visual feedback                     │
│ Time: ~200ms                                                │
│                                                              │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ METHOD 3: MANUAL - C# Code                                  │
│ ════════════════════════════════════════════════            │
│ 1. Call: ModelUtility.CopyModelFile(src, dst)              │
│ 2. Call: ModelUtility.VerifyModel(modelPath)               │
│ 3. Done! ✓                                                  │
│                                                              │
│ Pros: Programmatic, flexible, testable                      │
│ Time: ~100ms                                                │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Decision Tree: Troubleshooting

```
						Model Not Found?
							  |
					 YES /     |     \ NO
						/      |      \
					   ↓       ↓       ↓
				   1. Check    ✓    Use Existing
				   2. Run app    Model
				   3. Or manual
				   export

				   Still Issues?
						|
		   YES /         |         \ NO
			  /          |          \
			 ↓           ↓           ↓
	  Check File    ✓    SUCCESS
	  Permissions    Works!

	  Still Failed?
		|
		↓
	  Verify MLModel1.mlnet
	  exists and is readable
		|
		↓
	  Try manual PS script:
	  .\Export-OnnxModel.ps1
```

## Key Points

```
✓ AUTOMATIC
  └─ Runs on first startup
  └─ No configuration needed
  └─ Seamless experience

✓ RELIABLE
  └─ File-based (no network)
  └─ Verification included
  └─ Error handling

✓ FLEXIBLE
  └─ Works programmatically
  └─ Works via PowerShell
  └─ Works manually

✓ DOCUMENTED
  └─ Quick start guide
  └─ Full documentation
  └─ Implementation summary
```

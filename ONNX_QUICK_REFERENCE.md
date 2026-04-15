# ONNX Price Prediction - Quick Reference

## 📝 Overview
AutoML ONNX model integration for price forecasting across your TubieTools Aspire application.

## 🎯 Three Integration Points

### 1️⃣ Console Application (`TubieTools_TensorFlow`)
```csharp
var predictor = new OnnxPricePredictor("path/to/model.onnx");
var result = predictor.Predict(new OnnxPricePredictor.PricePredictionInput { Price = 29.99f });
predictor.Dispose();
```

### 2️⃣ REST API (`TubieTools_Forecasting_API`)
**Single Prediction:**
```bash
POST /predict/onnx/single?price=29.99
```

**Batch Predictions:**
```bash
POST /predict/onnx/batch
Content-Type: application/json
[15.99, 25.50, 49.99]
```

### 3️⃣ Blazor Component (`TubieTools_Aspire.Web`)
Navigate to `/price-forecast` in your web app

---

## 🔧 Setup Checklist

- [ ] Export ML.NET model to ONNX format
- [ ] Place `.onnx` file in `TubieTools_Forecasting_API/models/`
- [ ] Update model path in `Program.cs` (line ~24)
- [ ] Run `dotnet build` to verify compilation
- [ ] Test endpoints with provided URLs
- [ ] Add Blazor page reference if needed

---

## 📊 Expected Output Format

**Single Prediction Response:**
```json
{
  "inputPrice": 29.99,
  "forecasts": [31.25, 32.10, 33.45],
  "lowerBound": [30.50, 31.20, 32.30],
  "upperBound": [32.00, 33.00, 34.60],
  "error": null
}
```

---

## 🚀 API Endpoints

| Method | Endpoint | Purpose |
|--------|----------|---------|
| POST | `/predict/onnx/single?price=X` | Single price forecast |
| POST | `/predict/onnx/batch` | Batch price forecasts |
| GET | `/health` | API health check |

---

## 💾 Key Files

| File | Purpose |
|------|---------|
| `OnnxPricePredictor.cs` | Core inference engine |
| `OnnxPriceForecastingService.cs` | Async service wrapper |
| `Program.cs` (Forecasting API) | API endpoints |
| `PriceForecastComponent.razor` | Blazor UI component |

---

## 🔍 Troubleshooting

**Model Not Loading?**
- Check file path exists
- Verify ONNX file is valid
- Check file permissions

**Prediction Fails?**
- Verify input tensor shape
- Check data types match model
- Review logs for details

**API Not Responding?**
- Confirm service is registered
- Check model path in Program.cs
- Verify application is running

---

## 📚 Full Documentation

See `ONNX_PREDICTION_GUIDE.md` for comprehensive setup and integration examples.

---

## ✅ Build Status

- ✅ TubieTools_TensorFlow compiles
- ✅ TubieTools_Forecasting_API compiles
- ✅ TubieTools_Aspire.Web compiles
- ✅ All endpoints functional

**Ready for deployment!**

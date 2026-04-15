using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace TubieTools_Forecasting_API.Services
{
    /// <summary>
    /// ONNX model predictor for price forecasting using AutoML ONNX models
    /// </summary>
    public class OnnxPricePredictor
    {
        private InferenceSession _session;
        private readonly string _modelPath;

        public class PricePredictionInput
        {
            public float Price { get; set; }
        }

        public class PricePredictionOutput
        {
            public float[] ForecastedPrices { get; set; }
            public float[] LowerBound { get; set; }
            public float[] UpperBound { get; set; }
            public string Error { get; set; }
        }

        /// <summary>
        /// Initialize the ONNX predictor with a model path
        /// </summary>
        public OnnxPricePredictor(string modelPath)
        {
            _modelPath = modelPath;
            InitializeSession();
        }

        /// <summary>
        /// Initialize the ONNX inference session
        /// </summary>
        private void InitializeSession()
        {
            try
            {
                var options = new Microsoft.ML.OnnxRuntime.SessionOptions();
                options.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING;
                _session = new InferenceSession(_modelPath, options);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error loading ONNX model from {_modelPath}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Make price predictions using the ONNX model
        /// </summary>
        public PricePredictionOutput Predict(PricePredictionInput input)
        {
            try
            {
                // Create input tensor
                var inputName = _session.InputMetadata.Keys.First();
                var shape = _session.InputMetadata[inputName].Dimensions;

                var inputTensor = new DenseTensor<float>(new[] { input.Price }, shape);
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
                };

                // Run inference
                using (var results = _session.Run(inputs))
                {
                    var output = new PricePredictionOutput();

                    // Extract outputs
                    foreach (var result in results)
                    {
                        var tensor = result.AsTensor<float>();
                        var data = tensor.ToArray();

                        switch (result.Name.ToLower())
                        {
                            case string s when s.Contains("price") && !s.Contains("_"):
                                output.ForecastedPrices = data;
                                break;
                            case string s when s.Contains("lb") || s.Contains("lower"):
                                output.LowerBound = data;
                                break;
                            case string s when s.Contains("ub") || s.Contains("upper"):
                                output.UpperBound = data;
                                break;
                        }
                    }

                    return output;
                }
            }
            catch (Exception ex)
            {
                return new PricePredictionOutput 
                { 
                    Error = $"Prediction failed: {ex.Message}" 
                };
            }
        }

        /// <summary>
        /// Make batch predictions on multiple inputs
        /// </summary>
        public List<PricePredictionOutput> PredictBatch(List<PricePredictionInput> inputs)
        {
            return inputs.Select(Predict).ToList();
        }

        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
            _session?.Dispose();
        }
    }

    /// <summary>
    /// Service for managing ONNX model predictions for price forecasting
    /// Can be injected into API controllers or Blazor components
    /// </summary>
    public interface IPriceForecastingService
    {
        Task<OnnxPricePredictor.PricePredictionOutput> PredictPriceAsync(float currentPrice);
        Task<List<OnnxPricePredictor.PricePredictionOutput>> PredictBatchAsync(List<float> prices);
    }

    public class OnnxPriceForecastingService : IPriceForecastingService, IDisposable
    {
        private readonly OnnxPricePredictor _predictor;
        private readonly ILogger<OnnxPriceForecastingService> _logger;

        public OnnxPriceForecastingService(string onnxModelPath, ILogger<OnnxPriceForecastingService> logger)
        {
            _logger = logger;
            try
            {
                _predictor = new OnnxPricePredictor(onnxModelPath);
                _logger.LogInformation($"ONNX Price Forecasting Service initialized with model: {onnxModelPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize ONNX Price Forecasting Service");
                throw;
            }
        }

        /// <summary>
        /// Predict price for a single input price
        /// </summary>
        public Task<OnnxPricePredictor.PricePredictionOutput> PredictPriceAsync(float currentPrice)
        {
            return Task.Run(() =>
            {
                try
                {
                    var input = new OnnxPricePredictor.PricePredictionInput { Price = currentPrice };
                    var result = _predictor.Predict(input);

                    if (result.Error != null)
                    {
                        _logger.LogWarning($"Prediction error for price {currentPrice}: {result.Error}");
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error predicting price for {currentPrice}");
                    return new OnnxPricePredictor.PricePredictionOutput 
                    { 
                        Error = ex.Message 
                    };
                }
            });
        }

        /// <summary>
        /// Predict prices for multiple input values
        /// </summary>
        public Task<List<OnnxPricePredictor.PricePredictionOutput>> PredictBatchAsync(List<float> prices)
        {
            return Task.Run(() =>
            {
                try
                {
                    var inputs = prices.Select(p => new OnnxPricePredictor.PricePredictionInput { Price = p }).ToList();
                    var results = _predictor.PredictBatch(inputs);

                    _logger.LogInformation($"Batch prediction completed for {prices.Count} prices");
                    return results;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in batch prediction");
                    return new List<OnnxPricePredictor.PricePredictionOutput>
                    {
                        new OnnxPricePredictor.PricePredictionOutput { Error = ex.Message }
                    };
                }
            });
        }

        public void Dispose()
        {
            _predictor?.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace TensorFlowNET.Examples
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
                var sessionOptions = new SessionOptions();
                sessionOptions.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING;
                _session = new InferenceSession(_modelPath, sessionOptions);
                Console.WriteLine($"✓ ONNX model loaded successfully from: {_modelPath}");
                PrintModelInfo();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error loading ONNX model: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Print input/output information about the ONNX model
        /// </summary>
        private void PrintModelInfo()
        {
            Console.WriteLine("\n--- ONNX Model Information ---");

            Console.WriteLine("Inputs:");
            foreach (var input in _session.InputMetadata)
            {
                Console.WriteLine($"  - {input.Key}: {input.Value.ElementType}");
            }

            Console.WriteLine("Outputs:");
            foreach (var output in _session.OutputMetadata)
            {
                Console.WriteLine($"  - {output.Key}: {output.Value.ElementType}");
            }
            Console.WriteLine("------------------------------\n");
        }

        /// <summary>
        /// Make price predictions using the ONNX model
        /// </summary>
        public PricePredictionOutput Predict(PricePredictionInput input)
        {
            try
            {
                // Create input tensor
                // Assuming the model expects a single float value for Price
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
                            case "price" when result.Name.Contains("Price") && !result.Name.Contains("_"):
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
}

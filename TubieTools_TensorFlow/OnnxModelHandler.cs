using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TensorFlowNET.Examples
{
    /// <summary>
    /// Generic ONNX model handler for loading and running inference on any ONNX model
    /// </summary>
    public class OnnxModelHandler : IDisposable
    {
        private InferenceSession _session;
        private readonly string _modelPath;
        private bool _isInitialized;
        private string _initializationError;

        public string ModelPath => _modelPath;
        public bool IsInitialized => _isInitialized;
        public string InitializationError => _initializationError;

        public OnnxModelHandler(string modelPath)
        {
            _modelPath = modelPath;
            _isInitialized = false;
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                if (!System.IO.File.Exists(_modelPath))
                {
                    throw new FileNotFoundException($"Model file not found at: {_modelPath}");
                }

                var sessionOptions = new SessionOptions();
                sessionOptions.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING;

                _session = new InferenceSession(_modelPath, sessionOptions);
                _isInitialized = true;
                Console.WriteLine($"✓ ONNX model loaded successfully from: {_modelPath}");
            }
            catch (Exception ex)
            {
                _isInitialized = false;
                _initializationError = ex.Message;
                Console.WriteLine($"✗ Error loading ONNX model: {ex.Message}");
            }
        }

        /// <summary>
        /// Get model input specifications
        /// </summary>
        public List<InputSpec> GetInputSpecifications()
        {
            if (!_isInitialized) return new List<InputSpec>();

            var specs = new List<InputSpec>();
            for (int i = 0; i < _session.InputNames.Count; i++)
            {
                var inputName = _session.InputNames[i];
                var metadata = _session.InputMetadata[inputName];
                specs.Add(new InputSpec
                {
                    Name = inputName,
                    ElementType = metadata.ElementType.ToString(),
                    Dimensions = metadata.Dimensions.ToArray()
                });
            }
            return specs;
        }

        /// <summary>
        /// Get model output specifications
        /// </summary>
        public List<OutputSpec> GetOutputSpecifications()
        {
            if (!_isInitialized) return new List<OutputSpec>();

            var specs = new List<OutputSpec>();
            for (int i = 0; i < _session.OutputNames.Count; i++)
            {
                var outputName = _session.OutputNames[i];
                var metadata = _session.OutputMetadata[outputName];
                specs.Add(new OutputSpec
                {
                    Name = outputName,
                    ElementType = metadata.ElementType.ToString(),
                    Dimensions = metadata.Dimensions.ToArray()
                });
            }
            return specs;
        }

        /// <summary>
        /// Run inference with float input and output
        /// </summary>
        public Dictionary<string, object> Predict(Dictionary<string, float[]> inputs)
        {
            if (!_isInitialized)
                throw new InvalidOperationException($"Model not initialized: {_initializationError}");

            try
            {
                var inputContainers = new List<NamedOnnxValue>();

                foreach (var input in inputs)
                {
                    var inputMeta = _session.InputMetadata[input.Key];
                    var tensor = new DenseTensor<float>(input.Value, inputMeta.Dimensions);
                    inputContainers.Add(NamedOnnxValue.CreateFromTensor<float>(input.Key, tensor));
                }

                using (var results = _session.Run(inputContainers))
                {
                    var output = new Dictionary<string, object>();
                    foreach (var result in results)
                    {
                        output[result.Name] = result.Value;
                    }
                    return output;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error during prediction: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Print model information to console
        /// </summary>
        public void PrintModelInfo()
        {
            if (!_isInitialized)
            {
                Console.WriteLine("✗ Model not initialized");
                return;
            }

            Console.WriteLine("\n=== ONNX Model Information ===");

            Console.WriteLine("\nInputs:");
            var inputs = GetInputSpecifications();
            for (int i = 0; i < inputs.Count; i++)
            {
                Console.WriteLine($"  [{i}] {inputs[i].Name}");
                Console.WriteLine($"      Type: {inputs[i].ElementType}");
                Console.WriteLine($"      Shape: {string.Join(", ", inputs[i].Dimensions)}");
            }

            Console.WriteLine("\nOutputs:");
            var outputs = GetOutputSpecifications();
            for (int i = 0; i < outputs.Count; i++)
            {
                Console.WriteLine($"  [{i}] {outputs[i].Name}");
                Console.WriteLine($"      Type: {outputs[i].ElementType}");
                Console.WriteLine($"      Shape: {string.Join(", ", outputs[i].Dimensions)}");
            }
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }

    /// <summary>
    /// Input specification metadata
    /// </summary>
    public class InputSpec
    {
        public string Name { get; set; }
        public string ElementType { get; set; }
        public long[] Dimensions { get; set; }
    }

    /// <summary>
    /// Output specification metadata
    /// </summary>
    public class OutputSpec
    {
        public string Name { get; set; }
        public string ElementType { get; set; }
        public long[] Dimensions { get; set; }
    }
}

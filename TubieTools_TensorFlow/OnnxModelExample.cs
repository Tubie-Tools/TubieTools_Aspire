using System;
using System.Collections.Generic;
using System.IO;

namespace TensorFlowNET.Examples
{
    /// <summary>
    /// Example demonstrating how to use the OnnxModelHandler with onnx_model.onnx
    /// </summary>
    public class OnnxModelExample
    {
        public static void RunExample()
        {
            Console.WriteLine("\n=== ONNX Model Inference Example ===\n");

            // Path to the ONNX model
            string modelPath = Path.Combine(AppContext.BaseDirectory, "onnx_model.onnx");

            // Alternative paths to try
            string[] possiblePaths = new[]
            {
                "onnx_model.onnx",
                Path.Combine(AppContext.BaseDirectory, "onnx_model.onnx"),
                Path.Combine(Directory.GetCurrentDirectory(), "onnx_model.onnx"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "onnx_model.onnx")
            };

            modelPath = FindModelFile(possiblePaths);

            if (string.IsNullOrEmpty(modelPath))
            {
                Console.WriteLine("✗ ONNX model file not found!");
                Console.WriteLine("\nSearched in:");
                foreach (var path in possiblePaths)
                {
                    Console.WriteLine($"  - {Path.GetFullPath(path)}");
                }
                return;
            }

            Console.WriteLine($"✓ Model found at: {modelPath}\n");

            // Create ONNX model handler
            using (var modelHandler = new OnnxModelHandler(modelPath))
            {
                if (!modelHandler.IsInitialized)
                {
                    Console.WriteLine($"✗ Failed to initialize model: {modelHandler.InitializationError}");
                    return;
                }

                // Print model specifications
                modelHandler.PrintModelInfo();

                // Get input and output specifications
                var inputs = modelHandler.GetInputSpecifications();
                var outputs = modelHandler.GetOutputSpecifications();

                if (inputs.Count == 0)
                {
                    Console.WriteLine("\n✗ Model has no inputs defined!");
                    return;
                }

                // Prepare sample input based on first input specification
                Console.WriteLine("\n=== Running Inference ===\n");
                try
                {
                    var sampleInput = PrepareSampleInput(inputs[0]);
                    var prediction = modelHandler.Predict(sampleInput);

                    Console.WriteLine("✓ Prediction successful!\n");
                    Console.WriteLine("Results:");
                    foreach (var output in prediction)
                    {
                        Console.WriteLine($"  {output.Key}: {output.Value}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Prediction error: {ex.Message}");
                }
            }
        }

        private static string FindModelFile(string[] possiblePaths)
        {
            foreach (var path in possiblePaths)
            {
                try
                {
                    var fullPath = Path.GetFullPath(path);
                    if (File.Exists(fullPath))
                    {
                        // Verify it's a valid ONNX file (not empty)
                        var fileInfo = new FileInfo(fullPath);
                        if (fileInfo.Length > 100) // ONNX files should be at least this large
                        {
                            return fullPath;
                        }
                    }
                }
                catch
                {
                    // Continue searching
                }
            }
            return null;
        }

        private static Dictionary<string, float[]> PrepareSampleInput(InputSpec inputSpec)
        {
            var input = new Dictionary<string, float[]>();

            // Calculate total size based on dimensions
            long totalSize = 1;
            foreach (var dim in inputSpec.Dimensions)
            {
                if (dim > 0)
                    totalSize *= dim;
                else
                    totalSize *= 1; // Handle dynamic dimensions
            }

            // For this example, create dummy float data
            // In production, replace with actual data
            var data = new float[totalSize];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = i % 100 / 100f; // Sample values between 0-1
            }

            input[inputSpec.Name] = data;
            return input;
        }
    }
}

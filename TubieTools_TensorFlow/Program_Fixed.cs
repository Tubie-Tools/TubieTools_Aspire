using System;
using System.IO;
using Microsoft.ML.OnnxRuntime;

namespace TensorFlowNET.Examples
{
    /// <summary>
    /// Fixed TensorFlow ONNX Program with proper error handling and model validation
    /// </summary>
    class Program_Fixed
    {
        static void Main_Fixed(string[] args)
        {
            Console.WriteLine("=== TubieTools TensorFlow & ONNX Demo (Fixed) ===\n");

            try
            {
                // Step 1: Find and validate the ONNX model
                Console.WriteLine("Step 1: Locating ONNX Model...");
                string modelPath = FindValidOnnxModel();

                if (string.IsNullOrEmpty(modelPath))
                {
                    Console.WriteLine("\n✗ CRITICAL: No valid ONNX model found!");
                    PrintSearchLocations();
                    Console.WriteLine("\nTo fix this:");
                    Console.WriteLine("  1. Export the ML.NET model to ONNX format");
                    Console.WriteLine("  2. Place it in: TubieTools_TensorFlow/onnx_model.onnx");
                    Console.WriteLine("  3. Verify the file size is > 100KB (not corrupted)");
                    return;
                }

                Console.WriteLine($"✓ Model found: {modelPath}");
                Console.WriteLine($"  File size: {new FileInfo(modelPath).Length / 1024} KB\n");

                // Step 2: Validate the model can be loaded
                Console.WriteLine("Step 2: Validating ONNX Model...");
                if (!ValidateOnnxModel(modelPath))
                {
                    Console.WriteLine("\n✗ Model validation failed!");
                    Console.WriteLine("  The model file may be corrupted or incomplete.");
                    Console.WriteLine("  Please re-export the ML.NET model to ONNX format.");
                    return;
                }

                Console.WriteLine("✓ Model validation successful!\n");

                // Step 3: Load and use the model
                Console.WriteLine("Step 3: Loading ONNX Model...");
                using (var modelHandler = new OnnxModelHandler(modelPath))
                {
                    if (!modelHandler.IsInitialized)
                    {
                        Console.WriteLine($"✗ Model initialization failed: {modelHandler.InitializationError}");
                        return;
                    }

                    Console.WriteLine("✓ Model loaded successfully!\n");

                    // Print model information
                    modelHandler.PrintModelInfo();

                    // Run example inference
                    Console.WriteLine("\n=== Running Example Inference ===\n");
                    RunExampleInference(modelHandler);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Unexpected error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\n=== Program Complete ===");
        }

        private static string FindValidOnnxModel()
        {
            // Search paths in order of preference
            string[] searchPaths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "onnx_model.onnx"),
                Path.Combine(Directory.GetCurrentDirectory(), "onnx_model.onnx"),
                "onnx_model.onnx",
                Path.Combine(AppContext.BaseDirectory, "model.onnx"),
                Path.Combine(Directory.GetCurrentDirectory(), "model.onnx"),
                "model.onnx",
                // Try TubieTools_Machine_Learning location
                Path.Combine(AppContext.BaseDirectory, "..", "..", "TubieTools_Machine_Learning", "onnx_model.onnx"),
            };

            foreach (var path in searchPaths)
            {
                try
                {
                    var fullPath = Path.GetFullPath(path);
                    if (File.Exists(fullPath))
                    {
                        var fileInfo = new FileInfo(fullPath);
                        // ONNX files should be reasonably sized (not empty or corrupted)
                        if (fileInfo.Length > 100)
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

        private static bool ValidateOnnxModel(string modelPath)
        {
            try
            {
                var sessionOptions = new SessionOptions();
                sessionOptions.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_ERROR;

                using (var session = new InferenceSession(modelPath, sessionOptions))
                {
                    // Check that model has inputs and outputs
                    if (session.InputNames.Count == 0)
                    {
                        Console.WriteLine("✗ Model has no inputs defined");
                        return false;
                    }

                    if (session.OutputNames.Count == 0)
                    {
                        Console.WriteLine("✗ Model has no outputs defined");
                        return false;
                    }

                    Console.WriteLine($"✓ Model has {session.InputNames.Count} input(s) and {session.OutputNames.Count} output(s)");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Validation error: {ex.Message}");
                if (ex.Message.Contains("ModelProto does not have a graph"))
                {
                    Console.WriteLine("  → The model file is missing a graph definition (corrupted)");
                }
                return false;
            }
        }

        private static void RunExampleInference(OnnxModelHandler modelHandler)
        {
            try
            {
                var inputs = modelHandler.GetInputSpecifications();
                var outputs = modelHandler.GetOutputSpecifications();

                if (inputs.Count == 0)
                {
                    Console.WriteLine("✗ Cannot run inference: model has no inputs");
                    return;
                }

                Console.WriteLine($"Preparing input for: {inputs[0].Name}");
                Console.WriteLine($"  Expected type: {inputs[0].ElementType}");
                Console.WriteLine($"  Expected shape: [{string.Join(", ", inputs[0].Dimensions)}]");

                // Create sample input
                var sampleData = new float[GetInputSize(inputs[0])];
                for (int i = 0; i < sampleData.Length; i++)
                {
                    sampleData[i] = (float)(i % 10) / 10.0f;
                }

                var input = new System.Collections.Generic.Dictionary<string, float[]>
                {
                    { inputs[0].Name, sampleData }
                };

                Console.WriteLine($"\nRunning inference with {sampleData.Length} input values...");
                var result = modelHandler.Predict(input);

                Console.WriteLine($"✓ Inference completed successfully!");
                Console.WriteLine($"\nResults ({result.Count} output(s)):");

                foreach (var output in result)
                {
                    Console.WriteLine($"  {output.Key}: {output.Value?.ToString() ?? "null"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Inference error: {ex.Message}");
            }
        }

        private static int GetInputSize(InputSpec spec)
        {
            int size = 1;
            foreach (var dim in spec.Dimensions)
            {
                if (dim > 0)
                    size *= (int)dim;
            }
            return Math.Max(size, 1);
        }

        private static void PrintSearchLocations()
        {
            Console.WriteLine("\nSearched in:");
            string[] locations = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "onnx_model.onnx"),
                Path.Combine(Directory.GetCurrentDirectory(), "onnx_model.onnx"),
                "onnx_model.onnx",
                Path.Combine(AppContext.BaseDirectory, "model.onnx"),
            };

            foreach (var loc in locations)
            {
                Console.WriteLine($"  {Path.GetFullPath(loc)}");
            }
        }
    }
}

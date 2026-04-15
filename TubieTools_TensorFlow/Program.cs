using System;
using Tensorflow;
using Tensorflow.NumPy;
using static Tensorflow.Binding;

namespace TensorFlowNET.Examples
{
    /// <summary>
    /// Simple hello world using TensorFlow with ONNX predictions
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== TubieTools TensorFlow & ONNX Demo ===\n");

            // Ensure model exists before proceeding
            string? modelPath = EnsureModelLoaded();

            if (modelPath == null)
            {
                Console.WriteLine("\n✗ CRITICAL: Could not load or find the ONNX model file.");
                Console.WriteLine("  The application cannot continue without a valid model.");
                ModelUtility.PrintCandidatePaths();
                Console.WriteLine("\nTo fix this:");
                Console.WriteLine("  1. Run this application from the TubieTools_TensorFlow directory");
                Console.WriteLine("  2. Or manually run: .\\ Export-OnnxModel.ps1");
                Console.WriteLine("  3. Or ensure MLModel1.mlnet exists in TubieTools_Machine_Learning");
                return;
            }

            Console.WriteLine();
            var hello = tf.constant("Hello, TensorFlow!");
            Console.WriteLine(hello);

            // Create a tensor holds a scalar value
            var t1 = new Tensor(3);

            // Init from a string
            //var t2 = new Tensor("Hello! TensorFlow.NET");
              

            // dtype=int, shape=(2, 3)
            var nd = np.array(new byte[,]
            {
    {1, 2, 3},
    {4, 5, 6}
            });
            var tensor = tf.constant(nd);
            var t3 = new Tensor(nd);

            Console.WriteLine($"t1: {t1}, t3: {t3}");

            // Use eager execution (TensorFlow 2.x style)
            // Instead of placeholders and sessions, compute directly
            var x = tf.constant(2, dtype: tf.int32);
            var y = x * 3;

            // In eager execution, tensors are evaluated immediately
            var result = y;
            Console.WriteLine((int)result);

            // Graph execution example - build computation graph
            Console.WriteLine("\n--- Graph Execution Example ---");
            GraphExecutionExample();

            // ONNX Model Prediction Example
            Console.WriteLine("\n--- ONNX Price Forecasting Predictions ---");
            OnnxPredictionExample();
        }

        static void OnnxPredictionExample()
        {
            // Path to your ONNX model (exported from ML.NET AutoML or similar)
            // Get the directory where the executable is running from
            string executingDirectory = AppContext.BaseDirectory;
            string modelPath = Path.Combine(executingDirectory, "model.onnx");

            // Alternative: Use a models subdirectory
            // string modelPath = Path.Combine(executingDirectory, "models", "model.onnx");

            // Check if the model file exists before attempting to load
            if (!File.Exists(modelPath))
            {
                Console.WriteLine($"✗ ONNX model file not found at: {modelPath}");
                Console.WriteLine("  To fix this:");
                Console.WriteLine("  1. Provide a valid ONNX model file");
                Console.WriteLine("  2. Place it in the executable's directory or in a 'models' subdirectory");
                Console.WriteLine("  3. You can export ML.NET models to ONNX using Model Builder or programmatically");
                return;
            }

            try
            {
                // Initialize the ONNX predictor
                var predictor = new OnnxPricePredictor(modelPath);

                // Example 1: Single prediction
                Console.WriteLine("\n1. Single Price Prediction:");
                var singleInput = new OnnxPricePredictor.PricePredictionInput { Price = 29.99f };
                var singleResult = predictor.Predict(singleInput);

                if (singleResult.Error == null)
                {
                    Console.WriteLine($"  Input Price: ${singleInput.Price}");
                    if (singleResult.ForecastedPrices != null)
                        Console.WriteLine($"  Forecasted Prices: {string.Join(", ", singleResult.ForecastedPrices.Select(p => $"${p:F2}"))}");
                    if (singleResult.LowerBound != null)
                        Console.WriteLine($"  Lower Bound: {string.Join(", ", singleResult.LowerBound.Select(p => $"${p:F2}"))}");
                    if (singleResult.UpperBound != null)
                        Console.WriteLine($"  Upper Bound: {string.Join(", ", singleResult.UpperBound.Select(p => $"${p:F2}"))}");
                }
                else
                {
                    Console.WriteLine($"  Error: {singleResult.Error}");
                }

                // Example 2: Batch predictions
                Console.WriteLine("\n2. Batch Price Predictions:");
                var batchInputs = new List<OnnxPricePredictor.PricePredictionInput>
                {
                    new OnnxPricePredictor.PricePredictionInput { Price = 15.99f },
                    new OnnxPricePredictor.PricePredictionInput { Price = 25.50f },
                    new OnnxPricePredictor.PricePredictionInput { Price = 49.99f }
                };

                var batchResults = predictor.PredictBatch(batchInputs);
                for (int i = 0; i < batchResults.Count; i++)
                {
                    Console.WriteLine($"  Prediction {i + 1}: Input ${batchInputs[i].Price:F2}");
                    if (batchResults[i].Error == null && batchResults[i].ForecastedPrices != null)
                        Console.WriteLine($"    → Forecast: {string.Join(", ", batchResults[i].ForecastedPrices.Select(p => $"${p:F2}"))}");
                    else
                        Console.WriteLine($"    → Error: {batchResults[i].Error}");
                }

                predictor.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ ONNX Prediction Example Error: {ex.Message}");
            }
        }

        static void GraphExecutionExample()
        {
            // Create a computational graph
            var g = new Graph().as_default();

            // Define graph operations within the graph context
            var input = tf.placeholder(tf.int32);
            var mul_op = input * 3;
            var add_op = mul_op + 2;

            // Execute the graph with specific input values
            using (var session = tf.Session(graph: g))
            {
                // Test with input value 5
                var result1 = session.run(add_op, feed_dict: new FeedItem(input, 5));
                Console.WriteLine($"Graph result for input 5: {(int)result1}");

                // Test with input value 10
                var result2 = session.run(add_op, feed_dict: new FeedItem(input, 10));
                Console.WriteLine($"Graph result for input 10: {(int)result2}");
            }
        }

        static string? EnsureModelLoaded()
        {
            // First, try to find an existing model
            string? existingModel = ModelUtility.FindModelFile();
            if (existingModel != null && File.Exists(existingModel))
            {
                Console.WriteLine($"✓ Using existing model: {existingModel}");
                return existingModel;
            }

            Console.WriteLine("⚠ ONNX model not found. Attempting to auto-export from ML.NET model...\n");

            // Try to ensure the model exists (copy from source if needed)
            string? modelPath = ModelUtility.EnsureModelExists();

            if (modelPath != null && File.Exists(modelPath))
            {
                Console.WriteLine();
                // Verify the model
                ModelUtility.VerifyModel(modelPath);
                return modelPath;
            }

            return null;
        }

        static void ExportModelIfNeeded()
        {
            try
            {
                // Path to the existing ML.NET model - try multiple locations
                var sourceModelPaths = new[]
                {
                    @"C:\Users\xeque\source\repos\TubieTools_Aspire\TubieTools_Machine_Learning\MLModel1.mlnet",
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "TubieTools_Machine_Learning", "MLModel1.mlnet"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "TubieTools_Machine_Learning", "MLModel1.mlnet"),
                };

                string? mlnetModelPath = null;
                foreach (var path in sourceModelPaths)
                {
                    if (File.Exists(path))
                    {
                        mlnetModelPath = path;
                        break;
                    }
                }

                // Output path for the ONNX model
                string executingDirectory = AppContext.BaseDirectory;
                string outputOnnxPath = Path.Combine(executingDirectory, "model.onnx");

                if (mlnetModelPath == null)
                {
                    Console.WriteLine($"⚠ ML.NET model not found at expected locations");
                    Console.WriteLine("  Checked:");
                    foreach (var path in sourceModelPaths)
                    {
                        Console.WriteLine($"    - {path}");
                    }
                    return;
                }

                Console.WriteLine($"Setting up model...\n");

                // Copy the model file to the output location
                bool copied = ModelUtility.CopyModelFile(mlnetModelPath, outputOnnxPath);

                if (copied)
                {
                    Console.WriteLine();
                    // Verify the copied model
                    ModelUtility.VerifyModel(outputOnnxPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error during model setup: {ex.Message}\n");
            }
        }
    }
}
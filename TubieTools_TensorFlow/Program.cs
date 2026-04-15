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
            // Update this path to point to your actual ONNX model file
            string modelPath = "model.onnx"; // Replace with actual path

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
                Console.WriteLine("  Note: Make sure to provide a valid ONNX model file path.");
                Console.WriteLine("  You can export ML.NET models to ONNX using Model Builder or programmatically.");
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
    }
}
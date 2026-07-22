using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;

namespace TensorFlowNET.Examples
{
    /// <summary>
    /// Utility class to inspect and extract metadata from ONNX models
    /// </summary>
    public class OnnxModelInspector
    {
        public static void InspectModel(string modelPath)
        {
            try
            {
                Console.WriteLine($"\n=== ONNX Model Inspection: {modelPath} ===\n");

                var sessionOptions = new SessionOptions();
                sessionOptions.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING;

                using (var session = new InferenceSession(modelPath, sessionOptions))
                {
                    Console.WriteLine("✓ Model loaded successfully!\n");

                    // Input Information
                    Console.WriteLine("--- MODEL INPUTS ---");
                    PrintInputInfo(session);

                    // Output Information
                    Console.WriteLine("\n--- MODEL OUTPUTS ---");
                    PrintOutputInfo(session);

                    // Model Metadata
                    Console.WriteLine("\n--- MODEL METADATA ---");
                    PrintModelMetadata(session);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error inspecting model: {ex.Message}");
            }
        }

        private static void PrintInputInfo(InferenceSession session)
        {
            var inputCount = session.InputNames.Count;
            Console.WriteLine($"Number of inputs: {inputCount}\n");

            for (int i = 0; i < inputCount; i++)
            {
                var inputName = session.InputNames[i];
                var inputNodeArg = session.InputMetadata[inputName];

                Console.WriteLine($"Input {i + 1}: {inputName}");
                Console.WriteLine($"  Type: {inputNodeArg.ElementType}");
                Console.WriteLine($"  Shape: {string.Join(", ", inputNodeArg.Dimensions)}");
                Console.WriteLine();
            }
        }

        private static void PrintOutputInfo(InferenceSession session)
        {
            var outputCount = session.OutputNames.Count;
            Console.WriteLine($"Number of outputs: {outputCount}\n");

            for (int i = 0; i < outputCount; i++)
            {
                var outputName = session.OutputNames[i];
                var outputNodeArg = session.OutputMetadata[outputName];

                Console.WriteLine($"Output {i + 1}: {outputName}");
                Console.WriteLine($"  Type: {outputNodeArg.ElementType}");
                Console.WriteLine($"  Shape: {string.Join(", ", outputNodeArg.Dimensions)}");
                Console.WriteLine();
            }
        }

        private static void PrintModelMetadata(InferenceSession session)
        {
            try
            {
                var modelMetadata = session.ModelMetadata;
                Console.WriteLine($"Producer Name: {modelMetadata.ProducerName}");
                Console.WriteLine($"Description: {modelMetadata.Description}");
                Console.WriteLine($"Graph Name: {modelMetadata.GraphName}");
            }
            catch
            {
                Console.WriteLine("Metadata not available for this model");
            }
        }
    }
}

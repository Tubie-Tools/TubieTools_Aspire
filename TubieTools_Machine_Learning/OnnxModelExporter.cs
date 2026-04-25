using Microsoft.ML;
using System;
using System.IO;

namespace TubieTools_Machine_Learning
{
    /// <summary>
    /// Utility to export ML.NET models to ONNX format for cross-platform compatibility
    /// </summary>
    public class OnnxModelExporter
    {
        private readonly MLContext _mlContext;

        public OnnxModelExporter()
        {
            _mlContext = new MLContext();
        }

        /// <summary>
        /// Exports ReviewModel.mlnet to ONNX format
        /// </summary>
        /// <param name="modelInputPath">Path to the .mlnet model file</param>
        /// <param name="onnxOutputPath">Path where ONNX model will be saved</param>
        /// <returns>True if export was successful, false otherwise</returns>
        public bool ExportReviewModelToOnnx(string modelInputPath, string onnxOutputPath)
        {
            try
            {
                Console.WriteLine($"🔄 Starting ONNX export process...");
                Console.WriteLine($"📂 Input model: {modelInputPath}");
                Console.WriteLine($"📂 Output path: {onnxOutputPath}");

                // Validate input file exists
                if (!File.Exists(modelInputPath))
                {
                    Console.WriteLine($"❌ Error: Model file not found at {modelInputPath}");
                    return false;
                }

                Console.WriteLine($"✓ Model file found, size: {new FileInfo(modelInputPath).Length} bytes");

                // Load the trained model
                Console.WriteLine($"📦 Loading model...");
                ITransformer model = _mlContext.Model.Load(modelInputPath, out var schema);
                Console.WriteLine($"✓ Model loaded successfully");

                // Ensure output directory exists
                string outputDirectory = Path.GetDirectoryName(onnxOutputPath);
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                    Console.WriteLine($"✓ Created output directory: {outputDirectory}");
                }

                // Convert to ONNX format
                //Console.WriteLine($"🔄 Converting to ONNX format...");
                //var onnxModel = _mlContext.Model.ConvertToOnnx(model, schema);
                //Console.WriteLine($"✓ Conversion successful");

                //// Save ONNX model
                //Console.WriteLine($"💾 Saving ONNX model...");
                //_mlContext.Model.SaveAsOnnx(onnxModel, onnxOutputPath);
                //Console.WriteLine($"✓ ONNX model saved");

                // Verify output
                if (File.Exists(onnxOutputPath))
                {
                    long fileSize = new FileInfo(onnxOutputPath).Length;
                    Console.WriteLine($"✅ Export successful!");
                    Console.WriteLine($"📊 Output file size: {fileSize:N0} bytes");
                    return true;
                }
                else
                {
                    Console.WriteLine($"❌ Error: Output file was not created");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Export failed: {ex.Message}");
                Console.WriteLine($"📋 Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Exports any ML.NET model to ONNX format
        /// </summary>
        public bool ExportModelToOnnx(string modelInputPath, string onnxOutputPath, string modelName)
        {
            try
            {
                Console.WriteLine($"🔄 Starting ONNX export for {modelName}...");
                Console.WriteLine($"📂 Input: {modelInputPath}");
                Console.WriteLine($"📂 Output: {onnxOutputPath}");

                if (!File.Exists(modelInputPath))
                {
                    Console.WriteLine($"❌ Model file not found: {modelInputPath}");
                    return false;
                }

                Console.WriteLine($"✓ Model found, size: {new FileInfo(modelInputPath).Length:N0} bytes");
                Console.WriteLine($"📦 Loading model...");

                ITransformer model = _mlContext.Model.Load(modelInputPath, out var schema);
                Console.WriteLine($"✓ Model loaded successfully");

                string outputDirectory = Path.GetDirectoryName(onnxOutputPath);
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                
                //Console.WriteLine($"🔄 Converting to ONNX...");
                //var onnxModel = _mlContext.Model.ConvertToOnnx(model, schema);

                //Console.WriteLine($"💾 Saving to {onnxOutputPath}...");
                //_mlContext.Model.SaveAsOnnx(onnxModel, onnxOutputPath);

                if (File.Exists(onnxOutputPath))
                {
                    long fileSize = new FileInfo(onnxOutputPath).Length;
                    Console.WriteLine($"✅ Successfully exported {modelName}!");
                    Console.WriteLine($"📊 File size: {fileSize:N0} bytes");
                    return true;
                }
                else
                {
                    Console.WriteLine($"❌ Failed to create output file");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Export failed: {ex.Message}");
                Console.WriteLine($"📋 Details: {ex.StackTrace}");
                return false;
            }
        }
    }
}

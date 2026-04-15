using System;
using System.IO;
using Microsoft.ML;

namespace TensorFlowNET.Examples
{
    /// <summary>
    /// Utility for working with ML.NET and ONNX models
    /// Supports both local development and containerized environments
    /// </summary>
    public class ModelUtility
    {
        private const string DEFAULT_MODEL_NAME = "model.onnx";
        private const string MODELS_DIRECTORY = "models";

        /// <summary>
        /// Find the model file in multiple locations
        /// Checks: current directory, models subdirectory, and relative paths
        /// </summary>
        /// <returns>Full path to model file, or null if not found</returns>
        public static string? FindModelFile()
        {
            string executingDirectory = AppContext.BaseDirectory;

            // Candidate paths to check (in order of preference)
            var candidatePaths = new[]
            {
                // 1. Direct in output directory
                Path.Combine(executingDirectory, DEFAULT_MODEL_NAME),

                // 2. In models subdirectory
                Path.Combine(executingDirectory, MODELS_DIRECTORY, DEFAULT_MODEL_NAME),

                // 3. One level up (for relative paths in containers)
                Path.Combine(executingDirectory, "..", DEFAULT_MODEL_NAME),

                // 4. In parent's models directory
                Path.Combine(executingDirectory, "..", MODELS_DIRECTORY, DEFAULT_MODEL_NAME),

                // 5. Content directory (common for web apps)
                Path.Combine(executingDirectory, "content", MODELS_DIRECTORY, DEFAULT_MODEL_NAME),

                // 6. Working directory
                Path.Combine(Directory.GetCurrentDirectory(), DEFAULT_MODEL_NAME),

                // 7. Working directory models subdirectory
                Path.Combine(Directory.GetCurrentDirectory(), MODELS_DIRECTORY, DEFAULT_MODEL_NAME),
            };

            foreach (var path in candidatePaths)
            {
                string normalizedPath = Path.GetFullPath(path);
                if (File.Exists(normalizedPath))
                {
                    Console.WriteLine($"✓ Found model at: {normalizedPath}");
                    return normalizedPath;
                }
            }

            // If not found, return the default path (for error messages)
            return Path.Combine(executingDirectory, DEFAULT_MODEL_NAME);
        }

        /// <summary>
        /// Get all candidate paths for diagnostics
        /// </summary>
        public static void PrintCandidatePaths()
        {
            string executingDirectory = AppContext.BaseDirectory;
            Console.WriteLine("\nSearched locations:");

            var candidatePaths = new[]
            {
                ("Output directory", Path.Combine(executingDirectory, DEFAULT_MODEL_NAME)),
                ("Models subdirectory", Path.Combine(executingDirectory, MODELS_DIRECTORY, DEFAULT_MODEL_NAME)),
                ("Parent directory", Path.Combine(executingDirectory, "..", DEFAULT_MODEL_NAME)),
                ("Parent models dir", Path.Combine(executingDirectory, "..", MODELS_DIRECTORY, DEFAULT_MODEL_NAME)),
                ("Content models", Path.Combine(executingDirectory, "content", MODELS_DIRECTORY, DEFAULT_MODEL_NAME)),
                ("Working directory", Path.Combine(Directory.GetCurrentDirectory(), DEFAULT_MODEL_NAME)),
                ("Working models dir", Path.Combine(Directory.GetCurrentDirectory(), MODELS_DIRECTORY, DEFAULT_MODEL_NAME)),
            };

            foreach (var (description, path) in candidatePaths)
            {
                string normalizedPath = Path.GetFullPath(path);
                bool exists = File.Exists(normalizedPath);
                Console.WriteLine($"  {(exists ? "✓" : "✗")} {description}");
                Console.WriteLine($"      {normalizedPath}");
            }
        }

        /// <summary>
        /// Load an ML.NET model and display its metadata
        /// </summary>
        public static bool LoadAndDisplayModelInfo(string modelPath)
        {
            try
            {
                if (!File.Exists(modelPath))
                {
                    Console.WriteLine($"✗ Model not found at: {modelPath}");
                    return false;
                }

                var mlContext = new MLContext();
                Console.WriteLine($"Loading model from: {modelPath}");

                using (var fs = File.OpenRead(modelPath))
                {
                    var model = mlContext.Model.Load(fs, out var schema);

                    Console.WriteLine("\n--- Model Schema Information ---");
                    Console.WriteLine($"Model loaded successfully");
                    Console.WriteLine($"Schema columns: {schema.Count}");

                    int columnIndex = 0;
                    foreach (var column in schema)
                    {
                        Console.WriteLine($"  Column {columnIndex}: {column.Name} ({column.Type})");
                        columnIndex++;
                    }
                    Console.WriteLine("-------------------------------\n");

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error loading model: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  Inner error: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Copy an ML.NET model file to a new location
        /// </summary>
        public static bool CopyModelFile(string sourceModelPath, string destinationPath)
        {
            try
            {
                if (!File.Exists(sourceModelPath))
                {
                    Console.WriteLine($"✗ Source model not found at: {sourceModelPath}");
                    return false;
                }

                Console.WriteLine($"Copying model from: {sourceModelPath}");
                Console.WriteLine($"             to: {destinationPath}");

                // Ensure destination directory exists
                string destinationDir = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }

                File.Copy(sourceModelPath, destinationPath, overwrite: true);

                FileInfo fileInfo = new FileInfo(destinationPath);
                Console.WriteLine($"✓ Model copied successfully");
                Console.WriteLine($"✓ File size: {fileInfo.Length} bytes");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error copying model: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verify that a model file is valid and can be loaded
        /// </summary>
        public static bool VerifyModel(string modelPath)
        {
            try
            {
                if (!File.Exists(modelPath))
                {
                    Console.WriteLine($"✗ Model file not found: {modelPath}");
                    return false;
                }

                var mlContext = new MLContext();
                Console.WriteLine($"Verifying model: {modelPath}");

                using (var fs = File.OpenRead(modelPath))
                {
                    var model = mlContext.Model.Load(fs, out var schema);
                    Console.WriteLine($"✓ Model is valid and can be loaded");
                    Console.WriteLine($"✓ Schema has {schema.Count} columns");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Model verification failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Find and copy the model from the source project to the output directory
        /// Works in both development and container environments
        /// </summary>
        public static string? EnsureModelExists()
        {
            // First try to find an existing model
            string? existingModel = FindModelFile();
            if (existingModel != null && File.Exists(existingModel))
            {
                return existingModel;
            }

            // Try to copy from source if available
            var sourceModelPaths = new[]
            {
                // Development environment
                @"C:\Users\xeque\source\repos\TubieTools_Aspire\TubieTools_Machine_Learning\MLModel1.mlnet",

                // Relative paths for containers
                Path.Combine(AppContext.BaseDirectory, "..", "..", "TubieTools_Machine_Learning", "MLModel1.mlnet"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "TubieTools_Machine_Learning", "MLModel1.mlnet"),

                // Root directory search
                Path.Combine(AppContext.BaseDirectory, "MLModel1.mlnet"),
            };

            foreach (var sourcePath in sourceModelPaths)
            {
                if (File.Exists(sourcePath))
                {
                    string outputPath = Path.Combine(AppContext.BaseDirectory, DEFAULT_MODEL_NAME);
                    if (CopyModelFile(sourcePath, outputPath))
                    {
                        return outputPath;
                    }
                }
            }

            return null;
        }
    }
}

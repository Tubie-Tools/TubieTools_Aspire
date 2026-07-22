using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.ML.OnnxRuntime;

namespace TensorFlowNET.Examples
{
    /// <summary>
    /// Utility to diagnose and repair ONNX model issues
    /// </summary>
    public class OnnxModelDiagnostics
    {
        public static void RunFullDiagnostics()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════╗");
            Console.WriteLine("║     ONNX MODEL DIAGNOSTICS                  ║");
            Console.WriteLine("╚════════════════════════════════════════════╝\n");

            var candidates = FindAllModelCandidates();

            if (candidates.Count == 0)
            {
                Console.WriteLine("✗ No ONNX model files found!\n");
                PrintSearchDirectories();
                return;
            }

            Console.WriteLine($"Found {candidates.Count} model candidate(s):\n");

            for (int i = 0; i < candidates.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {candidates[i]}");
                DiagnoseModelFile(candidates[i]);
                Console.WriteLine();
            }
        }

        public static void DiagnoseModelFile(string modelPath)
        {
            try
            {
                var fileInfo = new FileInfo(modelPath);

                Console.WriteLine($"  📄 File Information:");
                Console.WriteLine($"     Path: {fileInfo.FullName}");
                Console.WriteLine($"     Size: {FormatFileSize(fileInfo.Length)}");
                Console.WriteLine($"     Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");

                if (fileInfo.Length == 0)
                {
                    Console.WriteLine($"  ⚠️  WARNING: File is empty (0 bytes)");
                    return;
                }

                if (fileInfo.Length < 100)
                {
                    Console.WriteLine($"  ⚠️  WARNING: File is too small ({fileInfo.Length} bytes)");
                    Console.WriteLine($"     ONNX files should be at least 100+ bytes");
                    return;
                }

                // Try to load the model
                Console.WriteLine($"\n  🔍 ONNX Runtime Analysis:");
                try
                {
                    var sessionOptions = new SessionOptions();
                    sessionOptions.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_ERROR;

                    using (var session = new InferenceSession(modelPath, sessionOptions))
                    {
                        Console.WriteLine($"  ✓ Model loaded successfully");
                        Console.WriteLine($"     Inputs: {session.InputNames.Count}");
                        foreach (var inputName in session.InputNames)
                        {
                            var meta = session.InputMetadata[inputName];
                            Console.WriteLine($"       - {inputName} ({meta.ElementType})");
                        }

                        Console.WriteLine($"     Outputs: {session.OutputNames.Count}");
                        foreach (var outputName in session.OutputNames)
                        {
                            var meta = session.OutputMetadata[outputName];
                            Console.WriteLine($"       - {outputName} ({meta.ElementType})");
                        }

                        Console.WriteLine($"\n  ✅ Model Status: VALID AND READY TO USE");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Load failed: {ex.Message}");
                    AnalyzeError(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Diagnostics error: {ex.Message}");
            }
        }

        private static void AnalyzeError(string errorMessage)
        {
            Console.WriteLine($"\n  🔧 Troubleshooting:");

            if (errorMessage.Contains("ModelProto does not have a graph"))
            {
                Console.WriteLine($"     Issue: Model is missing graph definition (corrupted)");
                Console.WriteLine($"     Action: Re-export the ML.NET model to ONNX format");
            }
            else if (errorMessage.Contains("not a valid ONNX"))
            {
                Console.WriteLine($"     Issue: File is not a valid ONNX model");
                Console.WriteLine($"     Action: Verify the file is in ONNX format (.onnx)");
            }
            else if (errorMessage.Contains("file not found"))
            {
                Console.WriteLine($"     Issue: Model file not found at specified path");
                Console.WriteLine($"     Action: Check file path and ensure file exists");
            }
            else
            {
                Console.WriteLine($"     Issue: {errorMessage}");
            }
        }

        private static List<string> FindAllModelCandidates()
        {
            var candidates = new List<string>();

            string[] searchPaths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "onnx_model.onnx"),
                Path.Combine(AppContext.BaseDirectory, "model.onnx"),
                Path.Combine(Directory.GetCurrentDirectory(), "onnx_model.onnx"),
                Path.Combine(Directory.GetCurrentDirectory(), "model.onnx"),
                "onnx_model.onnx",
                "model.onnx",
                Path.Combine(AppContext.BaseDirectory, "..", "..", "onnx_model.onnx"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "TubieTools_Machine_Learning", "onnx_model.onnx"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "TubieTools_TensorFlow", "onnx_model.onnx"),
            };

            foreach (var path in searchPaths)
            {
                try
                {
                    var fullPath = Path.GetFullPath(path);
                    if (File.Exists(fullPath) && !candidates.Contains(fullPath))
                    {
                        candidates.Add(fullPath);
                    }
                }
                catch
                {
                    // Continue searching
                }
            }

            return candidates;
        }

        private static void PrintSearchDirectories()
        {
            Console.WriteLine("Searched in:");
            string[] dirs = new[]
            {
                AppContext.BaseDirectory,
                Directory.GetCurrentDirectory(),
                Path.Combine(AppContext.BaseDirectory, ".."),
                Path.Combine(AppContext.BaseDirectory, "..", ".."),
            };

            foreach (var dir in dirs)
            {
                try
                {
                    Console.WriteLine($"  {Path.GetFullPath(dir)}");
                }
                catch { }
            }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}

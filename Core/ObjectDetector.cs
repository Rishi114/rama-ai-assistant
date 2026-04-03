using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rama.Core
{
    /// <summary>
    /// Object Detection — Real-time detection using camera/video.
    /// Identifies objects, people, text, and scenes.
    /// </summary>
    public class ObjectDetector : IDisposable
    {
        private string ModelsDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rama", "Models");

        private string DetectionLogPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rama", "Learning", "detections.json");

        private List<Detection> _detectionHistory = new();
        private bool _isRunning = false;

        public bool IsRunning => _isRunning;

        // Common object categories
        public static readonly string[] ObjectCategories = {
            "person", "car", "dog", "cat", "phone", "laptop", "book", "cup",
            "chair", "table", "bottle", "food", "plant", "clock", "tv",
            "keyboard", "mouse", "headphones", "bag", "shoes"
        };

        public ObjectDetector()
        {
            Directory.CreateDirectory(ModelsDir);
            LoadDetections();
        }

        /// <summary>
        /// Detect objects in an image.
        /// Uses ONNX Runtime or calls to a detection API.
        /// </summary>
        public async Task<DetectionResult> DetectAsync(string imagePath)
        {
            var result = new DetectionResult { ImagePath = imagePath };

            try
            {
                // Option 1: Use local ONNX model (if available)
                string modelPath = Path.Combine(ModelsDir, "yolov8n.onnx");
                if (File.Exists(modelPath))
                {
                    result = await DetectWithONNX(imagePath, modelPath);
                }
                else
                {
                    // Option 2: Use cloud API (Roboflow, Google Vision, etc.)
                    result = await DetectWithCloudAPI(imagePath);
                }

                // Log detection
                foreach (var obj in result.Objects)
                {
                    _detectionHistory.Add(new Detection
                    {
                        ObjectClass = obj.Class,
                        Confidence = obj.Confidence,
                        ImagePath = imagePath,
                        Timestamp = DateTime.Now
                    });
                }

                SaveDetections();
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Describe what's in an image (using GPT-4 Vision or local model).
        /// </summary>
        public async Task<string> DescribeScene(string imagePath, GPT4Cloud? cloud = null)
        {
            if (cloud?.IsEnabled == true)
            {
                return await cloud.AnalyzeImageAsync(imagePath, 
                    "Describe this image in detail. What objects do you see? What's happening? " +
                    "Describe the scene, colors, and any text you can read.");
            }

            // Fallback: basic detection
            var detection = await DetectAsync(imagePath);
            if (detection.Objects.Any())
            {
                return $"I can see: {string.Join(", ", detection.Objects.Select(o => $"{o.Class} ({o.Confidence:P0})"))}";
            }

            return "I can see the image but need a cloud AI key for detailed descriptions. Say 'setup gpt4' to enable.";
        }

        /// <summary>
        /// Detect text in an image (OCR).
        /// </summary>
        public async Task<string> ReadText(string imagePath, GPT4Cloud? cloud = null)
        {
            if (cloud?.IsEnabled == true)
            {
                return await cloud.AnalyzeImageAsync(imagePath,
                    "Read ALL text in this image. Output only the text, nothing else.");
            }

            return "[OCR requires cloud AI. Say 'setup gpt4' to enable.]";
        }

        /// <summary>
        /// Start continuous detection from camera.
        /// </summary>
        public void StartContinuousDetection()
        {
            _isRunning = true;
        }

        public void StopContinuousDetection()
        {
            _isRunning = false;
        }

        /// <summary>
        /// Get detection statistics.
        /// </summary>
        public string GetStats()
        {
            if (!_detectionHistory.Any())
                return "👁️ No detections yet. Take a photo and I'll identify objects!";

            var topObjects = _detectionHistory
                .GroupBy(d => d.ObjectClass)
                .OrderByDescending(g => g.Count())
                .Take(10);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("👁️ **Detection History:**\n");
            sb.AppendLine($"Total detections: {_detectionHistory.Count}");
            sb.AppendLine($"Unique objects: {_detectionHistory.Select(d => d.ObjectClass).Distinct().Count()}\n");
            sb.AppendLine("**Most detected:**");
            foreach (var obj in topObjects)
                sb.AppendLine($"  • {obj.Key}: {obj.Count()} times");

            return sb.ToString();
        }

        private Task<DetectionResult> DetectWithONNX(string imagePath, string modelPath)
        {
            // Placeholder for ONNX inference
            // In production, would use Microsoft.ML.OnnxRuntime
            var result = new DetectionResult
            {
                ImagePath = imagePath,
                Objects = new List<DetectedObject>
                {
                    new() { Class = "analyzing", Confidence = 0.95, X = 0, Y = 0, Width = 100, Height = 100 }
                }
            };
            return Task.FromResult(result);
        }

        private async Task<DetectionResult> DetectWithCloudAPI(string imagePath)
        {
            // Would call Roboflow, Google Vision, or Azure Computer Vision
            return new DetectionResult
            {
                ImagePath = imagePath,
                Objects = new List<DetectedObject>()
            };
        }

        private void LoadDetections()
        {
            try
            {
                if (File.Exists(DetectionLogPath))
                    _detectionHistory = JsonConvert.DeserializeObject<List<Detection>>(File.ReadAllText(DetectionLogPath)) ?? new();
            }
            catch { }
        }

        private void SaveDetections()
        {
            try
            {
                if (_detectionHistory.Count > 1000)
                    _detectionHistory = _detectionHistory.TakeLast(1000).ToList();

                Directory.CreateDirectory(Path.GetDirectoryName(DetectionLogPath)!);
                File.WriteAllText(DetectionLogPath, JsonConvert.SerializeObject(_detectionHistory, Formatting.Indented));
            }
            catch { }
        }

        public void Dispose() => SaveDetections();
    }

    #region Models

    public class DetectionResult
    {
        public string ImagePath { get; set; } = "";
        public List<DetectedObject> Objects { get; set; } = new();
        public string? Error { get; set; }
        public bool Success => string.IsNullOrEmpty(Error);
    }

    public class DetectedObject
    {
        public string Class { get; set; } = "";
        public double Confidence { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class Detection
    {
        public string ObjectClass { get; set; } = "";
        public double Confidence { get; set; }
        public string ImagePath { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    #endregion
}

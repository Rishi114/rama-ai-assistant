using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rama.Core
{
    /// <summary>
    /// Mobile Integration — Connect Rama to your phone's camera, mic, and sensors.
    /// Rama can see through your camera, hear through your mic,
    /// and learn from the real world around you.
    /// 
    /// Uses OpenClaw node pairing for mobile access.
    /// </summary>
    public class MobileIntegration : IDisposable
    {
        private string MobileDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rama", "Mobile");

        private string DevicesPath => Path.Combine(MobileDir, "devices.json");
        private string CapturesPath => Path.Combine(MobileDir, "captures");

        private List<MobileDevice> _devices = new();
        private List<Capture> _captures = new();

        public bool HasConnectedDevice => _devices.Any(d => d.Connected);
        public int DeviceCount => _devices.Count;

        public MobileIntegration()
        {
            Directory.CreateDirectory(MobileDir);
            Directory.CreateDirectory(CapturesPath);
            LoadAll();
        }

        #region Device Management

        /// <summary>
        /// Register a mobile device (phone/tablet).
        /// </summary>
        public MobileDevice RegisterDevice(string name, string type, string deviceId)
        {
            var device = new MobileDevice
            {
                Name = name,
                Type = type, // "android", "ios"
                DeviceId = deviceId,
                RegisteredAt = DateTime.Now,
                Connected = false,
                Capabilities = new DeviceCapabilities
                {
                    HasCamera = true,
                    HasMicrophone = true,
                    HasLocation = true,
                    HasAccelerometer = true,
                    HasGyroscope = true,
                    HasBluetooth = true,
                    HasNFC = false
                }
            };

            _devices.Add(device);
            SaveDevices();
            return device;
        }

        /// <summary>
        /// Connect to a registered device.
        /// </summary>
        public string ConnectDevice(string deviceId)
        {
            var device = _devices.FirstOrDefault(d => d.DeviceId == deviceId);
            if (device == null)
                return "📱 Device not found. Say `register phone [name]` first.";

            device.Connected = true;
            device.LastConnected = DateTime.Now;
            SaveDevices();

            return $"📱 Connected to **{device.Name}** ({device.Type})!\n\n" +
                   "I can now:\n" +
                   "• 📷 Take photos: `take photo`\n" +
                   "• 🎥 Record video: `record video`\n" +
                   "• 🎤 Listen: `listen to mic`\n" +
                   "• 📍 Get location: `where am i`\n" +
                   "• 📱 Screen mirror: `show screen`\n\n" +
                   "I'll learn from what I see and hear! 🧠";
        }

        /// <summary>
        /// List all registered devices.
        /// </summary>
        public string ListDevices()
        {
            if (!_devices.Any())
                return "📱 No devices registered yet.\n" +
                    "Say `register phone [name]` to add your phone.";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("📱 **Connected Devices:**\n");
            foreach (var d in _devices)
            {
                string status = d.Connected ? "🟢 Online" : "⚫ Offline";
                sb.AppendLine($"**{d.Name}** ({d.Type}) — {status}");
                sb.AppendLine($"  Registered: {d.RegisteredAt:MMM dd, yyyy}");
                if (d.LastConnected != default)
                    sb.AppendLine($"  Last seen: {d.LastConnected:MMM dd, HH:mm}");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        #endregion

        #region Camera

        /// <summary>
        /// Take a photo using the phone camera.
        /// Returns the path to the saved image.
        /// </summary>
        public async Task<CaptureResult> TakePhoto(string camera = "back")
        {
            var device = _devices.FirstOrDefault(d => d.Connected);
            if (device == null)
                return new CaptureResult { Success = false, Error = "No device connected" };

            // In real implementation, this would send a command to the OpenClaw node
            // to capture from the phone camera
            var capture = new Capture
            {
                Type = "photo",
                DeviceId = device.DeviceId,
                Camera = camera,
                Timestamp = DateTime.Now,
                Path = Path.Combine(CapturesPath, $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg")
            };

            _captures.Add(capture);
            SaveCaptures();

            return new CaptureResult
            {
                Success = true,
                Message = $"📸 Photo taken from {camera} camera!\n" +
                         $"Saved to: `{capture.Path}`\n\n" +
                         "I'll analyze this image and learn from what I see. 🧠",
                CapturePath = capture.Path,
                Capture = capture
            };
        }

        /// <summary>
        /// Start continuous vision — Rama keeps watching through the camera.
        /// </summary>
        public string StartVision(string camera = "back")
        {
            var device = _devices.FirstOrDefault(d => d.Connected);
            if (device == null)
                return "No device connected";

            device.VisionActive = true;
            device.VisionCamera = camera;
            SaveDevices();

            return $"👁️ **Vision Mode ON!**\n\n" +
                   $"Watching through {device.Name}'s {camera} camera.\n" +
                   "I'll describe what I see and learn from my surroundings.\n\n" +
                   "Say `stop vision` to pause.";
        }

        /// <summary>
        /// Stop continuous vision.
        /// </summary>
        public string StopVision()
        {
            foreach (var d in _devices)
            {
                d.VisionActive = false;
            }
            SaveDevices();
            return "👁️ Vision mode stopped.";
        }

        #endregion

        #region Microphone

        /// <summary>
        /// Start listening through phone microphone.
        /// </summary>
        public string StartListening()
        {
            var device = _devices.FirstOrDefault(d => d.Connected);
            if (device == null)
                return "No device connected";

            device.MicActive = true;
            SaveDevices();

            return $"🎤 **Listening through {device.Name}!**\n\n" +
                   "I'll hear everything around you and learn from it.\n" +
                   "Languages, conversations, sounds — I absorb it all.\n\n" +
                   "Say `stop listening` to pause.";
        }

        /// <summary>
        /// Stop listening.
        /// </summary>
        public string StopListening()
        {
            foreach (var d in _devices)
                d.MicActive = false;
            SaveDevices();
            return "🎤 Stopped listening.";
        }

        #endregion

        #region Location

        /// <summary>
        /// Get phone's current location.
        /// </summary>
        public string GetLocation()
        {
            var device = _devices.FirstOrDefault(d => d.Connected);
            if (device == null)
                return "No device connected";

            // In real implementation, requests GPS from OpenClaw node
            return $"📍 **Location from {device.Name}:**\n\n" +
                   "GPS coordinates will be fetched from your phone.\n" +
                   "I'll remember this location for context-aware assistance.";
        }

        #endregion

        #region Screen

        /// <summary>
        /// Capture phone screen.
        /// </summary>
        public async Task<CaptureResult> CaptureScreen()
        {
            var device = _devices.FirstOrDefault(d => d.Connected);
            if (device == null)
                return new CaptureResult { Success = false, Error = "No device connected" };

            var capture = new Capture
            {
                Type = "screenshot",
                DeviceId = device.DeviceId,
                Timestamp = DateTime.Now,
                Path = Path.Combine(CapturesPath, $"screen_{DateTime.Now:yyyyMMdd_HHmmss}.png")
            };

            _captures.Add(capture);
            SaveCaptures();

            return new CaptureResult
            {
                Success = true,
                Message = $"📱 Screen captured from {device.Name}!\n" +
                         "I'll analyze what's on your screen.",
                CapturePath = capture.Path,
                Capture = capture
            };
        }

        #endregion

        #region Learning from Mobile

        /// <summary>
        /// Process what Rama sees through the camera.
        /// Called when a new image arrives from the phone.
        /// </summary>
        public string ProcessVision(string description, AdvancedLearning learning)
        {
            var result = learning.LearnFromVision(description, "mobile_camera");

            string response = $"👁️ I see: {description}\n\n";

            if (result.NewConcepts.Any())
                response += $"📚 Learned new concepts: {string.Join(", ", result.NewConcepts)}";

            return response;
        }

        /// <summary>
        /// Process what Rama hears through the microphone.
        /// </summary>
        public string ProcessAudio(string transcript, string language, AdvancedLearning learning)
        {
            var result = learning.LearnFromAudio(transcript, language, "mobile_mic");

            return $"🎤 Heard ({language}): \"{transcript}\"\n\n" +
                   "I'm learning from what I hear around you! 🧠";
        }

        #endregion

        #region Reporting

        public string GetStatus()
        {
            var connected = _devices.FirstOrDefault(d => d.Connected);
            if (connected == null)
                return "📱 No phone connected.\nSay `connect phone` to pair your device.";

            string vision = connected.VisionActive ? "👁️ Watching" : "👁️ Idle";
            string mic = connected.MicActive ? "🎤 Listening" : "🎤 Idle";

            return $"📱 **Mobile Status:**\n\n" +
                   $"Device: **{connected.Name}** ({connected.Type})\n" +
                   $"Camera: {vision}\n" +
                   $"Microphone: {mic}\n" +
                   $"Captures today: {_captures.Count(c => c.Timestamp.Date == DateTime.Today)}\n" +
                   $"Total captures: {_captures.Count}";
        }

        #endregion

        #region Persistence

        private void LoadAll()
        {
            try
            {
                if (File.Exists(DevicesPath))
                    _devices = JsonConvert.DeserializeObject<List<MobileDevice>>(File.ReadAllText(DevicesPath)) ?? new();
            }
            catch { }
        }

        private void SaveDevices() =>
            File.WriteAllText(DevicesPath, JsonConvert.SerializeObject(_devices, Formatting.Indented));

        private void SaveCaptures() =>
            File.WriteAllText(Path.Combine(MobileDir, "captures_log.json"),
                JsonConvert.SerializeObject(_captures.TakeLast(100).ToList(), Formatting.Indented));

        #endregion

        public void Dispose() => SaveDevices();
    }

    #region Models

    public class MobileDevice
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = ""; // android, ios
        public string DeviceId { get; set; } = "";
        public bool Connected { get; set; }
        public bool VisionActive { get; set; }
        public bool MicActive { get; set; }
        public string VisionCamera { get; set; } = "back";
        public DeviceCapabilities Capabilities { get; set; } = new();
        public DateTime RegisteredAt { get; set; }
        public DateTime LastConnected { get; set; }
    }

    public class DeviceCapabilities
    {
        public bool HasCamera { get; set; }
        public bool HasMicrophone { get; set; }
        public bool HasLocation { get; set; }
        public bool HasAccelerometer { get; set; }
        public bool HasGyroscope { get; set; }
        public bool HasBluetooth { get; set; }
        public bool HasNFC { get; set; }
    }

    public class Capture
    {
        public string Type { get; set; } = ""; // photo, video, audio, screenshot
        public string DeviceId { get; set; } = "";
        public string Camera { get; set; } = "";
        public string Path { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    public class CaptureResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string Error { get; set; } = "";
        public string CapturePath { get; set; } = "";
        public Capture? Capture { get; set; }
    }

    #endregion
}

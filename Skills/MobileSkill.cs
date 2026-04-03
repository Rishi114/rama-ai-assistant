using System;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Mobile Skill — Connect phone, use camera, mic, GPS.
    /// Rama sees and hears through your phone!
    /// </summary>
    public class MobileSkill : SkillBase
    {
        private readonly MobileIntegration _mobile;
        private readonly AdvancedLearning _learning;
        private readonly ProfileManager _profile;

        public MobileSkill(MobileIntegration mobile, AdvancedLearning learning, ProfileManager profile)
        {
            _mobile = mobile;
            _learning = learning;
            _profile = profile;
        }

        public override string Name => "Mobile";
        public override string Description => "Connect phone, camera, mic, GPS — see the world";
        public override string[] Triggers => new[] {
            "phone", "mobile", "connect phone", "register phone",
            "take photo", "take picture", "camera", "record",
            "listen to mic", "listen to surroundings", "hear",
            "where am i", "location", "gps",
            "show screen", "screen capture", "screenshot",
            "start vision", "stop vision", "look at",
            "phone status", "mobile status", "devices"
        };

        public override bool CanHandle(string input)
        {
            string lower = input.ToLowerInvariant();
            return lower.Contains("phone") || lower.Contains("mobile") ||
                   lower.Contains("camera") || lower.Contains("photo") ||
                   lower.Contains("listen") && lower.Contains("mic") ||
                   lower.Contains("location") || lower.Contains("gps") ||
                   lower.Contains("screenshot") || lower.Contains("vision") ||
                   lower.Contains("look at");
        }

        public override async Task<string> ExecuteAsync(string input, Memory memory)
        {
            string lower = input.ToLowerInvariant();
            string nick = _profile.GetUserAddress();

            // Register/connect phone
            if (lower.Contains("register phone") || lower.Contains("connect phone") || lower.Contains("pair phone"))
            {
                string name = "My Phone";
                var device = _mobile.RegisterDevice(name, "android", Guid.NewGuid().ToString());
                return _mobile.ConnectDevice(device.DeviceId);
            }

            // Phone status
            if (lower.Contains("phone status") || lower.Contains("mobile status") || lower.Contains("devices"))
                return _mobile.GetStatus();

            // List devices
            if (lower.Contains("list device") || lower.Contains("show device"))
                return _mobile.ListDevices();

            // Take photo
            if (lower.Contains("take photo") || lower.Contains("take picture") || lower.Contains("camera"))
            {
                string cam = lower.Contains("front") ? "front" : "back";
                var result = await _mobile.TakePhoto(cam);
                return result.Success ? result.Message : $"❌ {result.Error}";
            }

            // Start/stop vision
            if (lower.Contains("start vision") || lower.Contains("look at"))
            {
                return _mobile.StartVision();
            }
            if (lower.Contains("stop vision"))
            {
                return _mobile.StopVision();
            }

            // Listen through mic
            if ((lower.Contains("listen") && lower.Contains("mic")) || lower.Contains("hear surroundings"))
            {
                return _mobile.StartListening();
            }
            if (lower.Contains("stop listening"))
            {
                return _mobile.StopListening();
            }

            // Location
            if (lower.Contains("where am i") || lower.Contains("location") || lower.Contains("gps"))
            {
                return _mobile.GetLocation();
            }

            // Screenshot
            if (lower.Contains("screenshot") || lower.Contains("screen capture") || lower.Contains("show screen"))
            {
                var result = await _mobile.CaptureScreen();
                return result.Success ? result.Message : $"❌ {result.Error}";
            }

            return GetHelp(nick);
        }

        private string GetHelp(string nick)
        {
            return $"📱 **Mobile — I can see and hear through your phone, {nick}!**\n\n" +
                "**Setup:**\n" +
                "• `register phone` — Pair your phone\n" +
                "• `phone status` — Check connection\n\n" +
                "**Camera:**\n" +
                "• `take photo` — Snap a picture\n" +
                "• `take photo front` — Selfie camera\n" +
                "• `start vision` — I keep watching\n" +
                "• `stop vision` — Pause watching\n\n" +
                "**Microphone:**\n" +
                "• `listen to mic` — I hear everything\n" +
                "• `stop listening` — Pause hearing\n\n" +
                "**Other:**\n" +
                "• `where am i` — GPS location\n" +
                "• `screenshot` — Capture screen\n\n" +
                "🧠 Everything I see/hear, I learn from! 🧠";
        }
    }
}

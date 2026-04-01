using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Rama.Core
{
    /// <summary>
    /// 3D Animated Avatar — Rama's visual representation.
    /// Optional feature that shows an animated character that reacts to conversations.
    /// Supports multiple avatar styles and animations.
    /// </summary>
    public class Avatar3D : IDisposable
    {
        private string ConfigPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rama", "avatar.json");

        private AvatarConfig _config = new();

        public bool IsEnabled => _config.Enabled;
        public string CurrentAvatar => _config.AvatarStyle;
        public string CurrentAnimation => _config.CurrentAnimation;

        // Available avatars
        public static readonly List<AvatarStyle> AvatarStyles = new()
        {
            new() { Id = "anime-girl", Name = "Anime Girl", Description = "Cute anime-style assistant", Preview = "👧" },
            new() { Id = "robot", Name = "Robot", Description = "Classic robot assistant", Preview = "🤖" },
            new() { Id = "hologram", Name = "Hologram", Description = "Futuristic holographic display", Preview = "👤" },
            new() { Id = "chibi", Name = "Chibi", Description = "Small cute character", Preview = "🧝" },
            new() { Id = "realistic", Name = "Realistic", Description = "3D realistic human avatar", Preview = "👩" },
            new() { Id = "pixel", Name = "Pixel Art", Description = "Retro pixel art style", Preview = "👾" },
            new() { Id = "minimal", Name = "Minimal", Description = "Simple animated circle", Preview = "🔵" },
            new() { Id = "cat", Name = "Cat", Description = "Cute cat assistant (neko)", Preview = "🐱" },
        };

        // Available animations
        public static readonly List<string> Animations = new()
        {
            "idle", "talking", "thinking", "happy", "excited",
            "sad", "confused", "waving", "nodding", "sleeping",
            "dancing", "laughing", "surprised", "angry", "love"
        };

        public Avatar3D()
        {
            LoadConfig();
        }

        #region Configuration

        /// <summary>
        /// Enable/disable the avatar.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _config.Enabled = enabled;
            SaveConfig();
        }

        /// <summary>
        /// Change avatar style.
        /// </summary>
        public string SetAvatar(string styleId)
        {
            var style = AvatarStyles.FirstOrDefault(s => s.Id == styleId);
            if (style == null)
                return $"Avatar '{styleId}' not found. Options: {string.Join(", ", AvatarStyles.Select(s => s.Id))}";

            _config.AvatarStyle = styleId;
            SaveConfig();

            return $"✅ Avatar changed to **{style.Name}** {style.Preview}";
        }

        /// <summary>
        /// Set avatar size.
        /// </summary>
        public void SetSize(int width, int height)
        {
            _config.Width = width;
            _config.Height = height;
            SaveConfig();
        }

        /// <summary>
        /// Set avatar position on screen.
        /// </summary>
        public void SetPosition(string position) // "bottom-right", "bottom-left", etc.
        {
            _config.Position = position;
            SaveConfig();
        }

        #endregion

        #region Animations

        /// <summary>
        /// Play an animation based on emotion/context.
        /// </summary>
        public string PlayAnimation(string emotion)
        {
            string animation = emotion.ToLower() switch
            {
                "happy" or "thanks" or "great" => "happy",
                "excited" or "wow" or "amazing" => "excited",
                "sad" or "sorry" => "sad",
                "thinking" or "hmm" => "thinking",
                "confused" or "what" => "confused",
                "hello" or "hi" => "waving",
                "yes" or "okay" or "sure" => "nodding",
                "laugh" or "haha" or "funny" => "laughing",
                "love" or "heart" => "love",
                "angry" or "ugh" => "angry",
                "bye" or "goodbye" => "waving",
                "sleep" or "tired" => "sleeping",
                _ => "idle"
            };

            _config.CurrentAnimation = animation;
            SaveConfig();

            return animation;
        }

        /// <summary>
        /// React to user input with appropriate animation.
        /// </summary>
        public void React(string input)
        {
            string lower = input.ToLowerInvariant();

            if (lower.Contains("thank") || lower.Contains("great") || lower.Contains("awesome"))
                PlayAnimation("happy");
            else if (lower.Contains("hello") || lower.Contains("hi"))
                PlayAnimation("waving");
            else if (lower.Contains("?"))
                PlayAnimation("thinking");
            else if (lower.Contains("haha") || lower.Contains("lol") || lower.Contains("funny"))
                PlayAnimation("laughing");
            else if (lower.Contains("love"))
                PlayAnimation("love");
            else
                PlayAnimation("talking");
        }

        /// <summary>
        /// React to Rama's own response.
        /// </summary>
        public void ReactToResponse(string response)
        {
            string lower = response.ToLowerInvariant();

            if (response.Contains("🔥") || response.Contains("Maximum"))
                PlayAnimation("excited");
            else if (response.Contains("🤔") || response.Contains("thinking"))
                PlayAnimation("thinking");
            else if (response.Contains("😅") || response.Contains("error"))
                PlayAnimation("confused");
            else
                PlayAnimation("talking");
        }

        #endregion

        #region Display

        /// <summary>
        /// Generate HTML for the avatar (for embedding in WPF WebView).
        /// </summary>
        public string GenerateAvatarHTML()
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ margin: 0; background: transparent; overflow: hidden; }}
        .avatar-container {{
            position: fixed;
            {_config.Position.Contains("bottom") ? "bottom: 20px;" : "top: 20px;"}
            {_config.Position.Contains("right") ? "right: 20px;" : "left: 20px;"}
            width: {_config.Width}px;
            height: {_config.Height}px;
        }}
        .avatar {{
            font-size: 80px;
            animation: {_config.CurrentAnimation} 1s ease-in-out infinite;
            filter: drop-shadow(0 0 10px rgba(137, 180, 250, 0.5));
        }}
        @keyframes idle {{ 0%, 100% {{ transform: translateY(0); }} 50% {{ transform: translateY(-5px); }} }}
        @keyframes talking {{ 0%, 100% {{ transform: scale(1); }} 50% {{ transform: scale(1.05); }} }}
        @keyframes thinking {{ 0%, 100% {{ transform: rotate(0deg); }} 50% {{ transform: rotate(5deg); }} }}
        @keyframes happy {{ 0%, 100% {{ transform: scale(1) rotate(0deg); }} 25% {{ transform: scale(1.1) rotate(-5deg); }} 75% {{ transform: scale(1.1) rotate(5deg); }} }}
        @keyframes excited {{ 0%, 100% {{ transform: translateY(0) scale(1); }} 50% {{ transform: translateY(-20px) scale(1.2); }} }}
        @keyframes waving {{ 0%, 100% {{ transform: rotate(0deg); }} 25% {{ transform: rotate(20deg); }} 75% {{ transform: rotate(-20deg); }} }}
        @keyframes sad {{ 0%, 100% {{ transform: scale(1); opacity: 1; }} 50% {{ transform: scale(0.95); opacity: 0.7; }} }}
        @keyframes confused {{ 0%, 100% {{ transform: rotate(0deg); }} 50% {{ transform: rotate(15deg); }} }}
        @keyframes laughing {{ 0%, 100% {{ transform: scale(1); }} 25% {{ transform: scale(1.1); }} 75% {{ transform: scale(0.9); }} }}
        @keyframes nodding {{ 0%, 100% {{ transform: rotateX(0deg); }} 50% {{ transform: rotateX(15deg); }} }}
        @keyframes sleeping {{ 0%, 100% {{ transform: scale(1); opacity: 1; }} 50% {{ transform: scale(0.95); opacity: 0.6; }} }}
        @keyframes dancing {{ 0% {{ transform: rotate(0deg); }} 25% {{ transform: rotate(10deg); }} 50% {{ transform: rotate(0deg); }} 75% {{ transform: rotate(-10deg); }} }}
        @keyframes love {{ 0%, 100% {{ transform: scale(1); }} 50% {{ transform: scale(1.3); filter: hue-rotate(330deg); }} }}
        @keyframes angry {{ 0%, 100% {{ transform: rotate(0deg); }} 25% {{ transform: rotate(5deg); }} 75% {{ transform: rotate(-5deg); }} }}
        @keyframes surprised {{ 0%, 100% {{ transform: scale(1); }} 50% {{ transform: scale(1.3); }} }}
        .speech-bubble {{
            position: absolute;
            bottom: 100%;
            right: 0;
            background: #313244;
            color: #CDD6F4;
            padding: 10px 15px;
            border-radius: 12px;
            font-size: 12px;
            max-width: 200px;
            display: none;
        }}
    </style>
</head>
<body>
    <div class='avatar-container'>
        <div class='avatar'>{GetAvatarEmoji()}</div>
    </div>
</body>
</html>";
        }

        private string GetAvatarEmoji()
        {
            return _config.AvatarStyle switch
            {
                "anime-girl" => "👧",
                "robot" => "🤖",
                "hologram" => "👤",
                "chibi" => "🧝",
                "realistic" => "👩",
                "pixel" => "👾",
                "minimal" => "🔵",
                "cat" => "🐱",
                _ => "🤖"
            };
        }

        #endregion

        #region Reporting

        public string GetStatus()
        {
            var style = AvatarStyles.FirstOrDefault(s => s.Id == _config.AvatarStyle);
            return $"🎭 **3D Avatar:**\n\n" +
                $"Enabled: {(_config.Enabled ? "✅" : "❌")}\n" +
                $"Style: {style?.Name ?? "Default"} {style?.Preview ?? "🤖"}\n" +
                $"Animation: {_config.CurrentAnimation}\n" +
                $"Size: {_config.Width}x{_config.Height}\n" +
                $"Position: {_config.Position}";
        }

        public string ListAvatars()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("🎭 **Available Avatars:**\n");
            foreach (var a in AvatarStyles)
            {
                string current = a.Id == _config.AvatarStyle ? " ← current" : "";
                sb.AppendLine($"{a.Preview} **{a.Name}** — {a.Description}{current}");
            }
            sb.AppendLine("\nSay `set avatar [name]` to change.");
            return sb.ToString();
        }

        #endregion

        #region Persistence

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                    _config = JsonConvert.DeserializeObject<AvatarConfig>(File.ReadAllText(ConfigPath)) ?? new();
            }
            catch { _config = new AvatarConfig(); }
        }

        private void SaveConfig()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(_config, Formatting.Indented));
            }
            catch { }
        }

        #endregion

        public void Dispose() => SaveConfig();
    }

    #region Models

    public class AvatarConfig
    {
        public bool Enabled { get; set; } = false; // Off by default (optional feature)
        public string AvatarStyle { get; set; } = "robot";
        public string CurrentAnimation { get; set; } = "idle";
        public int Width { get; set; } = 150;
        public int Height { get; set; } = 150;
        public string Position { get; set; } = "bottom-right";
    }

    public class AvatarStyle
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Preview { get; set; } = "";
    }

    #endregion
}

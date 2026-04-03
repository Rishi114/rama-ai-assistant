using System;
using System.IO;
using Newtonsoft.Json;

namespace Rama.Core
{
    /// <summary>
    /// Profile Manager — Rama's identity and user relationship settings.
    /// Change her name, voice, and how she addresses you.
    /// </summary>
    public class ProfileManager : IDisposable
    {
        private string ProfilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rama", "profile.json");

        private UserProfile _profile = new();

        // Properties
        public string BotName => _profile.BotName;
        public string UserName => _profile.UserName;
        public string UserNickname => _profile.UserNickname;
        public string PreferredVoice => _profile.PreferredVoice;
        public string BotPersonality => _profile.BotPersonality;

        public ProfileManager()
        {
            Load();
        }

        /// <summary>
        /// Change Rama's name.
        /// </summary>
        public string SetBotName(string name)
        {
            string oldName = _profile.BotName;
            _profile.BotName = name.Trim();
            Save();

            string[] responses = {
                $"Okay! From now on, I'm **{_profile.BotName}**! Nice to meet you... again? 😏",
                $"Name changed! Call me **{_profile.BotName}** now. I like it! ✨",
                $"**{_profile.BotName}** it is! Sounds way cooler than {oldName}. 😎",
                $"Done! I'm **{_profile.BotName}** now. Don't forget it! 💅"
            };
            return responses[new Random().Next(responses.Length)];
        }

        /// <summary>
        /// Set what Rama calls the user.
        /// </summary>
        public string SetUserNickname(string nickname)
        {
            _profile.UserNickname = nickname.Trim().ToLower();
            Save();

            string greeting = _profile.UserNickname switch
            {
                "bhai" => $"Bhai! 😎 Ab se main tumhe bhai bolungi. Kya haal hai?",
                "bro" or "brother" => $"Bro! 💪 From now on, you're my bro!",
                "boss" => $"Yes Boss! 👔 Tumhare orders mere liye command hain!",
                "sir ji" => $"Ji Sir Ji! 🙏 Aapka hukm sar aankhon par!",
                "dude" => $"Dude! 🤙 Cool vibes only from now on!",
                "yaar" => $"Yaar! 💕 Ab se yaar bolungi. Pakka!",
                "chief" => $"Aye aye, Chief! 🫡 Ready for your commands!",
                "captain" => $"Captain! ⚓ Aapki ship, meri service!",
                "guru" => $"Guru ji! 🙏 Seekhna abhi baki hai!",
                "king" => $"My King! 👑 Hukm kijiye!",
                "lord" => $"My Lord! ⚔️ At your service!",
                _ => $"Got it! I'll call you **{_profile.UserNickname}** from now on! 😊"
            };
            return greeting;
        }

        /// <summary>
        /// Set user's real name.
        /// </summary>
        public string SetUserName(string name)
        {
            _profile.UserName = name.Trim();
            Save();
            return $"Nice to meet you, **{_profile.UserName}**! I'll remember that. 🤝";
        }

        /// <summary>
        /// Get the greeting with nickname.
        /// </summary>
        public string GetGreeting()
        {
            string nick = GetUserAddress();
            return _profile.BotPersonality switch
            {
                "sassy" => $"Hey {nick}! Miss me? 😏",
                "cute" => $"Hiii {nick}! 🥰 I'm so happy to see you!",
                "professional" => $"Hello {nick}. How may I assist you today?",
                "naughty" => $"Oye {nick}! Kya chal raha hai? 😜",
                _ => $"Hey {nick}! What's up? 😊"
            };
        }

        /// <summary>
        /// How Rama addresses the user.
        /// </summary>
        public string GetUserAddress()
        {
            if (!string.IsNullOrEmpty(_profile.UserNickname))
                return _profile.UserNickname;

            if (!string.IsNullOrEmpty(_profile.UserName))
                return _profile.UserName;

            return "boss";
        }

        /// <summary>
        /// Set bot personality style.
        /// </summary>
        public string SetPersonality(string personality)
        {
            _profile.BotPersonality = personality.ToLower().Trim();
            Save();

            return _profile.BotPersonality switch
            {
                "sassy" => "Sassy mode activated! 💅 Attitude incoming!",
                "cute" => "Cute mode! 🥰 I'll be extra sweet now!",
                "professional" => "Professional mode. Formal and efficient. 📋",
                "naughty" => "Naughty mode ON! 😜 Things are about to get fun!",
                "chill" => "Chill vibes only 🌊 Relax and let me handle things.",
                _ => $"Personality set to {_profile.BotPersonality}!"
            };
        }

        public string GetProfileInfo()
        {
            return $"👤 **Profile:**\n\n" +
                $"🤖 My Name: **{_profile.BotName}**\n" +
                $"👤 Your Name: **{_profile.UserName ?? "Not set"}**\n" +
                $"💬 I Call You: **{GetUserAddress()}**\n" +
                $"🎭 Personality: **{_profile.BotPersonality}**\n" +
                $"🗣️ Voice: **{_profile.PreferredVoice}**\n\n" +
                "Say:\n" +
                "• `change your name to [name]` — Rename me\n" +
                "• `call me [nickname]` — Set what I call you\n" +
                "• `my name is [name]` — Tell me your name\n" +
                "• `be [sassy/cute/naughty/professional]` — Change personality";
        }

        public UserProfile GetProfile() => _profile;

        private void Load()
        {
            try
            {
                if (File.Exists(ProfilePath))
                    _profile = JsonConvert.DeserializeObject<UserProfile>(File.ReadAllText(ProfilePath)) ?? new();
            }
            catch { _profile = new UserProfile(); }
        }

        private void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ProfilePath)!);
                File.WriteAllText(ProfilePath, JsonConvert.SerializeObject(_profile, Formatting.Indented));
            }
            catch { }
        }

        public void Dispose() => Save();
    }

    public class UserProfile
    {
        public string BotName { get; set; } = "Rama";
        public string UserName { get; set; } = "";
        public string UserNickname { get; set; } = "boss";
        public string PreferredVoice { get; set; } = "default";
        public string BotPersonality { get; set; } = "sassy";
        public string BotLanguage { get; set; } = "en";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

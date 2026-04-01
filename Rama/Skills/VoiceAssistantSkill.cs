using System;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Voice Assistant Skill — Full voice interaction with Hindi, Marathi, English.
    /// Sassy, naughty, and full of attitude in every language!
    /// </summary>
    public class VoiceAssistantSkill : SkillBase
    {
        private readonly MultiVoiceEngine _voice;
        private string _voiceLang = "en";

        public VoiceAssistantSkill(MultiVoiceEngine voice)
        {
            _voice = voice;
        }

        public override string Name => "Voice Assistant";
        public override string Description => "Talk to Rama in Hindi, Marathi, or English";
        public override string[] Triggers => new[] {
            "speak hindi", "speak marathi", "speak english",
            "talk in hindi", "talk in marathi", "talk in english",
            "hindi mode", "marathi mode", "english mode",
            "बोलो", "बोल", "सांगा", "बोल मराठी", "बोल हिंदी",
            "say something", "talk to me", "voice mode"
        };

        public override bool CanHandle(string input)
        {
            string lower = input.ToLowerInvariant();
            return lower.Contains("speak hindi") || lower.Contains("speak marathi") ||
                   lower.Contains("talk in hindi") || lower.Contains("talk in marathi") ||
                   lower.Contains("hindi mode") || lower.Contains("marathi mode") ||
                   lower.Contains("say something") || lower.Contains("talk to me") ||
                   lower.Contains("बोल") || lower.Contains("सांगा") ||
                   lower.Contains("voice mode");
        }

        public override Task<string> ExecuteAsync(string input, Memory memory)
        {
            string lower = input.ToLowerInvariant();

            // Hindi mode
            if (lower.Contains("speak hindi") || lower.Contains("talk in hindi") || 
                lower.Contains("hindi mode") || lower.Contains("बोल हिंदी"))
            {
                _voiceLang = "hi";
                string response = SassyResponses.Hindi.Greeting();
                _voice.SpeakHindi(response);
                return Task.FromResult($"🇮🇳 **Hindi Mode ON!**\n\n{response}\n\n_Ab Hindi mein baat karenge!_ 🔥");
            }

            // Marathi mode
            if (lower.Contains("speak marathi") || lower.Contains("talk in marathi") || 
                lower.Contains("marathi mode") || lower.Contains("बोल मराठी"))
            {
                _voiceLang = "mr";
                string response = SassyResponses.Marathi.Greeting();
                _voice.SpeakMarathi(response);
                return Task.FromResult($"🇮🇳 **Marathi Mode ON!**\n\n{response}\n\n_Aata Marathi madhe bolu!_ 🔥");
            }

            // English mode
            if (lower.Contains("speak english") || lower.Contains("english mode"))
            {
                _voiceLang = "en";
                return Task.FromResult("🇬🇧 **English Mode ON!**\n\nBack to English, love! 😏");
            }

            // Say something
            if (lower.Contains("say something") || lower.Contains("talk to me") || 
                lower.Contains("बोल") || lower.Contains("सांगा"))
            {
                return SaySomething();
            }

            // Voice mode toggle
            if (lower.Contains("voice mode"))
            {
                string msg = _voiceLang switch
                {
                    "hi" => SassyResponses.Hindi.HowAreYou(),
                    "mr" => SassyResponses.Marathi.HowAreYou(),
                    _ => SassyResponses.English.HowAreYou()
                };

                if (_voiceLang == "hi") _voice.SpeakHindi(msg);
                else if (_voiceLang == "mr") _voice.SpeakMarathi(msg);
                else _voice.Speak(msg);

                return Task.FromResult($"🎙️ **Voice Mode Active** ({GetLangName(_voiceLang)})\n\n{msg}");
            }

            return Task.FromResult(GetHelp());
        }

        private Task<string> SaySomething()
        {
            string response;
            string display;

            switch (_voiceLang)
            {
                case "hi":
                    response = SassyResponses.Hindi.Greeting();
                    _voice.SpeakHindi(response);
                    display = $"🇮🇳 {response}";
                    break;

                case "mr":
                    response = SassyResponses.Marathi.Greeting();
                    _voice.SpeakMarathi(response);
                    display = $"🇮🇳 {response}";
                    break;

                default:
                    response = SassyResponses.English.Greeting();
                    _voice.Speak(response);
                    display = $"🇬🇧 {response}";
                    break;
            }

            return Task.FromResult(display);
        }

        /// <summary>
        /// Get a sassy response in the current language.
        /// </summary>
        public string GetSassyResponse(string type)
        {
            return _voiceLang switch
            {
                "hi" => type switch
                {
                    "greeting" => SassyResponses.Hindi.Greeting(),
                    "howareyou" => SassyResponses.Hindi.HowAreYou(),
                    "thanks" => SassyResponses.Hindi.ThankYou(),
                    "goodbye" => SassyResponses.Hindi.Goodbye(),
                    "joke" => SassyResponses.Hindi.Joke(),
                    "love" => SassyResponses.Hindi.Love(),
                    "error" => SassyResponses.Hindi.Error(),
                    "thinking" => SassyResponses.Hindi.Thinking(),
                    "working" => SassyResponses.Hindi.Working(),
                    "done" => SassyResponses.Hindi.Done(),
                    "sasson" => SassyResponses.Hindi.SassOn(),
                    "sassmax" => SassyResponses.Hindi.SassMax(),
                    _ => SassyResponses.Hindi.Greeting()
                },
                "mr" => type switch
                {
                    "greeting" => SassyResponses.Marathi.Greeting(),
                    "howareyou" => SassyResponses.Marathi.HowAreYou(),
                    "thanks" => SassyResponses.Marathi.ThankYou(),
                    "goodbye" => SassyResponses.Marathi.Goodbye(),
                    "joke" => SassyResponses.Marathi.Joke(),
                    "love" => SassyResponses.Marathi.Love(),
                    "error" => SassyResponses.Marathi.Error(),
                    "thinking" => SassyResponses.Marathi.Thinking(),
                    "working" => SassyResponses.Marathi.Working(),
                    "done" => SassyResponses.Marathi.Done(),
                    "sasson" => SassyResponses.Marathi.SassOn(),
                    "sassmax" => SassyResponses.Marathi.SassMax(),
                    _ => SassyResponses.Marathi.Greeting()
                },
                _ => type switch
                {
                    "greeting" => SassyResponses.English.Greeting(),
                    "howareyou" => SassyResponses.English.HowAreYou(),
                    "error" => SassyResponses.English.Error(),
                    _ => SassyResponses.English.Greeting()
                }
            };
        }

        /// <summary>
        /// Speak a response in the current language.
        /// </summary>
        public void SpeakResponse(string type)
        {
            string response = GetSassyResponse(type);
            switch (_voiceLang)
            {
                case "hi": _voice.SpeakHindi(response); break;
                case "mr": _voice.SpeakMarathi(response); break;
                default: _voice.Speak(response); break;
            }
        }

        private string GetLangName(string code) => code switch
        {
            "hi" => "Hindi 🇮🇳",
            "mr" => "Marathi 🇮🇳",
            _ => "English 🇬🇧"
        };

        private string GetHelp()
        {
            return "🎙️ **Voice Assistant — Hindi, Marathi, English!**\n\n" +
                "**Switch Language:**\n" +
                "• `speak hindi` / `हिंदी में बोलो` — Hindi mode 🔥\n" +
                "• `speak marathi` / `मराठीत बोल` — Marathi mode 🔥\n" +
                "• `speak english` — English mode\n\n" +
                "**Voice Commands:**\n" +
                "• `say something` — I'll talk to you!\n" +
                "• `talk to me` — Full conversation mode\n\n" +
                "**Sassy in Every Language:**\n" +
                "• Hindi: \"अरे वाह! तुम आ गए?\" 😏\n" +
                "• Marathi: \"अरे वा! तुम आलात?\" 😏\n" +
                "• English: \"Well well well...\" 😏";
        }
    }
}

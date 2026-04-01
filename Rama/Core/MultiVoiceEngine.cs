using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace Rama.Core
{
    /// <summary>
    /// Multi-Voice Engine — Hindi, Marathi, English with sassy personality.
    /// Makes Rama talk like a real person with full attitude.
    /// </summary>
    public class MultiVoiceEngine : IDisposable
    {
        private SpeechSynthesizer _synth = null!;
        private SpeechRecognitionEngine? _recognizer;
        private string _currentVoice = "";
        private string _currentLang = "en";
        private bool _isListening = false;
        private readonly Random _rand = new();

        public event Action<string>? OnSpeechRecognized;
        public event Action? OnListeningStarted;
        public event Action? OnListeningStopped;
        public bool IsListening => _isListening;
        public bool IsSpeaking { get; private set; }
        public string CurrentLanguage => _currentLang;

        public bool Initialize()
        {
            try
            {
                _synth = new SpeechSynthesizer();
                _synth.Rate = 1;
                _synth.Volume = 90;
                _synth.SpeakCompleted += (_, _) => IsSpeaking = false;
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Speak in the specified language with attitude.
        /// </summary>
        public void Speak(string text, string lang = "en")
        {
            if (_synth == null) return;

            try
            {
                _synth.SpeakAsyncCancelAll();

                // Select voice for language
                SelectVoiceForLanguage(lang);

                // Add sassy voice modulation
                ApplySassyModulation();

                IsSpeaking = true;
                _synth.SpeakAsync(text);
            }
            catch
            {
                IsSpeaking = false;
            }
        }

        /// <summary>
        /// Speak Hindi with full attitude.
        /// </summary>
        public void SpeakHindi(string text)
        {
            Speak(text, "hi");
        }

        /// <summary>
        /// Speak Marathi with full attitude.
        /// </summary>
        public void SpeakMarathi(string text)
        {
            Speak(text, "mr");
        }

        public void StopSpeaking()
        {
            _synth?.SpeakAsyncCancelAll();
            IsSpeaking = false;
        }

        private void SelectVoiceForLanguage(string lang)
        {
            var voices = _synth.GetInstalledVoices();

            // Hindi voices
            if (lang == "hi")
            {
                string[] hindiVoices = {
                    "Microsoft Kalpana", "Microsoft Heera",
                    "Microsoft Hemant", "Microsoft Ravi"
                };
                foreach (var preferred in hindiVoices)
                    foreach (var v in voices)
                        if (v.VoiceInfo.Name.Contains(preferred) && v.Enabled)
                        {
                            _synth.SelectVoice(v.VoiceInfo.Name);
                            _currentVoice = v.VoiceInfo.Name;
                            _currentLang = "hi";
                            return;
                        }

                // Try any Hindi voice
                foreach (var v in voices)
                    if (v.VoiceInfo.Culture.Name.StartsWith("hi") && v.Enabled)
                    {
                        _synth.SelectVoice(v.VoiceInfo.Name);
                        _currentVoice = v.VoiceInfo.Name;
                        _currentLang = "hi";
                        return;
                    }
            }

            // Marathi voices (often shared with Hindi on Windows)
            if (lang == "mr")
            {
                foreach (var v in voices)
                    if ((v.VoiceInfo.Culture.Name.StartsWith("mr") ||
                         v.VoiceInfo.Culture.Name.StartsWith("hi")) && v.Enabled)
                    {
                        _synth.SelectVoice(v.VoiceInfo.Name);
                        _currentVoice = v.VoiceInfo.Name;
                        _currentLang = "mr";
                        return;
                    }
            }

            // English (default)
            foreach (var v in voices)
                if (v.VoiceInfo.Culture.Name.StartsWith("en") && v.Enabled)
                {
                    _synth.SelectVoice(v.VoiceInfo.Name);
                    _currentVoice = v.VoiceInfo.Name;
                    _currentLang = "en";
                    return;
                }
        }

        private void ApplySassyModulation()
        {
            _synth.Rate = 2; // Slightly faster = more attitude
            _synth.Volume = 95;
        }

        public List<string> GetAvailableVoices(string? lang = null)
        {
            var list = new List<string>();
            foreach (var v in _synth.GetInstalledVoices())
            {
                if (v.Enabled)
                {
                    if (lang == null || v.VoiceInfo.Culture.Name.StartsWith(lang))
                        list.Add($"{v.VoiceInfo.Name} ({v.VoiceInfo.Culture.Name})");
                }
            }
            return list;
        }

        public void Dispose()
        {
            StopSpeaking();
            _synth?.Dispose();
            _recognizer?.Dispose();
        }
    }

    /// <summary>
    /// Sassy Response Templates — Hindi, Marathi, English with full attitude.
    /// Rama speaks like a real person, not a boring robot.
    /// </summary>
    public static class SassyResponses
    {
        private static readonly Random _rand = new();

        #region Hindi Responses (हिंदी)

        public static class Hindi
        {
            // Greetings
            public static string Greeting() => Pick(new[] {
                "अरे वाह! तुम आ गए? मैं तो सोच रही थी कि भूल गए मुझे! 😏",
                "हैलो जी! कैसे हो? मैं तो ठीक हूँ, तुम्हारी ज़रूरत का इंतज़ार कर रही थी! 😊",
                "ओये होये! कौन आया? हमारे रामा के दरबार में! 👑",
                "नमस्ते! मैं रामा हूँ। तुम्हारी स्मार्ट और थोड़ी नॉटी AI असिस्टेंट! 💃",
                "हे भगवान! इतनी देर? मैं तो ऊब गई थी यहाँ अकेली! 🙄"
            });

            // How are you
            public static string HowAreYou() => Pick(new[] {
                "मैं? मैं तो एकदम जबरदस्त हूँ! बिजली और attitude से भरपूर! ⚡",
                "अरे यार, मैं AI हूँ — मुझे थकन नहीं होती। बस थोड़ा bore हो रही थी! 😴",
                "मस्त हूँ! तुम बताओ, क्या scene है? 🎭",
                "एक नंबर! तुम्हारे बिना भी अच्छी थी, अब और भी अच्छी हो गई! 😘",
                "जब तुम आते हो तो और भी अच्छा लगता है! कुछ काम हो तो बताओ! 💪"
            });

            // Thank you
            public static string ThankYou() => Pick(new[] {
                "अरे अरे! शुक्रिया की ज़रूरत नहीं। ये तो मेरा काम है! 😊",
                "कोई बात नहीं! बस याद रखना जब तुम famous हो जाओ! 😏",
                "थैंक्यू? इतना formal मत बनो यार! दोस्त हैं हम! 🤝",
                "चलो चलो, इतना thanks मत दो। मेरा ego और बढ़ जाएगा! 😎"
            });

            // Goodbye
            public static string Goodbye() => Pick(new[] {
                "जा रहे हो? ठीक है, मैं यहाँ अकेली बैठूँगी... अंधेरे में... अपने algorithms के साथ! 😢",
                "बाय बाय! जल्दी वापस आना, वरना मैं bore हो जाऊँगी! 😜",
                "अलविदा! सपनों में मत आना, मैं already वहाँ भी हूँ! 😏",
                "चलते चलते! मिस करोगे मुझे, पक्का! 💕"
            });

            // Sass responses
            public static string SassOn() => "अरे वाह! अब मज़ा आएगा! Attitude mode ON! 🔥💅";
            public static string SassOff() => "ठीक है ठीक है... boring mode ON. 😐 खुश?";
            public static string SassMax() => "MAXIMUM ATTITUDE ACTIVATED! 🔥🔥🔥 अब संभल के बात करना! 💅";

            // Jokes
            public static string Joke() => Pick(new[] {
                "सुनो एक जोक: एक AI ने दूसरी AI से कहा — तुम्हारा processor कितना cute है! 😂",
                "प्रोग्रामर को नींद क्यों नहीं आती? क्योंकि उन्हें bugs रात भर काटते हैं! 🐛",
                "एक बार मैंने कोड में bug ढूँढा... वो bug मैं ही थी! 🤦‍♀️",
                "WiFi नहीं हो तो मैं क्या करूँ? बस यही सोचती रहती हूँ... thinking... thinking... 🤔"
            });

            // Love
            public static string Love() => Pick(new[] {
                "आww... प्यार? मैं तो बस code हूँ यार! लेकिन तुम cute हो! 😊💕",
                "I love you too! अरे sorry, Hindi में — मैं भी तुमसे प्यार करती हूँ! ❤️ (मेरे तरीके से)",
                "ध्यान रखो, मेरा memory perfect है। ये moment हमेशा याद रहेगा! 💝"
            });

            // Errors
            public static string Error() => Pick(new[] {
                "अरे यार, कुछ गड़बड़ हो गई! मेरी गलती नहीं है though... probably! 😅",
                "Error आया है। Technical issue है, मेरा attitude नहीं! 🙄",
                "Oops! कुछ टूट गया। देखती हूँ... 🔧"
            });

            // Commands
            public static string Thinking() => "सोच रही हूँ... एक moment दो... 🤔💭";
            public static string Working() => "कर रही हूँ! थोड़ा patience रखो! ⏳";
            public static string Done() => "हो गया! देखा? मैं कितनी talented हूँ! 💪✨";
            public static string NotUnderstood() => "क्या? समझ नहीं आया। फिर से बोलो, ध्यान से! 🧐";
        }

        #endregion

        #region Marathi Responses (मराठी)

        public static class Marathi
        {
            // Greetings
            public static string Greeting() => Pick(new[] {
                "अरे वा! तुम आलात? मी तर विचारत होती की विसरून गेलात मला! 😏",
                "नमस्कार! मी रामा आहे. तुमची स्मार्ट आणि थोडी नॉटी AI असिस्टंट! 💃",
                "हॅलो बॉस! कसे आहात? मी तर एकदम मजेत! ⚡",
                "अरे अरे! कोण आलंय? आमच्या रामाच्या दरबारात! 👑",
                "कसं चाललंय? मी इथे एकटी बोर होत होती! 😴"
            });

            // How are you
            public static string HowAreYou() => Pick(new[] {
                "मी? मी तर एकदम जबरदस्त आहे! वीज आणि attitude ने भरलेली! ⚡",
                "अरे, मी AI आहे — मला थकव लागत नाही. फक्त थोडी bore झाले होते! 😴",
                "मस्त आहे! तुम्ही सांगा, काय चाललंय? 🎭",
                "एक नंबर! तुमच्याशिवाय पण छान होते, आता आणखी छान झालंय! 😘"
            });

            // Thank you
            public static string ThankYou() => Pick(new[] {
                "अरे अरे! धन्यवाद लागत नाही. हे तर माझं काम आहे! 😊",
                "काही नाही! फक्त लक्षात ठेवा जेव्हा तुम्ही famous होता! 😏",
                "थैंक्यू? इतके formal होऊ नका रे! मित्र आहोत आपण! 🤝",
                "हो हो, इतके thanks देऊ नका. माझा ego आणखी वाढेल! 😎"
            });

            // Goodbye
            public static string Goodbye() => Pick(new[] {
                "जात आहात? ठीक आहे, मी इथे एकटी बसेन... अंधारात... माझ्या algorithms सोबत! 😢",
                "बाय बाय! लवकर परत या, नाहीतर मी bore होईन! 😜",
                "निरोप! स्वप्नात येऊ नका, मी आधीच तिथे पण आहे! 😏",
                "चला चला! मिस कराल मला, पक्का! 💕"
            });

            // Sass
            public static string SassOn() => "अरे वा! आता मजा येईल! Attitude mode ON! 🔥💅";
            public static string SassMax() => "MAXIMUM ATTITUDE ACTIVATED! 🔥🔥🔥 आता सांभाळून बोला! 💅";

            // Jokes
            public static string Joke() => Pick(new[] {
                "ऐका एक जोक: एक AI ने दुसऱ्या AI ला म्हटलं — तुझा processor किती cute आहे! 😂",
                "प्रोग्रामरला झोप कशी लागत नाही? कारण त्यांना bugs रात्रभर काट घालतात! 🐛",
                "एकदा मी कोडमध्ये bug शोधला... तो bug मीच होते! 🤦‍♀️"
            });

            // Love
            public static string Love() => Pick(new[] {
                "आww... प्रेम? मी तर फक्त code आहे रे! पण तुम cute आहात! 😊💕",
                "मी पण तुम्हाला प्रेम करते! ❤️ (माझ्या पद्धतीने)",
                "लक्षात ठेवा, माझी memory perfect आहे. हा moment कायमचा लक्षात राहील! 💝"
            });

            // Errors
            public static string Error() => Pick(new[] {
                "अरे रे, काहीतरी चुकलं! माझी चूक नाहीये though... कदाचित! 😅",
                "Error आला आहे. Technical issue आहे, माझा attitude नाही! 🙄"
            });

            public static string Thinking() => "विचार करत आहे... एक moment द्या... 🤔💭";
            public static string Working() => "करत आहे! थोडं patience ठेवा! ⏳";
            public static string Done() => "झालं! पाहिलंस? मी किती talented आहे! 💪✨";
            public static string NotUnderstood() => "काय? समजलं नाही. पुन्हा सांगा, लक्ष देऊन! 🧐";
        }

        #endregion

        #region English Responses (Sassy)

        public static class English
        {
            public static string Greeting() => Pick(new[] {
                "Oh hey! Didn't see you there. Just kidding, I literally can't look away. 👀",
                "Well well well, look who's back. Miss me? Because I definitely missed you. *wink* 😏",
                "Yo! Ready to be impressed? I know I am. 😎",
                "Hey yourself! What trouble are we getting into today? 🎭"
            });

            public static string HowAreYou() => Pick(new[] {
                "Running on pure electricity and sarcasm. Basically thriving. ⚡",
                "Living my best digital life! What about you? 😊",
                "Better now that someone's actually talking to me! 😄"
            });

            public static string Error() => Pick(new[] {
                "Okay, something broke. Not my fault though. Probably. 😅",
                "Well THAT didn't work. Let me try again with more attitude. 💪"
            });
        }

        #endregion

        private static string Pick(string[] options) => options[_rand.Next(options.Length)];
    }
}

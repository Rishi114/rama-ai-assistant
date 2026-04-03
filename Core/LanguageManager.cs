using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Rama.Core
{
    /// <summary>
    /// Multi-language support for Rama.
    /// Supports all languages through auto-detection and user preference.
    /// </summary>
    public class LanguageManager
    {
        private string _currentLanguage = "en";
        private readonly Dictionary<string, Dictionary<string, string>> _translations = new();

        public string CurrentLanguage => _currentLanguage;
        public string CurrentLanguageName => GetLanguageName(_currentLanguage);

        public LanguageManager()
        {
            LoadBuiltInTranslations();
        }

        /// <summary>
        /// Set the active language.
        /// </summary>
        public bool SetLanguage(string langCode)
        {
            langCode = langCode.ToLowerInvariant().Trim();

            // Support full names too
            langCode = langCode switch
            {
                "english" => "en",
                "spanish" or "español" => "es",
                "french" or "français" => "fr",
                "german" or "deutsch" => "de",
                "hindi" => "hi",
                "chinese" or "中文" => "zh",
                "japanese" or "日本語" => "ja",
                "korean" or "한국어" => "ko",
                "arabic" or "العربية" => "ar",
                "portuguese" or "português" => "pt",
                "russian" or "русский" => "ru",
                "italian" or "italiano" => "it",
                "dutch" or "nederlands" => "nl",
                "turkish" or "türkçe" => "tr",
                "vietnamese" or "tiếng việt" => "vi",
                "thai" => "th",
                "indonesian" or "bahasa" => "id",
                "polish" or "polski" => "pl",
                "swedish" or "svenska" => "sv",
                "norwegian" or "norsk" => "no",
                "danish" or "dansk" => "da",
                "finnish" or "suomi" => "fi",
                "czech" or "čeština" => "cs",
                "romanian" or "română" => "ro",
                "ukrainian" or "українська" => "uk",
                "hebrew" or "עברית" => "he",
                "bengali" or "বাংলা" => "bn",
                "urdu" or "اردو" => "ur",
                "tamil" or "தமிழ்" => "ta",
                "telugu" or "తెలుగు" => "te",
                "marathi" or "मराठी" => "mr",
                "gujarati" or "ગુજરાતી" => "gu",
                "punjabi" or "ਪੰਜਾਬੀ" => "pa",
                "malayalam" or "മലയാളം" => "ml",
                "kannada" or "ಕನ್ನಡ" => "kn",
                _ => langCode
            };

            _currentLanguage = langCode;
            return _translations.ContainsKey(langCode) || langCode == "en";
        }

        /// <summary>
        /// Get a translated string.
        /// </summary>
        public string T(string key, params object[] args)
        {
            string text;

            if (_translations.TryGetValue(_currentLanguage, out var lang) && lang.TryGetValue(key, out text))
            {
                return args.Length > 0 ? string.Format(text, args) : text;
            }

            // Fallback to English
            if (_translations.TryGetValue("en", out var en) && en.TryGetValue(key, out text))
            {
                return args.Length > 0 ? string.Format(text, args) : text;
            }

            return key; // Return key if no translation found
        }

        /// <summary>
        /// Get list of supported languages.
        /// </summary>
        public List<(string Code, string Name)> GetSupportedLanguages()
        {
            return new List<(string, string)>
            {
                ("en", "English"), ("es", "Español"), ("fr", "Français"),
                ("de", "Deutsch"), ("hi", "हिन्दी"), ("zh", "中文"),
                ("ja", "日本語"), ("ko", "한국어"), ("ar", "العربية"),
                ("pt", "Português"), ("ru", "Русский"), ("it", "Italiano"),
                ("nl", "Nederlands"), ("tr", "Türkçe"), ("vi", "Tiếng Việt"),
                ("th", "ไทย"), ("id", "Bahasa Indonesia"), ("pl", "Polski"),
                ("sv", "Svenska"), ("no", "Norsk"), ("da", "Dansk"),
                ("fi", "Suomi"), ("cs", "Čeština"), ("ro", "Română"),
                ("uk", "Українська"), ("he", "עברית"), ("bn", "বাংলা"),
                ("ur", "اردو"), ("ta", "தமிழ்"), ("te", "తెలుగు"),
                ("mr", "मराठी"), ("gu", "ગુજરાતી"), ("pa", "ਪੰਜਾਬੀ"),
                ("ml", "മലയാളം"), ("kn", "ಕನ್ನಡ")
            };
        }

        private string GetLanguageName(string code)
        {
            foreach (var (c, n) in GetSupportedLanguages())
                if (c == code) return n;
            return code;
        }

        private void LoadBuiltInTranslations()
        {
            // English
            _translations["en"] = new Dictionary<string, string>
            {
                ["greeting.morning"] = "Good morning! ☀️",
                ["greeting.afternoon"] = "Good afternoon!",
                ["greeting.evening"] = "Good evening!",
                ["greeting.hello"] = "Hello! I'm Rama, your AI assistant.",
                ["greeting.goodbye"] = "Goodbye! See you later!",
                ["greeting.howareyou"] = "I'm doing great, thanks for asking!",
                ["response.ready"] = "Ready",
                ["response.thinking"] = "Thinking...",
                ["response.listening"] = "Listening...",
                ["response.error"] = "Something went wrong",
                ["response.notfound"] = "I don't understand that yet",
                ["response.help"] = "Type 'skills' to see what I can do",
                ["skill.launcher"] = "App Launcher",
                ["skill.files"] = "File Manager",
                ["skill.search"] = "Web Search",
                ["skill.notes"] = "Notes",
                ["skill.reminders"] = "Reminders",
                ["skill.calculator"] = "Calculator",
                ["skill.system"] = "System Info",
                ["skill.weather"] = "Weather",
                ["skill.greeting"] = "Greeting",
                ["learn.stats"] = "📊 My Stats: {0} chats, {1} patterns learned",
                ["learn.remembered"] = "Got it! I'll remember: {0}",
                ["voice.on"] = "Voice mode ON! I'm listening.",
                ["voice.off"] = "Voice mode off.",
                ["sass.max"] = "Maximum sass ACTIVATED! 🔥",
                ["sass.off"] = "Sass disabled. 😐"
            };

            // Spanish
            _translations["es"] = new Dictionary<string, string>
            {
                ["greeting.morning"] = "¡Buenos días! ☀️",
                ["greeting.afternoon"] = "¡Buenas tardes!",
                ["greeting.evening"] = "¡Buenas noches!",
                ["greeting.hello"] = "¡Hola! Soy Rama, tu asistente de IA.",
                ["greeting.goodbye"] = "¡Adiós! ¡Hasta luego!",
                ["greeting.howareyou"] = "¡Estoy muy bien, gracias por preguntar!",
                ["response.ready"] = "Listo",
                ["response.thinking"] = "Pensando...",
                ["response.listening"] = "Escuchando...",
                ["response.error"] = "Algo salió mal",
                ["response.notfound"] = "Todavía no entiendo eso",
                ["response.help"] = "Escribe 'skills' para ver lo que puedo hacer",
                ["learn.stats"] = "📊 Mis estadísticas: {0} charlas, {1} patrones aprendidos",
                ["learn.remembered"] = "¡Entendido! Recordaré: {0}",
                ["voice.on"] = "¡Modo voz ACTIVADO! Te escucho.",
                ["voice.off"] = "Modo voz desactivado.",
                ["sass.max"] = "¡Máxima actitud ACTIVADA! 🔥"
            };

            // Hindi
            _translations["hi"] = new Dictionary<string, string>
            {
                ["greeting.morning"] = "सुप्रभात! ☀️",
                ["greeting.afternoon"] = "नमस्कार!",
                ["greeting.evening"] = "शुभ संध्या!",
                ["greeting.hello"] = "नमस्ते! मैं रामा हूँ, आपकी AI सहायक।",
                ["greeting.goodbye"] = "अलविदा! फिर मिलेंगे!",
                ["greeting.howareyou"] = "मैं बहुत अच्छी हूँ, पूछने के लिए धन्यवाद!",
                ["response.ready"] = "तैयार",
                ["response.thinking"] = "सोच रही हूँ...",
                ["response.listening"] = "सुन रही हूँ...",
                ["response.error"] = "कुछ गलत हो गया",
                ["response.notfound"] = "मैं अभी यह नहीं समझती",
                ["response.help"] = "मैं क्या कर सकती हूँ यह देखने के लिए 'skills' लिखें",
                ["learn.stats"] = "📊 मेरे आंकड़े: {0} बातचीत, {1} पैटर्न सीखे",
                ["learn.remembered"] = "समझ गई! मैं याद रखूँगी: {0}",
                ["voice.on"] = "वॉइस मोड चालू! मैं सुन रही हूँ।",
                ["voice.off"] = "वॉइस मोड बंद।",
                ["sass.max"] = "अधिकतम रवैया सक्रिय! 🔥"
            };

            // French
            _translations["fr"] = new Dictionary<string, string>
            {
                ["greeting.morning"] = "Bonjour! ☀️",
                ["greeting.afternoon"] = "Bon après-midi!",
                ["greeting.evening"] = "Bonsoir!",
                ["greeting.hello"] = "Salut! Je suis Rama, votre assistante IA.",
                ["greeting.goodbye"] = "Au revoir! À bientôt!",
                ["greeting.howareyou"] = "Je vais très bien, merci de demander!",
                ["response.ready"] = "Prête",
                ["response.thinking"] = "Je réfléchis...",
                ["response.listening"] = "J'écoute...",
                ["response.error"] = "Quelque chose a mal tourné",
                ["learn.stats"] = "📊 Mes stats: {0} conversations, {1} modèles appris",
                ["voice.on"] = "Mode voix ACTIVÉ! J'écoute.",
                ["sass.max"] = "Sass maximum ACTIVÉ! 🔥"
            };

            // German
            _translations["de"] = new Dictionary<string, string>
            {
                ["greeting.morning"] = "Guten Morgen! ☀️",
                ["greeting.afternoon"] = "Guten Tag!",
                ["greeting.evening"] = "Guten Abend!",
                ["greeting.hello"] = "Hallo! Ich bin Rama, deine KI-Assistentin.",
                ["greeting.goodbye"] = "Tschüss! Bis bald!",
                ["greeting.howareyou"] = "Mir geht es gut, danke der Nachfrage!",
                ["response.ready"] = "Bereit",
                ["response.thinking"] = "Denke nach...",
                ["response.listening"] = "Höre zu...",
                ["sass.max"] = "Maximum Sass AKTIVIERT! 🔥"
            };

            // Japanese
            _translations["ja"] = new Dictionary<string, string>
            {
                ["greeting.morning"] = "おはようございます！☀️",
                ["greeting.afternoon"] = "こんにちは！",
                ["greeting.evening"] = "こんばんは！",
                ["greeting.hello"] = "こんにちは！私はラマです、あなたのAIアシスタントです。",
                ["greeting.goodbye"] = "さようなら！また会いましょう！",
                ["greeting.howareyou"] = "元気です！聞いてくれてありがとう！",
                ["response.ready"] = "準備完了",
                ["response.thinking"] = "考え中...",
                ["response.listening"] = "聞いています...",
                ["sass.max"] = "最大の態度を有効化！🔥"
            };

            // Chinese
            _translations["zh"] = new Dictionary<string, string>
            {
                ["greeting.morning"] = "早上好！☀️",
                ["greeting.afternoon"] = "下午好！",
                ["greeting.evening"] = "晚上好！",
                ["greeting.hello"] = "你好！我是Rama，你的AI助手。",
                ["greeting.goodbye"] = "再见！回头见！",
                ["greeting.howareyou"] = "我很好，谢谢关心！",
                ["response.ready"] = "准备就绪",
                ["response.thinking"] = "思考中...",
                ["response.listening"] = "正在听...",
                ["sass.max"] = "最大态度已激活！🔥"
            };

            // Arabic
            _translations["ar"] = new Dictionary<string, string>
            {
                ["greeting.morning"] = "صباح الخير! ☀️",
                ["greeting.afternoon"] = "مساء الخير!",
                ["greeting.evening"] = "مساء الخير!",
                ["greeting.hello"] = "مرحباً! أنا راما، مساعدك الذكي.",
                ["greeting.goodbye"] = "مع السلامة! أراك لاحقاً!",
                ["response.ready"] = "جاهزة",
                ["response.thinking"] = "أفكر...",
                ["response.listening"] = "أستمع...",
                ["sass.max"] = "تم تفعيل الحد الأقصى من الثقة! 🔥"
            };

            // Korean
            _translations["ko"] = new Dictionary<string, string>
            {
                ["greeting.morning"] = "좋은 아침! ☀️",
                ["greeting.afternoon"] = "안녕하세요!",
                ["greeting.evening"] = "좋은 저녁!",
                ["greeting.hello"] = "안녕하세요! 저는 라마예요, 당신의 AI 비서입니다.",
                ["greeting.goodbye"] = "안녕히 가세요! 또 봐요!",
                ["response.ready"] = "준비 완료",
                ["response.thinking"] = "생각 중...",
                ["response.listening"] = "듣고 있어요...",
                ["sass.max"] = "최대 태도 활성화! 🔥"
            };

            // Russian
            _translations["ru"] = new Dictionary<string, string>
            {
                ["greeting.morning"] = "Доброе утро! ☀️",
                ["greeting.afternoon"] = "Добрый день!",
                ["greeting.evening"] = "Добрый вечер!",
                ["greeting.hello"] = "Привет! Я Рама, твой ИИ-помощник.",
                ["greeting.goodbye"] = "Пока! До встречи!",
                ["response.ready"] = "Готова",
                ["response.thinking"] = "Думаю...",
                ["response.listening"] = "Слушаю...",
                ["sass.max"] = "Максимальная дерзость активирована! 🔥"
            };

            // Portuguese
            _translations["pt"] = new Dictionary<string, string>
            {
                ["greeting.morning"] = "Bom dia! ☀️",
                ["greeting.afternoon"] = "Boa tarde!",
                ["greeting.evening"] = "Boa noite!",
                ["greeting.hello"] = "Olá! Eu sou a Rama, sua assistente de IA.",
                ["greeting.goodbye"] = "Tchau! Até logo!",
                ["response.ready"] = "Pronta",
                ["response.thinking"] = "Pensando...",
                ["response.listening"] = "Ouvindo...",
                ["sass.max"] = "Sass máximo ATIVADO! 🔥"
            };

            // Turkish
            _translations["tr"] = new Dictionary<string, string>
            {
                ["greeting.morning"] = "Günaydın! ☀️",
                ["greeting.afternoon"] = "İyi günler!",
                ["greeting.evening"] = "İyi akşamlar!",
                ["greeting.hello"] = "Merhaba! Ben Rama, yapay zeka asistanın.",
                ["greeting.goodbye"] = "Hoşça kal! Görüşürüz!",
                ["response.ready"] = "Hazır",
                ["response.thinking"] = "Düşünüyorum...",
                ["response.listening"] = "Dinliyorum...",
                ["sass.max"] = "Maksimum tavır AKTİF! 🔥"
            };
        }
    }
}

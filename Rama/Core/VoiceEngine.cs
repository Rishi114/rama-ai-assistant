using System;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Rama.Core
{
    /// <summary>
    /// Voice engine for Rama — handles speech recognition (listening) 
    /// and text-to-speech (talking back with attitude).
    /// Uses Windows built-in System.Speech APIs.
    /// </summary>
    public class VoiceEngine : IDisposable
    {
        private SpeechSynthesizer _synthesizer = null!;
        private SpeechRecognitionEngine _recognizer = null!;
        private bool _isListening = false;
        private bool _isInitialized = false;
        private string _voiceName = "";
        private CancellationTokenSource? _listenCts;

        // Events
        public event Action<string>? OnSpeechRecognized;
        public event Action<string>? OnSpeechPartial;
        public event Action? OnListeningStarted;
        public event Action? OnListeningStopped;
        public event Action<string>? OnError;

        // Properties
        public bool IsListening => _isListening;
        public bool IsSpeaking { get; private set; }
        public bool IsInitialized => _isInitialized;
        public string CurrentVoice => _voiceName;
        public int SpeechRate { get; set; } = 2;    // -10 to 10, default slightly faster
        public int SpeechVolume { get; set; } = 85; // 0 to 100

        /// <summary>
        /// Initialize the voice engine. Call once at startup.
        /// </summary>
        public bool Initialize()
        {
            try
            {
                // Initialize synthesizer (text-to-speech)
                _synthesizer = new SpeechSynthesizer();
                _synthesizer.Rate = SpeechRate;
                _synthesizer.Volume = SpeechVolume;
                _synthesizer.SpeakCompleted += (_, _) => IsSpeaking = false;

                // Pick the best available voice
                SelectBestVoice();

                // Initialize recognizer (speech-to-text)
                _recognizer = new SpeechRecognitionEngine();

                // Load dictation grammar for free-form speech
                var dictation = new DictationGrammar();
                dictation.Name = "Dictation";
                _recognizer.LoadGrammar(dictation);

                // Also load a grammar for common commands
                var commands = new Choices(new[]
                {
                    "hey rama", "rama", "stop listening", "be quiet",
                    "what can you do", "help me", "thank you",
                    "open", "close", "search", "calculate",
                    "take note", "show notes", "remind me",
                    "weather", "what time is it", "good morning",
                    "good night", "how are you", "tell me a joke"
                });
                var commandGrammar = new Grammar(new GrammarBuilder(commands));
                commandGrammar.Name = "Commands";
                _recognizer.LoadGrammar(commandGrammar);

                // Wire up events
                _recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
                _recognizer.SpeechHypothesized += Recognizer_SpeechHypothesized;
                _recognizer.SpeechRecognitionRejected += Recognizer_SpeechRejected;
                _recognizer.RecognizeCompleted += Recognizer_RecognizeCompleted;

                // Set input to default microphone
                _recognizer.SetInputToDefaultAudioDevice();

                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Voice init failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Select the best available voice (prefer female voices for Rama).
        /// </summary>
        private void SelectBestVoice()
        {
            var voices = _synthesizer.GetInstalledVoices();

            // Preference order
            string[] preferredVoices = {
                "Microsoft Zira Desktop",       // Windows 10/11 female
                "Microsoft Hazel Desktop",      // UK English female
                "Microsoft Eva",                // Modern female
                "Microsoft Aria",               // Newer female
                "Microsoft Jenny",              // Neural voice
                "Microsoft Heera",              // Indian English
                "Microsoft Catherine",          // Australian
            };

            foreach (var preferred in preferredVoices)
            {
                foreach (var voice in voices)
                {
                    if (voice.VoiceInfo.Name.Contains(preferred, StringComparison.OrdinalIgnoreCase) &&
                        voice.Enabled)
                    {
                        _synthesizer.SelectVoice(voice.VoiceInfo.Name);
                        _voiceName = voice.VoiceInfo.Name;
                        return;
                    }
                }
            }

            // Fallback: use any enabled female voice
            foreach (var voice in voices)
            {
                if (voice.VoiceInfo.Gender == VoiceGender.Female && voice.Enabled)
                {
                    _synthesizer.SelectVoice(voice.VoiceInfo.Name);
                    _voiceName = voice.VoiceInfo.Name;
                    return;
                }
            }

            // Last resort: first available voice
            if (voices.Count > 0)
            {
                _synthesizer.SelectVoice(voices[0].VoiceInfo.Name);
                _voiceName = voices[0].VoiceInfo.Name;
            }
        }

        /// <summary>
        /// Get all available voices.
        /// </summary>
        public List<string> GetAvailableVoices()
        {
            var names = new List<string>();
            foreach (var voice in _synthesizer.GetInstalledVoices())
            {
                if (voice.Enabled)
                    names.Add(voice.VoiceInfo.Name);
            }
            return names;
        }

        /// <summary>
        /// Change the voice.
        /// </summary>
        public bool SetVoice(string voiceName)
        {
            try
            {
                _synthesizer.SelectVoice(voiceName);
                _voiceName = voiceName;
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Text-to-Speech

        /// <summary>
        /// Speak text aloud. Non-blocking.
        /// </summary>
        public void Speak(string text)
        {
            if (!_isInitialized || string.IsNullOrWhiteSpace(text)) return;

            try
            {
                // Clean up text for speech (remove markdown, emojis)
                string cleanText = CleanForSpeech(text);

                // Cancel any current speech
                _synthesizer.SpeakAsyncCancelAll();
                IsSpeaking = true;
                _synthesizer.SpeakAsync(cleanText);
            }
            catch (Exception ex)
            {
                IsSpeaking = false;
                OnError?.Invoke($"Speech error: {ex.Message}");
            }
        }

        /// <summary>
        /// Speak text and wait for completion.
        /// </summary>
        public void SpeakSync(string text)
        {
            if (!_isInitialized || string.IsNullOrWhiteSpace(text)) return;

            try
            {
                string cleanText = CleanForSpeech(text);
                IsSpeaking = true;
                _synthesizer.Speak(cleanText);
                IsSpeaking = false;
            }
            catch (Exception ex)
            {
                IsSpeaking = false;
                OnError?.Invoke($"Speech error: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop speaking immediately.
        /// </summary>
        public void StopSpeaking()
        {
            try
            {
                _synthesizer?.SpeakAsyncCancelAll();
                IsSpeaking = false;
            }
            catch { }
        }

        /// <summary>
        /// Clean text for speech output.
        /// Removes markdown, emojis, special formatting.
        /// </summary>
        private string CleanForSpeech(string text)
        {
            // Remove emojis (rough approximation)
            var cleaned = System.Text.RegularExpressions.Regex.Replace(text,
                @"[\x{1F600}-\x{1F64F}\x{1F300}-\x{1F5FF}\x{1F680}-\x{1F6FF}\x{2600}-\x{26FF}\x{2700}-\x{27BF}]",
                "");

            // Remove markdown bold
            cleaned = cleaned.Replace("**", "");

            // Remove markdown headers
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"^#{1,6}\s*", "",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            // Remove bullet points formatting
            cleaned = cleaned.Replace("•", "").Replace("  ", " ");

            // Remove code blocks
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"`[^`]+`", "");

            // Clean up excess whitespace
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ").Trim();

            return cleaned;
        }

        #endregion

        #region Speech Recognition

        /// <summary>
        /// Start listening for voice input. Runs continuously until stopped.
        /// </summary>
        public void StartListening()
        {
            if (!_isInitialized || _isListening) return;

            try
            {
                _listenCts = new CancellationTokenSource();
                _isListening = true;
                OnListeningStarted?.Invoke();

                // Start async recognition
                _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch (Exception ex)
            {
                _isListening = false;
                OnError?.Invoke($"Couldn't start listening: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop listening for voice input.
        /// </summary>
        public void StopListening()
        {
            if (!_isListening) return;

            try
            {
                _listenCts?.Cancel();
                _recognizer.RecognizeAsyncStop();
                _isListening = false;
                OnListeningStopped?.Invoke();
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Stop listening error: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggle listening on/off.
        /// </summary>
        public void ToggleListening()
        {
            if (_isListening)
                StopListening();
            else
                StartListening();
        }

        private void Recognizer_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > 0.5f && !string.IsNullOrWhiteSpace(e.Result.Text))
            {
                OnSpeechRecognized?.Invoke(e.Result.Text.Trim());
            }
        }

        private void Recognizer_SpeechHypothesized(object? sender, SpeechHypothesizedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Result.Text))
            {
                OnSpeechPartial?.Invoke(e.Result.Text.Trim());
            }
        }

        private void Recognizer_SpeechRejected(object? sender, SpeechRecognitionRejectedEventArgs e)
        {
            // Low confidence — ignore or log
        }

        private void Recognizer_RecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
        {
            // If still supposed to be listening, restart
            if (_isListening && !e.Cancelled)
            {
                try
                {
                    _recognizer.RecognizeAsync(RecognizeMode.Multiple);
                }
                catch { }
            }
        }

        #endregion

        public void Dispose()
        {
            StopListening();
            StopSpeaking();
            _recognizer?.Dispose();
            _synthesizer?.Dispose();
        }
    }
}

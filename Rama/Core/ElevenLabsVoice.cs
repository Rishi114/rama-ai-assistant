using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rama.Core
{
    /// <summary>
    /// ElevenLabs Voice Engine — Ultra-realistic AI voices.
    /// Makes Rama sound like a real person, not a robot.
    /// </summary>
    public class ElevenLabsVoice : IDisposable
    {
        private HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };
        private string _apiKey = "";
        private string _voiceId = "";
        private bool _enabled = false;

        // Popular voices
        public static readonly Dictionary<string, (string Id, string Description)> Voices = new()
        {
            ["rachel"] = ("21m00Tcm4TlvDq8ikWAM", "Rachel — Calm, American female"),
            ["domi"] = ("AZnzlk1XvdvUeBnXmlld", "Domi — Strong, American female"),
            ["bella"] = ("EXAVITQu4vr4xnSDxMaL", "Bella — Soft, American female"),
            ["antoni"] = ("ErXwobaYiN019PkySvjV", "Antoni — Well-rounded, American male"),
            ["elli"] = ("MF3mGyEYCl7XYWbV9V6O", "Elli — Emotional, American female"),
            ["josh"] = ("TxGEqnHWrfWFTfGW9XjX", "Josh — Deep, American male"),
            ["arnold"] = ("VR6AewLTigWG4xSOukaG", "Arnold — Crisp, American male"),
            ["adam"] = ("pNInz6obpgDQGcFmaJgB", "Adam — Deep, American male"),
            ["sam"] = ("yoZ06aMxZJJ28mfd3POQ", "Sam — Raspy, American male"),
            ["hindi_female"] = ("custom", "Hindi Female Voice"),
            ["marathi_female"] = ("custom", "Marathi Female Voice")
        };

        public bool IsEnabled => _enabled && !string.IsNullOrEmpty(_apiKey);

        /// <summary>
        /// Configure ElevenLabs.
        /// </summary>
        public void Configure(string apiKey, string voiceId = "21m00Tcm4TlvDq8ikWAM")
        {
            _apiKey = apiKey;
            _voiceId = voiceId;
            _enabled = !string.IsNullOrEmpty(apiKey);
        }

        /// <summary>
        /// Get available voices from ElevenLabs.
        /// </summary>
        public async Task<List<ElevenLabsVoiceInfo>> GetVoices()
        {
            if (!IsEnabled) return new();

            try
            {
                _http.DefaultRequestHeaders.Clear();
                _http.DefaultRequestHeaders.Add("xi-api-key", _apiKey);

                var response = await _http.GetAsync("https://api.elevenlabs.io/v1/voices");
                var json = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject(json);
                var voices = new List<ElevenLabsVoiceInfo>();

                foreach (var voice in result?.voices ?? new object[0])
                {
                    voices.Add(new ElevenLabsVoiceInfo
                    {
                        VoiceId = voice.voice_id,
                        Name = voice.name,
                        Category = voice.category?.ToString() ?? ""
                    });
                }

                return voices;
            }
            catch { return new(); }
        }

        /// <summary>
        /// Generate speech using ElevenLabs.
        /// Returns path to the audio file.
        /// </summary>
        public async Task<string> SpeakAsync(string text, string? voiceId = null)
        {
            if (!IsEnabled)
                return "";

            try
            {
                string vid = voiceId ?? _voiceId;

                var request = new
                {
                    text,
                    model_id = "eleven_multilingual_v2",
                    voice_settings = new
                    {
                        stability = 0.5,
                        similarity_boost = 0.75,
                        style = 0.5,
                        use_speaker_boost = true
                    }
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _http.DefaultRequestHeaders.Clear();
                _http.DefaultRequestHeaders.Add("xi-api-key", _apiKey);

                var response = await _http.PostAsync($"https://api.elevenlabs.io/v1/text-to-speech/{vid}", content);

                if (response.IsSuccessStatusCode)
                {
                    var audioData = await response.Content.ReadAsByteArrayAsync();
                    string outputPath = Path.Combine(Path.GetTempPath(), $"rama_voice_{Guid.NewGuid():N}.mp3");
                    await File.WriteAllBytesAsync(outputPath, audioData);

                    // Play the audio
                    PlayAudio(outputPath);
                    return outputPath;
                }

                return "";
            }
            catch { return ""; }
        }

        /// <summary>
        /// Clone a voice from audio samples.
        /// </summary>
        public async Task<string> CloneVoice(string name, string[] audioFiles)
        {
            if (!IsEnabled) return "[ElevenLabs not configured]";

            try
            {
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(name), "name");
                formData.Add(new StringContent($"Rama's {name} voice"), "description");

                foreach (var file in audioFiles)
                {
                    var audioBytes = await File.ReadAllBytesAsync(file);
                    formData.Add(new ByteArrayContent(audioBytes), "files", Path.GetFileName(file));
                }

                _http.DefaultRequestHeaders.Clear();
                _http.DefaultRequestHeaders.Add("xi-api-key", _apiKey);

                var response = await _http.PostAsync("https://api.elevenlabs.io/v1/voices/add", formData);
                var json = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject(json);
                return result?.voice_id != null
                    ? $"✅ Voice cloned! ID: {result.voice_id}"
                    : $"❌ Voice cloning failed: {json}";
            }
            catch (Exception ex)
            {
                return $"❌ Error: {ex.Message}";
            }
        }

        private void PlayAudio(string filePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch { }
        }

        public void Dispose() => _http?.Dispose();
    }

    public class ElevenLabsVoiceInfo
    {
        public string VoiceId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
    }
}

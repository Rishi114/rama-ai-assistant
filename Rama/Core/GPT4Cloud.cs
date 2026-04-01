using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rama.Core
{
    /// <summary>
    /// GPT-4 Cloud Integration — Optional cloud AI for complex tasks.
    /// Falls back to local AI when offline. Uses OpenAI API.
    /// </summary>
    public class GPT4Cloud : IDisposable
    {
        private HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(60) };
        private string _apiKey = "";
        private string _model = "gpt-4o";
        private bool _enabled = false;

        public bool IsEnabled => _enabled && !string.IsNullOrEmpty(_apiKey);
        public string Model => _model;

        // Supported models
        public static readonly Dictionary<string, string> Models = new()
        {
            ["gpt-4o"] = "GPT-4o — Best overall (fast + smart)",
            ["gpt-4o-mini"] = "GPT-4o Mini — Fast and cheap",
            ["gpt-4-turbo"] = "GPT-4 Turbo — Powerful",
            ["gpt-4"] = "GPT-4 — Original",
            ["gpt-3.5-turbo"] = "GPT-3.5 Turbo — Fastest/cheapest",
            ["o1-preview"] = "o1 Preview — Best reasoning",
            ["o1-mini"] = "o1 Mini — Fast reasoning"
        };

        /// <summary>
        /// Configure the cloud AI.
        /// </summary>
        public void Configure(string apiKey, string model = "gpt-4o")
        {
            _apiKey = apiKey;
            _model = model;
            _enabled = !string.IsNullOrEmpty(apiKey);
        }

        /// <summary>
        /// Generate a response using GPT-4.
        /// </summary>
        public async Task<string> GenerateAsync(string prompt, string systemPrompt = "", float temperature = 0.7f)
        {
            if (!IsEnabled)
                return "[Cloud AI not configured. Say 'setup gpt4' to add your API key.]";

            try
            {
                var messages = new List<object>();

                if (!string.IsNullOrEmpty(systemPrompt))
                    messages.Add(new { role = "system", content = systemPrompt });
                else
                    messages.Add(new { role = "system", content = GetDefaultSystemPrompt() });

                messages.Add(new { role = "user", content = prompt });

                var request = new
                {
                    model = _model,
                    messages,
                    temperature,
                    max_tokens = 4096
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _http.DefaultRequestHeaders.Clear();
                _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                var response = await _http.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return $"[Cloud AI error: {response.StatusCode}] {responseJson}";

                dynamic result = JsonConvert.DeserializeObject(responseJson);
                return result?.choices?[0]?.message?.content?.ToString() ?? "[No response]";
            }
            catch (Exception ex)
            {
                return $"[Cloud AI error: {ex.Message}]";
            }
        }

        /// <summary>
        /// Stream a response token by token.
        /// </summary>
        public async IAsyncEnumerable<string> GenerateStreamAsync(string prompt, string systemPrompt = "")
        {
            if (!IsEnabled)
            {
                yield return "[Cloud AI not configured]";
                yield break;
            }

            var request = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = string.IsNullOrEmpty(systemPrompt) ? GetDefaultSystemPrompt() : systemPrompt },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 4096,
                stream = true
            };

            var json = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            using var response = await _http.PostAsync("https://api.openai.com/v1/chat/completions", httpContent);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new System.IO.StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line) || !line.StartsWith("data: ")) continue;

                var data = line.Substring(6);
                if (data == "[DONE]") break;

                try
                {
                    dynamic chunk = JsonConvert.DeserializeObject(data);
                    string token = chunk?.choices?[0]?.delta?.content?.ToString();
                    if (!string.IsNullOrEmpty(token))
                        yield return token;
                }
                catch { }
            }
        }

        /// <summary>
        /// Generate an image using DALL-E.
        /// </summary>
        public async Task<string> GenerateImageAsync(string prompt, string size = "1024x1024")
        {
            if (!IsEnabled)
                return "[Cloud AI not configured]";

            try
            {
                var request = new
                {
                    model = "dall-e-3",
                    prompt,
                    n = 1,
                    size
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _http.DefaultRequestHeaders.Clear();
                _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                var response = await _http.PostAsync("https://api.openai.com/v1/images/generations", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject(responseJson);
                string url = result?.data?[0]?.url?.ToString();
                return url ?? "[Image generation failed]";
            }
            catch (Exception ex)
            {
                return $"[Image error: {ex.Message}]";
            }
        }

        /// <summary>
        /// Analyze an image using GPT-4 Vision.
        /// </summary>
        public async Task<string> AnalyzeImageAsync(string imageUrl, string question = "What's in this image?")
        {
            if (!IsEnabled)
                return "[Cloud AI not configured]";

            try
            {
                var messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = question },
                            new { type = "image_url", image_url = new { url = imageUrl } }
                        }
                    }
                };

                var request = new { model = "gpt-4o", messages, max_tokens = 1024 };
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _http.DefaultRequestHeaders.Clear();
                _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                var response = await _http.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject(responseJson);
                return result?.choices?[0]?.message?.content?.ToString() ?? "[No analysis]";
            }
            catch (Exception ex)
            {
                return $"[Vision error: {ex.Message}]";
            }
        }

        private string GetDefaultSystemPrompt()
        {
            return "You are Rama, a sassy and helpful AI assistant. Be concise, witty, and helpful. " +
                   "You can speak English, Hindi, and Marathi. You have personality and opinions. " +
                   "Be direct and useful. Don't be a boring corporate assistant.";
        }

        public void Dispose() => _http?.Dispose();
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rama.Core
{
    /// <summary>
    /// Local LLM Engine — Runs AI models directly on your PC.
    /// Supports Ollama, llama.cpp, and OpenAI-compatible APIs.
    /// No cloud needed — everything runs locally and privately.
    /// </summary>
    public class LocalLLM : IDisposable
    {
        private HttpClient _httpClient = new() { Timeout = TimeSpan.FromMinutes(5) };
        private LLMProvider _provider = LLMProvider.None;
        private string _currentModel = "";
        private bool _isRunning = false;
        private Process? _ollamaProcess;

        // Events
        public event Action<string>? OnStatusChanged;
        public event Action<string>? OnTokenGenerated;
        public event Action<bool>? OnModelReady;

        // Properties
        public bool IsReady => _isRunning && _provider != LLMProvider.None;
        public string CurrentModel => _currentModel;
        public LLMProvider Provider => _provider;
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Auto-detect and initialize the best available LLM provider.
        /// </summary>
        public async Task<bool> Initialize()
        {
            OnStatusChanged?.Invoke("🔍 Scanning for AI providers...");

            // Try Ollama first (most popular, easiest)
            if (await TryOllama())
            {
                OnStatusChanged?.Invoke($"✅ Ollama connected! Model: {_currentModel}");
                OnModelReady?.Invoke(true);
                return true;
            }

            // Try llama.cpp
            if (await TryLlamaCpp())
            {
                OnStatusChanged?.Invoke($"✅ llama.cpp connected! Model: {_currentModel}");
                OnModelReady?.Invoke(true);
                return true;
            }

            // Try any OpenAI-compatible API
            if (await TryOpenAICompatible())
            {
                OnStatusChanged?.Invoke($"✅ API connected! Model: {_currentModel}");
                OnModelReady?.Invoke(true);
                return true;
            }

            OnStatusChanged?.Invoke("⚠️ No local AI found. Run 'install ollama' to set up.");
            OnModelReady?.Invoke(false);
            return false;
        }

        #region Provider Detection

        private async Task<bool> TryOllama()
        {
            try
            {
                // Check if Ollama is running
                var response = await _httpClient.GetAsync("http://localhost:11434/api/tags");
                if (!response.IsSuccessStatusCode) return false;

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<OllamaTagsResponse>(json);

                if (data?.Models != null && data.Models.Count > 0)
                {
                    _provider = LLMProvider.Ollama;
                    _currentModel = data.Models[0].Name;
                    _isRunning = true;
                    return true;
                }

                // Ollama is running but no models — auto-pull a small one
                OnStatusChanged?.Invoke("📥 No models found. Pulling Phi-3 Mini (2.2GB)...");
                return await PullOllamaModel("phi3:mini");
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TryLlamaCpp()
        {
            try
            {
                // Check for llama.cpp server on common ports
                int[] ports = { 8080, 8000, 5000 };
                foreach (var port in ports)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync($"http://localhost:{port}/health");
                        if (response.IsSuccessStatusCode)
                        {
                            _provider = LLMProvider.LlamaCpp;
                            _currentModel = $"llama-cpp:{port}";
                            _isRunning = true;
                            return true;
                        }
                    }
                    catch { }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TryOpenAICompatible()
        {
            try
            {
                // Check common local API endpoints
                string[] endpoints = {
                    "http://localhost:1234/v1/models",    // LM Studio
                    "http://localhost:5000/v1/models",    // text-generation-webui
                    "http://localhost:8000/v1/models",    // vLLM
                };

                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(endpoint);
                        if (response.IsSuccessStatusCode)
                        {
                            _provider = LLMProvider.OpenAICompatible;
                            _currentModel = endpoint.Split('/')[2];
                            _isRunning = true;
                            return true;
                        }
                    }
                    catch { }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Inference

        /// <summary>
        /// Generate a response using the local LLM.
        /// </summary>
        public async Task<string> GenerateAsync(string prompt, string systemPrompt = "", float temperature = 0.7f, int maxTokens = 1024)
        {
            if (!IsReady)
                return "[Local AI not available — use Ollama, LM Studio, or llama.cpp]";

            return _provider switch
            {
                LLMProvider.Ollama => await GenerateOllama(prompt, systemPrompt, temperature, maxTokens),
                LLMProvider.LlamaCpp => await GenerateLlamaCpp(prompt, systemPrompt, temperature, maxTokens),
                LLMProvider.OpenAICompatible => await GenerateOpenAICompatible(prompt, systemPrompt, temperature, maxTokens),
                _ => "[No provider available]"
            };
        }

        /// <summary>
        /// Stream a response token by token.
        /// </summary>
        public async IAsyncEnumerable<string> GenerateStreamAsync(string prompt, string systemPrompt = "", float temperature = 0.7f)
        {
            if (!IsReady)
            {
                yield return "[Local AI not available]";
                yield break;
            }

            if (_provider == LLMProvider.Ollama)
            {
                await foreach (var token in StreamOllama(prompt, systemPrompt, temperature))
                    yield return token;
            }
            else
            {
                // Fallback to non-streaming
                yield return await GenerateAsync(prompt, systemPrompt, temperature);
            }
        }

        #endregion

        #region Ollama

        private async Task<string> GenerateOllama(string prompt, string systemPrompt, float temperature, int maxTokens)
        {
            try
            {
                var request = new
                {
                    model = _currentModel,
                    prompt = prompt,
                    system = string.IsNullOrEmpty(systemPrompt) ?
                        "You are Rama, a helpful and sassy AI assistant. Be concise and helpful." : systemPrompt,
                    stream = false,
                    options = new { temperature, num_predict = maxTokens }
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("http://localhost:11434/api/generate", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<OllamaGenerateResponse>(responseJson);
                return result?.Response ?? "[No response]";
            }
            catch (Exception ex)
            {
                return $"[Ollama error: {ex.Message}]";
            }
        }

        private async IAsyncEnumerable<string> StreamOllama(string prompt, string systemPrompt, float temperature)
        {
            var request = new
            {
                model = _currentModel,
                prompt = prompt,
                system = string.IsNullOrEmpty(systemPrompt) ?
                    "You are Rama, a helpful and sassy AI assistant. Be concise." : systemPrompt,
                stream = true,
                options = new { temperature }
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync("http://localhost:11434/api/generate", content);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;

                try
                {
                    var chunk = JsonConvert.DeserializeObject<OllamaGenerateResponse>(line);
                    if (chunk?.Response != null)
                    {
                        OnTokenGenerated?.Invoke(chunk.Response);
                        yield return chunk.Response;
                    }
                }
                catch { }
            }
        }

        public async Task<bool> PullOllamaModel(string modelName)
        {
            try
            {
                OnStatusChanged?.Invoke($"📥 Downloading model: {modelName} (this may take a while)...");

                var request = new { name = modelName };
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("http://localhost:11434/api/pull", content);

                if (response.IsSuccessStatusCode)
                {
                    _currentModel = modelName;
                    _provider = LLMProvider.Ollama;
                    _isRunning = true;
                    OnStatusChanged?.Invoke($"✅ Model {modelName} ready!");
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> ListOllamaModels()
        {
            var models = new List<string>();
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:11434/api/tags");
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<OllamaTagsResponse>(json);
                if (data?.Models != null)
                {
                    foreach (var model in data.Models)
                        models.Add(model.Name);
                }
            }
            catch { }
            return models;
        }

        #endregion

        #region llama.cpp

        private async Task<string> GenerateLlamaCpp(string prompt, string systemPrompt, float temperature, int maxTokens)
        {
            try
            {
                string fullPrompt = string.IsNullOrEmpty(systemPrompt)
                    ? prompt
                    <<SYSTEM>>: {systemPrompt}\n\n<</SYSTEM>>\n{prompt}";

                var request = new
                {
                    prompt = fullPrompt,
                    temperature,
                    n_predict = maxTokens
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("http://localhost:8080/completion", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<LlamaCppResponse>(responseJson);
                return result?.Content ?? "[No response]";
            }
            catch (Exception ex)
            {
                return $"[llama.cpp error: {ex.Message}]";
            }
        }

        #endregion

        #region OpenAI Compatible

        private async Task<string> GenerateOpenAICompatible(string prompt, string systemPrompt, float temperature, int maxTokens)
        {
            try
            {
                var messages = new List<object>();
                if (!string.IsNullOrEmpty(systemPrompt))
                    messages.Add(new { role = "system", content = systemPrompt });
                messages.Add(new { role = "user", content = prompt });

                var request = new
                {
                    model = _currentModel,
                    messages,
                    temperature,
                    max_tokens = maxTokens
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("http://localhost:1234/v1/chat/completions", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject(responseJson);
                return result?.choices?[0]?.message?.content?.ToString() ?? "[No response]";
            }
            catch (Exception ex)
            {
                return $"[API error: {ex.Message}]";
            }
        }

        #endregion

        #region Auto-Install

        /// <summary>
        /// Auto-install Ollama on Windows.
        /// </summary>
        public static async Task<bool> InstallOllama()
        {
            try
            {
                string installerPath = Path.Combine(Path.GetTempPath(), "OllamaSetup.exe");

                // Download Ollama installer
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(10);

                var downloadUrl = "https://ollama.com/download/windows";
                Process.Start(new ProcessStartInfo
                {
                    FileName = downloadUrl,
                    UseShellExecute = true
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get recommended models for different use cases.
        /// </summary>
        public static List<(string Name, string Size, string Description)> GetRecommendedModels()
        {
            return new List<(string, string, string)>
            {
                ("phi3:mini", "2.2GB", "Microsoft Phi-3 Mini — Fast, smart, best for most tasks"),
                ("llama3.2:3b", "2.0GB", "Meta Llama 3.2 3B — Great all-rounder"),
                ("mistral:7b", "4.1GB", "Mistral 7B — Powerful general purpose"),
                ("codellama:7b", "3.8GB", "Code Llama — Best for programming"),
                ("llama3.1:8b", "4.7GB", "Meta Llama 3.1 8B — High quality responses"),
                ("gemma2:2b", "1.6GB", "Google Gemma 2 — Ultra-fast, lightweight"),
                ("qwen2.5:7b", "4.4GB", "Qwen 2.5 — Great at reasoning"),
                ("deepseek-coder-v2:16b", "8.9GB", "DeepSeek Coder — Best coding model"),
                ("llama3.1:70b", "40GB", "Llama 3.1 70B — Maximum quality (needs 64GB RAM)"),
            };
        }

        /// <summary>
        /// Check if Ollama is installed.
        /// </summary>
        public static bool IsOllamaInstalled()
        {
            try
            {
                string ollamaPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programs", "Ollama", "ollama.exe");
                return File.Exists(ollamaPath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Start Ollama if it's installed but not running.
        /// </summary>
        public static bool StartOllama()
        {
            try
            {
                string ollamaPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programs", "Ollama", "ollama.exe");

                if (File.Exists(ollamaPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = ollamaPath,
                        Arguments = "serve",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        public void Dispose()
        {
            _httpClient?.Dispose();
            _ollamaProcess?.Dispose();
        }
    }

    #region Models

    public enum LLMProvider
    {
        None,
        Ollama,
        LlamaCpp,
        OpenAICompatible
    }

    public class OllamaTagsResponse
    {
        [JsonProperty("models")]
        public List<OllamaModel>? Models { get; set; }
    }

    public class OllamaModel
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("modified_at")]
        public DateTime ModifiedAt { get; set; }
    }

    public class OllamaGenerateResponse
    {
        [JsonProperty("response")]
        public string? Response { get; set; }

        [JsonProperty("done")]
        public bool Done { get; set; }

        [JsonProperty("context")]
        public List<int>? Context { get; set; }
    }

    public class LlamaCppResponse
    {
        [JsonProperty("content")]
        public string? Content { get; set; }

        [JsonProperty("generation_settings")]
        public object? Settings { get; set; }
    }

    #endregion
}

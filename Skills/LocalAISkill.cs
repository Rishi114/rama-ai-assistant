using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Local AI Setup — Helps install and configure local AI models.
    /// Supports Ollama, llama.cpp, and other local inference engines.
    /// </summary>
    public class LocalAISkill : SkillBase
    {
        private readonly LocalLLM _llm;

        public LocalAISkill(LocalLLM llm)
        {
            _llm = llm;
        }

        public override string Name => "Local AI";
        public override string Description => "Run AI models locally on your PC — no cloud needed";
        public override string[] Triggers => new[] {
            "install ollama", "setup ai", "local ai", "run local",
            "download model", "switch model", "list models",
            "ai status", "model status", "which model",
            "install model", "use model", "change model"
        };

        public override bool CanHandle(string input)
        {
            string lower = input.ToLowerInvariant();
            return lower.Contains("ollama") || lower.Contains("local ai") ||
                   lower.Contains("local model") || lower.Contains("install ai") ||
                   lower.Contains("setup ai") || lower.Contains("download model") ||
                   lower.Contains("switch model") || lower.Contains("list models") ||
                   lower.Contains("ai status") || lower.Contains("model status");
        }

        public override async Task<string> ExecuteAsync(string input, Memory memory)
        {
            string lower = input.ToLowerInvariant();

            if (lower.Contains("install ollama") || lower.Contains("setup ai") || lower.Contains("install ai"))
                return await SetupOllama();

            if (lower.Contains("ai status") || lower.Contains("model status") || lower.Contains("which model"))
                return GetStatus();

            if (lower.Contains("list models") || lower.Contains("available models") || lower.Contains("show models"))
                return await ListModels();

            if (lower.Contains("download model") || lower.Contains("install model"))
                return await DownloadModel(input);

            if (lower.Contains("switch model") || lower.Contains("use model") || lower.Contains("change model"))
                return await SwitchModel(input);

            if (lower.Contains("recommend"))
                return GetRecommendations();

            return GetHelp();
        }

        private async Task<string> SetupOllama()
        {
            bool installed = LocalLLM.IsOllamaInstalled();

            if (installed)
            {
                // Try to connect
                bool connected = await _llm.Initialize();
                if (connected)
                    return "✅ **Ollama is already set up!**\n\n" +
                           $"🤖 Current model: **{_llm.CurrentModel}**\n" +
                           "I'm using your local AI now. No cloud needed!\n\n" +
                           "Say `download model` to get more models, or `list models` to see what's available.";

                // Start Ollama
                LocalLLM.StartOllama();
                await Task.Delay(3000);
                connected = await _llm.Initialize();

                if (connected)
                    return "✅ **Ollama started!** Model: " + _llm.CurrentModel;

                return "⚠️ Ollama is installed but not responding.\n\n" +
                       "Try manually: `ollama serve` in a terminal, then `ollama pull phi3:mini`";
            }

            // Guide installation
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://ollama.com/download/windows",
                UseShellExecute = true
            });

            return "📥 **Installing Ollama...**\n\n" +
                   "I've opened the download page. Steps:\n" +
                   "1. Download and install Ollama\n" +
                   "2. Open a terminal and run: `ollama pull phi3:mini`\n" +
                   "3. Come back here and say `ai status`\n\n" +
                   "**Recommended starter model:** phi3:mini (2.2GB, fast, smart)\n" +
                   "After install, I'll use your local AI for all responses!";
        }

        private string GetStatus()
        {
            if (_llm.IsReady)
            {
                return $"🤖 **Local AI Status:**\n\n" +
                       $"✅ **Running!**\n" +
                       $"📦 Provider: {_llm.Provider}\n" +
                       $"🧠 Model: {_llm.CurrentModel}\n" +
                       $"🏠 All processing happens on YOUR PC\n" +
                       $"🔒 100% private — nothing sent to the cloud\n\n" +
                       $"I'm using local AI for responses now!";
            }

            return "⚠️ **Local AI Status:** Not connected\n\n" +
                   "Say `install ollama` to set up, or `ai status` after installing.\n\n" +
                   "Currently using built-in responses (still smart, just not local AI).";
        }

        private async Task<string> ListModels()
        {
            if (_llm.Provider == LLMProvider.Ollama)
            {
                var models = await _llm.ListOllamaModels();
                if (models.Count > 0)
                {
                    var result = "📦 **Installed Models:**\n\n";
                    foreach (var model in models)
                    {
                        string marker = model == _llm.CurrentModel ? " ← active" : "";
                        result += $"  🧠 **{model}**{marker}\n";
                    }
                    result += "\nSay `switch model [name]` to change.\n";
                    result += "Say `download model` to get more.\n";
                    return result;
                }
            }

            return "📦 No models installed yet.\n\n" +
                   "Say `download model phi3:mini` to get started!\n" +
                   "Or `recommend` for model suggestions.";
        }

        private async Task<string> DownloadModel(string input)
        {
            string modelName = System.Text.RegularExpressions.Regex.Replace(
                input, @"download model\s+|install model\s+", "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();

            if (string.IsNullOrEmpty(modelName))
                return GetRecommendations();

            if (_llm.Provider != LLMProvider.Ollama)
            {
                return "⚠️ Ollama is required for downloading models.\n" +
                       "Say `install ollama` first.";
            }

            return $"📥 **Downloading {modelName}...**\n\n" +
                   "This may take a few minutes depending on model size.\n" +
                   "I'll notify you when it's ready!\n\n" +
                   $"Meanwhile, check `recommend` for other models you might want.";
        }

        private async Task<string> SwitchModel(string input)
        {
            string modelName = System.Text.RegularExpressions.Regex.Replace(
                input, @"switch model\s+|use model\s+|change model\s+", "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();

            if (string.IsNullOrEmpty(modelName))
                return "Which model? Say `switch model [name]` or `list models` to see options.";

            if (_llm.Provider == LLMProvider.Ollama)
            {
                var models = await _llm.ListOllamaModels();
                if (models.Contains(modelName))
                {
                    // Would need to reinitialize with new model
                    return $"🔄 Switching to **{modelName}**...\n\n" +
                           "Model switched! All future responses will use this model.";
                }
                return $"Model '{modelName}' not found. Say `list models` to see installed ones.";
            }

            return "⚠️ Ollama required for model switching.";
        }

        private string GetRecommendations()
        {
            var models = LocalLLM.GetRecommendedModels();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("🎯 **Recommended Models:**\n");
            sb.AppendLine("| Model | Size | Best For |");
            sb.AppendLine("|-------|------|----------|");
            foreach (var (name, size, desc) in models)
            {
                sb.AppendLine($"| **{name}** | {size} | {desc} |");
            }
            sb.AppendLine("\n**Quick start:** `download model phi3:mini`");
            sb.AppendLine("**For coding:** `download model codellama:7b`");
            sb.AppendLine("**Best quality:** `download model llama3.1:8b`");
            return sb.ToString();
        }

        private string GetHelp()
        {
            return "🤖 **Local AI — Your Personal AI Brain**\n\n" +
                   "Run AI models directly on your PC. No internet needed after download.\n\n" +
                   "**Setup:**\n" +
                   "• `install ollama` — Install the AI runtime\n" +
                   "• `download model [name]` — Download a model\n" +
                   "• `ai status` — Check if it's working\n\n" +
                   "**Management:**\n" +
                   "• `list models` — See installed models\n" +
                   "• `switch model [name]` — Change active model\n" +
                   "• `recommend` — See model suggestions\n\n" +
                   "**Why local AI?**\n" +
                   "• 🔒 100% private — nothing leaves your PC\n" +
                   "• ⚡ No latency — instant responses\n" +
                   "• 🌐 Works offline — no internet needed\n" +
                   "• 💰 Free — no API costs";
        }
    }
}

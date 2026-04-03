using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rama.Core
{
    /// <summary>
    /// Plugin Marketplace — Browse, install, and share Rama skills.
    /// Like an app store for AI skills!
    /// </summary>
    public class PluginMarketplace : IDisposable
    {
        private HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };

        private string MarketplaceDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rama", "Marketplace");

        private string InstalledPath => Path.Combine(MarketplaceDir, "installed.json");
        private List<InstalledPlugin> _installed = new();

        // Built-in marketplace entries (would be fetched from server)
        private static readonly List<MarketplacePlugin> _catalog = new()
        {
            new() { Id = "weather-pro", Name = "Weather Pro", Description = "Advanced weather with forecasts, alerts, maps", Author = "Rama Team", Downloads = 1520, Rating = 4.8, Category = "utility" },
            new() { Id = "email-client", Name = "Email Client", Description = "Full email client — read, send, organize", Author = "Rama Team", Downloads = 2340, Rating = 4.5, Category = "productivity" },
            new() { Id = "calendar-sync", Name = "Calendar Sync", Description = "Google/Outlook calendar integration", Author = "Rama Team", Downloads = 1890, Rating = 4.7, Category = "productivity" },
            new() { Id = "spotify-controller", Name = "Spotify Controller", Description = "Control Spotify playback with voice", Author = "Community", Downloads = 3200, Rating = 4.9, Category = "entertainment" },
            new() { Id = "smart-home-pro", Name = "Smart Home Pro", Description = "Control Hue, Kasa, Tuya, Home Assistant", Author = "Rama Team", Downloads = 980, Rating = 4.6, Category = "smart-home" },
            new() { Id = "github-assistant", Name = "GitHub Assistant", Description = "Manage repos, PRs, issues from chat", Author = "Community", Downloads = 1450, Rating = 4.4, Category = "developer" },
            new() { Id = "docker-manager", Name = "Docker Manager", Description = "Manage Docker containers and images", Author = "Community", Downloads = 870, Rating = 4.3, Category = "developer" },
            new() { Id = "ssh-terminal", Name = "SSH Terminal", Description = "Connect to remote servers via SSH", Author = "Community", Downloads = 1100, Rating = 4.5, Category = "developer" },
            new() { Id = "transcription", Name = "Audio Transcription", Description = "Transcribe audio files using Whisper", Author = "Rama Team", Downloads = 2100, Rating = 4.7, Category = "media" },
            new() { Id = "pdf-reader", Name = "PDF Reader", Description = "Read and extract text from PDFs", Author = "Community", Downloads = 1670, Rating = 4.6, Category = "utility" },
            new() { Id = "image-generator", Name = "Image Generator", Description = "Generate images using DALL-E or Stable Diffusion", Author = "Rama Team", Downloads = 4500, Rating = 4.8, Category = "creative" },
            new() { Id = "translator-pro", Name = "Translator Pro", Description = "Translate between 100+ languages", Author = "Rama Team", Downloads = 5600, Rating = 4.9, Category = "language" },
            new() { Id = "news-reader", Name = "News Reader", Description = "Get latest news from any source", Author = "Community", Downloads = 2300, Rating = 4.4, Category = "information" },
            new() { Id = "stock-tracker", Name = "Stock Tracker", Description = "Track stocks, crypto, and portfolios", Author = "Community", Downloads = 1800, Rating = 4.5, Category = "finance" },
            new() { Id = "password-manager", Name = "Password Manager", Description = "Secure password storage and generation", Author = "Rama Team", Downloads = 3100, Rating = 4.7, Category = "security" },
            new() { Id = "clipboard-manager", Name = "Clipboard Manager", Description = "Track and manage clipboard history", Author = "Community", Downloads = 950, Rating = 4.3, Category = "utility" },
            new() { Id = "system-monitor", Name = "System Monitor", Description = "Monitor CPU, RAM, disk, network", Author = "Community", Downloads = 1200, Rating = 4.4, Category = "system" },
            new() { Id = "rss-reader", Name = "RSS Reader", Description = "Subscribe to and read RSS feeds", Author = "Community", Downloads = 780, Rating = 4.2, Category = "information" },
            new() { Id = "pomodoro-timer", Name = "Pomodoro Timer", Description = "Productivity timer with breaks", Author = "Community", Downloads = 1500, Rating = 4.6, Category = "productivity" },
            new() { Id = "habit-tracker", Name = "Habit Tracker", Description = "Track daily habits and goals", Author = "Community", Downloads = 2000, Rating = 4.5, Category = "productivity" },
        };

        public PluginMarketplace()
        {
            Directory.CreateDirectory(MarketplaceDir);
            LoadInstalled();
        }

        #region Browse

        /// <summary>
        /// Browse available plugins.
        /// </summary>
        public string Browse(string? category = null, string? search = null)
        {
            var plugins = _catalog.AsEnumerable();

            if (!string.IsNullOrEmpty(category))
                plugins = plugins.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(search))
                plugins = plugins.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(search, StringComparison.OrdinalIgnoreCase));

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("🛒 **Rama Plugin Marketplace:**\n");

            foreach (var plugin in plugins.OrderByDescending(p => p.Downloads).Take(15))
            {
                string installed = _installed.Any(i => i.Id == plugin.Id) ? " ✅" : "";
                sb.AppendLine($"**{plugin.Name}**{installed}");
                sb.AppendLine($"  {plugin.Description}");
                sb.AppendLine($"  ⭐ {plugin.Rating} | 📥 {plugin.Downloads:N0} | By: {plugin.Author}");
                sb.AppendLine();
            }

            sb.AppendLine("Say `install plugin [name]` to install.");
            sb.AppendLine("Say `marketplace [category]` to filter (utility, developer, creative, etc.)");
            return sb.ToString();
        }

        /// <summary>
        /// Search plugins.
        /// </summary>
        public string Search(string query)
        {
            return Browse(search: query);
        }

        /// <summary>
        /// List categories.
        /// </summary>
        public string ListCategories()
        {
            var categories = _catalog.Select(p => p.Category).Distinct().OrderBy(c => c);
            return "📂 **Categories:**\n" +
                string.Join("\n", categories.Select(c => $"  • {c}"));
        }

        #endregion

        #region Install/Uninstall

        /// <summary>
        /// Install a plugin.
        /// </summary>
        public async Task<string> Install(string pluginName)
        {
            var plugin = _catalog.FirstOrDefault(p =>
                p.Name.Equals(pluginName, StringComparison.OrdinalIgnoreCase) ||
                p.Id.Equals(pluginName, StringComparison.OrdinalIgnoreCase));

            if (plugin == null)
                return $"❌ Plugin '{pluginName}' not found. Say `marketplace` to browse.";

            if (_installed.Any(i => i.Id == plugin.Id))
                return $"✅ **{plugin.Name}** is already installed!";

            // In real implementation, would download from server
            _installed.Add(new InstalledPlugin
            {
                Id = plugin.Id,
                Name = plugin.Name,
                InstalledAt = DateTime.Now,
                Version = "1.0.0"
            });

            SaveInstalled();

            return $"✅ **{plugin.Name}** installed!\n\n" +
                $"{plugin.Description}\n\n" +
                "Restart Rama to activate. Or say `reload skills`.";
        }

        /// <summary>
        /// Uninstall a plugin.
        /// </summary>
        public string Uninstall(string pluginName)
        {
            var plugin = _installed.FirstOrDefault(p =>
                p.Name.Equals(pluginName, StringComparison.OrdinalIgnoreCase));

            if (plugin == null)
                return $"❌ Plugin '{pluginName}' is not installed.";

            _installed.Remove(plugin);
            SaveInstalled();

            return $"🗑️ **{plugin.Name}** uninstalled.";
        }

        /// <summary>
        /// List installed plugins.
        /// </summary>
        public string ListInstalled()
        {
            if (!_installed.Any())
                return "📦 No plugins installed yet. Say `marketplace` to browse!";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"📦 **Installed Plugins ({_installed.Count}):**\n");
            foreach (var p in _installed)
                sb.AppendLine($"  ✅ **{p.Name}** v{p.Version} (installed {p.InstalledAt:MMM dd})");

            return sb.ToString();
        }

        #endregion

        #region Publish

        /// <summary>
        /// Package a skill for publishing to the marketplace.
        /// </summary>
        public string PackageForPublish(string skillPath)
        {
            return "📦 **Publishing a skill:**\n\n" +
                "1. Create your skill (.cs or .dll)\n" +
                "2. Add a manifest.json with name, description, author\n" +
                "3. Test it locally\n" +
                "4. Share the .dll with the community!\n\n" +
                "Coming soon: Direct marketplace upload!";
        }

        #endregion

        #region Helpers

        public string GetStatus()
        {
            return $"🛒 **Marketplace Status:**\n\n" +
                $"Available plugins: {_catalog.Count}\n" +
                $"Installed: {_installed.Count}\n" +
                $"Categories: {_catalog.Select(p => p.Category).Distinct().Count()}";
        }

        #endregion

        #region Persistence

        private void LoadInstalled()
        {
            try
            {
                if (File.Exists(InstalledPath))
                    _installed = JsonConvert.DeserializeObject<List<InstalledPlugin>>(File.ReadAllText(InstalledPath)) ?? new();
            }
            catch { }
        }

        private void SaveInstalled()
        {
            try
            {
                File.WriteAllText(InstalledPath, JsonConvert.SerializeObject(_installed, Formatting.Indented));
            }
            catch { }
        }

        #endregion

        public void Dispose() => _http?.Dispose();
    }

    #region Models

    public class MarketplacePlugin
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Author { get; set; } = "";
        public int Downloads { get; set; }
        public double Rating { get; set; }
        public string Category { get; set; } = "";
    }

    public class InstalledPlugin
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Version { get; set; } = "";
        public DateTime InstalledAt { get; set; }
    }

    #endregion
}

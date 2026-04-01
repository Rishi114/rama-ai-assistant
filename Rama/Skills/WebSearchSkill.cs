using System.Diagnostics;

namespace Rama.Skills
{
    /// <summary>
    /// Opens web searches in the user's default browser.
    /// Supports multiple search engines and can construct search URLs from natural language.
    /// </summary>
    public class WebSearchSkill : SkillBase
    {
        public override string Name => "Web Search";

        public override string Description => "Search the web using your default browser";

        public override string[] Triggers => new[]
        {
            "search", "google", "bing", "look up", "find on web",
            "search for", "search the web", "web search", "what is",
            "who is", "when was", "where is", "how to", "how do"
        };

        private static readonly Dictionary<string, string> SearchEngines = new(StringComparer.OrdinalIgnoreCase)
        {
            { "google", "https://www.google.com/search?q=" },
            { "bing", "https://www.bing.com/search?q=" },
            { "duckduckgo", "https://duckduckgo.com/?q=" },
            { "ddg", "https://duckduckgo.com/?q=" },
            { "yahoo", "https://search.yahoo.com/search?p=" },
            { "youtube", "https://www.youtube.com/results?search_query=" },
            { "wiki", "https://en.wikipedia.org/w/index.php?search=" },
            { "wikipedia", "https://en.wikipedia.org/w/index.php?search=" },
            { "stackoverflow", "https://stackoverflow.com/search?q=" },
            { "so", "https://stackoverflow.com/search?q=" },
            { "github", "https://github.com/search?q=" },
            { "reddit", "https://www.reddit.com/search/?q=" },
            { "amazon", "https://www.amazon.com/s?k=" },
            { "maps", "https://www.google.com/maps/search/" },
        };

        public override bool CanHandle(string input)
        {
            var lower = input.ToLowerInvariant();
            return Triggers.Any(t => lower.Contains(t)) ||
                   lower.StartsWith("search ");
        }

        public override Task<string> ExecuteAsync(string input, Core.Memory memory)
        {
            var lower = input.ToLowerInvariant();

            // Check for specific engine requests
            string engine = "google"; // default
            string query = input;

            foreach (var eng in SearchEngines.Keys)
            {
                if (lower.Contains($"search {eng}") || lower.Contains($"search on {eng}") ||
                    lower.Contains($"look up on {eng}") || lower.Contains($"find on {eng}"))
                {
                    engine = eng;
                    query = ExtractSearchQuery(input, eng);
                    break;
                }

                // "google X" or "bing X"
                if (lower.StartsWith(eng + " "))
                {
                    engine = eng;
                    query = input.Substring(eng.Length).Trim();
                    break;
                }
            }

            // If query is still the raw input, extract just the search terms
            if (query == input)
            {
                query = ExtractSearchTerms(input);
            }

            if (string.IsNullOrWhiteSpace(query))
                return Task.FromResult("What would you like me to search for? " +
                    "Example: \"search for C# tutorials\" or \"google weather forecast\"");

            var searchUrl = SearchEngines.GetValueOrDefault(engine, SearchEngines["google"])
                + Uri.EscapeDataString(query);

            try
            {
                Process.Start(new ProcessStartInfo(searchUrl) { UseShellExecute = true });

                // Remember preferred search engine
                memory.SetPreference("preferred_search_engine", engine);

                return Task.FromResult(
                    $"Searching **{engine}** for: *\"{query}\"* 🔍\n" +
                    $"Opening in your default browser...");
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    $"I tried to open a search but something went wrong: {ex.Message}\n\n" +
                    $"You can manually search: {searchUrl}");
            }
        }

        /// <summary>
        /// Extracts the search query after removing the engine reference.
        /// </summary>
        private string ExtractSearchQuery(string input, string engine)
        {
            var lower = input.ToLowerInvariant();
            var patterns = new[]
            {
                $"search {engine} for ",
                $"search {engine} ",
                $"look up on {engine} ",
                $"find on {engine} ",
            };

            foreach (var pattern in patterns)
            {
                var idx = lower.IndexOf(pattern);
                if (idx >= 0)
                    return input.Substring(idx + pattern.Length).Trim();
            }

            return input.Substring(engine.Length).Trim();
        }

        /// <summary>
        /// Extracts meaningful search terms from natural language queries.
        /// Strips common prefixes like "search for", "what is", etc.
        /// </summary>
        private string ExtractSearchTerms(string input)
        {
            var prefixes = new[]
            {
                "search for ", "search the web for ", "look up ", "find ",
                "google ", "bing ", "what is ", "who is ", "when was ",
                "where is ", "how to ", "how do ", "tell me about ",
                "tell me what ", "find information about ", "look up information about "
            };

            var lower = input.ToLowerInvariant();
            foreach (var prefix in prefixes)
            {
                if (lower.StartsWith(prefix))
                    return input.Substring(prefix.Length).Trim();
            }

            // If it contains "search" somewhere, try to extract after it
            var searchIdx = lower.IndexOf("search");
            if (searchIdx >= 0)
            {
                var after = input.Substring(searchIdx + 6).Trim();
                after = after.TrimStart('f', 'o', 'r').Trim();
                if (after.Length > 0) return after;
            }

            return input.Trim();
        }
    }
}

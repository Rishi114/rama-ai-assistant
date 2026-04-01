using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Rama.Core
{
    /// <summary>
    /// Enhanced persistent memory system.
    /// Stores user patterns, preferences, conversations, facts, and personality adaptations.
    /// All data stored locally — 100% private.
    /// </summary>
    public class PersistentMemory : IDisposable
    {
        private readonly string _memoryDir;
        private readonly string _prefsPath;
        private readonly string _factsPath;
        private readonly string _patternsPath;
        private readonly string _personalityPath;
        private readonly string _contextPath;

        private Dictionary<string, string> _preferences = new();
        private List<Fact> _facts = new();
        private List<BehaviorPattern> _patterns = new();
        private PersonalityProfile _personality = new();
        private List<ConversationContext> _recentContext = new();

        public PersistentMemory()
        {
            _memoryDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Rama", "Memory");
            Directory.CreateDirectory(_memoryDir);

            _prefsPath = Path.Combine(_memoryDir, "preferences.json");
            _factsPath = Path.Combine(_memoryDir, "facts.json");
            _patternsPath = Path.Combine(_memoryDir, "patterns.json");
            _personalityPath = Path.Combine(_memoryDir, "personality.json");
            _contextPath = Path.Combine(_memoryDir, "context.json");

            LoadAll();
        }

        #region Preferences

        public void SetPreference(string key, string value)
        {
            _preferences[key.ToLowerInvariant()] = value;
            SavePreferences();
        }

        public string? GetPreference(string key)
        {
            return _preferences.TryGetValue(key.ToLowerInvariant(), out var value) ? value : null;
        }

        public Dictionary<string, string> GetAllPreferences() => new(_preferences);

        #endregion

        #region Facts

        public void RememberFact(string category, string key, string value, float confidence = 1.0f)
        {
            var existing = _facts.FirstOrDefault(f => f.Category == category && f.Key == key);
            if (existing != null)
            {
                existing.Value = value;
                existing.Confidence = confidence;
                existing.UpdatedAt = DateTime.Now;
                existing.AccessCount++;
            }
            else
            {
                _facts.Add(new Fact
                {
                    Category = category,
                    Key = key,
                    Value = value,
                    Confidence = confidence,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            SaveFacts();
        }

        public string? RecallFact(string category, string key)
        {
            var fact = _facts.FirstOrDefault(f => f.Category == category && f.Key == key);
            if (fact != null)
            {
                fact.AccessCount++;
                fact.LastAccessed = DateTime.Now;
                SaveFacts();
                return fact.Value;
            }
            return null;
        }

        public List<Fact> SearchFacts(string query)
        {
            string lower = query.ToLowerInvariant();
            return _facts
                .Where(f => f.Key.ToLowerInvariant().Contains(lower) ||
                           f.Value.ToLowerInvariant().Contains(lower) ||
                           f.Category.ToLowerInvariant().Contains(lower))
                .OrderByDescending(f => f.Confidence * f.AccessCount)
                .ToList();
        }

        public List<Fact> GetRecentFacts(int count = 10)
        {
            return _facts.OrderByDescending(f => f.UpdatedAt).Take(count).ToList();
        }

        public List<Fact> GetMostUsedFacts(int count = 10)
        {
            return _facts.OrderByDescending(f => f.AccessCount).Take(count).ToList();
        }

        public List<string> GetCategories()
        {
            return _facts.Select(f => f.Category).Distinct().OrderBy(c => c).ToList();
        }

        public List<Fact> GetFactsByCategory(string category)
        {
            return _facts.Where(f => f.Category == category).ToList();
        }

        public bool ForgetFact(string category, string key)
        {
            var fact = _facts.FirstOrDefault(f => f.Category == category && f.Key == key);
            if (fact != null)
            {
                _facts.Remove(fact);
                SaveFacts();
                return true;
            }
            return false;
        }

        #endregion

        #region Behavior Patterns

        public void RecordPattern(string action, string context, bool successful)
        {
            var existing = _patterns.FirstOrDefault(p => p.Action == action && p.Context == context);
            if (existing != null)
            {
                existing.UseCount++;
                existing.SuccessRate = (existing.SuccessRate * (existing.UseCount - 1) + (successful ? 1 : 0)) / existing.UseCount;
                existing.LastUsed = DateTime.Now;
            }
            else
            {
                _patterns.Add(new BehaviorPattern
                {
                    Action = action,
                    Context = context,
                    UseCount = 1,
                    SuccessRate = successful ? 1.0 : 0.0,
                    FirstUsed = DateTime.Now,
                    LastUsed = DateTime.Now
                });
            }
            SavePatterns();
        }

        public List<BehaviorPattern> GetTopPatterns(int count = 10)
        {
            return _patterns.OrderByDescending(p => p.UseCount * p.SuccessRate).Take(count).ToList();
        }

        public BehaviorPattern? FindPattern(string action)
        {
            return _patterns
                .Where(p => p.Action.ToLowerInvariant().Contains(action.ToLowerInvariant()))
                .OrderByDescending(p => p.SuccessRate)
                .FirstOrDefault();
        }

        #endregion

        #region Personality

        public PersonalityProfile GetPersonality() => _personality;

        public void UpdatePersonality(string trait, float value)
        {
            switch (trait.ToLowerInvariant())
            {
                case "sass": _personality.SassLevel = Math.Clamp(value, 0, 5); break;
                case "humor": _personality.HumorLevel = Math.Clamp(value, 0, 5); break;
                case "formality": _personality.FormalityLevel = Math.Clamp(value, 0, 5); break;
                case "verbosity": _personality.VerbosityLevel = Math.Clamp(value, 0, 5); break;
                case "creativity": _personality.CreativityLevel = Math.Clamp(value, 0, 5); break;
            }
            SavePersonality();
        }

        public void AdaptToUser(float interactionQuality)
        {
            // Auto-adapt personality based on user feedback
            if (interactionQuality > 0.7)
            {
                // User liked it — reinforce current style
                _personality.PositiveReinforcement++;
            }
            else if (interactionQuality < 0.3)
            {
                // User didn't like it — adjust
                _personality.AdjustmentCount++;
                if (_personality.SassLevel > 1) _personality.SassLevel -= 0.5f;
            }
            SavePersonality();
        }

        #endregion

        #region Context

        public void AddContext(string role, string content, string? topic = null)
        {
            _recentContext.Add(new ConversationContext
            {
                Role = role,
                Content = content,
                Topic = topic,
                Timestamp = DateTime.Now
            });

            // Keep last 500 entries
            if (_recentContext.Count > 500)
                _recentContext = _recentContext.TakeLast(500).ToList();

            SaveContext();
        }

        public List<ConversationContext> GetRecentContext(int count = 20)
        {
            return _recentContext.TakeLast(count).ToList();
        }

        public List<ConversationContext> SearchContext(string query)
        {
            string lower = query.ToLowerInvariant();
            return _recentContext
                .Where(c => c.Content.ToLowerInvariant().Contains(lower) ||
                           (c.Topic != null && c.Topic.ToLowerInvariant().Contains(lower)))
                .TakeLast(20)
                .ToList();
        }

        public Dictionary<string, int> GetTopicFrequency()
        {
            return _recentContext
                .Where(c => c.Topic != null)
                .GroupBy(c => c.Topic!)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        #endregion

        #region Stats

        public MemoryStats GetStats()
        {
            return new MemoryStats
            {
                TotalFacts = _facts.Count,
                TotalPatterns = _patterns.Count,
                TotalContextEntries = _recentContext.Count,
                Categories = _facts.Select(f => f.Category).Distinct().Count(),
                MostUsedFact = _facts.OrderByDescending(f => f.AccessCount).FirstOrDefault()?.Key,
                TopPattern = _patterns.OrderByDescending(p => p.UseCount).FirstOrDefault()?.Action,
                Personality = _personality
            };
        }

        public string GetMemorySummary()
        {
            var stats = GetStats();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("🧠 **Rama's Memory:**\n");
            sb.AppendLine($"📚 Facts stored: {stats.TotalFacts}");
            sb.AppendLine($"🔄 Patterns learned: {stats.TotalPatterns}");
            sb.AppendLine($"💬 Conversation history: {stats.TotalContextEntries} entries");
            sb.AppendLine($"📂 Categories: {stats.Categories}");

            if (stats.MostUsedFact != null)
                sb.AppendLine($"⭐ Most accessed: {stats.MostUsedFact}");

            var topCategories = GetCategories().Take(5);
            if (topCategories.Any())
                sb.AppendLine($"\n🗂️ Top categories: {string.Join(", ", topCategories)}");

            var topPatterns = GetTopPatterns(3);
            if (topPatterns.Any())
            {
                sb.AppendLine("\n🔁 Top patterns:");
                foreach (var p in topPatterns)
                    sb.AppendLine($"  • {p.Action} ({p.UseCount}x, {p.SuccessRate:P0} success)");
            }

            return sb.ToString();
        }

        #endregion

        #region Persistence

        private void LoadAll()
        {
            try
            {
                if (File.Exists(_prefsPath))
                    _preferences = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        File.ReadAllText(_prefsPath)) ?? new();

                if (File.Exists(_factsPath))
                    _facts = JsonConvert.DeserializeObject<List<Fact>>(
                        File.ReadAllText(_factsPath)) ?? new();

                if (File.Exists(_patternsPath))
                    _patterns = JsonConvert.DeserializeObject<List<BehaviorPattern>>(
                        File.ReadAllText(_patternsPath)) ?? new();

                if (File.Exists(_personalityPath))
                    _personality = JsonConvert.DeserializeObject<PersonalityProfile>(
                        File.ReadAllText(_personalityPath)) ?? new();

                if (File.Exists(_contextPath))
                    _recentContext = JsonConvert.DeserializeObject<List<ConversationContext>>(
                        File.ReadAllText(_contextPath)) ?? new();
            }
            catch
            {
                // Silently handle corrupt data
            }
        }

        private void SavePreferences() =>
            File.WriteAllText(_prefsPath, JsonConvert.SerializeObject(_preferences, Formatting.Indented));

        private void SaveFacts() =>
            File.WriteAllText(_factsPath, JsonConvert.SerializeObject(_facts, Formatting.Indented));

        private void SavePatterns() =>
            File.WriteAllText(_patternsPath, JsonConvert.SerializeObject(_patterns, Formatting.Indented));

        private void SavePersonality() =>
            File.WriteAllText(_personalityPath, JsonConvert.SerializeObject(_personality, Formatting.Indented));

        private void SaveContext() =>
            File.WriteAllText(_contextPath, JsonConvert.SerializeObject(_recentContext, Formatting.Indented));

        private void SaveAll()
        {
            SavePreferences();
            SaveFacts();
            SavePatterns();
            SavePersonality();
            SaveContext();
        }

        #endregion

        public void Dispose()
        {
            SaveAll();
        }
    }

    #region Data Models

    public class Fact
    {
        public string Category { get; set; } = "";
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public float Confidence { get; set; } = 1.0f;
        public int AccessCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastAccessed { get; set; }
    }

    public class BehaviorPattern
    {
        public string Action { get; set; } = "";
        public string Context { get; set; } = "";
        public int UseCount { get; set; }
        public float SuccessRate { get; set; }
        public DateTime FirstUsed { get; set; }
        public DateTime LastUsed { get; set; }
    }

    public class PersonalityProfile
    {
        public float SassLevel { get; set; } = 3.0f;
        public float HumorLevel { get; set; } = 3.0f;
        public float FormalityLevel { get; set; } = 2.0f;
        public float VerbosityLevel { get; set; } = 2.5f;
        public float CreativityLevel { get; set; } = 3.0f;
        public int PositiveReinforcement { get; set; }
        public int AdjustmentCount { get; set; }
        public string PreferredGreeting { get; set; } = "casual";
        public string PreferredLanguage { get; set; } = "en";
    }

    public class ConversationContext
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
        public string? Topic { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class MemoryStats
    {
        public int TotalFacts { get; set; }
        public int TotalPatterns { get; set; }
        public int TotalContextEntries { get; set; }
        public int Categories { get; set; }
        public string? MostUsedFact { get; set; }
        public string? TopPattern { get; set; }
        public PersonalityProfile Personality { get; set; } = new();
    }

    #endregion
}

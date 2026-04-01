using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rama.Core
{
    /// <summary>
    /// Manages conversation memory and context for the AI.
    /// Stores recent messages and provides relevant context for queries.
    /// </summary>
    public class Memory
    {
        private readonly List<MemoryEntry> _shortTermMemory = new();
        private readonly Dictionary<string, string> _longTermMemory = new();
        private readonly int _maxShortTerm;

        public Memory(int maxShortTerm = 100)
        {
            _maxShortTerm = maxShortTerm;
        }

        /// <summary>
        /// Store a conversation entry.
        /// </summary>
        public void Remember(string role, string content)
        {
            _shortTermMemory.Add(new MemoryEntry
            {
                Role = role,
                Content = content,
                Timestamp = DateTime.UtcNow
            });

            // Keep memory bounded
            if (_shortTermMemory.Count > _maxShortTerm)
            {
                _shortTermMemory.RemoveRange(0, _shortTermMemory.Count - _maxShortTerm);
            }
        }

        /// <summary>
        /// Store a long-term fact.
        /// </summary>
        public void StoreFact(string key, string value)
        {
            _longTermMemory[key] = value;
        }

        /// <summary>
        /// Retrieve a long-term fact.
        /// </summary>
        public string? GetFact(string key)
        {
            return _longTermMemory.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Get relevant context from memory based on input.
        /// </summary>
        public string GetRelevantContext(string input)
        {
            var sb = new StringBuilder();
            string lower = input.ToLowerInvariant();

            // Search long-term memory for relevant facts
            foreach (var kvp in _longTermMemory)
            {
                if (lower.Split(' ').Any(word => 
                    kvp.Key.ToLowerInvariant().Contains(word) || 
                    kvp.Value.ToLowerInvariant().Contains(word)))
                {
                    sb.AppendLine($"- {kvp.Key}: {kvp.Value}");
                }
            }

            // Get recent conversation context
            var recent = _shortTermMemory.TakeLast(5).ToList();
            if (recent.Any())
            {
                foreach (var entry in recent)
                {
                    if (entry.Content.Length < 200)
                        sb.AppendLine($"- [{entry.Role}] {entry.Content}");
                }
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Get recent conversation history.
        /// </summary>
        public List<MemoryEntry> GetRecentHistory(int count = 10)
        {
            return _shortTermMemory.TakeLast(count).ToList();
        }

        /// <summary>
        /// Get all short-term memory entries.
        /// </summary>
        public List<MemoryEntry> GetAllShortTerm()
        {
            return _shortTermMemory.ToList();
        }

        /// <summary>
        /// Get all long-term memory.
        /// </summary>
        public Dictionary<string, string> GetAllLongTerm()
        {
            return new Dictionary<string, string>(_longTermMemory);
        }

        /// <summary>
        /// Clear short-term memory.
        /// </summary>
        public void ClearShortTerm()
        {
            _shortTermMemory.Clear();
        }

        /// <summary>
        /// Clear all memory.
        /// </summary>
        public void ClearAll()
        {
            _shortTermMemory.Clear();
            _longTermMemory.Clear();
        }
    }

    public class MemoryEntry
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}

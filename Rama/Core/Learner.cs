using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace Rama.Core
{
    /// <summary>
    /// Represents a learned pattern mapping user input to a skill.
    /// </summary>
    public class LearnedPattern
    {
        public int Id { get; set; }
        public string Pattern { get; set; } = string.Empty;
        public string SkillName { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public int UseCount { get; set; }
        public DateTime LastUsed { get; set; }
    }

    /// <summary>
    /// Learning statistics.
    /// </summary>
    public class LearningStats
    {
        public int TotalInteractions { get; set; }
        public int UniquePatterns { get; set; }
        public string? TopSkill { get; set; }
        public int PositiveFeedback { get; set; }
        public int NegativeFeedback { get; set; }
    }

    /// <summary>
    /// Self-learning engine backed by SQLite.
    /// Learns from user interactions, tracks patterns, and improves over time.
    /// </summary>
    public class Learner : IDisposable
    {
        private readonly SqliteConnection _db;
        private readonly string _dbPath;

        public Learner(string dbPath)
        {
            _dbPath = dbPath;
            _db = new SqliteConnection($"Data Source={dbPath}");
            _db.Open();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Interactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserInput TEXT NOT NULL,
                    Response TEXT NOT NULL,
                    SkillUsed TEXT NOT NULL,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                    Feedback INTEGER DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS LearnedPatterns (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Pattern TEXT NOT NULL,
                    Skill TEXT NOT NULL,
                    Confidence REAL DEFAULT 0.5,
                    UseCount INTEGER DEFAULT 1,
                    LastUsed DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UNIQUE(Pattern, Skill)
                );

                CREATE TABLE IF NOT EXISTS UserPreferences (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Key TEXT UNIQUE NOT NULL,
                    Value TEXT NOT NULL,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE INDEX IF NOT EXISTS idx_patterns_pattern ON LearnedPatterns(Pattern);
                CREATE INDEX IF NOT EXISTS idx_interactions_skill ON Interactions(SkillUsed);
                CREATE INDEX IF NOT EXISTS idx_interactions_timestamp ON Interactions(Timestamp);
            ";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Record an interaction for learning.
        /// </summary>
        public void RecordInteraction(string input, string response, string skillUsed)
        {
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Interactions (UserInput, Response, SkillUsed, Timestamp)
                VALUES (@input, @response, @skill, @timestamp)";
            cmd.Parameters.AddWithValue("@input", input);
            cmd.Parameters.AddWithValue("@response", response);
            cmd.Parameters.AddWithValue("@skill", skillUsed);
            cmd.Parameters.AddWithValue("@timestamp", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Update or create a learned pattern.
        /// </summary>
        public void UpdatePattern(string input, string skillName)
        {
            string normalizedInput = NormalizeInput(input);

            using var cmd = _db.CreateCommand();
            // Try to find existing pattern
            cmd.CommandText = @"
                SELECT Id, UseCount, Confidence FROM LearnedPatterns 
                WHERE Pattern = @pattern AND Skill = @skill";
            cmd.Parameters.AddWithValue("@pattern", normalizedInput);
            cmd.Parameters.AddWithValue("@skill", skillName);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int id = reader.GetInt32(0);
                int useCount = reader.GetInt32(1);
                double confidence = reader.GetDouble(2);
                useCount++;
                // Increase confidence with usage, cap at 0.99
                confidence = Math.Min(0.99, confidence + (0.05 * (1 - confidence)));

                using var update = _db.CreateCommand();
                update.CommandText = @"
                    UPDATE LearnedPatterns 
                    SET UseCount = @count, Confidence = @conf, LastUsed = @last 
                    WHERE Id = @id";
                update.Parameters.AddWithValue("@count", useCount);
                update.Parameters.AddWithValue("@conf", confidence);
                update.Parameters.AddWithValue("@last", DateTime.UtcNow.ToString("o"));
                update.Parameters.AddWithValue("@id", id);
                update.ExecuteNonQuery();
            }
            else
            {
                using var insert = _db.CreateCommand();
                insert.CommandText = @"
                    INSERT INTO LearnedPatterns (Pattern, Skill, Confidence, UseCount, LastUsed)
                    VALUES (@pattern, @skill, @conf, 1, @last)";
                insert.Parameters.AddWithValue("@pattern", normalizedInput);
                insert.Parameters.AddWithValue("@skill", skillName);
                insert.Parameters.AddWithValue("@conf", 0.5);
                insert.Parameters.AddWithValue("@last", DateTime.UtcNow.ToString("o"));
                insert.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Get the best learned skill for a given input.
        /// </summary>
        public LearnedPattern? GetBestLearnedSkill(string input)
        {
            string normalized = NormalizeInput(input);

            // First try exact match
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, Pattern, Skill, Confidence, UseCount, LastUsed 
                FROM LearnedPatterns 
                WHERE Pattern = @pattern
                ORDER BY Confidence DESC, UseCount DESC
                LIMIT 1";
            cmd.Parameters.AddWithValue("@pattern", normalized);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new LearnedPattern
                {
                    Id = reader.GetInt32(0),
                    Pattern = reader.GetString(1),
                    SkillName = reader.GetString(2),
                    Confidence = reader.GetDouble(3),
                    UseCount = reader.GetInt32(4),
                    LastUsed = DateTime.Parse(reader.GetString(5))
                };
            }

            // Try fuzzy match - check if any learned pattern is contained in input
            using var fuzzyCmd = _db.CreateCommand();
            fuzzyCmd.CommandText = @"
                SELECT Id, Pattern, Skill, Confidence, UseCount, LastUsed 
                FROM LearnedPatterns 
                ORDER BY Confidence DESC, UseCount DESC";
            
            using var fuzzyReader = fuzzyCmd.ExecuteReader();
            LearnedPattern? best = null;
            double bestScore = 0;

            while (fuzzyReader.Read())
            {
                string pattern = fuzzyReader.GetString(1);
                double confidence = fuzzyReader.GetDouble(3);
                
                // Simple similarity: check if pattern words overlap with input words
                double similarity = CalculateSimilarity(normalized, pattern);
                double score = similarity * confidence;

                if (score > bestScore && score > 0.6)
                {
                    bestScore = score;
                    best = new LearnedPattern
                    {
                        Id = fuzzyReader.GetInt32(0),
                        Pattern = pattern,
                        SkillName = fuzzyReader.GetString(2),
                        Confidence = score,
                        UseCount = fuzzyReader.GetInt32(4),
                        LastUsed = DateTime.Parse(fuzzyReader.GetString(5))
                    };
                }
            }

            return best;
        }

        /// <summary>
        /// Set feedback for an interaction.
        /// </summary>
        public void SetFeedback(string input, int feedback)
        {
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"
                UPDATE Interactions 
                SET Feedback = @feedback 
                WHERE UserInput = @input 
                ORDER BY Timestamp DESC 
                LIMIT 1";
            cmd.Parameters.AddWithValue("@feedback", feedback);
            cmd.Parameters.AddWithValue("@input", input);
            cmd.ExecuteNonQuery();

            // Adjust pattern confidence based on feedback
            if (feedback > 0)
            {
                using var up = _db.CreateCommand();
                up.CommandText = @"
                    UPDATE LearnedPatterns 
                    SET Confidence = MIN(0.99, Confidence + 0.1) 
                    WHERE Pattern = @pattern";
                up.Parameters.AddWithValue("@pattern", NormalizeInput(input));
                up.ExecuteNonQuery();
            }
            else
            {
                using var down = _db.CreateCommand();
                down.CommandText = @"
                    UPDATE LearnedPatterns 
                    SET Confidence = MAX(0.1, Confidence - 0.15) 
                    WHERE Pattern = @pattern";
                down.Parameters.AddWithValue("@pattern", NormalizeInput(input));
                down.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Store a user preference.
        /// </summary>
        public void StorePreference(string key, string value)
        {
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO UserPreferences (Key, Value, UpdatedAt)
                VALUES (@key, @value, @updated)
                ON CONFLICT(Key) DO UPDATE SET Value = @value, UpdatedAt = @updated";
            cmd.Parameters.AddWithValue("@key", key);
            cmd.Parameters.AddWithValue("@value", value);
            cmd.Parameters.AddWithValue("@updated", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Get a user preference.
        /// </summary>
        public string? GetPreference(string key)
        {
            using var cmd = _db.CreateCommand();
            cmd.CommandText = "SELECT Value FROM UserPreferences WHERE Key = @key";
            cmd.Parameters.AddWithValue("@key", key);
            var result = cmd.ExecuteScalar();
            return result?.ToString();
        }

        /// <summary>
        /// Get top learned patterns.
        /// </summary>
        public List<string> GetTopPatterns(int count)
        {
            var patterns = new List<string>();
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"
                SELECT Pattern, Skill, Confidence, UseCount 
                FROM LearnedPatterns 
                ORDER BY UseCount DESC, Confidence DESC 
                LIMIT @count";
            cmd.Parameters.AddWithValue("@count", count);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                patterns.Add($"'{reader.GetString(0)}' → {reader.GetString(1)} " +
                           $"(confidence: {reader.GetDouble(2):P0}, used {reader.GetInt32(3)}x)");
            }
            return patterns;
        }

        /// <summary>
        /// Get all user preferences.
        /// </summary>
        public Dictionary<string, string> GetAllPreferences()
        {
            var prefs = new Dictionary<string, string>();
            using var cmd = _db.CreateCommand();
            cmd.CommandText = "SELECT Key, Value FROM UserPreferences";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                prefs[reader.GetString(0)] = reader.GetString(1);
            }
            return prefs;
        }

        /// <summary>
        /// Get learning statistics.
        /// </summary>
        public LearningStats GetStats()
        {
            var stats = new LearningStats();

            using var cmd1 = _db.CreateCommand();
            cmd1.CommandText = "SELECT COUNT(*) FROM Interactions";
            stats.TotalInteractions = Convert.ToInt32(cmd1.ExecuteScalar());

            using var cmd2 = _db.CreateCommand();
            cmd2.CommandText = "SELECT COUNT(*) FROM LearnedPatterns";
            stats.UniquePatterns = Convert.ToInt32(cmd2.ExecuteScalar());

            using var cmd3 = _db.CreateCommand();
            cmd3.CommandText = @"
                SELECT SkillUsed, COUNT(*) as cnt FROM Interactions 
                WHERE SkillUsed != 'none' AND SkillUsed != 'conversation'
                GROUP BY SkillUsed ORDER BY cnt DESC LIMIT 1";
            using var r3 = cmd3.ExecuteReader();
            if (r3.Read()) stats.TopSkill = r3.GetString(0);

            using var cmd4 = _db.CreateCommand();
            cmd4.CommandText = "SELECT COUNT(*) FROM Interactions WHERE Feedback > 0";
            stats.PositiveFeedback = Convert.ToInt32(cmd4.ExecuteScalar());

            using var cmd5 = _db.CreateCommand();
            cmd5.CommandText = "SELECT COUNT(*) FROM Interactions WHERE Feedback < 0";
            stats.NegativeFeedback = Convert.ToInt32(cmd5.ExecuteScalar());

            return stats;
        }

        /// <summary>
        /// Get recent interactions.
        /// </summary>
        public List<(string Input, string Skill, DateTime Time)> GetRecentInteractions(int count = 20)
        {
            var list = new List<(string, string, DateTime)>();
            using var cmd = _db.CreateCommand();
            cmd.CommandText = @"
                SELECT UserInput, SkillUsed, Timestamp FROM Interactions 
                ORDER BY Timestamp DESC LIMIT @count";
            cmd.Parameters.AddWithValue("@count", count);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add((
                    reader.GetString(0),
                    reader.GetString(1),
                    DateTime.Parse(reader.GetString(2))
                ));
            }
            return list;
        }

        private string NormalizeInput(string input)
        {
            return input.Trim().ToLowerInvariant();
        }

        private double CalculateSimilarity(string a, string b)
        {
            var wordsA = new HashSet<string>(a.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var wordsB = new HashSet<string>(b.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            if (wordsA.Count == 0 || wordsB.Count == 0) return 0;

            var intersection = wordsA.Intersect(wordsB).Count();
            var union = wordsA.Union(wordsB).Count();

            return (double)intersection / union;
        }

        public void Dispose()
        {
            _db?.Close();
            _db?.Dispose();
        }
    }
}

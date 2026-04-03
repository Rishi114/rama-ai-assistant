using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Rama.Core
{
    /// <summary>
    /// Critical Thinking Engine — Rama thinks before acting.
    /// Analyzes problems, validates solutions, catches mistakes,
    /// and learns from every error to avoid repeating them.
    /// </summary>
    public class CriticalThinking : IDisposable
    {
        private string MemoryDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rama", "Memory");

        private string MistakesPath => Path.Combine(MemoryDir, "mistakes.json");
        private string SolutionsPath => Path.Combine(MemoryDir, "solutions.json");
        private string ValidationsPath => Path.Combine(MemoryDir, "validations.json");

        private List<Mistake> _mistakes = new();
        private List<Solution> _solutions = new();
        private List<ValidationRule> _validationRules = new();

        // Stats
        public int TotalMistakes => _mistakes.Count;
        public int LearnedMistakes => _mistakes.Count(m => m.Learned);
        public double MistakeRate => _mistakes.Count > 0
            ? (double)_mistakes.Count(m => !m.Resolved) / _mistakes.Count
            : 0;

        public CriticalThinking()
        {
            Directory.CreateDirectory(MemoryDir);
            LoadAll();
            InitializeValidationRules();
        }

        #region Think Before Acting

        /// <summary>
        /// Analyze a request before executing. Returns a thinking analysis.
        /// </summary>
        public ThinkingResult Analyze(string input, string proposedAction)
        {
            var result = new ThinkingResult
            {
                Input = input,
                ProposedAction = proposedAction,
                Timestamp = DateTime.Now
            };

            // Step 1: Check for known mistakes
            var similarMistakes = FindSimilarMistakes(input);
            if (similarMistakes.Any())
            {
                result.Warnings.Add($"⚠️ I've made mistakes with similar requests before:");
                foreach (var mistake in similarMistakes.Take(3))
                {
                    result.Warnings.Add($"  • {mistake.What} → {mistake.Lesson}");
                }
                result.RiskLevel = RiskLevel.Medium;
            }

            // Step 2: Validate against rules
            foreach (var rule in _validationRules)
            {
                if (rule.IsMatch(input))
                {
                    if (rule.Action == ValidationAction.Warn)
                        result.Warnings.Add($"⚠️ {rule.Message}");
                    else if (rule.Action == ValidationAction.Block)
                    {
                        result.Blocked = true;
                        result.BlockReason = rule.Message;
                    }
                    result.RiskLevel = rule.Severity switch
                    {
                        RuleSeverity.High => RiskLevel.High,
                        RuleSeverity.Critical => RiskLevel.Critical,
                        _ => result.RiskLevel
                    };
                }
            }

            // Step 3: Check if we've solved this before
            var pastSolution = FindPastSolution(input);
            if (pastSolution != null)
            {
                result.Suggestions.Add($"💡 I solved something similar before: {pastSolution.Summary}");
                result.PastSolution = pastSolution;
            }

            // Step 4: Think about edge cases
            result.EdgeCases = ThinkAboutEdgeCases(input, proposedAction);

            // Step 5: Confidence score
            result.Confidence = CalculateConfidence(input, proposedAction, similarMistakes.Count);

            return result;
        }

        /// <summary>
        /// Post-action review. Did it work? Log the result.
        /// </summary>
        public void Review(string input, string action, bool success, string? error = null, string? result = null)
        {
            if (!success && error != null)
            {
                // Log the mistake
                var mistake = new Mistake
                {
                    Input = input,
                    Action = action,
                    Error = error,
                    Timestamp = DateTime.Now,
                    Context = GetContext(),
                    Category = CategorizeMistake(error)
                };

                // Check if we've made this mistake before
                var existing = _mistakes.FirstOrDefault(m =>
                    m.Category == mistake.Category && m.Action == action);

                if (existing != null)
                {
                    existing.Count++;
                    existing.LastSeen = DateTime.Now;
                }
                else
                {
                    _mistakes.Add(mistake);
                }

                // Auto-generate a lesson
                if (!mistake.Learned)
                {
                    mistake.Lesson = GenerateLesson(mistake);
                    mistake.Learned = true;
                }

                SaveMistakes();
            }
            else if (success)
            {
                // Store successful solution
                var solution = new Solution
                {
                    Input = input,
                    Action = action,
                    Result = result ?? "",
                    Timestamp = DateTime.Now,
                    UseCount = 1
                };

                var existing = _solutions.FirstOrDefault(s => s.Action == action);
                if (existing != null)
                {
                    existing.UseCount++;
                    existing.LastUsed = DateTime.Now;
                }
                else
                {
                    _solutions.Add(solution);
                }

                SaveSolutions();
            }
        }

        #endregion

        #region Mistake Analysis

        /// <summary>
        /// Find mistakes similar to current input.
        /// </summary>
        public List<Mistake> FindSimilarMistakes(string input)
        {
            string lower = input.ToLowerInvariant();
            return _mistakes
                .Where(m => CalculateSimilarity(lower, m.Input.ToLowerInvariant()) > 0.5 ||
                           lower.Contains(m.Category.ToLowerInvariant()))
                .OrderByDescending(m => m.Count)
                .Take(5)
                .ToList();
        }

        /// <summary>
        /// Get a summary of all mistakes and lessons learned.
        /// </summary>
        public string GetMistakeReport()
        {
            if (!_mistakes.Any())
                return "🧠 **Mistake Tracker:** No mistakes recorded yet! (Or I'm just perfect 😏)";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("🧠 **Critical Thinking Report:**\n");

            // Stats
            sb.AppendLine($"📊 **Statistics:**");
            sb.AppendLine($"• Total mistakes: {_mistakes.Count}");
            sb.AppendLine($"• Lessons learned: {LearnedMistakes}");
            sb.AppendLine($"• Unresolved: {_mistakes.Count(m => !m.Resolved)}");
            sb.AppendLine($"• Solutions stored: {_solutions.Count}");
            sb.AppendLine();

            // Top mistakes by category
            var categories = _mistakes
                .GroupBy(m => m.Category)
                .OrderByDescending(g => g.Sum(m => m.Count));

            sb.AppendLine("📂 **Mistake Categories:**");
            foreach (var cat in categories.Take(5))
            {
                sb.AppendLine($"  • {cat.Key}: {cat.Sum(m => m.Count)} occurrences");
            }
            sb.AppendLine();

            // Recent lessons
            var recentLessons = _mistakes
                .Where(m => m.Learned)
                .OrderByDescending(m => m.LastSeen)
                .Take(5);

            sb.AppendLine("📚 **Recent Lessons:**");
            foreach (var lesson in recentLessons)
            {
                sb.AppendLine($"  • {lesson.Lesson}");
            }

            // Improvement suggestions
            sb.AppendLine("\n💡 **How I'm Improving:**");
            sb.AppendLine("  • I check my mistake history before acting");
            sb.AppendLine("  • I validate risky operations before executing");
            sb.AppendLine("  • I learn from every error automatically");
            sb.AppendLine("  • I remember successful solutions");

            return sb.ToString();
        }

        /// <summary>
        /// Check if a proposed action might cause a known mistake.
        /// </summary>
        public MistakeWarning? CheckForMistake(string input, string action)
        {
            var similar = _mistakes
                .Where(m => m.Action == action || CalculateSimilarity(input.ToLowerInvariant(), m.Input.ToLowerInvariant()) > 0.6)
                .OrderByDescending(m => m.Count)
                .FirstOrDefault();

            if (similar != null)
            {
                return new MistakeWarning
                {
                    Mistake = similar,
                    Warning = $"⚠️ Careful! I made this mistake {similar.Count} time(s) before.\n" +
                             $"Error: {similar.Error}\n" +
                             $"Lesson: {similar.Lesson}\n" +
                             "I'll be extra careful this time."
                };
            }

            return null;
        }

        #endregion

        #region Solution Finding

        /// <summary>
        /// Find a past solution for similar input.
        /// </summary>
        public Solution? FindPastSolution(string input)
        {
            string lower = input.ToLowerInvariant();
            return _solutions
                .Where(s => CalculateSimilarity(lower, s.Input.ToLowerInvariant()) > 0.6)
                .OrderByDescending(s => s.UseCount)
                .FirstOrDefault();
        }

        /// <summary>
        /// Get top successful patterns.
        /// </summary>
        public List<Solution> GetTopSolutions(int count = 10)
        {
            return _solutions
                .OrderByDescending(s => s.UseCount)
                .Take(count)
                .ToList();
        }

        #endregion

        #region Edge Case Thinking

        private List<string> ThinkAboutEdgeCases(string input, string action)
        {
            var cases = new List<string>();
            string lower = input.ToLowerInvariant();

            // File operations
            if (action.Contains("file") || lower.Contains("delete") || lower.Contains("move"))
            {
                cases.Add("📁 What if the file doesn't exist?");
                cases.Add("📁 What if the path has spaces or special characters?");
                cases.Add("📁 What if permissions are denied?");
            }

            // Web requests
            if (action.Contains("url") || action.Contains("web") || action.Contains("search"))
            {
                cases.Add("🌐 What if there's no internet connection?");
                cases.Add("🌐 What if the URL is invalid?");
                cases.Add("🌐 What if the server returns an error?");
            }

            // Code execution
            if (action.Contains("code") || action.Contains("script") || action.Contains("run"))
            {
                cases.Add("💻 What if the code has syntax errors?");
                cases.Add("💻 What if it runs an infinite loop?");
                cases.Add("💻 What if it needs elevated permissions?");
            }

            // User input
            if (lower.Contains("calculate") || lower.Contains("convert"))
            {
                cases.Add("🔢 What if the input isn't a number?");
                cases.Add("🔢 What if it's a division by zero?");
            }

            return cases;
        }

        #endregion

        #region Confidence Scoring

        private double CalculateConfidence(string input, string action, int mistakeCount)
        {
            double confidence = 0.8; // Base confidence

            // Lower confidence if we've made mistakes with similar inputs
            confidence -= mistakeCount * 0.1;

            // Higher confidence if we've solved this before
            var pastSolution = FindPastSolution(input);
            if (pastSolution != null)
                confidence += 0.15;

            // Lower confidence for risky operations
            string lower = input.ToLowerInvariant();
            if (lower.Contains("delete") || lower.Contains("format") || lower.Contains("remove"))
                confidence -= 0.2;
            if (lower.Contains("all") || lower.Contains("everything"))
                confidence -= 0.15;

            return Math.Clamp(confidence, 0.1, 0.99);
        }

        #endregion

        #region Mistake Categorization

        private string CategorizeMistake(string error)
        {
            string lower = error.ToLowerInvariant();

            if (lower.Contains("file") || lower.Contains("path") || lower.Contains("directory"))
                return "File Operations";
            if (lower.Contains("network") || lower.Contains("connection") || lower.Contains("timeout"))
                return "Network";
            if (lower.Contains("permission") || lower.Contains("access") || lower.Contains("denied"))
                return "Permissions";
            if (lower.Contains("syntax") || lower.Contains("parse") || lower.Contains("format"))
                return "Syntax/Parsing";
            if (lower.Contains("null") || lower.Contains("empty") || lower.Contains("missing"))
                return "Missing Data";
            if (lower.Contains("not found") || lower.Contains("404"))
                return "Not Found";
            if (lower.Contains("timeout") || lower.Contains("slow"))
                return "Performance";

            return "General";
        }

        private string GenerateLesson(Mistake mistake)
        {
            return mistake.Category switch
            {
                "File Operations" => "Always check if file exists before operating on it",
                "Network" => "Add timeout and retry logic for network operations",
                "Permissions" => "Check permissions before attempting operations",
                "Syntax/Parsing" => "Validate input format before processing",
                "Missing Data" => "Check for null/empty values before using",
                "Not Found" => "Verify resources exist before accessing",
                "Performance" => "Add timeouts and handle slow responses",
                _ => "Validate inputs and handle errors gracefully"
            };
        }

        private string GetContext()
        {
            return $"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }

        #endregion

        #region Validation Rules

        private void InitializeValidationRules()
        {
            _validationRules = new List<ValidationRule>
            {
                // Dangerous file operations
                new()
                {
                    Pattern = @"delete\s+(?:all|everything|C:\\|D:\\)",
                    Action = ValidationAction.Warn,
                    Message = "This will delete multiple files/folders. Are you sure?",
                    Severity = RuleSeverity.High
                },
                new()
                {
                    Pattern = @"format\s+(?:C:|D:|drive)",
                    Action = ValidationAction.Block,
                    Message = "⛔ Cannot format drives — this is too dangerous!",
                    Severity = RuleSeverity.Critical
                },

                // Recursive operations
                new()
                {
                    Pattern = @"(?:delete|remove|move)\s+.*\*.*(?:/s|/r|recursive)",
                    Action = ValidationAction.Warn,
                    Message = "This is a recursive operation. Let me double-check the path.",
                    Severity = RuleSeverity.High
                },

                // System commands
                new()
                {
                    Pattern = @"(?:shutdown|restart|taskkill|rm -rf|sudo)",
                    Action = ValidationAction.Warn,
                    Message = "This affects the system. I'll be extra careful.",
                    Severity = RuleSeverity.High
                },

                // Empty inputs
                new()
                {
                    Pattern = @"^\s*$",
                    Action = ValidationAction.Warn,
                    Message = "Empty input detected. What would you like me to do?",
                    Severity = RuleSeverity.Low
                }
            };
        }

        #endregion

        #region Helpers

        private double CalculateSimilarity(string a, string b)
        {
            var wordsA = new HashSet<string>(a.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var wordsB = new HashSet<string>(b.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            if (wordsA.Count == 0 || wordsB.Count == 0) return 0;

            int intersection = wordsA.Intersect(wordsB).Count();
            int union = wordsA.Union(wordsB).Count();

            return (double)intersection / union;
        }

        #endregion

        #region Persistence

        private void LoadAll()
        {
            try
            {
                if (File.Exists(MistakesPath))
                    _mistakes = JsonConvert.DeserializeObject<List<Mistake>>(File.ReadAllText(MistakesPath)) ?? new();
                if (File.Exists(SolutionsPath))
                    _solutions = JsonConvert.DeserializeObject<List<Solution>>(File.ReadAllText(SolutionsPath)) ?? new();
            }
            catch { }
        }

        private void SaveMistakes() =>
            File.WriteAllText(MistakesPath, JsonConvert.SerializeObject(_mistakes, Formatting.Indented));

        private void SaveSolutions() =>
            File.WriteAllText(SolutionsPath, JsonConvert.SerializeObject(_solutions, Formatting.Indented));

        #endregion

        public void Dispose()
        {
            SaveMistakes();
            SaveSolutions();
        }
    }

    #region Data Models

    public class ThinkingResult
    {
        public string Input { get; set; } = "";
        public string ProposedAction { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;
        public double Confidence { get; set; } = 0.8;
        public bool Blocked { get; set; }
        public string BlockReason { get; set; } = "";
        public List<string> Warnings { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public List<string> EdgeCases { get; set; } = new();
        public Solution? PastSolution { get; set; }

        public string GetReport()
        {
            var sb = new System.Text.StringBuilder();

            if (Blocked)
                return $"⛔ **BLOCKED:** {BlockReason}";

            sb.AppendLine($"🤔 **Thinking...**");
            sb.AppendLine($"Confidence: {Confidence:P0}");
            sb.AppendLine($"Risk: {RiskLevel}");

            if (Warnings.Any())
            {
                sb.AppendLine("\n⚠️ **Warnings:**");
                foreach (var w in Warnings) sb.AppendLine($"  {w}");
            }

            if (Suggestions.Any())
            {
                sb.AppendLine("\n💡 **Suggestions:**");
                foreach (var s in Suggestions) sb.AppendLine($"  {s}");
            }

            if (EdgeCases.Any())
            {
                sb.AppendLine("\n🔍 **Edge Cases to Consider:**");
                foreach (var e in EdgeCases) sb.AppendLine($"  {e}");
            }

            return sb.ToString();
        }
    }

    public class Mistake
    {
        public string Input { get; set; } = "";
        public string Action { get; set; } = "";
        public string Error { get; set; } = "";
        public string Category { get; set; } = "";
        public string Lesson { get; set; } = "";
        public string Context { get; set; } = "";
        public bool Learned { get; set; }
        public bool Resolved { get; set; }
        public int Count { get; set; } = 1;
        public DateTime Timestamp { get; set; }
        public DateTime LastSeen { get; set; }
    }

    public class Solution
    {
        public string Input { get; set; } = "";
        public string Action { get; set; } = "";
        public string Result { get; set; } = "";
        public string Summary { get; set; } = "";
        public int UseCount { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime LastUsed { get; set; }
    }

    public class ValidationRule
    {
        public string Pattern { get; set; } = "";
        public ValidationAction Action { get; set; }
        public string Message { get; set; } = "";
        public RuleSeverity Severity { get; set; }

        public bool IsMatch(string input) =>
            System.Text.RegularExpressions.Regex.IsMatch(input, Pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    public class MistakeWarning
    {
        public Mistake Mistake { get; set; } = new();
        public string Warning { get; set; } = "";
    }

    public enum RiskLevel { Low, Medium, High, Critical }
    public enum ValidationAction { Warn, Block }
    public enum RuleSeverity { Low, Medium, High, Critical }

    #endregion
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Rama.Core
{
    /// <summary>
    /// Advanced Learning Engine — Learns from EVERYTHING.
    /// Images, audio, documents, web, environment, people, patterns.
    /// The more data Rama gets, the smarter she becomes.
    /// </summary>
    public class AdvancedLearning : IDisposable
    {
        private string LearningDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rama", "Learning");

        private string ConceptsPath => Path.Combine(LearningDir, "concepts.json");
        private string ObservationsPath => Path.Combine(LearningDir, "observations.json");
        private string SkillsKnowledgePath => Path.Combine(LearningDir, "skills_knowledge.json");
        private string WorldModelPath => Path.Combine(LearningDir, "world_model.json");

        private List<Concept> _concepts = new();
        private List<Observation> _observations = new();
        private WorldModel _worldModel = new();

        public int TotalConcepts => _concepts.Count;
        public int TotalObservations => _observations.Count;

        public AdvancedLearning()
        {
            Directory.CreateDirectory(LearningDir);
            LoadAll();
        }

        #region Learn from Text

        /// <summary>
        /// Learn concepts from any text — extracts ideas, relationships, facts.
        /// </summary>
        public ConceptLearnResult LearnFromText(string text, string source = "user")
        {
            var result = new ConceptLearnResult();

            // Extract key concepts
            var concepts = ExtractConcepts(text);
            foreach (var concept in concepts)
            {
                var existing = _concepts.FirstOrDefault(c => c.Name.Equals(concept, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    existing.Confidence = Math.Min(0.99, existing.Confidence + 0.05);
                    existing.References++;
                    existing.LastSeen = DateTime.Now;
                    existing.Sources.Add(source);
                }
                else
                {
                    _concepts.Add(new Concept
                    {
                        Name = concept,
                        Category = CategorizeConcept(concept, text),
                        Confidence = 0.6,
                        References = 1,
                        FirstSeen = DateTime.Now,
                        LastSeen = DateTime.Now,
                        Sources = new List<string> { source },
                        Context = text.Length > 200 ? text[..200] : text
                    });
                    result.NewConcepts.Add(concept);
                }
            }

            // Extract relationships between concepts
            var relationships = ExtractRelationships(text, concepts);
            foreach (var rel in relationships)
            {
                _worldModel.Relationships.Add(rel);
                result.NewRelationships.Add(rel);
            }

            // Extract rules/patterns
            var rules = ExtractRules(text);
            foreach (var rule in rules)
            {
                _worldModel.Rules.Add(rule);
                result.NewRules.Add(rule);
            }

            // Record observation
            _observations.Add(new Observation
            {
                Content = text.Length > 500 ? text[..500] : text,
                Source = source,
                Timestamp = DateTime.Now,
                ConceptsFound = concepts.Count,
                Type = "text"
            });

            SaveAll();
            return result;
        }

        /// <summary>
        /// Learn from an image description (what Rama sees through camera).
        /// </summary>
        public ConceptLearnResult LearnFromVision(string imageDescription, string source = "camera")
        {
            var result = LearnFromText($"[VISUAL] {imageDescription}", source);

            // Store visual observations separately
            _worldModel.VisualMemories.Add(new VisualMemory
            {
                Description = imageDescription,
                Timestamp = DateTime.Now,
                Source = source
            });

            // Keep bounded
            if (_worldModel.VisualMemories.Count > 500)
                _worldModel.VisualMemories = _worldModel.VisualMemories.TakeLast(500).ToList();

            SaveAll();
            return result;
        }

        /// <summary>
        /// Learn from audio/speech (what Rama hears through mic).
        /// </summary>
        public ConceptLearnResult LearnFromAudio(string transcript, string detectedLanguage, string source = "microphone")
        {
            var result = LearnFromText($"[AUDIO:{detectedLanguage}] {transcript}", source);

            _worldModel.AudioMemories.Add(new AudioMemory
            {
                Transcript = transcript,
                Language = detectedLanguage,
                Timestamp = DateTime.Now,
                Source = source
            });

            if (_worldModel.AudioMemories.Count > 500)
                _worldModel.AudioMemories = _worldModel.AudioMemories.TakeLast(500).ToList();

            SaveAll();
            return result;
        }

        /// <summary>
        /// Learn from a document/file.
        /// </summary>
        public ConceptLearnResult LearnFromDocument(string content, string fileName, string fileType)
        {
            var result = LearnFromText(content, $"file:{fileName}");

            _worldModel.DocumentKnowledge.Add(new DocumentKnowledge
            {
                FileName = fileName,
                FileType = fileType,
                Summary = content.Length > 300 ? content[..300] : content,
                KeyConcepts = result.NewConcepts,
                Timestamp = DateTime.Now
            });

            SaveAll();
            return result;
        }

        /// <summary>
        /// Learn from code — understand what code does.
        /// </summary>
        public ConceptLearnResult LearnFromCode(string code, string language)
        {
            var analysis = AnalyzeCode(code, language);
            string description = $"[CODE:{language}] {analysis.Purpose}. Functions: {string.Join(", ", analysis.Functions)}";
            return LearnFromText(description, $"code:{language}");
        }

        /// <summary>
        /// Learn from experience — remember what worked and what didn't.
        /// </summary>
        public void LearnFromExperience(string situation, string action, bool success, string outcome)
        {
            _worldModel.Experiences.Add(new Experience
            {
                Situation = situation,
                Action = action,
                Success = success,
                Outcome = outcome,
                Timestamp = DateTime.Now
            });

            // Extract lesson
            if (!success)
            {
                _worldModel.Lessons.Add(new Lesson
                {
                    Problem = situation,
                    FailedAction = action,
                    Lesson = $"Don't {action} when {situation}. Outcome: {outcome}",
                    Timestamp = DateTime.Now
                });
            }

            if (_worldModel.Experiences.Count > 1000)
                _worldModel.Experiences = _worldModel.Experiences.TakeLast(1000).ToList();

            SaveAll();
        }

        #endregion

        #region Deep Thinking

        /// <summary>
        /// Deep analysis — break down a problem into parts, analyze each, synthesize.
        /// </summary>
        public ThinkingChain DeepThink(string problem)
        {
            var chain = new ThinkingChain { Problem = problem };

            // Step 1: Understand
            chain.Steps.Add(new ThinkingStep
            {
                Name = "Understand",
                Content = $"What is being asked: {problem}",
                Insights = ExtractKeyInsights(problem)
            });

            // Step 2: Recall relevant knowledge
            var relevant = FindRelevantConcepts(problem);
            chain.Steps.Add(new ThinkingStep
            {
                Name = "Recall",
                Content = $"Found {relevant.Count} relevant concepts",
                Insights = relevant.Select(c => $"{c.Name} (confidence: {c.Confidence:P0})").ToList()
            });

            // Step 3: Check past experiences
            var pastExperiences = _worldModel.Experiences
                .Where(e => CalculateSimilarity(e.Situation, problem) > 0.4)
                .Take(5)
                .ToList();

            chain.Steps.Add(new ThinkingStep
            {
                Name = "Experience",
                Content = $"Found {pastExperiences.Count} similar past experiences",
                Insights = pastExperiences.Select(e => $"{(e.Success ? "✅" : "❌")} {e.Action} → {e.Outcome}").ToList()
            });

            // Step 4: Consider alternatives
            chain.Steps.Add(new ThinkingStep
            {
                Name = "Alternatives",
                Content = "Possible approaches:",
                Insights = GenerateAlternatives(problem)
            });

            // Step 5: Evaluate risks
            chain.Steps.Add(new ThinkingStep
            {
                Name = "Risks",
                Content = "Potential issues:",
                Insights = EvaluateRisks(problem)
            });

            // Step 6: Synthesize
            chain.Conclusion = SynthesizeConclusion(chain);
            chain.Confidence = CalculateConfidence(relevant, pastExperiences);

            return chain;
        }

        /// <summary>
        /// Creative thinking — generate new ideas by combining concepts.
        /// </summary>
        public List<string> CreativeThink(string topic, int ideaCount = 5)
        {
            var ideas = new List<string>();
            var relevant = FindRelevantConcepts(topic);

            var rand = new Random();
            for (int i = 0; i < ideaCount; i++)
            {
                // Combine random concepts
                if (relevant.Count >= 2)
                {
                    var a = relevant[rand.Next(relevant.Count)];
                    var b = relevant[rand.Next(relevant.Count)];
                    ideas.Add($"What if we combine {a.Name} with {b.Name}?");
                }
            }

            return ideas;
        }

        /// <summary>
        /// Question generation — generate questions to understand something better.
        /// </summary>
        public List<string> GenerateQuestions(string topic)
        {
            var questions = new List<string>
            {
                $"What is the main purpose of {topic}?",
                $"How does {topic} work?",
                $"What are the limitations of {topic}?",
                $"What would happen if {topic} changed?",
                $"How does {topic} relate to what I already know?",
                $"What's the most important aspect of {topic}?",
                $"What mistakes should I avoid with {topic}?"
            };

            return questions;
        }

        #endregion

        #region Helpers

        private List<string> ExtractConcepts(string text)
        {
            var concepts = new HashSet<string>();

            // Extract capitalized words (potential proper nouns/concepts)
            var matches = Regex.Matches(text, @"\b[A-Z][a-z]{2,}(?:\s+[A-Z][a-z]+)*\b");
            foreach (Match m in matches)
                concepts.Add(m.Value);

            // Extract quoted terms
            var quoted = Regex.Matches(text, @"""([^""]+)""|'([^']+)'");
            foreach (Match m in quoted)
                concepts.Add(m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value);

            // Extract technical terms
            var techTerms = Regex.Matches(text, @"\b(?:API|SDK|HTTP|JSON|SQL|AI|ML|LLM|GPU|CPU|RAM|SSD|DNS|SSH|TCP|UDP|REST|GraphQL|Docker|Kubernetes|React|Angular|Vue|Python|JavaScript|TypeScript|Java|C\#|Go|Rust)\b");
            foreach (Match m in techTerms)
                concepts.Add(m.Value);

            return concepts.Take(20).ToList();
        }

        private List<Relationship> ExtractRelationships(string text, List<string> concepts)
        {
            var relationships = new List<Relationship>();

            for (int i = 0; i < concepts.Count - 1; i++)
            {
                for (int j = i + 1; j < concepts.Count; j++)
                {
                    if (text.Contains(concepts[i]) && text.Contains(concepts[j]))
                    {
                        relationships.Add(new Relationship
                        {
                            From = concepts[i],
                            To = concepts[j],
                            Type = "related",
                            Strength = 0.5
                        });
                    }
                }
            }

            return relationships.Take(10).ToList();
        }

        private List<string> ExtractRules(string text)
        {
            var rules = new List<string>();

            // "If...then" patterns
            var ifThen = Regex.Matches(text, @"(?:if|when|whenever)\s+(.+?)\s+(?:then|,)\s+(.+?)(?:\.|$)", RegexOptions.IgnoreCase);
            foreach (Match m in ifThen)
                rules.Add($"If {m.Groups[1].Value.Trim()}, then {m.Groups[2].Value.Trim()}");

            return rules.Take(5).ToList();
        }

        private string CategorizeConcept(string concept, string context)
        {
            string lower = context.ToLowerInvariant();
            if (lower.Contains("code") || lower.Contains("program") || lower.Contains("function")) return "programming";
            if (lower.Contains("learn") || lower.Contains("study") || lower.Contains("understand")) return "learning";
            if (lower.Contains("system") || lower.Contains("software") || lower.Contains("app")) return "technology";
            if (lower.Contains("people") || lower.Contains("person") || lower.Contains("user")) return "social";
            return "general";
        }

        private List<Concept> FindRelevantConcepts(string query)
        {
            string lower = query.ToLowerInvariant();
            return _concepts
                .Where(c => lower.Contains(c.Name.ToLowerInvariant()) || c.Name.ToLowerInvariant().Contains(lower))
                .OrderByDescending(c => c.Confidence * c.References)
                .Take(10)
                .ToList();
        }

        private List<string> ExtractKeyInsights(string text)
        {
            var insights = new List<string>();
            var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in sentences.Take(3))
                if (s.Trim().Length > 10)
                    insights.Add(s.Trim());
            return insights;
        }

        private List<string> GenerateAlternatives(string problem)
        {
            return new List<string>
            {
                "Approach directly with minimal steps",
                "Break into smaller sub-problems",
                "Use a known solution from similar past experience",
                "Try a completely new approach",
                "Ask the user for more information"
            };
        }

        private List<string> EvaluateRisks(string problem)
        {
            var risks = new List<string>();
            string lower = problem.ToLowerInvariant();

            if (lower.Contains("delete") || lower.Contains("remove")) risks.Add("⚠️ Data loss risk");
            if (lower.Contains("send") || lower.Contains("share")) risks.Add("⚠️ Privacy risk");
            if (lower.Contains("install") || lower.Contains("modify")) risks.Add("⚠️ System change risk");
            if (lower.Contains("all") || lower.Contains("everything")) risks.Add("⚠️ Scope too broad");

            return risks;
        }

        private string SynthesizeConclusion(ThinkingChain chain)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Based on analysis of '{chain.Problem}':");

            if (chain.Steps.FirstOrDefault(s => s.Name == "Experience")?.Insights.Any() == true)
                sb.AppendLine("- Past experience available to guide decision");
            else
                sb.AppendLine("- No direct past experience; will proceed carefully");

            sb.AppendLine($"- Confidence: {chain.Confidence:P0}");
            sb.AppendLine("- Recommend proceeding with validation steps");
            return sb.ToString();
        }

        private double CalculateConfidence(List<Concept> concepts, List<Experience> experiences)
        {
            double base_ = 0.5;
            base_ += concepts.Count * 0.05;
            base_ += experiences.Count(e => e.Success) * 0.1;
            base_ -= experiences.Count(e => !e.Success) * 0.05;
            return Math.Clamp(base_, 0.1, 0.95);
        }

        private double CalculateSimilarity(string a, string b)
        {
            var wordsA = new HashSet<string>(a.ToLowerInvariant().Split(' '));
            var wordsB = new HashSet<string>(b.ToLowerInvariant().Split(' '));
            if (!wordsA.Any() || !wordsB.Any()) return 0;
            return (double)wordsA.Intersect(wordsB).Count() / wordsA.Union(wordsB).Count();
        }

        private CodeAnalysis AnalyzeCode(string code, string lang)
        {
            var analysis = new CodeAnalysis();
            var funcMatches = Regex.Matches(code, @"(?:def|function|func|fn|public|private|static)\s+(\w+)");
            analysis.Functions = funcMatches.Select(m => m.Groups[1].Value).Distinct().ToList();
            analysis.Purpose = code.ToLowerInvariant().Contains("class") ? "Class definition" : "Script/Functions";
            return analysis;
        }

        #endregion

        #region Reporting

        public string GetLearningReport()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("🧠 **Advanced Learning Report:**\n");
            sb.AppendLine($"📚 Concepts: {_concepts.Count}");
            sb.AppendLine($"👁️ Observations: {_observations.Count}");
            sb.AppendLine($"🖼️ Visual memories: {_worldModel.VisualMemories.Count}");
            sb.AppendLine($"🎤 Audio memories: {_worldModel.AudioMemories.Count}");
            sb.AppendLine($"📄 Documents analyzed: {_worldModel.DocumentKnowledge.Count}");
            sb.AppendLine($"🔗 Relationships: {_worldModel.Relationships.Count}");
            sb.AppendLine($"📏 Rules learned: {_worldModel.Rules.Count}");
            sb.AppendLine($"💡 Experiences: {_worldModel.Experiences.Count}");
            sb.AppendLine($"📝 Lessons: {_worldModel.Lessons.Count}");

            if (_concepts.Any())
            {
                sb.AppendLine("\n**Top Concepts:**");
                foreach (var c in _concepts.OrderByDescending(x => x.References).Take(10))
                    sb.AppendLine($"  • {c.Name} ({c.Category}, {c.References} refs, {c.Confidence:P0})");
            }

            return sb.ToString();
        }

        #endregion

        #region Persistence

        private void LoadAll()
        {
            try
            {
                if (File.Exists(ConceptsPath))
                    _concepts = JsonConvert.DeserializeObject<List<Concept>>(File.ReadAllText(ConceptsPath)) ?? new();
                if (File.Exists(ObservationsPath))
                    _observations = JsonConvert.DeserializeObject<List<Observation>>(File.ReadAllText(ObservationsPath)) ?? new();
                if (File.Exists(WorldModelPath))
                    _worldModel = JsonConvert.DeserializeObject<WorldModel>(File.ReadAllText(WorldModelPath)) ?? new();
            }
            catch { }
        }

        private void SaveAll()
        {
            File.WriteAllText(ConceptsPath, JsonConvert.SerializeObject(_concepts, Formatting.Indented));
            File.WriteAllText(ObservationsPath, JsonConvert.SerializeObject(_observations, Formatting.Indented));
            File.WriteAllText(WorldModelPath, JsonConvert.SerializeObject(_worldModel, Formatting.Indented));
        }

        #endregion

        public void Dispose() => SaveAll();
    }

    #region Models

    public class Concept
    {
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public double Confidence { get; set; }
        public int References { get; set; }
        public string Context { get; set; } = "";
        public List<string> Sources { get; set; } = new();
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
    }

    public class Observation
    {
        public string Content { get; set; } = "";
        public string Source { get; set; } = "";
        public string Type { get; set; } = "";
        public int ConceptsFound { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class WorldModel
    {
        public List<Relationship> Relationships { get; set; } = new();
        public List<string> Rules { get; set; } = new();
        public List<VisualMemory> VisualMemories { get; set; } = new();
        public List<AudioMemory> AudioMemories { get; set; } = new();
        public List<DocumentKnowledge> DocumentKnowledge { get; set; } = new();
        public List<Experience> Experiences { get; set; } = new();
        public List<Lesson> Lessons { get; set; } = new();
    }

    public class Relationship { public string From { get; set; } = ""; public string To { get; set; } = ""; public string Type { get; set; } = ""; public double Strength { get; set; } }
    public class VisualMemory { public string Description { get; set; } = ""; public DateTime Timestamp { get; set; } = DateTime.Now; public string Source { get; set; } = ""; }
    public class AudioMemory { public string Transcript { get; set; } = ""; public string Language { get; set; } = ""; public DateTime Timestamp { get; set; } = DateTime.Now; public string Source { get; set; } = ""; }
    public class DocumentKnowledge { public string FileName { get; set; } = ""; public string FileType { get; set; } = ""; public string Summary { get; set; } = ""; public List<string> KeyConcepts { get; set; } = new(); public DateTime Timestamp { get; set; } = DateTime.Now; }
    public class Experience { public string Situation { get; set; } = ""; public string Action { get; set; } = ""; public bool Success { get; set; } public string Outcome { get; set; } = ""; public DateTime Timestamp { get; set; } = DateTime.Now; }
    public class Lesson { public string Problem { get; set; } = ""; public string FailedAction { get; set; } = ""; public string LessonText { get; set; } = ""; public DateTime Timestamp { get; set; } = DateTime.Now; }

    public class ThinkingChain
    {
        public string Problem { get; set; } = "";
        public List<ThinkingStep> Steps { get; set; } = new();
        public string Conclusion { get; set; } = "";
        public double Confidence { get; set; }
    }

    public class ThinkingStep { public string Name { get; set; } = ""; public string Content { get; set; } = ""; public List<string> Insights { get; set; } = new(); }

    public class ConceptLearnResult
    {
        public List<string> NewConcepts { get; set; } = new();
        public List<Relationship> NewRelationships { get; set; } = new();
        public List<string> NewRules { get; set; } = new();
    }

    public class CodeAnalysis { public List<string> Functions { get; set; } = new(); public string Purpose { get; set; } = ""; }

    #endregion
}

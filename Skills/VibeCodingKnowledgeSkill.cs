using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Skill that provides access to Vibe Coding knowledge base.
    /// Includes easy-vibe, awesome-vibe-coding, claude-reflect, and vibe-coding-prompt-template.
    /// </summary>
    public class VibeCodingKnowledgeSkill : SkillBase
    {
        private readonly string _knowledgePath;
        
        public override string Name => "Vibe Coding Knowledge";
        public override string Description => "Access vibe coding tutorials, prompts, and best practices from curated knowledge bases";
        public override string[] Triggers => new[] { "vibe", "coding", "prompt", "template", "ai coding", "learn coding" };

        public VibeCodingKnowledgeSkill()
        {
            _knowledgePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BrainKnowledge");
        }

        public override bool CanHandle(string input)
        {
            string lower = input.ToLowerInvariant();
            return Triggers.Any(t => lower.Contains(t));
        }

        public override async Task<string> ExecuteAsync(string input, Memory memory)
        {
            string query = ExtractCommand(input).ToLowerInvariant();
            
            if (string.IsNullOrWhiteSpace(query) || query == input.ToLowerInvariant())
            {
                return GetIntroduction();
            }

            // Search across all knowledge repos
            var results = await SearchKnowledgeAsync(query);
            
            if (results.Count == 0)
            {
                return $"I searched my vibe coding knowledge but couldn't find anything about '{query}'. Want me to dig deeper?";
            }

            return FormatResults(query, results);
        }

        private string GetIntroduction()
        {
            return @"🎨 I've got a whole brain full of vibe coding knowledge! Here's what's in there:

📚 **easy-vibe** - A progressive curriculum from zero to advanced AI coding:
• Stage 0: Introduction through games
• Stage 1: AI Product Manager - building AI web prototypes
• Stage 2: Full-stack development with databases & deployment
• Stage 3: Cross-platform (WeChat, Android, MCP)

🌟 **awesome-vibe-coding** - Curated list of vibe coding tools, frameworks, and resources

🧠 **claude-reflect** - Self-reflecting AI patterns and deep research capabilities

📝 **vibe-coding-prompt-template** - Ready-to-use prompts for building with AI

Just ask me about specific topics like 'how to use MCP', 'create a web app', 'prompt engineering', or whatever you're curious about!";
        }

        private async Task<System.Collections.Generic.List<KnowledgeResult>> SearchKnowledgeAsync(string query)
        {
            var results = new System.Collections.Generic.List<KnowledgeResult>();
            
            // Search easy-vibe
            var easyVibePath = Path.Combine(_knowledgePath, "easy-vibe");
            if (Directory.Exists(easyVibePath))
            {
                results.AddRange(await SearchDirectoryAsync(easyVibePath, query, "easy-vibe"));
            }

            // Search awesome-vibe-coding
            var awesomePath = Path.Combine(_knowledgePath, "awesome-vibe-coding");
            if (Directory.Exists(awesomePath))
            {
                var readme = Path.Combine(awesomePath, "README.md");
                if (File.Exists(readme))
                {
                    var content = await File.ReadAllTextAsync(readme);
                    if (content.ToLowerInvariant().Contains(query))
                    {
                        results.Add(new KnowledgeResult 
                        { 
                            Repo = "awesome-vibe-coding", 
                            File = "README.md", 
                            Snippet = FindSnippet(content, query) 
                        });
                    }
                }
            }

            // Search claude-reflect
            var reflectPath = Path.Combine(_knowledgePath, "claude-reflect");
            if (Directory.Exists(reflectPath))
            {
                results.AddRange(await SearchDirectoryAsync(reflectPath, query, "claude-reflect", new[] { ".md" }));
            }

            // Search vibe-coding-prompt-template
            var templatePath = Path.Combine(_knowledgePath, "vibe-coding-prompt-template");
            if (Directory.Exists(templatePath))
            {
                results.AddRange(await SearchDirectoryAsync(templatePath, query, "vibe-coding-prompt-template"));
            }

            return results;
        }

        private async Task<System.Collections.Generic.List<KnowledgeResult>> SearchDirectoryAsync(
            string path, string query, string repo, string[] extensions = null)
        {
            var results = new System.Collections.Generic.List<KnowledgeResult>();
            extensions ??= new[] { ".md", ".txt", ".mdx" };

            try
            {
                var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(f => extensions.Any(e => f.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
                    .Take(50); // Limit to prevent timeout

                foreach (var file in files)
                {
                    try
                    {
                        var content = await File.ReadAllTextAsync(file);
                        if (content.ToLowerInvariant().Contains(query))
                        {
                            var relativePath = Path.GetRelativePath(path, file);
                            results.Add(new KnowledgeResult
                            {
                                Repo = repo,
                                File = relativePath,
                                Snippet = FindSnippet(content, query)
                            });
                        }
                    }
                    catch { /* Skip files we can't read */ }
                }
            }
            catch { /* Skip directories we can't access */ }

            return results;
        }

        private string FindSnippet(string content, string query)
        {
            var lowerContent = content.ToLowerInvariant();
            var idx = lowerContent.IndexOf(query);
            if (idx < 0) return content.Length > 200 ? content.Substring(0, 200) + "..." : content;

            int start = Math.Max(0, idx - 100);
            int end = Math.Min(content.Length, idx + query.Length + 100);
            var snippet = content.Substring(start, end - start);
            
            return (start > 0 ? "..." : "") + snippet + (end < content.Length ? "..." : "");
        }

        private string FormatResults(string query, System.Collections.Generic.List<KnowledgeResult> results)
        {
            var response = $"🔍 Found {results.Count} matches for '{query}':\n\n";
            
            foreach (var r in results.Take(5))
            {
                response += $"**{r.Repo}** - {r.File}\n{r.Snippet}\n\n";
            }

            if (results.Count > 5)
            {
                response += $"_...and {results.Count - 5} more. Want me to dig deeper into a specific area?_";
            }

            return response;
        }

        public override void OnLoad()
        {
            base.OnLoad();
            if (!Directory.Exists(_knowledgePath))
            {
                Console.WriteLine($"[VibeCodingKnowledge] Knowledge path not found: {_knowledgePath}");
            }
        }

        private class KnowledgeResult
        {
            public string Repo { get; set; }
            public string File { get; set; }
            public string Snippet { get; set; }
        }
    }
}
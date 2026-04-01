using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Knowledge Learner — Learns from videos, books, blogs, scripts, and any text source.
    /// Ingests knowledge and stores it in Rama's memory for future reference.
    /// </summary>
    public class KnowledgeSkill : SkillBase
    {
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

        private string KnowledgeDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rama", "Knowledge");

        private string KnowledgeDb => Path.Combine(KnowledgeDir, "knowledge.json");

        public override string Name => "Knowledge";
        public override string Description => "Learn from videos, books, blogs, scripts & files";
        public override string[] Triggers => new[] {
            "learn from", "read this", "study this", "learn this",
            "read book", "read blog", "watch video", "learn from video",
            "learn from book", "learn from blog", "learn from script",
            "ingest", "absorb", "memorize this", "teach me from",
            "read file", "read url", "learn url", "study file",
            "what have you learned from", "show knowledge",
            "knowledge base", "search knowledge", "find in knowledge"
        };

        public override bool CanHandle(string input)
        {
            string lower = input.ToLowerInvariant();
            return lower.Contains("learn from") || lower.Contains("read this") ||
                   lower.Contains("study") || lower.Contains("ingest") ||
                   lower.Contains("absorb") || lower.Contains("memorize") ||
                   lower.Contains("knowledge") || lower.Contains("read book") ||
                   lower.Contains("read blog") || lower.Contains("watch video") ||
                   lower.Contains("teach me from") || lower.Contains("read file") ||
                   lower.Contains("read url") || lower.Contains("learn url");
        }

        public override async Task<string> ExecuteAsync(string input, Memory memory)
        {
            string lower = input.ToLowerInvariant();

            if (lower.Contains("show knowledge") || lower.Contains("knowledge base") || 
                lower.Contains("what have you learned from"))
                return ShowKnowledge();

            if (lower.Contains("search knowledge") || lower.Contains("find in knowledge"))
                return SearchKnowledge(input);

            if (lower.Contains("clear knowledge") || lower.Contains("forget everything"))
                return ClearKnowledge();

            if (lower.Contains("learn from url") || lower.Contains("read url") || lower.Contains("learn url"))
                return await LearnFromUrl(input, memory);

            if (lower.Contains("learn from file") || lower.Contains("read file") || lower.Contains("study file"))
                return await LearnFromFile(input, memory);

            if (lower.Contains("learn from video") || lower.Contains("watch video"))
                return LearnFromVideo(input, memory);

            if (lower.Contains("learn from book") || lower.Contains("read book"))
                return await LearnFromBook(input, memory);

            if (lower.Contains("learn from blog") || lower.Contains("read blog"))
                return await LearnFromBlog(input, memory);

            if (lower.Contains("learn from script") || lower.Contains("read script"))
                return await LearnFromScript(input, memory);

            if (lower.Contains("learn from") || lower.Contains("teach me from"))
                return await LearnFromSource(input, memory);

            if (lower.Contains("read this") || lower.Contains("memorize this") || 
                lower.Contains("study this") || lower.Contains("ingest"))
                return await IngestText(input, memory);

            return ShowHelp();
        }

        /// <summary>
        /// Learn from a URL (blog, article, documentation, etc.)
        /// </summary>
        private async Task<string> LearnFromUrl(string input, Memory memory)
        {
            string url = ExtractUrl(input);
            if (string.IsNullOrEmpty(url))
                return "🔗 Give me a URL! Example: `learn from url https://example.com/article`";

            try
            {
                string content = await FetchUrlContent(url);
                if (string.IsNullOrEmpty(content))
                    return $"❌ Couldn't fetch content from {url}";

                // Store knowledge
                var entry = new KnowledgeEntry
                {
                    Source = url,
                    Type = "url",
                    Title = ExtractTitle(content) ?? url,
                    Content = SummarizeContent(content),
                    LearnedAt = DateTime.Now,
                    Tags = ExtractTags(content)
                };

                SaveKnowledge(entry);
                memory.StoreFact($"learned_from_{SanitizeKey(url)}", entry.Content);

                return $"📚 **Learned from URL!**\n\n" +
                       $"🔗 Source: {url}\n" +
                       $"📄 Title: {entry.Title}\n" +
                       $"📊 Content: {entry.Content.Length} characters\n" +
                       $"🏷️ Tags: {string.Join(", ", entry.Tags.Take(5))}\n\n" +
                       $"I've absorbed this knowledge! Ask me about it anytime.\n" +
                       $"Say `search knowledge [topic]` to find what I've learned.";
            }
            catch (Exception ex)
            {
                return $"❌ Error learning from URL: {ex.Message}\nCheck the URL and try again.";
            }
        }

        /// <summary>
        /// Learn from a local file
        /// </summary>
        private async Task<string> LearnFromFile(string input, Memory memory)
        {
            string filePath = ExtractFilePath(input);
            if (string.IsNullOrEmpty(filePath))
                return "📁 Give me a file path! Example: `learn from file C:\\path\\to\\file.txt`";

            if (!File.Exists(filePath))
                return $"❌ File not found: {filePath}";

            try
            {
                string content = await File.ReadAllTextAsync(filePath);
                string ext = Path.GetExtension(filePath).ToLower();

                var entry = new KnowledgeEntry
                {
                    Source = filePath,
                    Type = $"file:{ext}",
                    Title = Path.GetFileName(filePath),
                    Content = SummarizeContent(content),
                    LearnedAt = DateTime.Now,
                    Tags = ExtractTags(content)
                };

                // Add file-type specific tags
                if (IsCodeFile(ext)) entry.Tags.Add("code");
                if (IsDocumentFile(ext)) entry.Tags.Add("document");
                if (IsDataFile(ext)) entry.Tags.Add("data");

                SaveKnowledge(entry);
                memory.StoreFact($"learned_from_{SanitizeKey(filePath)}", entry.Content);

                return $"📚 **Learned from file!**\n\n" +
                       $"📁 File: `{filePath}`\n" +
                       $"📄 Type: {ext.ToUpper()} ({GetFileTypeDescription(ext)})\n" +
                       $"📊 Size: {content.Length:N0} characters\n" +
                       $"🏷️ Tags: {string.Join(", ", entry.Tags.Take(8))}\n\n" +
                       $"Knowledge absorbed! I can now reference this content.";
            }
            catch (Exception ex)
            {
                return $"❌ Error reading file: {ex.Message}";
            }
        }

        /// <summary>
        /// Learn from a video (extracts from subtitles/transcript/description)
        /// </summary>
        private string LearnFromVideo(string input, Memory memory)
        {
            string url = ExtractUrl(input);
            if (string.IsNullOrEmpty(url))
                return "🎥 Give me a video URL!\n\n" +
                    "Supported:\n" +
                    "• YouTube: `learn from video https://youtube.com/watch?v=...`\n" +
                    "• Any video with subtitles\n\n" +
                    "💡 **Tip:** For YouTube, I can learn from the description, " +
                    "comments, and any available transcript data.";

            // Parse video info
            string videoId = ExtractYouTubeId(url);
            if (!string.IsNullOrEmpty(videoId))
            {
                var entry = new KnowledgeEntry
                {
                    Source = url,
                    Type = "video:youtube",
                    Title = $"YouTube Video: {videoId}",
                    Content = $"Video reference stored. URL: {url}\nVideo ID: {videoId}\n" +
                              $"To fully learn from this video, paste the transcript or key points.",
                    LearnedAt = DateTime.Now,
                    Tags = new List<string> { "video", "youtube", videoId }
                };

                SaveKnowledge(entry);
                memory.StoreFact($"video_{videoId}", url);

                return $"🎥 **Video reference saved!**\n\n" +
                       $"🔗 URL: {url}\n" +
                       $"📌 Video ID: {videoId}\n\n" +
                       $"💡 **To get more from this video:**\n" +
                       "1. Copy the transcript from YouTube (click ... → Show transcript)\n" +
                       "2. Paste it: `memorize this: [paste transcript]`\n" +
                       "3. Or share key points: `learn from this: [summary]`\n\n" +
                       $"I've stored the reference so I can discuss it!";
            }

            // Generic video URL
            var genericEntry = new KnowledgeEntry
            {
                Source = url,
                Type = "video",
                Title = "Video Content",
                Content = url,
                LearnedAt = DateTime.Now,
                Tags = new List<string> { "video" }
            };
            SaveKnowledge(genericEntry);

            return $"🎥 **Video reference saved!**\n\nURL: {url}\nPaste the content/transcript for deeper learning!";
        }

        /// <summary>
        /// Learn from a book file (txt, epub, pdf text)
        /// </summary>
        private async Task<string> LearnFromBook(string input, Memory memory)
        {
            string filePath = ExtractFilePath(input);
            string url = ExtractUrl(input);

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                string content = await File.ReadAllTextAsync(filePath);
                var chapters = SplitIntoChapters(content);

                int totalChars = content.Length;
                int chapterCount = chapters.Count;

                var entry = new KnowledgeEntry
                {
                    Source = filePath,
                    Type = "book",
                    Title = Path.GetFileNameWithoutExtension(filePath),
                    Content = SummarizeContent(content, maxChars: 50000),
                    LearnedAt = DateTime.Now,
                    Tags = ExtractTags(content)
                };
                entry.Tags.Add("book");
                entry.Tags.Add($"chapters:{chapterCount}");

                SaveKnowledge(entry);
                memory.StoreFact($"book_{SanitizeKey(entry.Title)}", entry.Content);

                return $"📖 **Book learned!**\n\n" +
                       $"📚 Title: {entry.Title}\n" +
                       $"📄 Format: {Path.GetExtension(filePath).ToUpper()}\n" +
                       $"📊 Size: {totalChars:N0} characters\n" +
                       $"📑 Chapters detected: {chapterCount}\n" +
                       $"🏷️ Key topics: {string.Join(", ", entry.Tags.Where(t => !t.Contains(":")).Take(10))}\n\n" +
                       $"I've absorbed this book! Ask me about its contents.\n" +
                       $"Say `search knowledge [topic]` to find specific information.";
            }

            if (!string.IsNullOrEmpty(url))
                return await LearnFromUrl(input, memory);

            return "📖 Give me a book file!\n\n" +
                   "Supported formats: .txt, .epub (as text), .md\n" +
                   "Example: `learn from book C:\\Books\\moby-dick.txt`\n\n" +
                   "Or paste a URL: `learn from book https://gutenberg.org/...`";
        }

        /// <summary>
        /// Learn from a blog post
        /// </summary>
        private async Task<string> LearnFromBlog(string input, Memory memory)
        {
            string url = ExtractUrl(input);
            if (string.IsNullOrEmpty(url))
                return "📝 Give me a blog URL!\n" +
                    "Example: `learn from blog https://blog.example.com/article`";

            try
            {
                string content = await FetchUrlContent(url);
                string cleanContent = StripHtml(content);
                string title = ExtractTitle(content) ?? "Blog Post";

                var entry = new KnowledgeEntry
                {
                    Source = url,
                    Type = "blog",
                    Title = title,
                    Content = SummarizeContent(cleanContent, maxChars: 30000),
                    LearnedAt = DateTime.Now,
                    Tags = ExtractTags(cleanContent)
                };
                entry.Tags.Add("blog");
                entry.Tags.Add("article");

                SaveKnowledge(entry);
                memory.StoreFact($"blog_{SanitizeKey(title)}", entry.Content);

                return $"📝 **Blog post learned!**\n\n" +
                       $"📄 Title: {title}\n" +
                       $"🔗 URL: {url}\n" +
                       $"📊 Content: {cleanContent.Length:N0} characters\n" +
                       $"🏷️ Topics: {string.Join(", ", entry.Tags.Take(8))}\n\n" +
                       $"Blog knowledge absorbed! Ask me about it anytime.";
            }
            catch (Exception ex)
            {
                return $"❌ Couldn't fetch blog: {ex.Message}";
            }
        }

        /// <summary>
        /// Learn from a code script
        /// </summary>
        private async Task<string> LearnFromScript(string input, Memory memory)
        {
            string filePath = ExtractFilePath(input);
            string url = ExtractUrl(input);

            string content;
            string source;
            string lang = "unknown";

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                content = await File.ReadAllTextAsync(filePath);
                source = filePath;
                lang = DetectLanguageFromExtension(Path.GetExtension(filePath));
            }
            else if (!string.IsNullOrEmpty(url))
            {
                content = await FetchUrlContent(url);
                source = url;
                lang = DetectLanguageFromUrl(url);
            }
            else
            {
                // Maybe they pasted the script in the message
                string pastedCode = ExtractPastedCode(input);
                if (!string.IsNullOrEmpty(pastedCode))
                {
                    content = pastedCode;
                    source = "pasted";
                    lang = DetectLanguageFromContent(pastedCode);
                }
                else
                    return "📜 Give me a script!\n\n" +
                        "• File: `learn from script C:\\scripts\\app.py`\n" +
                        "• URL: `learn from script https://github.com/...`\n" +
                        "• Or paste it: `memorize this: [your code]`";
            }

            // Analyze the script
            var analysis = AnalyzeCode(content, lang);

            var entry = new KnowledgeEntry
            {
                Source = source,
                Type = $"script:{lang}",
                Title = Path.GetFileName(source) ?? $"Script ({lang})",
                Content = content,
                LearnedAt = DateTime.Now,
                Tags = new List<string> { "code", "script", lang }
                    .Concat(analysis.Functions.Select(f => $"fn:{f}"))
                    .Concat(analysis.Imports.Select(i => $"import:{i}"))
                    .ToList()
            };

            SaveKnowledge(entry);
            memory.StoreFact($"script_{SanitizeKey(entry.Title)}", content);

            return $"📜 **Script learned!**\n\n" +
                   $"📄 File: {entry.Title}\n" +
                   $"💻 Language: {lang}\n" +
                   $"📊 Lines: {content.Split('\n').Length}\n" +
                   $"🔧 Functions: {string.Join(", ", analysis.Functions.Take(10))}\n" +
                   $"📦 Imports: {string.Join(", ", analysis.Imports.Take(10))}\n" +
                   $"🎯 Purpose: {analysis.Purpose}\n\n" +
                   $"Script knowledge absorbed! I understand what this code does.";
        }

        /// <summary>
        /// Learn from arbitrary source (auto-detect type)
        /// </summary>
        private async Task<string> LearnFromSource(string input, Memory memory)
        {
            string url = ExtractUrl(input);
            string filePath = ExtractFilePath(input);

            if (!string.IsNullOrEmpty(url))
            {
                if (url.Contains("youtube.com") || url.Contains("youtu.be"))
                    return LearnFromVideo(input, memory);
                if (IsBlogUrl(url))
                    return await LearnFromBlog(input, memory);
                return await LearnFromUrl(input, memory);
            }

            if (!string.IsNullOrEmpty(filePath))
            {
                string ext = Path.GetExtension(filePath).ToLower();
                if (IsCodeFile(ext))
                    return await LearnFromScript(input, memory);
                if (IsBookFile(ext))
                    return await LearnFromBook(input, memory);
                return await LearnFromFile(input, memory);
            }

            return await IngestText(input, memory);
        }

        /// <summary>
        /// Ingest raw text pasted by user
        /// </summary>
        private Task<string> IngestText(string input, Memory memory)
        {
            string text = ExtractPastedText(input);
            if (string.IsNullOrEmpty(text))
                return Task.FromResult(
                    "📝 Paste some text and I'll learn from it!\n\n" +
                    "Examples:\n" +
                    "• `memorize this: [paste text]`\n" +
                    "• `study this: [paste notes]`\n" +
                    "• `learn from this: [paste content]`\n\n" +
                    "Or give me a URL/file path and I'll read it directly.");

            var entry = new KnowledgeEntry
            {
                Source = "pasted",
                Type = "text",
                Title = "Pasted Content",
                Content = SummarizeContent(text),
                LearnedAt = DateTime.Now,
                Tags = ExtractTags(text)
            };

            SaveKnowledge(entry);

            return Task.FromResult(
                $"📝 **Text learned!**\n\n" +
                $"📊 Characters: {text.Length:N0}\n" +
                $"📄 Words: {text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length:N0}\n" +
                $"🏷️ Topics: {string.Join(", ", entry.Tags.Take(8))}\n\n" +
                $"Knowledge absorbed! Ask me about this content anytime.");
        }

        /// <summary>
        /// Show all learned knowledge
        /// </summary>
        private string ShowKnowledge()
        {
            var entries = LoadAllKnowledge();
            if (!entries.Any())
                return "📚 My knowledge base is empty! Teach me something:\n\n" +
                    "• `learn from url [url]` — Learn from web content\n" +
                    "• `learn from file [path]` — Learn from a file\n" +
                    "• `learn from video [url]` — Learn from video\n" +
                    "• `learn from book [path]` — Learn from a book\n" +
                    "• `learn from blog [url]` — Learn from a blog\n" +
                    "• `memorize this: [text]` — Learn from pasted text";

            var sb = new StringBuilder();
            sb.AppendLine($"📚 **Knowledge Base: {entries.Count} entries**\n");

            var grouped = entries.GroupBy(e => e.Type.Split(':')[0]);
            foreach (var group in grouped)
            {
                string icon = group.Key switch
                {
                    "url" => "🔗", "video" => "🎥", "book" => "📖",
                    "blog" => "📝", "script" => "📜", "file" => "📁",
                    "text" => "📄", _ => "📚"
                };
                sb.AppendLine($"{icon} **{group.Key.ToUpper()}** ({group.Count()}):");
                foreach (var entry in group.Take(5))
                {
                    sb.AppendLine($"  • {entry.Title} — _{entry.LearnedAt:MMM dd, yyyy}_");
                }
                if (group.Count() > 5)
                    sb.AppendLine($"  ... and {group.Count() - 5} more");
                sb.AppendLine();
            }

            sb.AppendLine("Say `search knowledge [topic]` to find specific information.");
            return sb.ToString();
        }

        /// <summary>
        /// Search knowledge base
        /// </summary>
        private string SearchKnowledge(string input)
        {
            string query = Regex.Replace(input, @"search knowledge\s*|find in knowledge\s*", "", RegexOptions.IgnoreCase).Trim();
            if (string.IsNullOrEmpty(query))
                return "🔍 What are you looking for? Example: `search knowledge machine learning`";

            var entries = LoadAllKnowledge();
            var results = entries
                .Where(e => e.Content.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           e.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           e.Tags.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(e => e.LearnedAt)
                .Take(5)
                .ToList();

            if (!results.Any())
                return $"🔍 No results for '{query}'. Try a different term or learn more content first!";

            var sb = new StringBuilder();
            sb.AppendLine($"🔍 **Found {results.Count} result(s) for '{query}':**\n");
            foreach (var result in results)
            {
                string preview = result.Content.Length > 200
                    ? result.Content.Substring(0, 200) + "..."
                    : result.Content;
                sb.AppendLine($"📄 **{result.Title}** ({result.Type})");
                sb.AppendLine($"   {preview}");
                sb.AppendLine($"   _Source: {result.Source} | Learned: {result.LearnedAt:MMM dd}_");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Clear all knowledge
        /// </summary>
        private string ClearKnowledge()
        {
            if (File.Exists(KnowledgeDb))
            {
                File.Delete(KnowledgeDb);
                return "🗑️ Knowledge base cleared! I've forgotten everything. Teach me again!";
            }
            return "📚 Knowledge base was already empty.";
        }

        private string ShowHelp()
        {
            return "📚 **Knowledge Learner — I can learn from anything!**\n\n" +
                "**Sources I can learn from:**\n" +
                "• 🔗 `learn from url [url]` — Any webpage\n" +
                "• 📝 `learn from blog [url]` — Blog posts\n" +
                "• 🎥 `learn from video [url]` — YouTube videos\n" +
                "• 📖 `learn from book [path]` — Book files (.txt, .md)\n" +
                "• 📜 `learn from script [path]` — Code files\n" +
                "• 📁 `learn from file [path]` — Any text file\n" +
                "• 📝 `memorize this: [paste]` — Paste content directly\n\n" +
                "**Accessing knowledge:**\n" +
                "• `show knowledge` — See what I've learned\n" +
                "• `search knowledge [topic]` — Find specific info\n" +
                "• `clear knowledge` — Reset knowledge base\n\n" +
                "💡 I support ALL file types that contain text!";
        }

        #region Helper Methods

        private string ExtractUrl(string input)
        {
            var match = Regex.Match(input, @"https?://[^\s]+");
            return match.Success ? match.Value.TrimEnd('.', ',', ')') : "";
        }

        private string ExtractFilePath(string input)
        {
            // Windows paths
            var match = Regex.Match(input, @"[A-Z]:\\[^\s""]+");
            if (match.Success) return match.Value;

            // Unix paths
            match = Regex.Match(input, @"/[^\s""]+");
            if (match.Success) return match.Value;

            // After "file" keyword
            match = Regex.Match(input, @"(?:file|path)\s*[:=]?\s*[""']?([^\s""']+)", RegexOptions.IgnoreCase);
            if (match.Success && File.Exists(match.Groups[1].Value))
                return match.Groups[1].Value;

            return "";
        }

        private string ExtractYouTubeId(string url)
        {
            var match = Regex.Match(url, @"(?:youtube\.com/watch\?v=|youtu\.be/)([a-zA-Z0-9_-]+)");
            return match.Success ? match.Groups[1].Value : "";
        }

        private string ExtractPastedText(string input)
        {
            // After "memorize this:", "study this:", etc.
            var match = Regex.Match(input, @"(?:memorize this|study this|learn from this|ingest)\s*[:]\s*(.+)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        private string ExtractPastedCode(string input)
        {
            // Look for code blocks
            var match = Regex.Match(input, @"```[\s\S]*?```");
            if (match.Success)
                return match.Value.Trim('`').Trim();

            return ExtractPastedText(input);
        }

        private async Task<string> FetchUrlContent(string url)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Rama/1.0 Knowledge Learner");
            string html = await _httpClient.GetStringAsync(url);
            return StripHtml(html);
        }

        private string StripHtml(string html)
        {
            // Remove script and style blocks
            html = Regex.Replace(html, @"<script[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<style[\s\S]*?</style>", "", RegexOptions.IgnoreCase);
            // Remove HTML tags
            html = Regex.Replace(html, @"<[^>]+>", " ");
            // Decode entities
            html = System.Net.WebUtility.HtmlDecode(html);
            // Clean whitespace
            html = Regex.Replace(html, @"\s+", " ").Trim();
            return html;
        }

        private string ExtractTitle(string content)
        {
            var match = Regex.Match(content, @"<title>(.*?)</title>", RegexOptions.IgnoreCase);
            return match.Success ? System.Net.WebUtility.HtmlDecode(match.Groups[1].Value.Trim()) : null;
        }

        private List<string> ExtractTags(string content)
        {
            var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Extract from meta keywords
            var metaMatch = Regex.Match(content, @"<meta[^>]*name=[""']keywords[""'][^>]*content=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
            if (metaMatch.Success)
            {
                foreach (var tag in metaMatch.Groups[1].Value.Split(','))
                    tags.Add(tag.Trim());
            }

            // Extract common programming terms
            string[] codeTerms = { "function", "class", "import", "def", "const", "var", "let", "public", "private", "async", "await" };
            foreach (var term in codeTerms)
            {
                if (content.Contains(term, StringComparison.OrdinalIgnoreCase))
                    tags.Add(term);
            }

            // Extract capitalized words as potential topics
            var capWords = Regex.Matches(content, @"\b[A-Z][a-z]{3,}\b");
            foreach (Match m in capWords)
            {
                if (tags.Count < 20)
                    tags.Add(m.Value);
            }

            return tags.ToList();
        }

        private string SummarizeContent(string content, int maxChars = 100000)
        {
            if (content.Length <= maxChars)
                return content;

            // Take beginning and end
            int half = maxChars / 2;
            return content.Substring(0, half) + "\n\n[... content truncated ...]\n\n" +
                   content.Substring(content.Length - half);
        }

        private List<string> SplitIntoChapters(string content)
        {
            var chapters = new List<string>();
            var matches = Regex.Matches(content, @"(?:^|\n)(?:chapter|section|part)\s+\d+",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            if (matches.Count > 1)
            {
                for (int i = 0; i < matches.Count - 1; i++)
                {
                    int start = matches[i].Index;
                    int end = matches[i + 1].Index;
                    chapters.Add(content.Substring(start, end - start));
                }
                chapters.Add(content.Substring(matches[^1].Index));
            }
            else
            {
                // Split by approximate size
                int chunkSize = 10000;
                for (int i = 0; i < content.Length; i += chunkSize)
                {
                    int len = Math.Min(chunkSize, content.Length - i);
                    chapters.Add(content.Substring(i, len));
                }
            }

            return chapters;
        }

        private CodeAnalysis AnalyzeCode(string code, string lang)
        {
            var analysis = new CodeAnalysis();

            // Extract function/method names
            var funcMatches = Regex.Matches(code, @"(?:def|function|func|fn|public|private|static)\s+(\w+)");
            analysis.Functions = funcMatches.Select(m => m.Groups[1].Value).Distinct().ToList();

            // Extract imports
            var importMatches = Regex.Matches(code, @"(?:import|from|require|use)\s+(\w+)");
            analysis.Imports = importMatches.Select(m => m.Groups[1].Value).Distinct().ToList();

            // Determine purpose
            string lower = code.ToLower();
            if (lower.Contains("class")) analysis.Purpose = "Class/Object definition";
            else if (lower.Contains("def ") || lower.Contains("function ")) analysis.Purpose = "Function library";
            else if (lower.Contains("api") || lower.Contains("route")) analysis.Purpose = "API/Server";
            else if (lower.Contains("test")) analysis.Purpose = "Testing";
            else if (lower.Contains("database") || lower.Contains("sql")) analysis.Purpose = "Database operations";
            else if (lower.Contains("html") || lower.Contains("<div")) analysis.Purpose = "Web/HTML";
            else analysis.Purpose = "General script";

            return analysis;
        }

        private string DetectLanguageFromExtension(string ext) => ext.ToLower() switch
        {
            ".py" => "python", ".js" => "javascript", ".ts" => "typescript",
            ".java" => "java", ".cs" => "csharp", ".cpp" or ".cc" => "cpp",
            ".c" => "c", ".go" => "go", ".rs" => "rust", ".rb" => "ruby",
            ".php" => "php", ".swift" => "swift", ".kt" => "kotlin",
            ".sh" or ".bash" => "bash", ".ps1" => "powershell",
            ".r" => "r", ".lua" => "lua", ".pl" => "perl",
            ".sql" => "sql", ".html" => "html", ".css" => "css",
            _ => "unknown"
        };

        private string DetectLanguageFromUrl(string url)
        {
            if (url.Contains(".py")) return "python";
            if (url.Contains(".js")) return "javascript";
            if (url.Contains(".ts")) return "typescript";
            if (url.Contains(".java")) return "java";
            if (url.Contains(".rs")) return "rust";
            if (url.Contains(".go")) return "go";
            return "unknown";
        }

        private string DetectLanguageFromContent(string code)
        {
            if (code.Contains("def ") && code.Contains("import ")) return "python";
            if (code.Contains("function ") || code.Contains("const ") || code.Contains("=>")) return "javascript";
            if (code.Contains("func ") && code.Contains("package ")) return "go";
            if (code.Contains("fn ") && code.Contains("let ")) return "rust";
            if (code.Contains("public class")) return "java";
            if (code.Contains("#include")) return "c/cpp";
            if (code.Contains("SELECT") && code.Contains("FROM")) return "sql";
            return "unknown";
        }

        private bool IsCodeFile(string ext) => new[] {
            ".py", ".js", ".ts", ".java", ".cs", ".cpp", ".c", ".go", ".rs",
            ".rb", ".php", ".swift", ".kt", ".sh", ".ps1", ".lua", ".r"
        }.Contains(ext.ToLower());

        private bool IsDocumentFile(string ext) => new[] {
            ".txt", ".md", ".doc", ".docx", ".rtf", ".pdf", ".odt"
        }.Contains(ext.ToLower());

        private bool IsDataFile(string ext) => new[] {
            ".json", ".xml", ".csv", ".yaml", ".yml", ".toml"
        }.Contains(ext.ToLower());

        private bool IsBookFile(string ext) => new[] {
            ".txt", ".md", ".epub"
        }.Contains(ext.ToLower());

        private bool IsBlogUrl(string url) =>
            url.Contains("blog") || url.Contains("medium.com") ||
            url.Contains("dev.to") || url.Contains("hashnode") ||
            url.Contains("wordpress") || url.Contains("substack");

        private string GetFileTypeDescription(string ext) => ext.ToLower() switch
        {
            ".py" => "Python script", ".js" => "JavaScript", ".ts" => "TypeScript",
            ".java" => "Java", ".cs" => "C#", ".cpp" => "C++",
            ".txt" => "Text file", ".md" => "Markdown", ".json" => "JSON data",
            ".csv" => "CSV data", ".sql" => "SQL script", ".html" => "HTML",
            ".css" => "CSS", ".sh" => "Shell script", ".xml" => "XML data",
            _ => "Text file"
        };

        private string SanitizeKey(string key) =>
            Regex.Replace(key.ToLower(), @"[^a-z0-9]", "_").Trim('_');

        #endregion

        #region Knowledge Storage

        private void SaveKnowledge(KnowledgeEntry entry)
        {
            Directory.CreateDirectory(KnowledgeDir);
            var entries = LoadAllKnowledge();

            // Check for duplicate
            var existing = entries.FirstOrDefault(e => e.Source == entry.Source);
            if (existing != null)
            {
                existing.Content = entry.Content;
                existing.LearnedAt = DateTime.Now;
                existing.Tags = entry.Tags;
            }
            else
            {
                entries.Add(entry);
            }

            File.WriteAllText(KnowledgeDb, JsonConvert.SerializeObject(entries, Formatting.Indented));
        }

        private List<KnowledgeEntry> LoadAllKnowledge()
        {
            try
            {
                if (!File.Exists(KnowledgeDb)) return new List<KnowledgeEntry>();
                string json = File.ReadAllText(KnowledgeDb);
                return JsonConvert.DeserializeObject<List<KnowledgeEntry>>(json) ?? new List<KnowledgeEntry>();
            }
            catch { return new List<KnowledgeEntry>(); }
        }

        public override void OnLoad()
        {
            Directory.CreateDirectory(KnowledgeDir);
        }

        #endregion
    }

    #region Data Models

    public class KnowledgeEntry
    {
        public string Source { get; set; } = "";
        public string Type { get; set; } = "";
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime LearnedAt { get; set; }
        [JsonIgnore]
        public DateTime Now => DateTime.Now;
        public List<string> Tags { get; set; } = new();
    }

    public class CodeAnalysis
    {
        public List<string> Functions { get; set; } = new();
        public List<string> Imports { get; set; } = new();
        public string Purpose { get; set; } = "";
    }

    #endregion
}

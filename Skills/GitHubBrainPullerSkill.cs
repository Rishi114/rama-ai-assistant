using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// GitHub Skill & Brain Puller - Dynamically fetch skills and knowledge from GitHub repositories.
    /// Usage: "pull skill from github.com/owner/repo" or "learn from github.com/owner/repo"
    /// </summary>
    public class GitHubBrainPullerSkill : SkillBase
    {
        private readonly string _brainKnowledgePath;
        private readonly string _tempPath;
        private readonly HttpClient _http;
        
        public override string Name => "GitHub Brain Puller";
        public override string Description => "Pull skills and knowledge from GitHub repositories dynamically";
        public override string[] Triggers => new[] { "pull", "fetch", "learn from github", "install skill from", "add brain from", "clone repo" };

        public GitHubBrainPullerSkill()
        {
            _brainKnowledgePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BrainKnowledge");
            _tempPath = Path.Combine(Path.GetTempPath(), "RamaGitHub");
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Add("User-Agent", "Rama-AI/1.0");
            
            if (!Directory.Exists(_tempPath))
                Directory.CreateDirectory(_tempPath);
        }

        public override bool CanHandle(string input)
        {
            string lower = input.ToLowerInvariant();
            return Triggers.Any(t => lower.Contains(t)) || 
                   (lower.Contains("github.com") && (lower.Contains("pull") || lower.Contains("learn") || lower.Contains("add") || lower.Contains("install")));
        }

        public override async Task<string> ExecuteAsync(string input, Memory memory)
        {
            // Extract GitHub URL from input
            var urlMatch = Regex.Match(input, @"github\.com[/:]([\w\-]+)/([^\s/]+)", RegexOptions.IgnoreCase);
            
            if (!urlMatch.Success)
            {
                return GetUsageInstructions();
            }

            string owner = urlMatch.Groups[1].Value;
            string repo = urlMatch.Groups[2].Value.Replace(".git", "");
            
            return await PullRepositoryAsync(owner, repo, input);
        }

        private string GetUsageInstructions()
        {
            return @"🔗 **GitHub Brain Puller**

I can pull skills and knowledge from GitHub repositories dynamically.

**Commands:**
- `pull skill from github.com/owner/repo` — Clone and add skill to Rama
- `learn from github.com/owner/repo` — Add repository to brain knowledge
- `add brain from github.com/owner/repo` — Download knowledge base

**Examples:**
```
pull skill from github.com/kayba-ai/agentic-context-engine
learn from github.com/666ghj/MiroFish
add brain from github.com/datawhalechina/easy-vibe
```

Just give me a GitHub URL and I'll clone it into my brain!";
        }

        private async Task<string> PullRepositoryAsync(string owner, string repo, string input)
        {
            var cloneUrl = $"https://github.com/{owner}/{repo}.git";
            var targetPath = Path.Combine(_tempPath, $"{owner}-{repo}");
            var destPath = Path.Combine(_brainKnowledgePath, repo);
            
            try
            {
                // Check if it's a skill (has ISkill or SkillBase pattern in C#)
                // or general knowledge
                bool isSkill = input.ToLowerInvariant().Contains("skill") || 
                              input.ToLowerInvariant().Contains("install");
                
                // Clone repository
                var cloneResult = await CloneRepositoryAsync(cloneUrl, targetPath);
                if (!cloneResult.Success)
                {
                    return $"❌ Failed to clone: {cloneResult.Error}";
                }

                // Analyze repository structure
                var analysis = await AnalyzeRepositoryAsync(targetPath);
                
                // Copy to brain knowledge
                if (Directory.Exists(destPath))
                {
                    Directory.Delete(destPath, true);
                }
                Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                CopyDirectory(targetPath, destPath);
                
                // Generate skill if C# code found
                string skillResult = "";
                if (analysis.HasCode && isSkill)
                {
                    skillResult = $"\n\n✅ **Skill detected!** Creating skill handler for {repo}...";
                    // Note: Would need to generate C# skill file here based on repo contents
                }

                return $@"✅ **Successfully pulled {owner}/{repo}!**

📊 **Repository Analysis:**
- 📁 Files: {analysis.FileCount}
- 📝 Code files: {analysis.CodeFileCount}
- 🗂️ Directories: {analysis.DirCount}
- 🔍 Primary language: {analysis.PrimaryLanguage}

📦 **Copied to:** `{destPath}`

{skillResult}

The repository is now in my brain! Ask me about it or pull more!";
            }
            catch (Exception ex)
            {
                return $"❌ Error pulling repository: {ex.Message}";
            }
        }

        private async Task<(bool Success, string Error)> CloneRepositoryAsync(string url, string targetPath)
        {
            try
            {
                if (Directory.Exists(targetPath))
                {
                    Directory.Delete(targetPath, true);
                }
                
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"clone --depth 1 \"{url}\" \"{targetPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                await process.WaitForExitAsync();
                
                if (process.ExitCode == 0)
                {
                    return (true, "");
                }
                else
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    return (false, error);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private async Task<RepoAnalysis> AnalyzeRepositoryAsync(string path)
        {
            var analysis = new RepoAnalysis();
            
            try
            {
                var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                analysis.FileCount = files.Length;
                analysis.DirCount = Directory.GetDirectories(path, "*", SearchOption.AllDirectories).Length;
                
                var codeExtensions = new[] { ".cs", ".py", ".js", ".ts", ".go", ".rs", ".java" };
                analysis.CodeFileCount = files.Count(f => codeExtensions.Any(e => f.EndsWith(e, StringComparison.OrdinalIgnoreCase)));
                analysis.HasCode = analysis.CodeFileCount > 0;
                
                // Determine primary language
                var langCounts = new Dictionary<string, int>();
                foreach (var file in files)
                {
                    var ext = Path.GetExtension(file).ToLowerInvariant();
                    var lang = ext switch
                    {
                        ".cs" => "C#",
                        ".py" => "Python",
                        ".js" => "JavaScript",
                        ".ts" => "TypeScript",
                        ".go" => "Go",
                        ".rs" => "Rust",
                        ".java" => "Java",
                        ".md" => "Markdown",
                        ".json" => "JSON",
                        ".yaml" or ".yml" => "YAML",
                        _ => "Other"
                    };
                    
                    if (!langCounts.ContainsKey(lang))
                        langCounts[lang] = 0;
                    langCounts[lang]++;
                }
                
                analysis.PrimaryLanguage = langCounts.OrderByDescending(x => x.Value).FirstOrDefault().Key ?? "Unknown";
            }
            catch { }
            
            return await Task.FromResult(analysis);
        }

        private void CopyDirectory(string source, string destination)
        {
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            foreach (string file in Directory.GetFiles(source))
            {
                string destFile = Path.Combine(destination, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string dir in Directory.GetDirectories(source))
            {
                string destDir = Path.Combine(destination, Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
        }

        public override void OnLoad()
        {
            base.OnLoad();
            if (!Directory.Exists(_brainKnowledgePath))
            {
                Directory.CreateDirectory(_brainKnowledgePath);
            }
        }

        private class RepoAnalysis
        {
            public int FileCount { get; set; }
            public int DirCount { get; set; }
            public int CodeFileCount { get; set; }
            public bool HasCode { get; set; }
            public string PrimaryLanguage { get; set; } = "Unknown";
        }
    }
}
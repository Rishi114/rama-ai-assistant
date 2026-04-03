using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Easy Skill Loader — Add skills by editing a JSON file. No coding required!
    /// 
    /// Drop a skills.json file in %APPDATA%\Rama\Skills\ and Rama loads them automatically.
    /// 
    /// Example skills.json:
    /// {
    ///   "skills": [
    ///     {
    ///       "name": "Greeter",
    ///       "description": "Says hello in different languages",
    ///       "triggers": ["greet", "say hello"],
    ///       "responses": [
    ///         "Hello! 👋",
    ///         "Hola! 🇪🇸",
    ///         "Bonjour! 🇫🇷"
    ///       ]
    ///     }
    ///   ]
    /// }
    /// </summary>
    public class EasySkillLoader : SkillBase
    {
        private string ConfigPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rama", "Skills", "skills.json");

        private string TemplatesDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rama", "Skills", "Templates");

        private List<JsonSkill> _loadedSkills = new();

        public override string Name => "Easy Skills";
        public override string Description => "Add skills by editing JSON — no coding needed";
        public override string[] Triggers => new[] {
            "easy skill", "add easy skill", "create quick skill",
            "load skills", "reload skills", "show easy skills",
            "skill config", "edit skills", "skills json"
        };

        public override bool CanHandle(string input)
        {
            string lower = input.ToLowerInvariant();
            return lower.Contains("easy skill") || lower.Contains("quick skill") ||
                   lower.Contains("load skills") || lower.Contains("reload skills") ||
                   lower.Contains("show easy skills") || lower.Contains("skill config") ||
                   lower.Contains("edit skills") || lower.Contains("skills json");
        }

        public override Task<string> ExecuteAsync(string input, Memory memory)
        {
            string lower = input.ToLowerInvariant();

            if (lower.Contains("reload") || lower.Contains("load skills"))
            {
                LoadSkills();
                return Task.FromResult($"✅ Reloaded! {_loadedSkills.Count} easy skills loaded.");
            }

            if (lower.Contains("show easy skills") || lower.Contains("list easy"))
                return Task.FromResult(ShowLoadedSkills());

            if (lower.Contains("create quick skill") || lower.Contains("add easy skill"))
                return Task.FromResult(CreateQuickSkill(input));

            if (lower.Contains("skill config") || lower.Contains("edit skills") || lower.Contains("skills json"))
                return Task.FromResult(ShowConfig());

            return Task.FromResult(GetHelp());
        }

        /// <summary>
        /// Try to handle input with JSON-defined skills.
        /// Called by SkillManager before falling back.
        /// </summary>
        public string? TryHandle(string input)
        {
            string lower = input.ToLowerInvariant();

            foreach (var skill in _loadedSkills)
            {
                if (skill.Triggers.Any(t => lower.Contains(t.ToLowerInvariant())))
                {
                    // Pick a random response
                    if (skill.Responses.Count > 0)
                    {
                        var rand = new Random();
                        string response = skill.Responses[rand.Next(skill.Responses.Count)];

                        // Replace placeholders
                        response = response.Replace("{input}", input);
                        response = response.Replace("{time}", DateTime.Now.ToString("HH:mm"));
                        response = response.Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd"));
                        response = response.Replace("{day}", DateTime.Now.DayOfWeek.ToString());

                        return response;
                    }
                }
            }

            return null;
        }

        public void LoadSkills()
        {
            _loadedSkills.Clear();

            try
            {
                // Load from JSON config
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    var config = JsonConvert.DeserializeObject<EasySkillsConfig>(json);
                    if (config?.Skills != null)
                        _loadedSkills.AddRange(config.Skills);
                }

                // Load from individual JSON files in Skills folder
                string skillsDir = Path.GetDirectoryName(ConfigPath)!;
                if (Directory.Exists(skillsDir))
                {
                    foreach (var file in Directory.GetFiles(skillsDir, "*.skill.json"))
                    {
                        try
                        {
                            string json = File.ReadAllText(file);
                            var skill = JsonConvert.DeserializeObject<JsonSkill>(json);
                            if (skill != null && !string.IsNullOrEmpty(skill.Name))
                                _loadedSkills.Add(skill);
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        private string ShowLoadedSkills()
        {
            if (_loadedSkills.Count == 0)
                return "📂 No easy skills loaded yet!\n\n" +
                    "Say `skill config` to see how to add them.";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"⚡ **Easy Skills: {_loadedSkills.Count} loaded**\n");

            foreach (var skill in _loadedSkills)
            {
                sb.AppendLine($"🔹 **{skill.Name}** — {skill.Description}");
                sb.AppendLine($"   Triggers: {string.Join(", ", skill.Triggers)}");
                sb.AppendLine($"   Responses: {skill.Responses.Count}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string CreateQuickSkill(string input)
        {
            // Parse: "create quick skill [name] that says [response]"
            var match = Regex.Match(input,
                @"(?:create quick skill|add easy skill)\s+(\w+)\s+(?:that says|saying)\s+(.+)",
                RegexOptions.IgnoreCase);

            if (!match.Success)
                return "💡 **Quick Skill Creator:**\n\n" +
                    "Example: `create quick skill hello that says Hey there!`\n\n" +
                    "Or edit the JSON file directly: `skill config`";

            string name = match.Groups[1].Value;
            string response = match.Groups[2].Value;

            var skill = new JsonSkill
            {
                Name = name,
                Description = $"Says: {response}",
                Triggers = new List<string> { name.ToLower(), $"say {name.ToLower()}" },
                Responses = new List<string> { response }
            };

            _loadedSkills.Add(skill);
            SaveSkill(skill);

            return $"✨ **Quick skill created!**\n\n" +
                   $"Name: **{name}**\n" +
                   $"Trigger: `say {name.ToLower()}`\n" +
                   $"Response: \"{response}\"\n\n" +
                   "Try it now! Or say `show easy skills` to see all.";
        }

        private void SaveSkill(JsonSkill skill)
        {
            try
            {
                string skillsDir = Path.GetDirectoryName(ConfigPath)!;
                Directory.CreateDirectory(skillsDir);
                string filePath = Path.Combine(skillsDir, $"{skill.Name.ToLower()}.skill.json");
                File.WriteAllText(filePath, JsonConvert.SerializeObject(skill, Formatting.Indented));
            }
            catch { }
        }

        private string ShowConfig()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);

                if (!File.Exists(ConfigPath))
                {
                    // Create default config
                    var defaultConfig = new EasySkillsConfig
                    {
                        Skills = new List<JsonSkill>
                        {
                            new()
                            {
                                Name = "Motivator",
                                Description = "Gives motivational quotes",
                                Triggers = new List<string> { "motivate me", "inspire me", "pep talk" },
                                Responses = new List<string>
                                {
                                    "You've got this! 💪",
                                    "Believe in yourself — I believe in you! ✨",
                                    "Every expert was once a beginner. Keep going! 🚀",
                                    "The only way to do great work is to love what you do! ❤️"
                                }
                            },
                            new()
                            {
                                Name = "TimeChecker",
                                Description = "Tells the current time",
                                Triggers = new List<string> { "what time", "current time", "time please" },
                                Responses = new List<string>
                                {
                                    "It's {time} on {day}. ⏰",
                                    "The time is {time}. Don't waste it! 😏"
                                }
                            },
                            new()
                            {
                                Name = "Complimenter",
                                Description = "Gives compliments",
                                Triggers = new List<string> { "compliment me", "say something nice", "boost" },
                                Responses = new List<string>
                                {
                                    "You're absolutely amazing! 🌟",
                                    "Your taste in AI assistants is impeccable. 😎",
                                    "You look like someone who makes great decisions! 👑",
                                    "10/10 would recommend you as a human. 🏆"
                                }
                            }
                        }
                    };

                    File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
                }

                string configContent = File.ReadAllText(ConfigPath);
                return $"📄 **Skills Config File:**\n\n" +
                       $"📁 Location: `{ConfigPath}`\n\n" +
                       $"```json\n{configContent}\n```\n\n" +
                       "**How to add skills:**\n" +
                       "1. Open the file in any text editor\n" +
                       "2. Add a new skill object to the `skills` array\n" +
                       "3. Save the file\n" +
                       "4. Say `reload skills` in Rama\n\n" +
                       "**Each skill needs:**\n" +
                       "• `name` — Display name\n" +
                       "• `description` — What it does\n" +
                       "• `triggers` — Words that activate it\n" +
                       "• `responses` — What Rama says\n\n" +
                       "**Placeholders you can use:**\n" +
                       "• `{input}` — What the user said\n" +
                       "• `{time}` — Current time\n" +
                       "• `{date}` — Current date\n" +
                       "• `{day}` — Day of week";
            }
            catch (Exception ex)
            {
                return $"❌ Error: {ex.Message}";
            }
        }

        private string GetHelp()
        {
            return "⚡ **Easy Skills — Add Skills Without Coding!**\n\n" +
                "**Two ways to add skills:**\n\n" +
                "📝 **Quick (from chat):**\n" +
                "• `create quick skill hello that says Hey! 👋`\n\n" +
                "📄 **JSON file (more options):**\n" +
                "• `skill config` — View/edit the JSON file\n" +
                "• `reload skills` — Refresh after editing\n" +
                "• `show easy skills` — See loaded skills\n\n" +
                "The JSON file is at:\n" +
                $"`{ConfigPath}`";
        }

        public override void OnLoad()
        {
            LoadSkills();
        }
    }

    #region Models

    public class EasySkillsConfig
    {
        [JsonProperty("skills")]
        public List<JsonSkill> Skills { get; set; } = new();
    }

    public class JsonSkill
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("description")]
        public string Description { get; set; } = "";

        [JsonProperty("triggers")]
        public List<string> Triggers { get; set; } = new();

        [JsonProperty("responses")]
        public List<string> Responses { get; set; } = new();

        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;
    }

    #endregion
}

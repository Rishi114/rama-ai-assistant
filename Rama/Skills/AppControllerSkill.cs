using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// App Controller Skill — Control any software, learn features, automate tasks.
    /// Rama can access EVERY application and learn to use it by watching you.
    /// </summary>
    public class AppControllerSkill : SkillBase
    {
        private readonly AppController _controller;
        private readonly ProfileManager _profile;

        public AppControllerSkill(AppController controller, ProfileManager profile)
        {
            _controller = controller;
            _profile = profile;
        }

        public override string Name => "App Controller";
        public override string Description => "Control any software, learn features, automate tasks";
        public override string[] Triggers => new[] {
            "open", "launch", "start", "run", "close", "quit",
            "switch to", "focus", "show running", "running apps",
            "learn app", "learn how to", "teach me how",
            "do in", "perform in", "automate",
            "find app", "where is", "software list",
            "app list", "installed apps"
        };

        public override bool CanHandle(string input)
        {
            string lower = input.ToLowerInvariant();
            return lower.Contains("open ") || lower.Contains("launch ") ||
                   lower.Contains("start ") || lower.Contains("close ") ||
                   lower.Contains("switch to") || lower.Contains("focus ") ||
                   lower.Contains("running") || lower.Contains("learn app") ||
                   lower.Contains("learn how") || lower.Contains("do in ") ||
                   lower.Contains("perform") || lower.Contains("automate") ||
                   lower.Contains("find app") || lower.Contains("software") ||
                   lower.Contains("app list");
        }

        public override async Task<string> ExecuteAsync(string input, Memory memory)
        {
            string lower = input.ToLowerInvariant();
            string nick = _profile.GetUserAddress();

            // Show running apps
            if (lower.Contains("running") || lower.Contains("what's open") || lower.Contains("show apps"))
            {
                var apps = _controller.GetRunningApps();
                if (!apps.Any())
                    return $"Nothing running, {nick}. Want me to open something?";

                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"🖥️ **Running Apps** ({apps.Count}):\n");
                foreach (var app in apps.Take(20))
                    sb.AppendLine($"  • {app}");
                return sb.ToString();
            }

            // Software list
            if (lower.Contains("software list") || lower.Contains("app list") || lower.Contains("installed"))
            {
                var report = _controller.GetSoftwareReport();
                return report;
            }

            // Find app
            if (lower.Contains("find app") || lower.Contains("where is"))
            {
                string appName = Regex.Replace(input, @"find app\s+|where is\s+", "", RegexOptions.IgnoreCase).Trim();
                return $"🔍 Looking for **{appName}**... Say `open {appName}` to launch it!";
            }

            // Close app
            if (lower.Contains("close ") || lower.Contains("quit "))
            {
                string appName = ExtractAppName(input, new[] { "close", "quit", "exit" });
                return _controller.CloseApp(appName);
            }

            // Switch/focus
            if (lower.Contains("switch to") || lower.Contains("focus"))
            {
                string appName = ExtractAppName(input, new[] { "switch to", "focus", "go to" });
                bool focused = _controller.FocusWindow(appName);
                return focused
                    ? $"✅ Switched to **{appName}**, {nick}!"
                    : $"❌ Couldn't find **{appName}**. Is it running?";
            }

            // Learn how to do something
            if (lower.Contains("learn how to") || lower.Contains("teach me how") || lower.Contains("learn app"))
            {
                return ParseLearnCommand(input);
            }

            // Perform a learned task
            if (lower.Contains("do in") || lower.Contains("perform in") || lower.Contains("automate"))
            {
                return await ParsePerformCommand(input, memory);
            }

            // Open/launch/start (default)
            if (lower.Contains("open ") || lower.Contains("launch ") || lower.Contains("start ") || lower.Contains("run "))
            {
                string appName = ExtractAppName(input, new[] { "open", "launch", "start", "run" });
                
                // Check if there's a learned procedure
                var procedures = _controller.GetProcedures(appName);
                if (procedures.Any())
                {
                    string result = await _controller.LaunchApp(appName);
                    return $"{result}\n\n💡 I know {procedures.Count} task(s) for **{appName}**. Want me to do one?";
                }

                return await _controller.LaunchApp(appName);
            }

            return GetHelp(nick);
        }

        private string ParseLearnCommand(string input)
        {
            // Format: "learn [app] [task] step 1: [x] step 2: [y]"
            // Example: "learn chrome open new tab step 1: press Ctrl+T"

            var match = Regex.Match(input,
                @"learn\s+(?:how to|app)?\s*(\w+)\s+(.+?)(?:\s+step\s+\d+:?\s*(.*))",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (!match.Success)
            {
                return "🎓 **How to teach me:**\n\n" +
                    "Format: `learn [app] [task] step 1: [do this] step 2: [do that]`\n\n" +
                    "Examples:\n" +
                    "• `learn chrome open new tab step 1: press Ctrl+T`\n" +
                    "• `learn notepad save file step 1: press Ctrl+S`\n" +
                    "• `learn excel create chart step 1: select data step 2: press Alt+N step 3: press C`\n" +
                    "• `learn word make bold step 1: select text step 2: press Ctrl+B`";
            }

            string app = match.Groups[1].Value;
            string task = match.Groups[2].Value.Trim();
            string stepsText = match.Groups[3].Value;

            // Parse steps
            var steps = new List<string>();
            var stepMatches = Regex.Matches(input, @"step\s+\d+:?\s*([^s]+?)(?=step\s+\d+|$)", RegexOptions.IgnoreCase);
            foreach (Match sm in stepMatches)
            {
                steps.Add(sm.Groups[1].Value.Trim().TrimEnd(','));
            }

            if (steps.Count == 0)
                steps.Add(stepsText);

            // Learn the procedure
            return _controller.LearnProcedure(app, task, steps);
        }

        private async Task<string> ParsePerformCommand(string input, Memory memory)
        {
            // Format: "do [task] in [app]" or "perform [task] in [app]"
            var match = Regex.Match(input,
                @"(?:do|perform|automate)\s+(.+?)\s+(?:in|on)\s+(\w+)",
                RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return "🔧 Format: `do [task] in [app]`\n" +
                    "Example: `do open new tab in chrome`";
            }

            string task = match.Groups[1].Value.Trim();
            string app = match.Groups[2].Value;

            // Check if we know this procedure
            var procedures = _controller.GetProcedures(app);
            var known = procedures.FirstOrDefault(p =>
                p.Task.ToLowerInvariant().Contains(task.ToLowerInvariant()));

            if (known != null)
            {
                return await _controller.PerformProcedure(app, task);
            }

            // Ask to learn
            return _controller.AskHowToDo(app, task);
        }

        private string ExtractAppName(string input, string[] prefixes)
        {
            string result = input;
            foreach (var prefix in prefixes)
            {
                int idx = result.ToLowerInvariant().IndexOf(prefix.ToLowerInvariant());
                if (idx >= 0)
                {
                    result = result.Substring(idx + prefix.Length).Trim();
                    break;
                }
            }
            return result.Trim().Trim('"', '\'');
        }

        private string GetHelp(string nick)
        {
            return $"🖥️ **App Controller — I can control anything, {nick}!**\n\n" +
                "**Launch Apps:**\n" +
                "• `open chrome` — Open any application\n" +
                "• `open photoshop` — Launch any installed software\n\n" +
                "**Control Apps:**\n" +
                "• `close chrome` — Close an app\n" +
                "• `switch to word` — Focus a window\n" +
                "• `show running` — List open apps\n\n" +
                "**Learn Features:**\n" +
                "• `learn chrome open new tab step 1: press Ctrl+T`\n" +
                "• `learn excel create chart step 1: select data step 2: press Alt+N`\n\n" +
                "**Automate:**\n" +
                "• `do open new tab in chrome` — Perform learned task\n" +
                "• `perform save file in notepad` — Execute saved procedure\n\n" +
                "**Explore:**\n" +
                "• `software list` — See all known apps\n" +
                "• `find app [name]` — Search for an app\n\n" +
                "💡 Teach me once, I'll do it forever!";
        }
    }
}

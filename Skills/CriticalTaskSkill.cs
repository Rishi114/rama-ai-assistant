using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Critical Task Performer — Handles important/dangerous tasks with extra care.
    /// Validates before acting, double-checks, asks for confirmation, and learns from mistakes.
    /// </summary>
    public class CriticalTaskSkill : SkillBase
    {
        private readonly CriticalThinking _thinking;

        public CriticalTaskSkill(CriticalThinking thinking)
        {
            _thinking = thinking;
        }

        public override string Name => "Critical Tasks";
        public override string Description => "Thinks carefully before acting, learns from mistakes";
        public override string[] Triggers => new[] {
            "think about", "analyze", "careful", "critical",
            "verify", "validate", "check before", "double check",
            "mistakes", "what went wrong", "lessons learned",
            "how did you fail", "report mistakes", "improve",
            "think critically", "review", "audit"
        };

        public override bool CanHandle(string input)
        {
            string lower = input.ToLowerInvariant();
            return lower.Contains("think about") || lower.Contains("analyze") ||
                   lower.Contains("critical") || lower.Contains("verify") ||
                   lower.Contains("mistakes") || lower.Contains("lessons") ||
                   lower.Contains("what went wrong") || lower.Contains("review") ||
                   lower.Contains("audit") || lower.Contains("improve") ||
                   lower.Contains("careful") || lower.Contains("double check");
        }

        public override Task<string> ExecuteAsync(string input, Memory memory)
        {
            string lower = input.ToLowerInvariant();

            if (lower.Contains("mistakes") || lower.Contains("what went wrong") ||
                lower.Contains("lessons learned") || lower.Contains("how did you fail") ||
                lower.Contains("report mistakes"))
                return Task.FromResult(_thinking.GetMistakeReport());

            if (lower.Contains("improve") || lower.Contains("how are you improving"))
                return Task.FromResult(GetImprovementReport());

            if (lower.Contains("think about") || lower.Contains("analyze") || lower.Contains("think critically"))
                return Task.FromResult(ThinkAbout(input, memory));

            if (lower.Contains("verify") || lower.Contains("validate") || lower.Contains("check before"))
                return Task.FromResult(ValidateAction(input));

            if (lower.Contains("review") || lower.Contains("audit"))
                return Task.FromResult(ReviewPastActions());

            if (lower.Contains("careful") || lower.Contains("double check"))
                return Task.FromResult(PerformCarefully(input, memory));

            return Task.FromResult(GetHelp());
        }

        private string ThinkAbout(string input, Memory memory)
        {
            string topic = Regex.Replace(input, @"think about\s+|analyze\s+|think critically about\s+", "", RegexOptions.IgnoreCase).Trim();

            if (string.IsNullOrEmpty(topic))
                return "🤔 What should I think about? Give me a problem or topic to analyze.";

            // Perform critical thinking
            var analysis = _thinking.Analyze(topic, "analysis");

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"🤔 **Critical Analysis: {topic}**\n");

            // Step 1: Understand
            sb.AppendLine("**Step 1: Understanding**");
            sb.AppendLine($"  • Topic: {topic}");
            sb.AppendLine($"  • Confidence: {analysis.Confidence:P0}");
            sb.AppendLine($"  • Risk Level: {analysis.RiskLevel}");
            sb.AppendLine();

            // Step 2: Check past experience
            if (analysis.PastSolution != null)
            {
                sb.AppendLine("**Step 2: Past Experience**");
                sb.AppendLine($"  💡 I solved something like this before: {analysis.PastSolution.Result}");
                sb.AppendLine();
            }

            // Step 3: Warnings
            if (analysis.Warnings.Any())
            {
                sb.AppendLine("**Step 3: Warnings**");
                foreach (var w in analysis.Warnings) sb.AppendLine($"  {w}");
                sb.AppendLine();
            }

            // Step 4: Edge cases
            if (analysis.EdgeCases.Any())
            {
                sb.AppendLine("**Step 4: Edge Cases**");
                foreach (var e in analysis.EdgeCases) sb.AppendLine($"  {e}");
                sb.AppendLine();
            }

            // Step 5: Recommendation
            sb.AppendLine("**Step 5: Recommendation**");
            if (analysis.Blocked)
                sb.AppendLine($"  ⛔ {analysis.BlockReason}");
            else if (analysis.RiskLevel >= RiskLevel.High)
                sb.AppendLine("  ⚠️ High risk — proceed with caution. I'll validate each step.");
            else if (analysis.Confidence > 0.7)
                sb.AppendLine("  ✅ Looks good! I'm confident I can handle this.");
            else
                sb.AppendLine("  🤔 Moderate confidence. I'll be careful and double-check.");

            return sb.ToString();
        }

        private string ValidateAction(string input)
        {
            string action = Regex.Replace(input, @"verify\s+|validate\s+|check before\s+", "", RegexOptions.IgnoreCase).Trim();

            if (string.IsNullOrEmpty(action))
                return "✅ Validate what? Tell me what action to check.";

            var analysis = _thinking.Analyze(action, action);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"🔍 **Validation Report:**\n");
            sb.AppendLine($"Action: {action}");
            sb.AppendLine($"Confidence: {analysis.Confidence:P0}");
            sb.AppendLine($"Risk: {analysis.RiskLevel}");

            if (analysis.Blocked)
            {
                sb.AppendLine($"\n⛔ **BLOCKED:** {analysis.BlockReason}");
                return sb.ToString();
            }

            // Check for past mistakes
            var warning = _thinking.CheckForMistake(action, action);
            if (warning != null)
            {
                sb.AppendLine($"\n{warning.Warning}");
            }

            if (analysis.Warnings.Any())
            {
                sb.AppendLine("\n⚠️ **Warnings:**");
                foreach (var w in analysis.Warnings) sb.AppendLine($"  {w}");
            }

            if (!analysis.Warnings.Any() && warning == null && !analysis.Blocked)
            {
                sb.AppendLine("\n✅ **All checks passed!** Safe to proceed.");
            }

            return sb.ToString();
        }

        private string ReviewPastActions()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("📋 **Performance Review:**\n");

            // Mistakes
            var mistakes = _thinking.TotalMistakes;
            var learned = _thinking.LearnedMistakes;
            sb.AppendLine($"**Mistakes:**");
            sb.AppendLine($"  • Total: {mistakes}");
            sb.AppendLine($"  • Lessons learned: {learned}");
            sb.AppendLine($"  • Still unresolved: {mistakes - learned}");
            sb.AppendLine();

            // Solutions
            var solutions = _thinking.GetTopSolutions(5);
            if (solutions.Any())
            {
                sb.AppendLine("**Successful Patterns:**");
                foreach (var s in solutions)
                {
                    sb.AppendLine($"  • {s.Action} (used {s.UseCount}x)");
                }
                sb.AppendLine();
            }

            // Mistake rate
            double rate = _thinking.MistakeRate;
            sb.AppendLine($"**Error Rate:** {rate:P1}");

            if (rate < 0.1)
                sb.AppendLine("✅ Excellent! Very low error rate.");
            else if (rate < 0.3)
                sb.AppendLine("👍 Good. Some room for improvement.");
            else
                sb.AppendLine("⚠️ Higher error rate. I'm learning from these.");

            return sb.ToString();
        }

        private string PerformCarefully(string input, Memory memory)
        {
            string task = Regex.Replace(input, @"careful(?:ly)?\s+|double check\s+", "", RegexOptions.IgnoreCase).Trim();

            if (string.IsNullOrEmpty(task))
                return "🤔 What should I do carefully? Tell me the task.";

            // Think before acting
            var analysis = _thinking.Analyze(task, task);

            if (analysis.Blocked)
                return $"⛔ **Cannot proceed:** {analysis.BlockReason}";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"🛡️ **Performing with extra care:** {task}\n");

            // Pre-flight checks
            sb.AppendLine("**Pre-flight Checks:**");
            sb.AppendLine($"  ✅ Confidence: {analysis.Confidence:P0}");

            if (analysis.Warnings.Any())
            {
                foreach (var w in analysis.Warnings)
                    sb.AppendLine($"  {w}");
            }
            else
            {
                sb.AppendLine("  ✅ No warnings");
            }

            if (analysis.PastSolution != null)
                sb.AppendLine($"  💡 Past experience found — using learned approach");

            sb.AppendLine($"\n**Status:** Ready to proceed carefully.");
            sb.AppendLine("I'll validate each step and report any issues immediately.");

            return sb.ToString();
        }

        private string GetImprovementReport()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("📈 **How I'm Getting Smarter:**\n");

            sb.AppendLine("**Critical Thinking:**");
            sb.AppendLine("  ✅ I analyze before acting");
            sb.AppendLine("  ✅ I check for past mistakes");
            sb.AppendLine("  ✅ I validate risky operations");
            sb.AppendLine("  ✅ I consider edge cases");
            sb.AppendLine();

            sb.AppendLine("**Mistake Prevention:**");
            sb.AppendLine("  ✅ I maintain a mistake database");
            sb.AppendLine("  ✅ I auto-generate lessons from errors");
            sb.AppendLine("  ✅ I warn before repeating mistakes");
            sb.AppendLine("  ✅ I categorize errors by type");
            sb.AppendLine();

            sb.AppendLine("**Learning Loop:**");
            sb.AppendLine("  1. 🤔 Think before acting");
            sb.AppendLine("  2. ⚡ Execute carefully");
            sb.AppendLine("  3. 📊 Review the result");
            sb.AppendLine("  4. 📚 Learn from outcome");
            sb.AppendLine("  5. 🔄 Apply lesson next time");
            sb.AppendLine();

            sb.AppendLine($"Mistakes tracked: {_thinking.TotalMistakes}");
            sb.AppendLine($"Lessons learned: {_thinking.LearnedMistakes}");

            return sb.ToString();
        }

        private string GetHelp()
        {
            return "🧠 **Critical Thinking & Mistake Learning**\n\n" +
                "**Think Before Acting:**\n" +
                "• `think about [problem]` — Deep analysis\n" +
                "• `verify [action]` — Check before doing\n" +
                "• `careful [task]` — Extra caution mode\n\n" +
                "**Learn From Mistakes:**\n" +
                "• `mistakes` — See all recorded mistakes\n" +
                "• `lessons learned` — What I've learned\n" +
                "• `review` — Performance review\n" +
                "• `improve` — How I'm getting better\n\n" +
                "**How It Works:**\n" +
                "1. I think before every action\n" +
                "2. I check my mistake history\n" +
                "3. I validate risky operations\n" +
                "4. I learn from every error\n" +
                "5. I never repeat the same mistake twice";
        }
    }
}

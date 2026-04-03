using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Deep Learning Skill — Learn from everything, think deeply, remember forever.
    /// </summary>
    public class DeepLearningSkill : SkillBase
    {
        private readonly AdvancedLearning _learning;
        private readonly ProfileManager _profile;

        public DeepLearningSkill(AdvancedLearning learning, ProfileManager profile)
        {
            _learning = learning;
            _profile = profile;
        }

        public override string Name => "Deep Learning";
        public override string Description => "Learn from anything, deep thinking, creative ideas";
        public override string[] Triggers => new[] {
            "deep think", "think deeply", "analyze deeply",
            "learn about", "understand", "explain deeply",
            "creative ideas", "brainstorm", "generate ideas",
            "ask questions about", "what questions",
            "learning report", "what do you know",
            "world model", "knowledge graph"
        };

        public override bool CanHandle(string input)
        {
            string lower = input.ToLowerInvariant();
            return lower.Contains("deep think") || lower.Contains("think deeply") ||
                   lower.Contains("learn about") || lower.Contains("understand") ||
                   lower.Contains("brainstorm") || lower.Contains("creative") ||
                   lower.Contains("ask questions") || lower.Contains("learning report") ||
                   lower.Contains("world model") || lower.Contains("knowledge");
        }

        public override Task<string> ExecuteAsync(string input, Memory memory)
        {
            string lower = input.ToLowerInvariant();
            string nick = _profile.GetUserAddress();

            if (lower.Contains("learning report") || lower.Contains("what do you know"))
                return Task.FromResult(_learning.GetLearningReport());

            if (lower.Contains("deep think") || lower.Contains("think deeply") || lower.Contains("analyze deeply"))
                return Task.FromResult(DeepThink(input, nick));

            if (lower.Contains("brainstorm") || lower.Contains("creative ideas") || lower.Contains("generate ideas"))
                return Task.FromResult(Brainstorm(input, nick));

            if (lower.Contains("ask questions") || lower.Contains("what questions"))
                return Task.FromResult(AskQuestions(input));

            if (lower.Contains("learn about") || lower.Contains("understand"))
                return Task.FromResult(LearnTopic(input));

            if (lower.Contains("world model") || lower.Contains("knowledge graph"))
                return Task.FromResult(GetWorldModel());

            return Task.FromResult(GetHelp(nick));
        }

        private string DeepThink(string input, string nick)
        {
            string topic = Regex.Replace(input, @"deep think\s+(?:about\s+)?|think deeply\s+(?:about\s+)?|analyze deeply\s+", "", RegexOptions.IgnoreCase).Trim();

            if (string.IsNullOrEmpty(topic))
                return $"What should I think deeply about, {nick}?";

            var chain = _learning.DeepThink(topic);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"🧠 **Deep Analysis:** {topic}\n");

            foreach (var step in chain.Steps)
            {
                sb.AppendLine($"**{step.Name}:**");
                if (step.Insights.Any())
                {
                    foreach (var insight in step.Insights.Take(5))
                        sb.AppendLine($"  • {insight}");
                }
                else
                {
                    sb.AppendLine($"  {step.Content}");
                }
                sb.AppendLine();
            }

            sb.AppendLine($"**Conclusion:** {chain.Conclusion}");
            sb.AppendLine($"**Confidence:** {chain.Confidence:P0}");

            return sb.ToString();
        }

        private string Brainstorm(string input, string nick)
        {
            string topic = Regex.Replace(input, @"brainstorm\s+(?:about\s+)?|creative ideas\s+(?:about\s+)?|generate ideas\s+(?:about\s+)?", "", RegexOptions.IgnoreCase).Trim();

            if (string.IsNullOrEmpty(topic))
                return $"What should I brainstorm about, {nick}?";

            var ideas = _learning.CreativeThink(topic, 5);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"💡 **Creative Ideas about '{topic}':**\n");
            foreach (var idea in ideas)
                sb.AppendLine($"  • {idea}");

            return sb.ToString();
        }

        private string AskQuestions(string input)
        {
            string topic = Regex.Replace(input, @"ask questions\s+(?:about\s+)?|what questions\s+(?:about\s+)?", "", RegexOptions.IgnoreCase).Trim();

            if (string.IsNullOrEmpty(topic))
                return "About what topic? Say `ask questions about [topic]`";

            var questions = _learning.GenerateQuestions(topic);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"🤔 **Questions about '{topic}':**\n");
            foreach (var q in questions)
                sb.AppendLine($"  • {q}");

            return sb.ToString();
        }

        private string LearnTopic(string input)
        {
            string topic = Regex.Replace(input, @"learn about\s+|understand\s+", "", RegexOptions.IgnoreCase).Trim();

            if (string.IsNullOrEmpty(topic))
                return "What should I learn about?";

            var result = _learning.LearnFromText(topic, "user_direct");

            return $"📚 Learned about **{topic}**!\n\n" +
                   $"New concepts: {result.NewConcepts.Count}\n" +
                   $"Relationships found: {result.NewRelationships.Count}\n" +
                   $"Rules extracted: {result.NewRules.Count}";
        }

        private string GetWorldModel()
        {
            return _learning.GetLearningReport();
        }

        private string GetHelp(string nick)
        {
            return $"🧠 **Deep Learning — I learn from everything, {nick}!**\n\n" +
                "**Deep Thinking:**\n" +
                "• `deep think about [problem]` — Full analysis\n" +
                "• `brainstorm [topic]` — Creative ideas\n" +
                "• `ask questions about [topic]` — Generate questions\n\n" +
                "**Learning:**\n" +
                "• `learn about [topic]` — Extract concepts\n" +
                "• `learning report` — See what I know\n" +
                "• `world model` — My knowledge graph\n\n" +
                "**Auto-Learning:**\n" +
                "• Everything you teach me gets processed\n" +
                "• I extract concepts, relationships, rules\n" +
                "• I build a world model over time";
        }
    }
}

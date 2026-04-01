using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Abstract base class for skills with common functionality.
    /// </summary>
    public abstract class SkillBase : ISkill
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string[] Triggers { get; }

        public virtual bool CanHandle(string input)
        {
            string lower = input.ToLowerInvariant().Trim();
            foreach (var trigger in Triggers)
            {
                if (lower.Contains(trigger.ToLowerInvariant()))
                    return true;
            }
            return false;
        }

        public abstract Task<string> ExecuteAsync(string input, Memory memory);

        public virtual void OnLoad() { }
        public virtual void OnUnload() { }

        /// <summary>
        /// Extract text after the trigger keyword.
        /// </summary>
        protected string ExtractAfterTrigger(string input, string trigger)
        {
            string lower = input.ToLowerInvariant();
            int idx = lower.IndexOf(trigger.ToLowerInvariant());
            if (idx < 0) return input.Trim();
            return input.Substring(idx + trigger.Length).Trim();
        }

        /// <summary>
        /// Extract text after any of the triggers.
        /// </summary>
        protected string ExtractCommand(string input)
        {
            string lower = input.ToLowerInvariant();
            foreach (var trigger in Triggers)
            {
                int idx = lower.IndexOf(trigger.ToLowerInvariant());
                if (idx >= 0)
                {
                    string result = input.Substring(idx + trigger.Length).Trim();
                    if (!string.IsNullOrEmpty(result)) return result;
                }
            }
            return input.Trim();
        }
    }
}

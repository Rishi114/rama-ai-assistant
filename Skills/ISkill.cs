using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Interface that all Rama skills must implement.
    /// Skills are the building blocks that give Rama her abilities.
    /// </summary>
    public interface ISkill
    {
        /// <summary>
        /// Unique name for this skill.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Human-readable description of what this skill does.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Keywords/phrases that trigger this skill.
        /// </summary>
        string[] Triggers { get; }

        /// <summary>
        /// Check if this skill can handle the given input.
        /// </summary>
        bool CanHandle(string input);

        /// <summary>
        /// Execute the skill with the given input.
        /// Returns the response string.
        /// </summary>
        Task<string> ExecuteAsync(string input, Memory memory);

        /// <summary>
        /// Called when the skill is loaded.
        /// </summary>
        void OnLoad();

        /// <summary>
        /// Called when the skill is unloaded.
        /// </summary>
        void OnUnload();
    }
}

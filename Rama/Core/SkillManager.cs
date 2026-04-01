using System.Reflection;

namespace Rama.Core
{
    /// <summary>
    /// Manages skill discovery, loading, unloading, and lifecycle.
    /// Supports both built-in skills (hardcoded) and external skills (loaded from DLL assemblies).
    /// External skills must implement the ISkill interface from the Rama.Skills namespace.
    /// </summary>
    public class SkillManager
    {
        private readonly List<Skills.ISkill> _skills = new();
        private readonly List<string> _loadedAssemblyPaths = new();
        private readonly string _skillsDirectory;

        /// <summary>
        /// All currently loaded and active skills.
        /// </summary>
        public IReadOnlyList<Skills.ISkill> Skills => _skills.AsReadOnly();

        /// <summary>
        /// Fired when a skill is loaded or unloaded, allowing the UI to refresh.
        /// </summary>
        public event Action? SkillsChanged;

        /// <summary>
        /// Creates a new SkillManager. The skills directory is relative to the application base.
        /// </summary>
        /// <param name="skillsDirectory">Directory to scan for external skill DLLs.</param>
        public SkillManager(string? skillsDirectory = null)
        {
            _skillsDirectory = skillsDirectory ?? Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Plugins");

            if (!Directory.Exists(_skillsDirectory))
            {
                Directory.CreateDirectory(_skillsDirectory);
            }
        }

        /// <summary>
        /// Discovers and loads all built-in skills from the current assembly.
        /// Call this once at application startup.
        /// </summary>
        public void LoadBuiltInSkills()
        {
            var skillTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(Skills.ISkill).IsAssignableFrom(t)
                    && !t.IsInterface
                    && !t.IsAbstract
                    && t.Namespace == "Rama.Skills");

            foreach (var type in skillTypes)
            {
                try
                {
                    if (Activator.CreateInstance(type) is Skills.ISkill skill)
                    {
                        AddSkill(skill);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Failed to load built-in skill {type.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Scans the Plugins directory for external skill DLL assemblies and loads them.
        /// External skills must implement ISkill and have a parameterless constructor.
        /// </summary>
        public void LoadExternalSkills()
        {
            if (!Directory.Exists(_skillsDirectory))
                return;

            var dllFiles = Directory.GetFiles(_skillsDirectory, "*.dll");
            foreach (var dllPath in dllFiles)
            {
                try
                {
                    LoadSkillFromAssembly(dllPath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Failed to load external skill from {dllPath}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Loads skill implementations from a specific assembly DLL.
        /// </summary>
        private void LoadSkillFromAssembly(string assemblyPath)
        {
            if (_loadedAssemblyPaths.Contains(assemblyPath))
                return; // Already loaded

            var assembly = Assembly.LoadFrom(assemblyPath);
            var skillTypes = assembly.GetTypes()
                .Where(t => typeof(Skills.ISkill).IsAssignableFrom(t)
                    && !t.IsInterface
                    && !t.IsAbstract);

            foreach (var type in skillTypes)
            {
                if (Activator.CreateInstance(type) is Skills.ISkill skill)
                {
                    AddSkill(skill);
                    _loadedAssemblyPaths.Add(assemblyPath);
                }
            }
        }

        /// <summary>
        /// Adds a skill instance to the manager and calls its OnLoad method.
        /// If a skill with the same name already exists, it is replaced.
        /// </summary>
        public void AddSkill(Skills.ISkill skill)
        {
            // Remove existing skill with same name if present
            var existing = _skills.FirstOrDefault(s => s.Name == skill.Name);
            if (existing != null)
            {
                existing.OnUnload();
                _skills.Remove(existing);
            }

            skill.OnLoad();
            _skills.Add(skill);
            SkillsChanged?.Invoke();
        }

        /// <summary>
        /// Removes a skill by name and calls its OnUnload method.
        /// </summary>
        public bool RemoveSkill(string name)
        {
            var skill = _skills.FirstOrDefault(s => s.Name == name);
            if (skill == null) return false;

            skill.OnUnload();
            _skills.Remove(skill);
            SkillsChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Finds the best skill to handle the given input.
        /// Returns all skills that can handle the input, sorted by relevance.
        /// The first result is the best match.
        /// </summary>
        public List<Skills.ISkill> FindMatchingSkills(string input)
        {
            return _skills
                .Where(s => s.CanHandle(input))
                .ToList();
        }

        /// <summary>
        /// Gets a skill by name. Returns null if not found.
        /// </summary>
        public Skills.ISkill? GetSkillByName(string name)
        {
            return _skills.FirstOrDefault(s =>
                s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all skill names and descriptions for display in the UI.
        /// </summary>
        public List<(string Name, string Description, string[] Triggers)> GetAllSkillInfo()
        {
            return _skills.Select(s => (s.Name, s.Description, s.Triggers)).ToList();
        }

        /// <summary>
        /// Registers a custom skill DLL path in the database for persistence.
        /// On next startup, this DLL will be loaded automatically.
        /// </summary>
        public void RegisterExternalSkill(string dllPath, Memory memory)
        {
            if (!File.Exists(dllPath))
                throw new FileNotFoundException($"Skill DLL not found: {dllPath}");

            // Copy to plugins directory
            var destPath = Path.Combine(_skillsDirectory, Path.GetFileName(dllPath));
            if (!File.Exists(destPath))
            {
                File.Copy(dllPath, destPath);
            }

            // Load it
            LoadSkillFromAssembly(destPath);

            // Register in database
            var assembly = Assembly.LoadFrom(destPath);
            var skillTypes = assembly.GetTypes()
                .Where(t => typeof(Skills.ISkill).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in skillTypes)
            {
                if (Activator.CreateInstance(type) is Skills.ISkill skill)
                {
                    memory.RegisterCustomSkill(skill.Name, destPath);
                }
            }
        }

        /// <summary>
        /// Loads previously registered custom skills from the database.
        /// Called during startup after built-in skills are loaded.
        /// </summary>
        public void LoadRegisteredSkills(Memory memory)
        {
            var customSkills = memory.GetCustomSkills();
            foreach (var (name, path, enabled) in customSkills)
            {
                if (!enabled) continue;
                if (!File.Exists(path)) continue;

                try
                {
                    LoadSkillFromAssembly(path);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Failed to load registered skill '{name}': {ex.Message}");
                }
            }
        }
    }
}

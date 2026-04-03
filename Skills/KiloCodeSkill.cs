using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// KiloCode skill - Kilo CLI agentic coding assistant
    /// </summary>
    public class KiloCodeSkill : SkillBase
    {
        private readonly string _knowledgePath;
        
        public override string Name => "KiloCode";
        public override string Description => "Kilo CLI - agentic coding assistant with config, skills, and autonomous mode";
        public override string[] Triggers => new[] { "kilocode", "kilo", "code", "ai coding", "agentic coding", "autonomous" };

        public KiloCodeSkill()
        {
            _knowledgePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BrainKnowledge", "kilocode");
        }

        public override bool CanHandle(string input)
        {
            string lower = input.ToLowerInvariant();
            return Triggers.Any(t => lower.Contains(t));
        }

        public override Task<string> ExecuteAsync(string input, Memory memory)
        {
            string query = ExtractCommand(input).ToLowerInvariant();
            
            if (string.IsNullOrWhiteSpace(query) || query == input.ToLowerInvariant())
            {
                return Task.FromResult(GetIntroduction());
            }

            return Task.FromResult(ProcessQuery(query));
        }

        private string GetIntroduction()
        {
            return @"⚡ **KiloCode**

Kilo CLI - agentic coding assistant for the terminal.

**Features:**
- **Interactive mode:** `kilo` - Chat with AI
- **Autonomous mode:** `kilo run --auto "task"` - Let AI work autonomously
- **Config:** `~/.config/kilo/opencode.json` - Customizable
- **Skills:** Extensible skill system
- **Model:** Multiple LLM providers support

**Installation:**
```bash
# Usually via npm or binary
npm install -g @kilocode/cli
# or
curl -fsSL https://get.kilo.ai | bash
```

**Usage:**
```bash
# Interactive coding
kilo

# Autonomous agent
kilo run --auto "refactor the authentication module"

# With specific model
kilo run --model claude-3.5-sonnet --auto "write tests"
```";
        }

        private string ProcessQuery(string query)
        {
            if (query.Contains("config"))
            {
                return @"**KiloCode Configuration**

Config file: `~/.config/kilo/opencode.json`

```json
{
  "model": "claude-3.5-sonnet",
  "temperature": 0.7,
  "maxTokens": 4096,
  "systemPrompt": "You are a helpful coding assistant"
}
```

Key options:
- model: LLM to use
- temperature: Creativity level
- maxTokens: Response length
- systemPrompt: Custom instructions";
            }

            if (query.Contains("autonomous") || query.Contains("run"))
            {
                return @"**Autonomous Mode**

Run Kilo in autonomous mode for self-directed tasks:

```bash
# Basic autonomous
kilo run --auto "fix the login bug"

# With specific model
kilo run --model gpt-4 --auto "refactor codebase"

# With timeout
kilo run --auto "write tests" --timeout 300
```

The AI will:
1. Understand the task
2. Plan steps
3. Execute autonomously
4. Report results";
            }

            if (query.Contains("skill"))
            {
                return @"**Kilo Skills**

Kilo supports extensible skills:

- Skills are defined in `skills/` directory
- Each skill has: name, triggers, action
- Auto-loaded at startup
- Can be custom-built or from marketplace

To add a skill:
1. Create skill in skills/ folder
2. Define triggers and actions
3. Kilo auto-discovers and loads it";
            }

            return @"I found KiloCode content but nothing specific. Try: config, autonomous, or skills!";
        }

        public override void OnLoad()
        {
            base.OnLoad();
            if (!Directory.Exists(_knowledgePath))
            {
                Console.WriteLine($"[KiloCode] Knowledge path not found: {_knowledgePath}");
            }
        }
    }
}
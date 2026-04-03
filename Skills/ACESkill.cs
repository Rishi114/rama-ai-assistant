using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Agentic Context Engine (ACE) skill - Self-improving AI agents.
    /// </summary>
    public class ACESkill : SkillBase
    {
        private readonly string _knowledgePath;
        
        public override string Name => "ACE (Agentic Context Engine)";
        public override string Description => "Self-improving AI agents that learn from experience - Skillbook, Reflector, SkillManager";
        public override string[] Triggers => new[] { "ace", "agentic context", "self-improve", "skillbook", "reflector", "learn from experience", "self-learning agent" };

        public ACESkill()
        {
            _knowledgePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BrainKnowledge", "agentic-context-engine");
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
            return @"🧠 **ACE - Agentic Context Engine**

AI agents don't naturally learn from experience. ACE adds a persistent learning loop that makes them better over time.

**Core Components:**
- **Skillbook** — Persistent collection of strategies that evolves with every task
- **Agent** — Executes tasks, enhanced with Skillbook strategies
- **Reflector** — Analyzes execution traces to extract what worked/failed
- **SkillManager** — Curates the Skillbook (adds, refines, removes strategies)

**Key Innovation:** Recursive Reflector - writes/executes Python code in sandbox to programmatically search for patterns, isolate errors, and iterate until actionable insights found.

**Results:**
- 2x consistency on airline benchmark (15 learned strategies)
- 49% token reduction in browser automation
- $1.50 learning cost for Claude Code translation

**Install:** `uv add ace-framework`

Ask me about setup, how it works, or the architecture!";
        }

        private string ProcessQuery(string query)
        {
            if (query.Contains("setup") || query.Contains("install") || query.Contains("quick"))
            {
                return @"**ACE Quick Setup:**

```bash
# Install
uv add ace-framework

# Interactive setup (recommended)
ace setup

# Or manual
export OPENAI_API_KEY="your-key"  # or ANTHROPIC_API_KEY
```

**Usage:**
```python
from ace import ACELiteLLM

agent = ACELiteLLM(model="gpt-4o-mini")
answer = agent.ask("Is there a seahorse emoji?")

# Feed correction - ACE extracts strategy
agent.learn_from_feedback("There is no seahorse emoji in Unicode.")

# Next call benefits from learned strategy
answer = agent.ask("Is there a seahorse emoji?")
```";
            }

            if (query.Contains("skillbook") || query.Contains("strategy"))
            {
                return @"**Skillbook:**

Persistent collection of strategies that evolves with every task.

- Stores learned patterns from feedback
- No fine-tuning, no training data, no vector database
- Strategies extracted by Reflector and curated by SkillManager
- Agent uses strategies to enhance responses

**Inspect learned strategies:**
```python
print(agent.get_strategies())
```";
            }

            if (query.Contains("reflector") || query.Contains("reflection"))
            {
                return @"**Recursive Reflector:**

Key innovation in ACE:

1. Analyzes execution traces (what the agent did)
2. Writes Python code in sandboxed environment
3. Programmatically searches for patterns
4. Isolates errors and iterates until actionable insights
5. Passes findings to SkillManager

Unlike simple trace summarization - it can run code to test hypotheses about what failed.";
            }

            if (query.Contains("architecture") || query.Contains("how it works"))
            {
                return @"**ACE Architecture:**

```
Skillbook[(Skillbook)]
    Start([Task]) --> Agent[Agent]
    Agent <--> Environment[Environment]
    Environment -- Trace --> Reflector[Reflector]
    Reflector --> SkillManager[SkillManager]
    SkillManager -- Updates --> Skillbook
    Skillbook -. Strategies .-> Agent
```

**Flow:**
1. Task arrives → Agent executes using Skillbook strategies
2. Environment produces trace → Reflector analyzes
3. Reflector finds patterns → SkillManager updates Skillbook
4. Next task → Agent has more strategies to try

**Runners available:** ACELiteLLM, ACELangChain, ACEBrowserUse, ACEClaudeCode";
            }

            return @"I found ACE content but nothing specific for that query. Try asking about: setup, skillbook, reflector, or architecture!";
        }

        public override void OnLoad()
        {
            base.OnLoad();
            if (!Directory.Exists(_knowledgePath))
            {
                Console.WriteLine($"[ACE] Knowledge path not found: {_knowledgePath}");
            }
        }
    }
}
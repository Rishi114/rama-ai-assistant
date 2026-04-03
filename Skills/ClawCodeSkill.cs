using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Claw-Code Harness skill - AI agent harness engineering patterns
    /// </summary>
    public class ClawCodeSkill : SkillBase
    {
        private readonly string _knowledgePath;
        
        public override string Name => "Claw-Code Harness";
        public override string Description => "Harness engineering for AI agents - tool wiring, task orchestration, runtime context";
        public override string[] Triggers => new[] { "claw-code", "harness", "tool wiring", "orchestration", "runtime", "mcp", "plugin", "slash command" };

        public ClawCodeSkill()
        {
            _knowledgePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BrainKnowledge", "claw-code");
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
            return @"⚡ **Claw-Code Harness**

A harness system for AI coding agents. Built in Rust (fast) + Python (port).

**Architecture:**
- `rust/` - Rust implementation (api-client, runtime, tools, plugins, cli)
- `src/` - Python porting workspace

**Key Concepts:**
- **Harness Engineering** - Wire tools to agents, orchestrate tasks, manage context
- **Multi-Provider** - Claude, GPT, and more with OAuth + streaming
- **Tool System** - Manifest-based definitions, MCP orchestration
- **Plugin System** - Hook pipeline, lifecycle events, bundled plugins

**Workflow Patterns:**
- `$team` mode - Parallel review, architectural feedback
- `$ralph` mode - Persistent execution, verification loops

Ask about specific components!";
        }

        private string ProcessQuery(string query)
        {
            if (query.Contains("harness"))
            {
                return @"**Harness Engineering**

The core concept behind Claw-Code:

1. **Tool Wiring** - Connect agent to filesystem, terminal, APIs
2. **Task Orchestration** - Structure multi-step operations
3. **Runtime Context** - Manage state across sessions
4. **Permission System** - Control what agent can/cannot do

Think of it as the "operating system" for AI agents - handles all the messy details so the agent can focus on solving problems.";
            }

            if (query.Contains("mcp") || query.Contains("model context"))
            {
                return @"**MCP - Model Context Protocol**

A protocol for wiring tools to AI models:

- Standardized tool definitions
- Context passing between calls
- State management across interactions
- Built into the runtime crate

Enables: any tool → any model, regardless of implementation!";
            }

            if (query.Contains("plugin"))
            {
                return @"**Plugin System**

Claw-Code supports extensible plugins:

- **Hook Pipeline** - Pre/post execution events
- **Lifecycle Events** - On start, on end, on error
- **Bundled Plugins** - Common utilities included

```rust
// Plugin example
struct MyPlugin;
impl Hook for MyPlugin {
    fn pre_tool(&self, tool: &Tool) {
        // Called before tool execution
    }
}
```";
            }

            if (query.Contains("session") || query.Contains("runtime"))
            {
                return @"**Runtime & Session**

- **Session State** - Persists context across turns
- **Context Compaction** - Compress when near limits
- **Prompt Construction** - Builds context from history + tools
- **MCP Orchestration** - Coordinates multiple tools

The runtime is the "brain" that keeps everything organized!";
            }

            if (query.Contains("workflow") || query.Contains("team") || query.Contains("ralph"))
            {
                return @"**Workflow Patterns**

**$team mode:**
- Multiple agents work in parallel
- Coordinated code review
- Architectural feedback from different perspectives

**$ralph mode:**
- Persistent execution loops
- Verification at each step
- Completion discipline - keeps going until done

Both patterns driven by AI orchestration (OmX/OmO)";
            }

            return @"I found Claw-Code content but nothing specific for that query. Try asking about: harness, MCP, plugins, runtime, or workflow patterns!";
        }

        public override void OnLoad()
        {
            base.OnLoad();
            if (!Directory.Exists(_knowledgePath))
            {
                Console.WriteLine($"[ClawCode] Knowledge path not found: {_knowledgePath}");
            }
        }
    }
}
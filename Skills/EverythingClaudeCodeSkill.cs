using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Everything Claude Code skill - 156+ AI agent skills and patterns
    /// </summary>
    public class EverythingClaudeCodeSkill : SkillBase
    {
        private readonly string _knowledgePath;
        
        public override string Name => "Everything Claude Code";
        public override string Description => "156+ AI agent skills: agentic engineering, autonomous loops, MCP, harness, evaluation, and more";
        public override string[] Triggers => new[] { "ecc", "everything claude", "agent skill", "mcp", "autonomous", "harness", "evaluation", "benchmark" };

        public EverythingClaudeCodeSkill()
        {
            _knowledgePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BrainKnowledge", "everything-claude-code");
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
            return @"📦 **Everything Claude Code (ECC)**

156+ AI agent skills and patterns!

**Categories:**
- **Agent Engineering**: agentic-engineering, autonomous-loops, autonomous-agent-harness
- **Harness**: agent-harness-construction, autonomous-agent-harness
- **MCP**: Model Context Protocol configurations
- **Evaluation**: agent-eval, ai-regression-testing, benchmark
- **Coding**: api-design, android-clean-architecture, backend-patterns
- **Tools**: bun-runtime, clickhouse-io, compose-multiplatform

**Key Files:**
- `skills/` - 156 skill directories
- `agents/` - Agent configurations
- `commands/` - CLI commands
- `mcp-configs/` - MCP server configs

Ask about specific skill categories!";
        }

        private string ProcessQuery(string query)
        {
            if (query.Contains("autonomous") || query.Contains("loop"))
            {
                return @"**Autonomous Agents**

Key skills:
- `autonomous-loops` - Self-sustaining agent loops
- `autonomous-agent-harness` - Building autonomous agent frameworks
- `autonomous-agent-code` - Code for autonomous agents

Pattern: Agent that can run indefinitely with self-correction!";
            }

            if (query.Contains("harness"))
            {
                return @"**Harness Engineering**

Key skills:
- `agent-harness-construction` - Build agent harnesses
- `autonomous-agent-harness` - Autonomous framework building
- `autonomous-agent-code` - Implementation

Harness = scaffolding that connects AI to tools/actions!";
            }

            if (query.Contains("mcp"))
            {
                return @"**MCP - Model Context Protocol**

Available in `mcp-configs/`:
- Server configurations
- Tool definitions
- Resource handlers

MCP enables: any tool ↔ any model through standardized protocol!";
            }

            if (query.Contains("eval") || query.Contains("benchmark"))
            {
                return @"**Evaluation & Benchmarking**

Key skills:
- `agent-eval` - Agent evaluation frameworks
- `ai-regression-testing` - Test AI outputs
- `benchmark` - Performance benchmarking

Build robust AI systems with proper testing!";
            }

            return @"I found ECC content but nothing specific. Try: autonomous, harness, mcp, or evaluation!";
        }

        public override void OnLoad()
        {
            base.OnLoad();
            if (!Directory.Exists(_knowledgePath))
            {
                Console.WriteLine($"[EverythingClaudeCode] Knowledge path not found: {_knowledgePath}");
            }
        }
    }
}
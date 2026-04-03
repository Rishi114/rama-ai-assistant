using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// AgentJSON skill - JSON-based agent specification and benchmarking
    /// </summary>
    public class AgentJSONSkill : SkillBase
    {
        private readonly string _knowledgePath;
        
        public override string Name => "AgentJSON";
        public override string Description => "JSON-based agent specification, benchmarking, and Rust/Python implementations";
        public override string[] Triggers => new[] { "agentjson", "agent json", "json agent", "benchmark", "rust", "python agent" };

        public AgentJSONSkill()
        {
            _knowledgePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BrainKnowledge", "agentjson");
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
            return @"🅰️ **AgentJSON**

JSON-based agent specification and benchmarking framework.

**What it is:**
- Define agents in JSON
- Benchmark agent performance
- Rust + Python implementations
- Compare different agent architectures

**Components:**
- `src/` - Core Rust implementation
- `rust-pyo3/` - Python bindings
- `benchmarks/` - Benchmark suites
- `demo/` - Demo configurations

**Use Cases:**
- Standardize agent definitions
- Compare agent performance
- Build reproducible experiments
- Share agent configs as JSON";
        }

        private string ProcessQuery(string query)
        {
            if (query.Contains("benchmark"))
            {
                return @"**AgentJSON Benchmarking**

Define agent tasks in JSON, measure performance:

- **Task definitions** - JSON specifications
- **Metrics** - Success rate, time, tokens
- **Comparison** - Compare agents/approaches
- **Reproducibility** - Share configs, reproduce results

Check `benchmarks/` for examples!";
            }

            if (query.Contains("rust") || query.Contains("python"))
            {
                return @"**Implementation**

**Rust** (`src/`):
- Fast, memory-safe core
- Type-safe agent definitions

**Python** (`rust-pyo3/`):
- PyO3 Python bindings
- Easy integration

**Build:**
```bash
# Rust
cargo build

# Python
pip install -e .
```";
            }

            if (query.Contains("json") || query.Contains("spec"))
            {
                return @"**JSON Agent Specification**

Define agents as JSON:

```json
{
  "name": "my-agent",
  "model": "gpt-4",
  "tools": ["search", "code"],
  "max_steps": 100,
  "timeout": 300
}
```

Benefits:
- Language-agnostic
- Easy to version control
- Shareable
- Standardizable";
            }

            return @"I found AgentJSON content but nothing specific. Try: benchmark, rust, python, or json spec!";
        }

        public override void OnLoad()
        {
            base.OnLoad();
            if (!Directory.Exists(_knowledgePath))
            {
                Console.WriteLine($"[AgentJSON] Knowledge path not found: {_knowledgePath}");
            }
        }
    }
}
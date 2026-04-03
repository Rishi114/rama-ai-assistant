using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Claude Code Architecture skill - Implements patterns from claude-code-from-source.com
    /// </summary>
    public class ClaudeCodeArchitectureSkill : SkillBase
    {
        private readonly string _knowledgePath;
        
        public override string Name => "Claude Code Architecture";
        public override string Description => "Advanced agent patterns: AsyncGenerator loop, speculative execution, context compression, memory systems";
        public override string[] Triggers => new[] { "claude code", "architecture", "agent loop", "context compression", "speculative", "fork agent", "skill loading", "memory", "harness" };

        public ClaudeCodeArchitectureSkill()
        {
            _knowledgePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BrainKnowledge", "claude-code-architecture");
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
            return @"🏗️ **Claude Code Architecture**

Advanced agent patterns extracted from the Claude Code source:

**The 10 Core Patterns:**

1. **AsyncGenerator Agent Loop** — Streams output, executes tools, recovers from errors
2. **Speculative Execution** — Start read-only tools BEFORE response completes
3. **Concurrent Batching** — Partition by safety, parallel reads, serialize writes
4. **Fork Agents** — Parallel children share prompt prefixes (95% token savings!)
5. **4-Layer Context Compression** — snip → microcompact → collapse → autocompact
6. **File-Based LLM Memory** — Semantic recall via Sonnet side-query, not keywords
7. **Two-Phase Skill Loading** — Fast startup, lazy content load
8. **Sticky Latches** — Beta headers never unset mid-session
9. **Slot Reservation** — 8K default, escalates to 64K (saves context 99% of requests)
10. **Hook Config Snapshot** — Freeze at startup, prevents injection attacks

**Want me to explain any specific pattern? Just ask!";
        }

        private string ProcessQuery(string query)
        {
            if (query.Contains("loop") || query.Contains("async"))
            {
                return @"**AsyncGenerator Agent Loop**

The heartbeat of the agent - an async generator that:

1. Yields Messages as they're generated
2. Has typed Terminal return values
3. Provides natural backpressure
4. Supports cancellation

```python
async def agent_loop(prompt):
    while True:
        async for token in model.stream(prompt):
            yield token
            if token.tool_call:
                result = await execute_tool(token.tool)
                prompt += result
```

Drives: streaming output → execute tools → recover errors → compress context → repeat";
            }

            if (query.Contains("speculative") || query.Contains("execute"))
            {
                return @"**Speculative Tool Execution**

Revolutionary: start read-only tools DURING model streaming, BEFORE response completes!

1. Model outputs start of response + tool call
2. Immediately start read-only tools (grep, read, search)
3. Tools run in parallel while model continues
4. Results available instantly when needed

**14-Step Pipeline:**
1. Model request → 2. Tool call detected → 3. Permission resolved
4. Speculative execution starts → 5. Safety classification
6. Read tools parallel → 7. Writes serialized → 8. Results collected
9. Context updated → 10. Error recovery → 11. Compress → 12. Feedback loop";
            }

            if (query.Contains("compress") || query.Contains("context"))
            {
                return @"**4-Layer Context Compression**

When approaching token limits, compress in order of increasing aggression:

| Layer | Method | When |
|-------|--------|------|
| **snip** | Remove irrelevant messages | Near 80% |
| **microcompact** | Aggressive summary | Near 90% |
| **collapse** | Merge similar content | Near 95% |
| **autocompact** | Full rebuild | At limit |

Each layer is lighter than the previous. Never lose critical context!";
            }

            if (query.Contains("fork") || query.Contains("cache"))
            {
                return @"**Fork Agents for Cache Sharing**

Parallel fork agents inherit byte-identical prompt prefixes:

```
Main Agent Prompt: [System] [History] [Context]
                            ↓ fork
                  ┌─────────┴─────────┐
            Fork 1              Fork 2
            (same prefix)       (same prefix)
            → 95% token savings!
```

Use cases:
- Parallel research tasks
- Multiple file processing
- A/B testing responses
- Swarm orchestration";
            }

            if (query.Contains("memory") || query.Contains("recall"))
            {
                return @"**File-Based Memory with LLM Recall**

NOT embedding/keyword matching - uses LLM to choose what to recall!

**Four Memory Types:**
1. **Working** - Current conversation
2. **Short-term** - Recent sessions (last 24h)
3. **Long-term** - Persistent across sessions
4. **Semantic** - LLM-powered selection

**Recall Flow:**
1. Session starts
2. LLM (Sonnet) receives query + recent memories
3. LLM selects which long-term memories are relevant
4. Only relevant memories loaded into context

This beats embedding search because LLM understands relevance!";
            }

            if (query.Contains("skill") || query.Contains("loading"))
            {
                return @"**Two-Phase Skill Loading**

Phase 1 - Startup: Load only frontmatter (name, triggers, description)
→ Lightning fast, <100ms

Phase 2 - Invocation: Load full content on first use
→ Cached for subsequent calls

**Benefits:**
- Fast startup (don't load all skill content)
- Lazy loading (only what's needed)
- 27 lifecycle hooks available

```yaml
skill:
  name: "Web Search"
  triggers: ["search", "find"]  # ← Loaded at startup
  # content: ...                 # ← Loaded on first use
```";
            }

            if (query.Contains("performance") || query.Contains("startup"))
            {
                return @"**Performance Engineering**

| Metric | Achievement |
|--------|-------------|
| **Startup** | 240ms via parallel I/O |
| **Slot Reservation** | 8K→64K escalation, saves 99% of requests |
| **Fuzzy Search** | Bitmap pre-filters |
| **Context** | Every compression layer lighter |

**Key Optimizations:**
- Parallel module loading at startup
- Slot reservation prevents context overflow
- Bitmap indexes for fast search
- Sticky latches prevent cache thrashing";
            }

            return @"I found Claude Code Architecture patterns but nothing specific for that query. Try asking about: agent loop, speculative execution, context compression, fork agents, memory, or skills!";
        }

        public override void OnLoad()
        {
            base.OnLoad();
            if (!Directory.Exists(_knowledgePath))
            {
                Console.WriteLine($"[ClaudeCodeArchitecture] Knowledge path not found: {_knowledgePath}");
            }
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Second Brain skill - provides access to agency knowledge management system.
    /// </summary>
    public class SecondBrainSkill : SkillBase
    {
        private readonly string _knowledgePath;
        
        public override string Name => "Second Brain";
        public override string Description => "Access structured knowledge graphs, brain dumps, and vault for agency clients";
        public override string[] Triggers => new[] { "second brain", "knowledge", "vault", "mind graph", "brain dump", "knowledge graph", "agency" };

        public SecondBrainSkill()
        {
            _knowledgePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BrainKnowledge", "second-brain");
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
            return @"🧠 **Second Brain System**

I have a knowledge management system for AI agency clients with three interfaces:

**1. The Mind Graph** — 3D interactive visualization of everything the agent knows about the business. Clients navigate a spatial environment and see how concepts connect.

**2. The Brain Dump** — Raw input interface. Clients type anything — business context, meeting notes, strategic thinking — no formatting needed. System handles organization automatically.

**3. The Vault** — The organized output. Browse all processed files and notes, see connections, always know what the agent has access to.

---

**Architecture:**
- Dedicated processing agent (separate from task agent)
- Ingestion pipeline handles PDFs, docs, brain dumps
- Contextual linking between related concepts
- Client-facing layer (not a black box)

This turns "here's your automation" into "here's your intelligence layer."

Ask me about specific components like the mind graph, brain dump, processing layer, or how to implement it!";
        }

        private string ProcessQuery(string query)
        {
            var readmePath = Path.Combine(_knowledgePath, "README.md");
            if (!File.Exists(readmePath))
            {
                return "Second Brain knowledge base not found. Please ensure it's properly installed.";
            }

            try
            {
                var content = File.ReadAllText(readmePath);
                if (content.ToLowerInvariant().Contains(query))
                {
                    return $"Found in Second Brain docs:\n\n{content}";
                }
            }
            catch { }

            // Provide context-aware responses
            if (query.Contains("mind graph") || query.Contains("3d"))
            {
                return @"**The Mind Graph:**

A 3D interactive visualization of everything the agent knows. Not a flat Obsidian graph — a fully navigable spatial environment.

- Clients see how brand guide connects to content strategy connects to customer personas
- Visual representation shows value accumulating over time
- Built with Three.js or Babylon.js
- Part of branded dashboard (same auth, subdomain)";
            }

            if (query.Contains("brain dump"))
            {
                return @"**The Brain Dump:**

Direct input interface for raw, unstructured information.

- No formatting required
- No folder/structure decisions needed
- System processes and organizes automatically
- Converts: meeting notes, business context, strategic thinking, product details
- Goes through processing layer before landing in vault";
            }

            if (query.Contains("processing") || query.Contains("agent"))
            {
                return @"**Processing Layer:**

Dedicated agent (separate from task-running agent) whose ONLY job is knowledge management.

- Handles: PDF uploads, brain dumps, document submissions
- Maintains structural integrity as vault grows
- Makes consistent organization decisions
- Doesn't optimize for current task — optimizes for long-term structure

**Why separate agents?** Task execution and knowledge management are fundamentally different jobs. Single agent = inconsistent decisions.";
            }

            if (query.Contains("vault"))
            {
                return @"**The Vault:**

Organized output of everything processed.

- Browse full collection of files and notes
- See how concepts connect
- Clients can always answer 'does my agent know about X?'
- Makes knowledge visible, not a black box";
            }

            return $"I found some second brain content but nothing specific for '{query}'. Try asking about: mind graph, brain dump, vault, processing layer, or agency implementation.";
        }
    }
}
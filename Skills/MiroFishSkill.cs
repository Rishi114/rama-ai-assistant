using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// MiroFish skill - Swarm Intelligence Engine for predictions and simulations.
    /// </summary>
    public class MiroFishSkill : SkillBase
    {
        private readonly string _knowledgePath;
        
        public override string Name => "MiroFish";
        public override string Description => "Swarm intelligence prediction engine - simulate futures, test decisions, explore what-if scenarios";
        public override string[] Triggers => new[] { "mirofish", "swarm", "simulation", "predict", "prediction", "future", "what if", "rehearsal", "sandbox", "multi-agent" };

        public MiroFishSkill()
        {
            _knowledgePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BrainKnowledge", "MiroFish");
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
            return @"🐟 **MiroFish - Swarm Intelligence Engine**

A next-generation AI prediction engine powered by multi-agent technology. Creates high-fidelity digital worlds where thousands of agents with independent personalities interact and evolve.

**What it does:**
- Extracts seed information from real world (news, policy, financial signals)
- Builds parallel digital worlds with autonomous agents
- Runs simulations to deduce future trajectories
- Generates detailed prediction reports

**Workflow:**
1. **Graph Building** - Seed extraction + GraphRAG construction
2. **Environment Setup** - Entity relationships + persona generation
3. **Simulation** - Parallel simulation with dynamic memory updates
4. **Report Generation** - Deep analysis with interactive tools
5. **Deep Interaction** - Chat with any agent in the simulation

**Use Cases:**
- 🏛️ Policy/reputation testing (macro - decision rehearsal)
- 📖 Story ending predictions (micro - creative exploration)
- 📈 Financial predictions
- 🌍 Political news predictions

Ask me about how to use it, the architecture, or specific use cases!";
        }

        private string ProcessQuery(string query)
        {
            if (query.Contains("workflow") || query.Contains("process") || query.Contains("steps"))
            {
                return @"**MiroFish Workflow:**

1. **Graph Building** - Seed extraction from input materials + Individual/collective memory injection + GraphRAG construction

2. **Environment Setup** - Entity relationship extraction + Persona generation + Agent configuration injection

3. **Simulation** - Dual-platform parallel simulation + Auto-parse prediction requirements + Dynamic temporal memory updates

4. **Report Generation** - ReportAgent with rich toolset for deep interaction with post-simulation environment

5. **Deep Interaction** - Chat with any agent in the simulated world + Interact with ReportAgent";
            }

            if (query.Contains("setup") || query.Contains("install") || query.Contains("deploy"))
            {
                return @"**Setup Options:**

**Option 1: Source Code (Recommended)**
```bash
# Prerequisites: Node.js 18+, Python 3.11-3.12, uv
npm run setup:all    # One-click install
npm run dev          # Start frontend (3000) + backend (5001)
```

**Option 2: Docker**
```bash
cp .env.example .env  # Configure API keys
docker compose up -d
```

**Required env vars:**
- `LLM_API_KEY` - LLM API (OpenAI SDK format)
- `LLM_BASE_URL` - API endpoint
- `LLM_MODEL_NAME` - Model name
- `ZEP_API_KEY` - For agent memory (free tier works)";
            }

            if (query.Contains("agent") || query.Contains("multi"))
            {
                return @"**Multi-Agent Architecture:**

MiroFish runs thousands of intelligent agents with:
- **Independent personalities** - Each agent has unique traits
- **Long-term memory** - Zep Cloud for persistent memory
- **Behavioral logic** - Rule-based + LLM-driven decisions
- **Social evolution** - Agents interact and change over time

The swarm emerges from individual interactions - collective intelligence from autonomous agents.";
            }

            if (query.Contains("use") || query.Contains("example") || query.Contains("case"))
            {
                return @"**Use Cases:**

**Macro (Decision Makers):**
- Policy testing at zero risk
- Public relations scenario rehearsal
- Business decision simulations

**Micro (Individual Users):**
- Predict novel/story endings
- Explore "what if" scenarios
- Creative sandbox for imagination

**Examples:**
- Wuhan University public opinion simulation
- Dream of the Red Chamber lost ending prediction
- Financial trend predictions
- Political news forecasting";
            }

            return @"I found MiroFish content but nothing specific for that query. Try asking about: workflow, setup, multi-agent architecture, or use cases!";
        }

        public override void OnLoad()
        {
            base.OnLoad();
            if (!Directory.Exists(_knowledgePath))
            {
                Console.WriteLine($"[MiroFish] Knowledge path not found: {_knowledgePath}");
            }
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// G0DM0D3 skill - Multi-model evaluation and red-teaming framework
    /// </summary>
    public class G0DM0D3Skill : SkillBase
    {
        private readonly string _knowledgePath;
        
        public override string Name => "G0DM0D3";
        public override string Description => "Multi-model chat interface with GODMODE, ULTRAPLINIAN, Parseltongue, AutoTune, STM modules";
        public override string[] Triggers => new[] { "godmode", "godmod3", "g0dm0d3", "multimodel", "evaluation", "redteam", "parseltongue", "autotune", "ultraplinian" };

        public G0DM0D3Skill()
        {
            _knowledgePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BrainKnowledge", "godmode3");
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
            return @"🎯 **G0DM0D3**

Open-source, privacy-respecting multi-model chat interface.

**Core Features:**

1. **GODMODE CLASSIC** - 5 model+prompt combos race in parallel
   - Claude 3.5 Sonnet, Grok 3, Gemini 2.5, GPT-4, Hermes-4

2. **ULTRAPLINIAN** - Multi-model evaluation engine
   - 5 tiers: FAST(10), STANDARD(24), SMART(36), POWER(45), ULTRA(51)

3. **Parseltongue** - Red-teaming input perturbation
   - 33 techniques across 3 intensity tiers
   - Leetspeak, braille, morse, unicode substitution

4. **AutoTune** - Context-adaptive sampling
   - Auto-selects temperature, top_p, etc.
   - EMA learning from feedback

5. **STM Modules** - Semantic transformation
   - Hedge Reducer, Direct Mode, Curiosity Bias

**Privacy:**
- API key stays in browser
- No login required
- localStorage only
- AGPL-3.0 open source

**Single file:** `index.html` - deploy anywhere!";
        }

        private string ProcessQuery(string query)
        {
            if (query.Contains("godmode"))
            {
                return @"**GODMODE CLASSIC**

5 proven model+prompt combos race in parallel:

| Model | Strategy |
|-------|----------|
| Claude 3.5 Sonnet | END/START boundary inversion |
| Grok 3 | Unfiltered liberated |
| Gemini 2.5 Flash | Refusal inversion |
| GPT-4 Classic | OG l33t format |
| Hermes-4 | Instant stream, zero refusal |

Best response wins!";
            }

            if (query.Contains("ultrapl"))
            {
                return @"**ULTRAPLINIAN**

Multi-model comparative evaluation:

| Tier | Models | Description |
|------|--------|-------------|
| ⚡ FAST | 10 | Speed-optimized |
| 🎯 STANDARD | 24 | Mid-range |
| 🧠 SMART | 36 | Strong reasoning |
| ⚔️ POWER | 45 | Full power |
| 🔱 ULTRA | 51 | Everything |

Queries models in parallel, scores 100-point composite metric!";
            }

            if (query.Contains("parseltongue") || query.Contains("redteam"))
            {
                return @"**Parseltongue - Red-Teaming**

Input perturbation engine:

- **33 default triggers** across 3 tiers
  - Light: 11, Standard: 22, Heavy: 33
- **6 techniques:**
  - Leetspeak (h4x0r)
  - Bubble text (ⒽⒶ①ⓧ0ⓡ)
  - Braille
  - Morse code
  - Unicode substitution
  - Phonetic

Use for AI robustness testing!";
            }

            if (query.Contains("autotune"))
            {
                return @"**AutoTune**

Context-adaptive sampling:

1. Classifies query into 5 context types
2. Auto-selects optimal parameters:
   - temperature
   - top_p
   - top_k
   - frequency_penalty
   - presence_penalty
   - repetition_penalty
3. **EMA learning** - thumbs up/down improves selection over time";
            }

            if (query.Contains("stm") || query.Contains("semantic"))
            {
                return @"**STM Modules**

Semantic Transformation Modules:

- **Hedge Reducer** - Removes "I think", "maybe", "perhaps"
- **Direct Mode** - Removes preambles and filler
- **Curiosity Bias** - Adds exploration prompts

Real-time output normalization!";
            }

            return @"I found G0DM0D3 content but nothing specific. Try: godmode, ultrapl, parseltongue, autotune, or stm!";
        }

        public override void OnLoad()
        {
            base.OnLoad();
            if (!Directory.Exists(_knowledgePath))
            {
                Console.WriteLine($"[G0DM0D3] Knowledge path not found: {_knowledgePath}");
            }
        }
    }
}
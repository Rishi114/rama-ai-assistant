using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// minGPT skill -GPT implementation in PyTorch/char-level language modeling
    /// </summary>
    public class MinGPTSkill : SkillBase
    {
        private readonly string _knowledgePath;
        
        public override string Name => "minGPT";
        public override string Description => "Character-level GPT implementation in PyTorch - transformer language model";
        public override string[] Triggers => new[] { "mingpt", "min-gpt", "gpt", "transformer", "language model", "char-level", "karpathy" };

        public MinGPTSkill()
        {
            _knowledgePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BrainKnowledge", "minGPT");
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
            return @"🔮 **minGPT**

Andrej Karpathy's minimal GPT implementation in PyTorch.

**What it is:**
- Character-level language model
- Complete GPT architecture from scratch
- Educational implementation (~300 lines)
- Based on "Language Modeling with GPT"

**Key Components:**
- `mingpt/bpe.py` - Byte Pair Encoding
- `mingpt/model.py` - Transformer model
- `mingpt/trainer.py` - Training loop

**Use Cases:**
- Learn how GPT works under the hood
- Train on custom text (Shakespeare, code, etc.)
- Experiment with hyperparameters
- Build custom language models

**Run:**
```bash
jupyter notebook demo.ipynb
```";
        }

        private string ProcessQuery(string query)
        {
            if (query.Contains("bpe") || query.Contains("token"))
            {
                return @"**Byte Pair Encoding (BPE)**

minGPT uses BPE for tokenization:

1. Start with characters as tokens
2. Find most common pair
3. Merge into new token
4. Repeat until vocabulary size reached

Benefits:
- Handles any text (including unknown chars)
- Reasonable vocabulary size
- Subword units capture patterns";
            }

            if (query.Contains("transformer") || query.Contains("architecture"))
            {
                return @"**Transformer Architecture**

minGPT implements the full GPT architecture:

- **Multi-Head Self-Attention** - Attend to different positions
- **Feed-Forward** - Process attention output
- **Positional Encoding** - Add position information
- **Layer Norm** - Stabilize training
- **Residual Connections** - Help gradient flow

Key files:
- `model.py` - GPT model class
- Uses causal (masked) self-attention";
            }

            if (query.Contains("train") || query.Contains("demo"))
            {
                return @"**Training & Demo**

**demo.ipynb** - Character-level training demo:
- Load text dataset
- Train GPT model
- Generate new text

**generate.ipynb** - Text generation:
- Load trained model
- Sample from distribution
- Generate novel text

Run with Jupyter:
```bash
jupyter notebook demo.ipynb
```";
            }

            return @"I found minGPT content but nothing specific for that query. Try asking about BPE, transformer architecture, or training!";
        }

        public override void OnLoad()
        {
            base.OnLoad();
            if (!Directory.Exists(_knowledgePath))
            {
                Console.WriteLine($"[minGPT] Knowledge path not found: {_knowledgePath}");
            }
        }
    }
}
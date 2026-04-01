# 🔌 Rama Skills Guide

This guide explains how to create custom skills for Rama. Skills are the primary way to extend Rama's capabilities. Each skill is a self-contained module that handles a specific category of user requests.

## Table of Contents

1. [Skill Architecture Overview](#skill-architecture-overview)
2. [The ISkill Interface](#the-iskill-interface)
3. [Creating Your First Skill](#creating-your-first-skill)
4. [SkillBase: The Easy Way](#skillbase-the-easy-way)
5. [Step-by-Step: External Skill DLL](#step-by-step-external-skill-dll)
6. [Complete Example: Dictionary Skill](#complete-example-dictionary-skill)
7. [Advanced Topics](#advanced-topics)
8. [Best Practices](#best-practices)
9. [Troubleshooting](#troubleshooting)

---

## Skill Architecture Overview

Rama's skill system is based on a simple interface: `ISkill`. The Brain (Rama's central engine) receives user input, consults the Learner for pattern predictions, then asks the SkillManager to find the best matching skill. Skills are loaded in two ways:

- **Built-in skills** — Compiled into the main Rama assembly. Discovered via reflection at startup.
- **External skills** — DLL plugins loaded from the `Plugins/` directory. Can be added at runtime.

```
User Input → Brain → Learner (predict?) → SkillManager → ISkill.ExecuteAsync() → Response
```

---

## The ISkill Interface

Every skill must implement this interface:

```csharp
public interface ISkill
{
    // Display name shown in the sidebar
    string Name { get; }

    // Brief description of what the skill does
    string Description { get; }

    // Keywords that signal this skill should be considered
    string[] Triggers { get; }

    // Returns true if this skill can handle the given input
    bool CanHandle(string input);

    // Execute the skill and return a response string
    Task<string> ExecuteAsync(string input, Memory memory);

    // Called when the skill is loaded
    void OnLoad();

    // Called when the skill is unloaded
    void OnUnload();
}
```

### Member Details

| Member | Type | Purpose |
|--------|------|---------|
| `Name` | `string` | Unique display name. Used for routing, logging, and UI display. |
| `Description` | `string` | Human-readable description. Shown in the sidebar skill list. |
| `Triggers` | `string[]` | Keywords that activate this skill. Matching is case-insensitive. |
| `CanHandle()` | `method` | Deep analysis — called after trigger match. Return true to claim the input. |
| `ExecuteAsync()` | `method` | Main logic. Receives input and Memory reference. Returns response string. |
| `OnLoad()` | `method` | Initialization hook. Load configs, open connections, etc. |
| `OnUnload()` | `method` | Cleanup hook. Close connections, flush buffers, etc. |

---

## Creating Your First Skill

### 1. The Simplest Possible Skill

```csharp
using Rama.Core;
using Rama.Skills;

public class CoinFlipSkill : SkillBase
{
    public override string Name => "Coin Flip";
    public override string Description => "Flip a virtual coin";
    public override string[] Triggers => new[] { "flip", "coin flip", "heads or tails" };

    private readonly Random _random = new();

    public override Task<string> ExecuteAsync(string input, Memory memory)
    {
        var result = _random.Next(2) == 0 ? "Heads" : "Tails";
        return Task.FromResult($"🪙 The coin landed on **{result}**!");
    }
}
```

That's it! The `SkillBase` class handles `CanHandle()` for you (checks if any trigger is in the input). You just need to implement `ExecuteAsync()`.

---

## SkillBase: The Easy Way

`SkillBase` is an abstract base class that provides sensible defaults. Use it instead of implementing `ISkill` directly.

### What SkillBase Provides

- **`CanHandle()`** — Default implementation checks if any trigger word appears in the input.
- **`OnLoad()` / `OnUnload()`** — Empty by default. Override only if you need initialization/cleanup.
- **`ExtractAfterTrigger(input)`** — Helper to extract text after the trigger word. E.g., "open notepad" → "notepad".
- **`GetHelpText()`** — Generates a formatted help string from your Name, Description, and Triggers.

### When to Override CanHandle

Override `CanHandle()` when you need more specific matching logic:

```csharp
public override bool CanHandle(string input)
{
    var lower = input.ToLowerInvariant();
    // Only handle "flip" at the start of the input
    return lower.StartsWith("flip ") || lower == "flip";
}
```

---

## Step-by-Step: External Skill DLL

This walkthrough creates a standalone skill DLL that can be dropped into Rama's `Plugins/` folder.

### Step 1: Create the Project

```bash
# Create a new class library
dotnet new classlib -n RamaDiceSkill -f net8.0
cd RamaDiceSkill
```

### Step 2: Add Dependencies

Edit the `.csproj` file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <!-- Reference the Rama project or copy the interface files -->
  <ItemGroup>
    <Reference Include="Rama">
      <HintPath>..\path\to\Rama.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

**Alternative: Copy interface files directly**

If you don't want to reference the full Rama project, copy these two files into your project:
- `ISkill.cs`
- `Memory.cs` (only the public API surface is needed)

### Step 3: Write the Skill

```csharp
// DiceSkill.cs
using Rama.Core;
using Rama.Skills;

namespace RamaDiceSkill
{
    public class DiceSkill : SkillBase
    {
        public override string Name => "Dice";
        public override string Description => "Roll dice with standard RPG notation";
        public override string[] Triggers => new[] { "roll", "dice", "d20", "d6" };

        private readonly Random _random = new();

        public override bool CanHandle(string input)
        {
            var lower = input.ToLowerInvariant();
            // Match patterns like "roll 2d6", "roll d20", "dice 3d8"
            return lower.Contains("roll") || 
                   System.Text.RegularExpressions.Regex.IsMatch(lower, @"\d*d\d+");
        }

        public override Task<string> ExecuteAsync(string input, Memory memory)
        {
            // Extract dice notation: 2d6, d20, 3d8+5, etc.
            var match = System.Text.RegularExpressions.Regex.Match(
                input.ToLowerInvariant(), @"(\d*)d(\d+)(?:\+(\d+))?(?:-(\d+))?");

            if (!match.Success)
                return Task.FromResult(
                    "Try: \"roll 2d6\" or \"roll d20+3\" or \"roll 4d6-1\"");

            var numDice = string.IsNullOrEmpty(match.Groups[1].Value) 
                ? 1 
                : int.Parse(match.Groups[1].Value);
            var sides = int.Parse(match.Groups[2].Value);
            var bonus = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
            var penalty = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 0;

            if (numDice > 100)
                return Task.FromResult("That's too many dice! Maximum is 100.");
            if (sides > 1000)
                return Task.FromResult("Maximum sides per die is 1000.");

            var rolls = new List<int>();
            for (int i = 0; i < numDice; i++)
                rolls.Add(_random.Next(1, sides + 1));

            var total = rolls.Sum() + bonus - penalty;
            var rollsStr = string.Join(", ", rolls);
            var modifier = bonus > 0 ? $" + {bonus}" : penalty > 0 ? $" - {penalty}" : "";

            return Task.FromResult(
                $"🎲 Rolling {numDice}d{sides}{modifier}...\n" +
                $"Individual rolls: [{rollsStr}]\n" +
                $"**Total: {total}**");
        }
    }
}
```

### Step 4: Build

```bash
dotnet build -c Release
```

### Step 5: Deploy

Copy the compiled DLL to Rama's Plugins directory:

```bash
# From your Rama installation directory
cp bin/Release/net8.0/RamaDiceSkill.dll Plugins/
```

### Step 6: Verify

Restart Rama (or it will auto-load on next start). Open the sidebar — you should see "Dice" listed as a skill.

Test it:
- "roll d20"
- "roll 2d6+3"
- "roll 4d6"

---

## Complete Example: Dictionary Skill

A more advanced example that uses an external API:

```csharp
using System.Net.Http;
using Newtonsoft.Json;
using Rama.Core;
using Rama.Skills;

namespace RamaDictionarySkill
{
    /// <summary>
    /// Looks up word definitions using the free dictionaryapi.dev API.
    /// </summary>
    public class DictionarySkill : SkillBase
    {
        public override string Name => "Dictionary";
        public override string Description => "Look up word definitions, pronunciations, and examples";
        public override string[] Triggers => new[] { 
            "define", "definition", "meaning", "what does", "what is the meaning",
            "dictionary", "lookup", "look up word"
        };

        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

        public override bool CanHandle(string input)
        {
            var lower = input.ToLowerInvariant();
            return lower.StartsWith("define ") || 
                   lower.StartsWith("meaning of ") ||
                   lower.Contains("what does") && lower.Contains("mean") ||
                   Triggers.Any(t => lower.Contains(t));
        }

        public override async Task<string> ExecuteAsync(string input, Memory memory)
        {
            var word = ExtractWord(input);
            if (string.IsNullOrWhiteSpace(word))
                return "What word should I look up? Example: \"define serendipity\"";

            try
            {
                var url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{Uri.EscapeDataString(word)}";
                var response = await _httpClient.GetStringAsync(url);
                var entries = JsonConvert.DeserializeObject<List<DictionaryEntry>>(response);

                if (entries == null || entries.Count == 0)
                    return $"No definition found for **{word}**.";

                var entry = entries[0];
                var sb = new System.Text.StringBuilder();

                sb.AppendLine($"📖 **{entry.Word}**");
                
                // Phonetics
                if (entry.Phonetics?.Count > 0)
                {
                    var phonetic = entry.Phonetics.FirstOrDefault(p => !string.IsNullOrEmpty(p.Text));
                    if (phonetic != null)
                        sb.AppendLine($"🔊 {phonetic.Text}");
                }

                // Meanings
                if (entry.Meanings?.Count > 0)
                {
                    foreach (var meaning in entry.Meanings.Take(3))
                    {
                        sb.AppendLine($"\n**{meaning.PartOfSpeech}:**");
                        foreach (var def in meaning.Definitions.Take(2))
                        {
                            sb.AppendLine($"  • {def.Definition}");
                            if (!string.IsNullOrEmpty(def.Example))
                                sb.AppendLine($"    _Example: \"{def.Example}\"_");
                        }
                    }
                }

                return sb.ToString();
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                return $"No definition found for **{word}**. Check the spelling?";
            }
            catch (Exception ex)
            {
                return $"Dictionary lookup failed: {ex.Message}";
            }
        }

        private string ExtractWord(string input)
        {
            var lower = input.ToLowerInvariant();
            var prefixes = new[] { "define ", "definition of ", "meaning of ", 
                                   "what does ", "look up ", "lookup " };
            foreach (var prefix in prefixes)
            {
                if (lower.StartsWith(prefix))
                {
                    var word = input.Substring(prefix.Length).Trim();
                    // Remove trailing " mean" or " means"
                    word = System.Text.RegularExpressions.Regex.Replace(
                        word, @"\s+mean(s)?\??$", "", 
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    return word;
                }
            }
            return "";
        }
    }

    // API response models
    public class DictionaryEntry
    {
        [JsonProperty("word")]
        public string Word { get; set; } = "";
        
        [JsonProperty("phonetics")]
        public List<Phonetic>? Phonetics { get; set; }
        
        [JsonProperty("meanings")]
        public List<Meaning>? Meanings { get; set; }
    }

    public class Phonetic
    {
        [JsonProperty("text")]
        public string? Text { get; set; }
    }

    public class Meaning
    {
        [JsonProperty("partOfSpeech")]
        public string PartOfSpeech { get; set; } = "";
        
        [JsonProperty("definitions")]
        public List<Definition> Definitions { get; set; } = new();
    }

    public class Definition
    {
        [JsonProperty("definition")]
        public string Definition { get; set; } = "";
        
        [JsonProperty("example")]
        public string? Example { get; set; }
    }
}
```

---

## Advanced Topics

### Accessing User Memory

The `Memory` object passed to `ExecuteAsync` gives you access to:

```csharp
// Store a preference
memory.SetPreference("my_skill_setting", "value");

// Get a preference
var setting = memory.GetPreference("my_skill_setting");

// Get recent conversation history
foreach (var interaction in memory.RecentInteractions.TakeLast(5))
{
    // interaction.UserInput, interaction.Response, interaction.SkillUsed
}

// Store a learned pattern (for the learning engine)
memory.StoreLearnedPattern("my pattern", "My Skill", confidence: 0.7);
```

### Async Operations

Skills can perform I/O operations. `ExecuteAsync` is naturally async:

```csharp
public override async Task<string> ExecuteAsync(string input, Memory memory)
{
    using var client = new HttpClient();
    var response = await client.GetStringAsync("https://api.example.com/data");
    return ProcessResponse(response);
}
```

### Skill Priority

When multiple skills can handle the same input, Rama uses this priority:

1. **Learned patterns** — If the Learner has a high-confidence prediction, that skill is used.
2. **Trigger matching** — Skills registered first have higher priority for trigger matching.
3. **CanHandle specificity** — Override `CanHandle()` for precise control.

### Skill Lifecycle

```
SkillManager.LoadBuiltInSkills() → OnLoad() for each
    ↓
User input → CanHandle() → ExecuteAsync()
    ↓
App shutdown → OnUnload() for each
```

---

## Best Practices

### DO ✅

- **Keep skills focused.** One skill = one capability domain.
- **Use descriptive triggers.** "weather", "forecast", "temperature" — cover user mental models.
- **Handle errors gracefully.** Return helpful error messages, not stack traces.
- **Override `OnUnload()`** if you open files, connections, or timers.
- **Use `ExtractAfterTrigger()`** for parsing. It handles edge cases.
- **Store preferences** using `memory.SetPreference()` for personalization.
- **Keep responses concise.** Use formatting (**bold**, lists) for readability.

### DON'T ❌

- **Don't block the UI thread.** All I/O should be async.
- **Don't swallow exceptions.** Let errors surface with helpful messages.
- **Don't make triggers too broad.** "use" or "the" as triggers will cause false matches.
- **Don't modify files outside user data.** Never touch system files.
- **Don't hardcode paths.** Use `AppDomain.CurrentDomain.BaseDirectory` or `Environment.GetFolderPath()`.

---

## Troubleshooting

### Skill Not Showing Up

1. Check that your DLL is in the `Plugins/` directory
2. Ensure your class implements `ISkill` (or extends `SkillBase`)
3. Verify it has a **public parameterless constructor**
4. Check the `Data/error.log` file for loading errors

### Skill Not Triggering

1. Test `CanHandle()` in isolation — does it return true for your test input?
2. Check trigger words are lowercase in the array (matching is case-insensitive)
3. Another skill with broader triggers may be matching first
4. The Learner may be predicting a different skill — check learned patterns in the sidebar

### Build Errors

- Make sure you're targeting `net8.0`
- The `ISkill` interface uses `Task<string>` — ensure you have `using System.Threading.Tasks`
- If referencing Rama project, check the project reference path

### Runtime Errors

- Check `Data/error.log` for stack traces
- Test your `ExecuteAsync()` method with sample inputs
- Use `System.Diagnostics.Debug.WriteLine()` for logging (visible in VS Output window)

---

## File Reference

| File | Purpose |
|------|---------|
| `Skills/ISkill.cs` | The interface every skill implements |
| `Skills/SkillBase.cs` | Abstract base class with defaults |
| `Core/Memory.cs` | Access to user preferences, history, learning |
| `Core/SkillManager.cs` | Loads and manages skills |
| `Core/Brain.cs` | Routes input to skills |

For the latest interface definitions, see the source files in `Rama/Skills/` and `Rama/Core/`.

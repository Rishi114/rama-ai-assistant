# 🧠 Rama Self-Learning Guide

This document explains how Rama's self-learning system works, what data it collects, how it uses that data, and how to configure or extend the learning behavior.

## Table of Contents

1. [Overview](#overview)
2. [How It Works](#how-it-works)
3. [What Rama Learns](#what-rama-learns)
4. [The Learning Pipeline](#the-learning-pipeline)
5. [Database Schema](#database-schema)
6. [Confidence Scoring](#confidence-scoring)
7. [Viewing Learned Data](#viewing-learned-data)
8. [Privacy & Data](#privacy--data)
9. [Extending the Learning System](#extending-the-learning-system)

---

## Overview

Rama's learning system is designed around one principle: **every interaction should make the assistant slightly better at its job**. Unlike cloud-based AI assistants that rely on massive pre-trained models, Rama learns from *your specific usage patterns* on *your machine*. It gets more personalized the more you use it.

The learning is backed by a local SQLite database (`Data/rama_memory.db`) and operates through three complementary mechanisms:

1. **Pattern Recognition** — Maps user input patterns to skills
2. **Frequency Analysis** — Tracks which skills you use most
3. **Preference Learning** — Remembers your choices and defaults

## How It Works

```
User Input → Brain.ProcessInputAsync()
                ↓
         Learner.PredictSkill()  ← Checks learned patterns
                ↓
         SkillManager.FindMatchingSkills()  ← Trigger word matching
                ↓
         Skill.ExecuteAsync() → Response
                ↓
         Learner.RecordAndLearn()  ← Records for future learning
                ↓
         Memory.StoreInteraction() → SQLite DB
```

### The Four Phases

**Phase 1: Prediction**
Before executing any skill, Rama checks its learned patterns database to see if it has a high-confidence prediction for which skill should handle this input. If a prediction exceeds the confidence threshold (0.3), that skill is used directly.

**Phase 2: Matching**
If no learned prediction exists, Rama falls back to trigger-word matching. Each skill declares keywords; if any keyword appears in the user's input, that skill is a candidate.

**Phase 3: Execution**
The selected skill processes the input and returns a response.

**Phase 4: Learning**
After every interaction, the Learner records:
- The raw interaction (input, response, skill used, timestamp)
- A normalized pattern derived from the input
- Any user preferences inferred from the interaction

## What Rama Learns

### 1. Command Patterns

When you say "open notepad" 3 times, Rama learns that "notepad" should trigger the App Launcher skill. The pattern is stored as a normalized version of your input (filler words removed).

**Example learned patterns:**

| Pattern | Skill | Confidence |
|---------|-------|------------|
| notepad | App Launcher | 0.45 |
| weather | Weather | 0.50 |
| calculate | Calculator | 0.35 |
| take a break | Reminders | 0.40 |

### 2. Usage Frequency

Rama tracks how often each skill is used. This data is shown in the sidebar stats and influences future routing decisions (more frequently used skills get a slight priority boost).

**Example usage stats:**

| Skill | Uses |
|-------|------|
| Calculator | 47 |
| Web Search | 32 |
| App Launcher | 28 |
| Notes | 15 |
| Weather | 12 |

### 3. User Preferences

Rama remembers specific preferences from your usage:

| Preference Key | Value | Learned From |
|---------------|-------|-------------|
| `preferred_search_engine` | google | You always use Google |
| `preferred_location` | San Francisco | You asked about SF weather |
| `preferred_app_chrome` | 12 | You launched Chrome 12 times |
| `notes_usage_count` | 23 | You use Notes frequently |

### 4. Interaction History

Every conversation is stored with:
- User input text
- Rama's response
- Which skill was used
- Timestamp
- User feedback (if provided)

## The Learning Pipeline

### Step 1: Normalize Input

The input is cleaned by removing filler words:

```
"Hey Rama, could you please open notepad for me"
→ "open notepad"
```

Filler words removed: `please`, `can you`, `could you`, `would you`, `i want to`, `hey`, `hi`, `hello`, `rama`, `tell me`, `show me`

### Step 2: Calculate Initial Confidence

New patterns start with a confidence score based on their length and specificity:

```csharp
confidence = Math.Min(0.5, 0.2 + (pattern.Length * 0.01));
```

- Short patterns ("hi"): 0.20
- Medium patterns ("notepad"): 0.27
- Long patterns ("my favorite text editor"): 0.45

### Step 3: Store or Update Pattern

If the pattern already exists for the same skill, its `UseCount` is incremented and confidence is recalculated:

```csharp
newConfidence = Math.Min(1.0, oldConfidence + (useCount * 0.05));
```

Each repetition adds 5% confidence, capped at 100%.

### Step 4: Learn Preferences

Specific skill interactions teach Rama about your preferences:

- **App Launcher** — Tracks which apps you launch and how often
- **Web Search** — Remembers your preferred search engine
- **Weather** — Can learn your default location
- **Notes/Calculator** — Tracks usage frequency for priority

### Step 5: Update In-Memory Context

The most recent 50 interactions are kept in RAM for quick access. Skills can query this context to provide responses that reference previous conversations.

## Database Schema

### Interactions Table

```sql
CREATE TABLE Interactions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserInput TEXT NOT NULL,
    Response TEXT NOT NULL,
    SkillUsed TEXT NOT NULL DEFAULT '',
    Timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Feedback INTEGER NOT NULL DEFAULT 0  -- 0=none, 1=positive, -1=negative
);
```

**Feedback values:**
- `0` — No feedback given (default)
- `1` — Positive feedback (the response was helpful)
- `-1` — Negative feedback (the response was wrong or unhelpful)

### LearnedPatterns Table

```sql
CREATE TABLE LearnedPatterns (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Pattern TEXT NOT NULL,          -- Normalized input pattern
    Skill TEXT NOT NULL,            -- Which skill this maps to
    Confidence REAL NOT NULL DEFAULT 0.0,  -- 0.0 to 1.0
    UseCount INTEGER NOT NULL DEFAULT 0,    -- How many times reinforced
    LastUsed DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

### UserPreferences Table

```sql
CREATE TABLE UserPreferences (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Key TEXT NOT NULL UNIQUE,       -- Preference name
    Value TEXT NOT NULL DEFAULT '',  -- Preference value
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

### CustomSkills Table

```sql
CREATE TABLE CustomSkills (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,             -- Skill display name
    Path TEXT NOT NULL,             -- Full path to the DLL
    Enabled INTEGER NOT NULL DEFAULT 1,
    AddedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

## Confidence Scoring

Confidence is a float from 0.0 to 1.0 that represents how much Rama trusts a learned pattern.

### Confidence Levels

| Range | Meaning | Behavior |
|-------|---------|----------|
| 0.0 - 0.3 | Untrusted | Pattern is stored but not used for auto-routing |
| 0.3 - 0.5 | Low trust | Pattern may be used as a hint |
| 0.5 - 0.7 | Moderate | Pattern is used for routing with other signals |
| 0.7 - 1.0 | High trust | Pattern is used directly for routing |

### How Confidence Changes

**Increases:**
- Pattern is matched and used again (+0.05 per use)
- Positive feedback (+0.15)
- High specificity of input (+0.01 per character)

**Decreases:**
- Negative feedback (-0.20)
- Skill fails to execute (-0.10)

### The Prediction Threshold

The `Learner.MinConfidenceThreshold` is set to 0.3 by default. This means patterns need at least 2-3 consistent uses before they influence routing. You can adjust this in `Core/Learner.cs`.

## Viewing Learned Data

### In the UI

The sidebar shows:
- **Skills list** — All available skills
- **Learned Patterns** — Top 10 patterns by confidence
- **Stats** — Total interactions, pattern count, most-used skill

### In the Database

You can inspect the SQLite database directly:

```bash
# Using sqlite3 command-line tool
sqlite3 Data/rama_memory.db

# View recent interactions
SELECT * FROM Interactions ORDER BY Timestamp DESC LIMIT 10;

# View top learned patterns
SELECT Pattern, Skill, Confidence, UseCount FROM LearnedPatterns 
ORDER BY Confidence DESC LIMIT 20;

# View all preferences
SELECT * FROM UserPreferences;

# Skill usage breakdown
SELECT SkillUsed, COUNT(*) as Uses FROM Interactions 
WHERE SkillUsed != '' GROUP BY SkillUsed ORDER BY Uses DESC;
```

## Privacy & Data

Rama is **100% local**. All learning data stays on your machine:

- No data is sent to any server
- No telemetry or analytics
- No cloud sync
- The SQLite database is stored in `Data/rama_memory.db` next to the executable
- Notes and reminders are stored in JSON files in `Data/`

### Data You Can Delete

| File | What It Contains |
|------|-----------------|
| `Data/rama_memory.db` | All interactions, patterns, preferences |
| `Data/notes.json` | Your saved notes |
| `Data/reminders.json` | Active reminders |
| `Data/error.log` | Error messages |

To reset Rama completely, delete the entire `Data/` directory. Rama will recreate the database on next launch.

## Extending the Learning System

### Adding Custom Learning Logic

You can extend the `Learner` class to add custom learning behaviors:

```csharp
// In your skill's ExecuteAsync method:
public override Task<string> ExecuteAsync(string input, Memory memory)
{
    // Store a custom preference
    memory.SetPreference("my_skill_last_query", input);
    
    // Track custom metrics
    var count = memory.GetPreference("my_skill_query_count");
    var newCount = (int.TryParse(count, out var c) ? c : 0) + 1;
    memory.SetPreference("my_skill_query_count", newCount.ToString());
    
    // Store a learned pattern for this specific use case
    memory.StoreLearnedPattern(
        pattern: ExtractKeyPhrase(input),
        skill: "My Skill",
        confidence: 0.4
    );
    
    return Task.FromResult("...");
}
```

### Providing User Feedback

The feedback system can be extended to accept user ratings. Currently, interactions store a `Feedback` field (0, 1, or -1). The UI could be extended with thumbs up/down buttons:

```csharp
// Give positive feedback on the last interaction
memory.SetFeedback(interactionIndex: -1, feedback: 1);

// Give negative feedback
memory.SetFeedback(interactionIndex: -1, feedback: -1);
```

### Custom Pattern Matching

If the default pattern recognition isn't sufficient for your skill, you can implement custom matching:

```csharp
public override bool CanHandle(string input)
{
    // Your custom logic here
    var tokens = input.ToLowerInvariant().Split(' ');
    return tokens.Contains("my") && tokens.Contains("command");
}
```

---

## Summary

Rama's learning system is simple but effective:

1. **Records** every interaction in SQLite
2. **Recognizes** patterns in repeated usage
3. **Remembers** your preferences and defaults
4. **Routes** future inputs using learned confidence scores
5. **Improves** over time as patterns are reinforced

The system is designed to be transparent (you can see everything in the sidebar and database), private (all local), and extensible (skills can add their own learning logic).

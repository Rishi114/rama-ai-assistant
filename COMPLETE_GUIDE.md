# 🚀 Rama — Complete Guide to Making Her Perfect

## Everything you need to know to make Rama the most intelligent AI assistant ever.

---

## 📑 Table of Contents

1. [Quick Start](#quick-start)
2. [Adding Skills (3 Easy Methods)](#adding-skills)
3. [Making Her Smarter](#making-her-smarter)
4. [Local AI Setup](#local-ai-setup)
5. [Customizing Personality](#customizing-personality)
6. [Memory & Learning](#memory--learning)
7. [Voice Setup](#voice-setup)
8. [Multi-Language](#multi-language)
9. [Knowledge Base](#knowledge-base)
10. [Advanced: Creating Plugins](#advanced-creating-plugins)
11. [Troubleshooting](#troubleshooting)

---

## Quick Start

### 1. Build & Run
```
1. Download from: https://github.com/Rishi114/rama-ai-assistant
2. Open Rama.sln in Visual Studio 2022
3. Press Ctrl+F5
4. Done! 🎉
```

### 2. First Commands to Try
```
hello                          → Rama introduces herself
help                           → See all commands
skills                         → List all abilities
set sass max                   → Maximum attitude 🔥
set language hindi             → Switch language
install ollama                 → Set up local AI
```

---

## Adding Skills

### Method 1: JSON Skills (Easiest — No Coding!)

Open the skills config file:
```
%APPDATA%\Rama\Skills\skills.json
```

Add a skill like this:
```json
{
  "skills": [
    {
      "name": "Motivator",
      "description": "Gives motivational quotes",
      "triggers": ["motivate me", "inspire me", "pep talk"],
      "responses": [
        "You've got this! 💪",
        "Believe in yourself! ✨",
        "Every expert was once a beginner! 🚀"
      ]
    },
    {
      "name": "Alarm",
      "description": "Morning alarm",
      "triggers": ["wake me up", "set alarm", "morning alarm"],
      "responses": [
        "⏰ Alarm set! I'll remind you.",
        "🔔 Got it! Wake up call incoming."
      ]
    },
    {
      "name": "DJ",
      "description": "Music recommendations",
      "triggers": ["play music", "recommend song", "music"],
      "responses": [
        "🎵 Try 'Bohemian Rhapsody' by Queen!",
        "🎶 How about 'Blinding Lights' by The Weeknd?",
        "🎵 'Hotel California' never gets old!"
      ]
    }
  ]
}
```

Then in Rama: `reload skills` → Done!

**Placeholders you can use:**
- `{input}` — What the user typed
- `{time}` — Current time (14:30)
- `{date}` — Today's date (2024-04-01)
- `{day}` — Day of week (Monday)

---

### Method 2: Quick Skills (From Chat!)

Just talk to Rama:
```
create quick skill hello that says Hey there! 👋
create quick skill bye that says See you later! 👋
create quick skill joke that says Why do programmers prefer dark mode? Because light attracts bugs! 🐛
```

Each creates a JSON file automatically. Edit it later for more options.

---

### Method 3: C# Code Skills (Most Powerful)

Create `MySkill.cs` in `%APPDATA%\Rama\Skills\`:

```csharp
using System;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    public class MySkill : SkillBase
    {
        public override string Name => "My Skill";
        public override string Description => "Does something awesome";
        public override string[] Triggers => new[] { "do something", "my command" };

        public override bool CanHandle(string input)
        {
            return input.ToLowerInvariant().Contains("do something");
        }

        public override Task<string> ExecuteAsync(string input, Memory memory)
        {
            string command = ExtractCommand(input);

            // Your logic here!
            // - Call APIs
            // - Read/write files
            // - Control hardware
            // - Run scripts
            // - Anything!

            return Task.FromResult($"✅ You said: {command}");
        }
    }
}
```

Build and restart Rama. Done!

---

### Method 4: DLL Plugins (For Sharing)

1. Create a Class Library project
2. Reference Rama project
3. Implement `ISkill`
4. Build → Copy DLL to `%APPDATA%\Rama\Skills\`
5. Restart Rama

Share your DLLs with friends!

---

## Making Her Smarter

### 1. Install Local AI (Ollama)

The #1 way to make Rama smarter — give her a real brain:

```
1. Say "install ollama" in Rama
2. Download Ollama from https://ollama.com
3. Open terminal: ollama pull phi3:mini
4. Say "ai status" in Rama
5. Done! Rama now has a local AI brain! 🧠
```

**Best models:**
| Command | Model | Size | Best For |
|---------|-------|------|----------|
| `ollama pull phi3:mini` | Phi-3 Mini | 2.2GB | Fast, smart, starter |
| `ollama pull codellama:7b` | Code Llama | 3.8GB | Programming |
| `ollama pull llama3.1:8b` | Llama 3.1 | 4.7GB | Best quality |
| `ollama pull mistral:7b` | Mistral | 4.1GB | All-rounder |
| `ollama pull gemma2:2b` | Gemma 2 | 1.6GB | Ultra-fast |

### 2. Teach Her Facts

```
remember my birthday is January 15
remember I live in New York
remember my favorite food is pizza
remember I work at Google
remember my dog's name is Max
```

She stores these and uses them in conversations!

### 3. Feed Her Knowledge

```
learn from url https://article.com/post
learn from blog https://blog.example.com
learn from video https://youtube.com/watch?v=...
learn from book C:\Books\textbook.txt
learn from script C:\code\app.py
memorize this: [paste anything]
```

### 4. Let Her Learn From You

The more you use Rama, the smarter she gets:
- She learns which skills you use most
- She learns your preferred phrasing
- She learns your schedule patterns
- She adapts her personality to your style

---

## Local AI Setup

### What is Local AI?

Instead of sending your messages to the cloud, Rama runs AI models **directly on your PC**:
- 🔒 100% private — nothing leaves your computer
- ⚡ No latency — instant responses
- 🌐 Works offline — no internet needed
- 💰 Free — no API costs ever

### Setup Steps

```
1. Install Ollama: https://ollama.com/download/windows
2. Open terminal/CMD
3. Run: ollama pull phi3:mini (downloads ~2.2GB)
4. Rama auto-detects it!
5. Say "ai status" to verify
```

### Using Different Models

```
download model phi3:mini         → Download a model
switch model llama3.1:8b         → Change active model
list models                      → See installed models
recommend                        → Get suggestions
```

---

## Customizing Personality

### Sass Levels

```
set sass off      → Polite corporate assistant 😐
set sass low      → Professional with hint of spice 🎀
set sass medium   → Balanced (default) ⚖️
set sass high     → Getting spicy 🌶️
set sass max      → Full attitude, no filter 🔥
```

### Languages

```
set language english      → English (default)
set language hindi        → हिन्दी
set language spanish      → Español
set language french       → Français
set language japanese     → 日本語
set language chinese      → 中文
set language arabic       → العربية
set language korean       → 한국어
set language russian      → Русский
... (35 total languages)
show languages            → See all options
```

### Voice

```
Click 🎤 button           → Toggle voice mode
Say "stop listening"      → Pause voice
Say "be quiet"            → Stop speaking
```

---

## Memory & Learning

### What Rama Remembers

| Type | What | Duration |
|------|------|----------|
| **Interactions** | Every chat | Permanent |
| **Patterns** | Your habits | Permanent |
| **Facts** | Things you teach | Permanent |
| **Preferences** | Your settings | Permanent |
| **Context** | Conversation topics | Session |

### Commands

```
what have you learned      → Learning statistics
show knowledge             → Knowledge base
search knowledge [topic]   → Find specific info
remember [fact]            → Teach a fact
my skills                  → List custom skills
```

### Data Location

All data is stored at: `%APPDATA%\Rama\`
```
Rama/
├── rama_memory.db        → Learning database
├── Memory/
│   ├── preferences.json  → Your settings
│   ├── facts.json        → Things you taught
│   ├── patterns.json     → Learned behaviors
│   └── personality.json  → Sass levels etc
├── Knowledge/
│   └── knowledge.json    → Ingested content
├── Skills/
│   ├── skills.json       → Easy skills config
│   ├── *.skill.json      → Individual skills
│   └── *.cs              → Code skills
└── notes.json            → Your notes
```

---

## Voice Setup

### Windows Speech Recognition

Rama uses Windows built-in speech:
1. Windows Settings → Time & Language → Speech
2. Download speech language pack
3. Set default voice
4. Rama auto-detects it!

### Voice Commands

```
🎤 Click mic button      → Start/stop listening
"hey rama"               → Wake word
"stop listening"         → Pause
"be quiet"               → Stop talking
```

---

## Multi-Language

Rama speaks 35 languages:

```
set language [name]      → Switch language
show languages           → See all options
```

**Supported:** English, Spanish, French, German, Hindi, Chinese, Japanese, Korean, Arabic, Portuguese, Russian, Italian, Dutch, Turkish, Vietnamese, Thai, Indonesian, Polish, Swedish, Norwegian, Danish, Finnish, Czech, Romanian, Ukrainian, Hebrew, Bengali, Urdu, Tamil, Telugu, Marathi, Gujarati, Punjabi, Malayalam, Kannada

**Note:** Rama understands commands in ANY language regardless of setting!

---

## Knowledge Base

### Learning Sources

```
learn from url [url]           → Any webpage
learn from blog [url]          → Blog posts
learn from video [url]         → YouTube videos
learn from book [path]         → Book files
learn from script [path]       → Code files
learn from file [path]         → Any text file
memorize this: [paste]         → Pasted content
```

### Accessing Knowledge

```
show knowledge                 → View all learned
search knowledge [topic]       → Find specific info
clear knowledge                → Reset everything
```

---

## Advanced: Creating Plugins

### Full C# Skill Template

```csharp
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    public class AdvancedSkill : SkillBase
    {
        public override string Name => "Advanced Skill";
        public override string Description => "Full-featured skill example";
        public override string[] Triggers => new[] { "advanced", "pro skill" };

        public override bool CanHandle(string input)
        {
            // Custom matching logic
            string lower = input.ToLowerInvariant();
            return lower.Contains("advanced") || lower.Contains("pro");
        }

        public override async Task<string> ExecuteAsync(string input, Memory memory)
        {
            // 1. Extract information from input
            string command = ExtractCommand(input);

            // 2. Check memory for context
            string? lastFact = memory.GetFact("last_topic");

            // 3. Do something (call API, run command, etc.)
            string result = await DoSomethingAsync(command);

            // 4. Store in memory
            memory.StoreFact("last_topic", command);
            memory.Remember("assistant", result);

            // 5. Return response
            return $"✅ Result: {result}";
        }

        private async Task<string> DoSomethingAsync(string input)
        {
            // Example: Run a system command
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c echo {input}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            return output.Trim();
        }

        // Optional: Called when skill loads
        public override void OnLoad()
        {
            // Initialize resources
        }

        // Optional: Called when skill unloads
        public override void OnUnload()
        {
            // Cleanup resources
        }
    }
}
```

### Skill Ideas to Build

| Skill | What It Does | Triggers |
|-------|-------------|----------|
| **Email** | Read/send emails | "check email", "send email" |
| **Calendar** | Manage events | "schedule", "meeting" |
| **Spotify** | Control music | "play music", "pause" |
| **Smart Home** | IoT control | "turn on lights" |
| **Git** | Version control | "git status", "commit" |
| **Docker** | Container management | "docker ps", "run container" |
| **SSH** | Remote servers | "ssh into server" |
| **Database** | SQL queries | "query database" |
| **API** | REST calls | "call api" |
| **Automation** | Run scripts | "run script" |

---

## Troubleshooting

### Rama won't start
- Install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
- Open in Visual Studio, rebuild

### Voice not working
- Settings → Time & Language → Speech → Download language pack
- Check microphone permissions

### Local AI not connecting
- Install Ollama: https://ollama.com
- Run `ollama serve` in terminal
- Run `ollama pull phi3:mini`
- Restart Rama

### Skills not loading
- Check JSON syntax (use jsonlint.com)
- Say `reload skills`
- Check `%APPDATA%\Rama\Skills\` folder

### Where is my data?
```
%APPDATA%\Rama\
```
Type this in Windows Explorer to open it.

---

## 🎯 Make Rama Perfect Checklist

- [ ] Build and run Rama
- [ ] Install Ollama for local AI
- [ ] Download a model (`ollama pull phi3:mini`)
- [ ] Set your preferred language
- [ ] Set your sass level
- [ ] Add 3+ custom skills via JSON
- [ ] Teach her 10+ facts about you
- [ ] Feed her knowledge from URLs/books
- [ ] Enable voice mode
- [ ] Use her daily to build patterns

**The more you use Rama, the smarter she gets!** 🧠

---

*Built with ❤️ by Rama AI — github.com/Rishi114/rama-ai-assistant*

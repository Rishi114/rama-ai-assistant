# 🚀 Rama AI Assistant — Complete Setup Guide

## Table of Contents
1. [System Requirements](#system-requirements)
2. [Installation Steps](#installation-steps)
3. [First Run](#first-run)
4. [Feature Setup](#feature-setup)
5. [Commands Reference](#commands-reference)
6. [Troubleshooting](#troubleshooting)
7. [Future Roadmap](#future-roadmap)

---

## System Requirements

### Minimum
- **OS:** Windows 10 (64-bit) or Windows 11
- **RAM:** 4 GB
- **Storage:** 500 MB (app) + space for AI models
- **CPU:** Any modern processor
- **.NET:** .NET 8.0 Runtime

### Recommended (for Local AI)
- **OS:** Windows 10/11 (64-bit)
- **RAM:** 8 GB+ (16 GB for larger models)
- **Storage:** 10 GB+ (for AI models)
- **GPU:** NVIDIA GPU with 4GB+ VRAM (optional, for faster AI)

---

## Installation Steps

### Step 1: Install Prerequisites

#### 1a. Install .NET 8 SDK
1. Go to: https://dotnet.microsoft.com/download/dotnet/8.0
2. Download **.NET 8.0 SDK** (not just Runtime)
3. Run the installer
4. Verify: Open CMD, type `dotnet --version` → should show `8.x.x`

#### 1b. Install Visual Studio 2022
1. Go to: https://visualstudio.microsoft.com/
2. Download **Visual Studio 2022 Community** (free)
3. During install, select:
   - ✅ **.NET desktop development**
   - ✅ **.NET 8.0 Runtime**
4. Complete installation

#### 1c. (Optional) Install Ollama for Local AI
1. Go to: https://ollama.com/download/windows
2. Download and install Ollama
3. After install, open CMD and run:
   ```
   ollama pull phi3:mini
   ```
4. Wait for download (2.2 GB)

### Step 2: Download Rama

#### Option A: Download ZIP (Easiest)
1. Go to: https://github.com/Rishi114/rama-ai-assistant
2. Click the green **"Code"** button
3. Click **"Download ZIP"**
4. Extract the ZIP to a folder (e.g., `C:\Projects\Rama\`)

#### Option B: Git Clone
```bash
git clone https://github.com/Rishi114/rama-ai-assistant.git
cd rama-ai-assistant
```

### Step 3: Build Rama

#### Option A: Visual Studio (Recommended)
1. Open **Visual Studio 2022**
2. Click **"Open a project or solution"**
3. Navigate to the extracted folder
4. Select **`Rama.sln`**
5. Wait for NuGet packages to restore
6. Press **Ctrl + F5** (Run without debugging)
7. Rama launches! 🎉

#### Option B: Command Line
```bash
cd C:\Projects\Rama\rama-ai-assistant
dotnet restore
dotnet build
dotnet run --project Rama
```

### Step 4: First Launch
1. Rama opens with a dark-themed chat window
2. She greets you automatically
3. Grant microphone permission if prompted
4. Type `help` to see all commands

---

## First Run

### Initial Setup Commands
```
1. my name is [YourName]          → Tell Rama your name
2. call me bhai                   → Set your nickname
3. set language hindi             → If you prefer Hindi
4. set sass max                   → Maximum attitude!
5. profile                        → Check your profile
```

### Recommended First Steps
```
1. help                           → See all commands
2. skills                         → List all abilities
3. what have you learned          → Check stats
4. set sass high                  → Set personality
5. open notepad                   → Test app control
6. speak hindi                    → Test voice
7. register phone                 → Pair your phone
```

---

## Feature Setup

### 🎙️ Voice Setup

#### Windows Speech Recognition
1. Open **Settings** → **Time & Language** → **Speech**
2. Under "Speech language", select your language
3. Click **"Add a voice"** if needed
4. For Hindi: Download **Hindi** voice pack
5. For Marathi: Download **Marathi** voice (or Hindi as fallback)

#### Test Voice
```
speak hindi          → "अरे वाह! तुम आ गए?"
speak marathi        → "अरे वा! तुम आलात?"
speak english        → "Hey! What's up?"
say something        → I'll talk to you!
```

#### Voice Commands
- Click the 🎤 button to toggle voice mode
- Say "Hey Rama" for wake word
- Say "stop listening" to pause
- Say "be quiet" to stop speaking

### 🤖 Local AI Setup (Ollama)

#### Install Ollama
```bash
# 1. Download from https://ollama.com/download/windows
# 2. Install it
# 3. Open CMD and pull a model:

ollama pull phi3:mini          # 2.2GB - Best starter
ollama pull codellama:7b       # 3.8GB - Best for coding
ollama pull llama3.1:8b        # 4.7GB - Best quality
```

#### Verify in Rama
```
ai status              → Check if Ollama is connected
list models            → See installed models
recommend              → Get model suggestions
```

### 📱 Mobile Setup

#### Pair Your Phone
1. Install **OpenClaw** app on your phone (if available)
2. In Rama, say `register phone`
3. Follow pairing instructions
4. Once connected:
   ```
   take photo             → Snap through camera
   start vision           → I keep watching
   listen to mic          → I hear everything
   where am i             → GPS location
   ```

### 🎓 Software Learning Setup

#### Teach Rama Any Software
```
Format: learn [app] [task] step 1: [action] step 2: [action]

Examples:
learn chrome open new tab step 1: press Ctrl+T
learn word make bold step 1: select text step 2: press Ctrl+B
learn excel save file step 1: press Ctrl+S
learn vscode open terminal step 1: press Ctrl+`
learn notepad save as step 1: press F12 step 2: type filename step 3: press Enter
```

#### Use Learned Tasks
```
do open new tab in chrome
do make bold in word
do save file in excel
```

### 🌍 Language Setup

#### Switch Interface Language
```
set language hindi           → Hindi mode
set language marathi         → Marathi mode
set language spanish         → Español
show languages               → See all 35 languages
```

#### Auto Language Detection
Rama automatically detects what language you're typing in and responds accordingly. No setup needed!

### ⚙️ Settings Panel
Click the ⚙️ button or type `settings` to open the full settings panel with:
- **Voice:** Language, speed, volume, auto-speak
- **Language:** Interface, auto-detect, ambient learning
- **Personality:** Sass, humor, naughty mode
- **AI:** Local AI, self-learning, critical thinking
- **System:** System tray, startup, notifications

---

## Commands Reference

### 👤 Profile & Identity
| Command | Description |
|---------|-------------|
| `my name is [name]` | Tell Rama your name |
| `call me bhai` | Set nickname (bhai/bro/boss/sir ji/yaar) |
| `change your name to [name]` | Rename Rama |
| `be sassy` | Sassy personality mode |
| `be naughty` | Naughty personality mode |
| `be cute` | Cute personality mode |
| `be professional` | Professional mode |
| `profile` | Show profile info |

### 🎙️ Voice
| Command | Description |
|---------|-------------|
| `speak hindi` | Hindi voice mode |
| `speak marathi` | Marathi voice mode |
| `speak english` | English voice mode |
| `say something` | Rama talks to you |
| `set sass max` | Maximum attitude! |

### 🖥️ App Control
| Command | Description |
|---------|-------------|
| `open [app]` | Launch any application |
| `close [app]` | Close an app |
| `switch to [app]` | Focus an app window |
| `show running` | List open apps |
| `software list` | See all known apps |

### 🎓 Software Learning
| Command | Description |
|---------|-------------|
| `learn [app] [task] step 1: [x]` | Teach Rama a task |
| `do [task] in [app]` | Perform learned task |
| `perform [task] in [app]` | Same as above |

### 💻 Coding
| Command | Description |
|---------|-------------|
| `code [task] in [language]` | Generate code |
| `explain code` | Explain code |
| `debug code` | Find bugs |
| `convert code to [language]` | Convert between languages |
| `optimize code` | Improve code |
| `list languages` | See 60+ languages |

### 📚 Knowledge
| Command | Description |
|---------|-------------|
| `learn from url [url]` | Learn from webpage |
| `learn from video [url]` | Learn from YouTube |
| `learn from book [path]` | Learn from book file |
| `learn from blog [url]` | Learn from blog post |
| `learn from script [path]` | Learn from code |
| `memorize this: [text]` | Learn from pasted text |
| `show knowledge` | View knowledge base |
| `search knowledge [topic]` | Find specific info |

### 🧠 Deep Learning & Thinking
| Command | Description |
|---------|-------------|
| `deep think about [problem]` | Full analysis |
| `brainstorm [topic]` | Creative ideas |
| `ask questions about [topic]` | Generate questions |
| `think about [problem]` | Quick analysis |
| `verify [action]` | Check before acting |
| `mistakes` | See recorded mistakes |
| `lessons learned` | What I've learned |
| `learning report` | Full knowledge report |

### 📱 Mobile
| Command | Description |
|---------|-------------|
| `register phone` | Pair your phone |
| `take photo` | Camera snap |
| `start vision` | Continuous watching |
| `listen to mic` | Hear surroundings |
| `where am i` | GPS location |
| `screenshot` | Capture screen |
| `phone status` | Check connection |

### 🌍 Language
| Command | Description |
|---------|-------------|
| `set language [name]` | Switch language |
| `show languages` | List 35 languages |

### 🛠️ Skills
| Command | Description |
|---------|-------------|
| `skills` | List all skills |
| `create skill [name] that [does x]` | Create new skill |
| `skill template` | Get code template |
| `my skills` | List custom skills |
| `skill config` | View JSON config |
| `reload skills` | Refresh skills |

### ⚙️ System
| Command | Description |
|---------|-------------|
| `help` | Show help |
| `what have you learned` | Learning stats |
| `remember [fact]` | Teach a fact |
| `set sass [level]` | Set sass (0-5) |

---

## Troubleshooting

### Build Errors

#### "dotnet not found"
- Install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
- Restart your terminal after install

#### "NuGet package restore failed"
```bash
dotnet nuget locals all --clear
dotnet restore
```

#### "UseWPF not supported"
- Make sure you have .NET 8 SDK (not just Runtime)
- Check your `.csproj` file has `<TargetFramework>net8.0-windows</TargetFramework>`

### Runtime Errors

#### "Voice not working"
1. Settings → Time & Language → Speech
2. Download speech language pack
3. Set default voice
4. Restart Rama

#### "Local AI not connecting"
1. Install Ollama: https://ollama.com
2. Open CMD: `ollama serve`
3. Pull model: `ollama pull phi3:mini`
4. Say `ai status` in Rama

#### "Skills not loading"
1. Check JSON syntax: https://jsonlint.com
2. Say `reload skills`
3. Check folder: `%APPDATA%\Rama\Skills\`

#### "Microphone not detected"
1. Windows Settings → Privacy → Microphone
2. Enable "Allow apps to access your microphone"
3. Restart Rama

### Performance Issues

#### "Rama is slow"
- Use smaller AI model: `ollama pull gemma2:2b`
- Disable local AI: Uncheck in Settings
- Close other heavy apps

#### "Out of memory"
- Use smaller model
- Reduce context size
- Close unused apps

### Data Location

All data is stored at:
```
%APPDATA%\Rama\
```

Structure:
```
Rama/
├── settings.json           → App settings
├── profile.json            → Your profile
├── rama_memory.db          → Learning database
├── Memory/
│   ├── preferences.json    → User preferences
│   ├── facts.json          → Learned facts
│   ├── patterns.json       → Behavior patterns
│   ├── personality.json    → Personality settings
│   ├── context.json        → Conversation context
│   ├── mistakes.json       → Mistake tracking
│   └── solutions.json      → Successful solutions
├── Knowledge/
│   └── knowledge.json      → Ingested content
├── Skills/
│   ├── skills.json         → Easy skills config
│   ├── *.skill.json        → Individual skills
│   └── *.cs                → Code skills
├── Learning/
│   ├── concepts.json       → Learned concepts
│   ├── observations.json   → All observations
│   ├── world_model.json    → Knowledge graph
│   └── skills_knowledge.json
├── Mobile/
│   └── devices.json        → Paired devices
└── notes.json              → Your notes
```

**To reset everything:** Delete the `%APPDATA%\Rama\` folder.

---

## Future Roadmap

### 🔮 Planned Features

#### v2.0 — Smarter AI
- [ ] **GPT-4 Integration** — Optional cloud AI for complex tasks
- [ ] **Multi-model support** — Use different models for different tasks
- [ ] **RAG (Retrieval Augmented Generation)** — Search knowledge base before responding
- [ ] **Fine-tuning** — Train a custom model on your data
- [ ] **Agents** — Multi-step task execution with planning

#### v2.0 — Better Vision
- [ ] **Real-time object detection** — Identify objects through camera
- [ ] **OCR** — Read text from images
- [ ] **Face recognition** — Remember people (opt-in)
- [ ] **Scene understanding** — Describe complex scenes
- [ ] **Screen reading** — Understand what's on your screen

#### v2.0 — Better Voice
- [ ] **ElevenLabs TTS** — More natural voices
- [ ] **Whisper STT** — Better speech recognition
- [ ] **Voice cloning** — Clone your voice for Rama
- [ ] **Emotion detection** — Understand how you feel from voice
- [ ] **Multi-speaker** — Identify who's talking

#### v2.0 — Better Learning
- [ ] **Reinforcement learning** — Learn from rewards/penalties
- [ ] **Few-shot learning** — Learn from just 2-3 examples
- [ ] **Transfer learning** — Apply knowledge across domains
- [ ] **Curiosity-driven** — Rama asks questions to learn faster
- [ ] **Collaborative learning** — Multiple Rama instances share knowledge

#### v2.0 — Better Integration
- [ ] **Smart home** — Control lights, thermostat, etc.
- [ ] **Calendar** — Google/Outlook calendar integration
- [ ] **Email** — Read and send emails
- [ ] **Messaging** — WhatsApp, Telegram, Discord bot
- [ ] **File sync** — Sync knowledge across devices
- [ ] **Browser extension** — Control web browser

#### v2.0 — Better UI
- [ ] **Themes** — Light mode, custom themes
- [ ] **Widgets** — Desktop widgets
- [ ] **Floating assistant** — Always-on-top mini window
- [ ] **3D Avatar** — Animated character
- [ ] **AR overlay** — Augmented reality interface

#### v2.0 — Mobile App
- [ ] **Native Android app** — Full mobile companion
- [ ] **Native iOS app** — iPhone companion
- [ ] **Background service** — Always listening
- [ ] **Widgets** — Home screen quick actions
- [ ] **Wear OS** — Smartwatch support

#### v2.0 — Enterprise
- [ ] **Team collaboration** — Multiple users, shared knowledge
- [ ] **API server** — REST API for integration
- [ ] **Plugin marketplace** — Share and download skills
- [ ] **Audit logging** — Track all actions
- [ ] **Role-based access** — Different permission levels

---

### 💡 How to Contribute

1. **Star the repo:** https://github.com/Rishi114/rama-ai-assistant
2. **Report bugs:** Open GitHub Issues
3. **Suggest features:** Open GitHub Discussions
4. **Submit code:** Fork → Code → Pull Request
5. **Share skills:** Create and share skill JSON/DLL files
6. **Translate:** Help translate to more languages

---

### 🙏 Credits

Built with ❤️ using:
- .NET 8 / WPF
- System.Speech (voice)
- Microsoft.Data.Sqlite (database)
- Newtonsoft.Json (data)
- Ollama (local AI)
- Catppuccin Mocha (theme)

---

## Quick Reference Card

```
┌─────────────────────────────────────────────────┐
│  RAMA QUICK COMMANDS                            │
├─────────────────────────────────────────────────┤
│  help              → All commands               │
│  my name is [x]    → Set your name              │
│  call me bhai      → Set nickname               │
│  set sass max      → Max attitude               │
│  speak hindi       → Hindi mode                 │
│  open chrome       → Launch app                 │
│  learn [app] [x]   → Teach me                   │
│  do [x] in [app]   → Perform task               │
│  code [x] in py    → Generate code              │
│  take photo        → Camera snap                │
│  deep think [x]    → Full analysis              │
│  mistakes          → Error report               │
│  learning report   → Knowledge stats            │
│  profile           → Your profile               │
│  settings          → Full settings              │
└─────────────────────────────────────────────────┘
```

---

**Enjoy your AI assistant! Make her yours!** 🤖🔥

*https://github.com/Rishi114/rama-ai-assistant*

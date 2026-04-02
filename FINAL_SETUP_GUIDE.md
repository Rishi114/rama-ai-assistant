# 🚀 RAMA AI ASSISTANT — ULTIMATE SETUP GUIDE (FINAL VERSION)

> **The most complete AI assistant for Windows. Self-learning, voice in 3 languages, controls any app, smart home, GPT-4, mobile camera, plugin marketplace, 3D avatar — and she has attitude!**

---

## 📋 Table of Contents

1. [What is Rama?](#what-is-rama)
2. [System Requirements](#system-requirements)
3. [Installation (Step by Step)](#installation)
4. [First Launch & Setup](#first-launch)
5. [Feature Setup Guides](#feature-setup)
6. [Complete Commands Reference](#commands)
7. [Troubleshooting](#troubleshooting)
8. [Data & Privacy](#data-privacy)
9. [Future Roadmap](#future-roadmap)

---

## What is Rama?

Rama is a **self-learning AI assistant** for Windows that:

- 🧠 **Learns from everything** — conversations, images, audio, documents, code
- 🗣️ **Speaks 3 languages** — English, Hindi, Marathi (with sassy attitude)
- 🖥️ **Controls any app** — Opens, closes, automates any Windows software
- 🤖 **Runs local AI** — No cloud needed, 100% private on your PC
- 📱 **Sees through your phone** — Camera, microphone, GPS
- 🏠 **Controls smart home** — Lights, thermostat, scenes
- 🛒 **Plugin marketplace** — 20+ installable plugins
- 🎭 **3D animated avatar** — Reacts to conversations
- 😏 **Has personality** — Calls you bhai/boss/sir ji with full attitude

---

## System Requirements

### Minimum
| Component | Requirement |
|-----------|-------------|
| **OS** | Windows 10 (64-bit) or Windows 11 |
| **RAM** | 4 GB |
| **Storage** | 1 GB (app) + space for AI models |
| **CPU** | Any modern processor (Intel i3 / AMD Ryzen 3+) |
| **.NET** | .NET 8.0 SDK |

### Recommended (for Local AI)
| Component | Requirement |
|-----------|-------------|
| **RAM** | 8 GB+ (16 GB for 8B models) |
| **Storage** | 10 GB+ (for AI models) |
| **GPU** | NVIDIA 4GB+ VRAM (optional, faster AI) |

### For GPT-4 Cloud (Optional)
| Component | Requirement |
|-----------|-------------|
| **API Key** | OpenAI API key with credits |
| **Internet** | Required for cloud features |

### For Smart Home (Optional)
| Component | Requirement |
|-----------|-------------|
| **Hub** | Home Assistant, Philips Hue Bridge, or Tuya |
| **Network** | Same WiFi as smart devices |

---

## Installation

### Step 1: Install .NET 8 SDK

1. Go to: **https://dotnet.microsoft.com/download/dotnet/8.0**
2. Download **".NET 8.0 SDK"** (NOT just Runtime)
3. Run the installer
4. **Restart your computer**
5. Verify: Open CMD, type:
   ```
   dotnet --version
   ```
   Should show: `8.0.xxx`

### Step 2: Install Visual Studio 2022

1. Go to: **https://visualstudio.microsoft.com/**
2. Download **Visual Studio 2022 Community** (free)
3. Run installer, select:
   - ✅ **".NET desktop development"**
   - ✅ **".NET 8.0 Runtime"**
4. Click **Install** (takes 10-20 minutes)
5. Restart computer when done

### Step 3: Download Rama

#### Option A: Download ZIP (Easiest)
1. Go to: **https://github.com/Rishi114/rama-ai-assistant**
2. Click green **"<> Code"** button
3. Click **"Download ZIP"**
4. Right-click ZIP → **Extract All**
5. Remember the folder path (e.g., `C:\Projects\Rama\`)

#### Option B: Git Clone
```bash
# Install Git from https://git-scm.com first
git clone https://github.com/Rishi114/rama-ai-assistant.git
cd rama-ai-assistant
```

### Step 4: Build Rama

#### Using Visual Studio (Recommended)
1. Open **Visual Studio 2022**
2. Click **"Open a project or solution"**
3. Navigate to extracted folder
4. Select **`Rama.sln`**
5. Wait for packages to restore (bottom status bar)
6. Press **Ctrl + F5** (Run without debugging)
7. ✅ Rama launches!

#### Using Command Line
```bash
cd C:\path\to\rama-ai-assistant
dotnet restore
dotnet build
dotnet run --project Rama
```

### Step 5: Create Desktop Shortcut (Optional)
1. Right-click Desktop → New → Shortcut
2. Browse to: `C:\path\to\rama-ai-assistant\Rama\bin\Debug\net8.0-windows\Rama.exe`
3. Name it "Rama AI"
4. Click Finish

---

## First Launch

### What You See
- Dark-themed chat window
- Left sidebar with skills list
- Stats panel at bottom
- Voice toggle button
- Chat area with welcome message

### Initial Setup Commands (Copy & Paste These)

```
my name is [Your Name]
```
Example: `my name is Rahul`

```
call me bhai
```
Options: bhai, bro, boss, sir ji, yaar, dude, chief, captain, king

```
set language hindi
```
Or: `set language marathi` / `set language english`

```
set sass max
```
Maximum attitude! Options: off, low, medium, high, max

```
profile
```
Check your profile setup

```
help
```
See all available commands

---

## Feature Setup

### 🎙️ Voice Setup

#### Windows Speech (Built-in, Free)
1. Open **Settings** → **Time & Language** → **Speech**
2. Under "Speech language", select your language
3. Click **"Add a voice"** if needed
4. For Hindi: Download **Hindi** voice pack
5. For Marathi: Download **Marathi** voice (or use Hindi)

#### Test Voice in Rama
```
speak hindi          → "अरे वाह! तुम आ गए?"
speak marathi        → "अरे वा! तुम आलात?"
speak english        → "Hey! What's up?"
say something        → Rama talks to you
```

#### ElevenLabs (Premium Voices, Optional)
1. Sign up at **https://elevenlabs.io**
2. Get API key from Settings
3. In Rama:
   ```
   setup elevenlabs [your-api-key]
   ```
4. Choose voice:
   ```
   set voice rachel
   ```

**Available ElevenLabs Voices:**
| Voice | Style |
|-------|-------|
| Rachel | Calm, American female |
| Domi | Strong, American female |
| Bella | Soft, American female |
| Antoni | Well-rounded, American male |
| Josh | Deep, American male |
| Adam | Deep, American male |
| Sam | Raspy, American male |

---

### 🤖 Local AI Setup (Ollama)

#### Why Local AI?
- 🔒 100% private — nothing leaves your PC
- ⚡ No latency — instant responses
- 🌐 Works offline
- 💰 Free — no API costs

#### Install Ollama
1. Go to: **https://ollama.com/download/windows**
2. Download and install
3. **Restart computer**

#### Download AI Models
Open CMD and run:
```bash
# Starter (2.2GB) - Best for most users
ollama pull phi3:mini

# Coding (3.8GB) - Best for developers
ollama pull codellama:7b

# Quality (4.7GB) - Best responses
ollama pull llama3.1:8b

# Fastest (1.6GB) - Ultra lightweight
ollama pull gemma2:2b
```

#### Verify in Rama
```
ai status              → Check connection
list models            → See installed models
recommend              → Get suggestions
```

---

### ☁️ GPT-4 Cloud Setup (Optional)

#### Get OpenAI API Key
1. Go to: **https://platform.openai.com/api-keys**
2. Sign up / Log in
3. Click **"Create new secret key"**
4. Copy the key (starts with `sk-`)
5. Add credits: **https://platform.openai.com/account/billing**

#### Configure in Rama
```
setup gpt4 sk-your-api-key-here
set model gpt4o
```

**Available Models:**
| Model | Best For |
|-------|----------|
| gpt-4o | Best overall (fast + smart) |
| gpt-4o-mini | Fast and cheap |
| gpt-4-turbo | Powerful |
| o1-preview | Best reasoning |

---

### 📱 Mobile Setup

#### Pair Your Phone
```
register phone
```
This registers your phone as a connected device.

#### Use Phone Features
```
take photo             → Snap through camera
start vision           → I keep watching 👁️
listen to mic          → I hear everything 🎤
where am i             → GPS location 📍
screenshot             → Capture screen 📱
phone status           → Check connection
```

---

### 🏠 Smart Home Setup

#### Option A: Home Assistant
```
setup home assistant http://your-ha-ip:8123 your-long-lived-token
discover devices
```

To get Home Assistant token:
1. Open Home Assistant
2. Click Profile (bottom left)
3. Scroll to "Long-Lived Access Tokens"
4. Create token, copy it

#### Option B: Philips Hue
```
setup hue 192.168.1.xxx your-hue-username
discover devices
```

#### Option C: Tuya/Smart Life
```
setup tuya your-client-id your-client-secret
discover devices
```

#### Control Devices
```
turn on living room light
turn off bedroom light
set living room brightness 50
set thermostat 22
good morning          → Morning routine
good night            → Night routine
movie mode            → Dim lights
```

---

### 📅📧 Calendar & Email Setup

#### Google Calendar
```
connect calendar
```
Opens browser for Google OAuth login.

#### Check Calendar
```
what's my schedule
today's events
add meeting tomorrow at 3pm
```

#### Email
```
connect email
check email
send email to [address] subject [subject]
```

---

### 🛒 Plugin Marketplace

#### Browse Plugins
```
marketplace                    → Browse all
marketplace developer          → Filter by category
search spotify                 → Search plugins
```

#### Install Plugins
```
install plugin Spotify Controller
install plugin GitHub Assistant
install plugin Docker Manager
```

#### Manage Plugins
```
my plugins                     → List installed
uninstall plugin [name]        → Remove plugin
```

**Available Categories:**
- utility, productivity, developer, entertainment
- smart-home, creative, language, information, finance, security

---

### 🎭 3D Avatar Setup

#### Enable Avatar
```
enable avatar
```

#### Choose Avatar Style
```
set avatar anime-girl    → 👧 Cute anime
set avatar robot         → 🤖 Classic robot
set avatar hologram      → 👤 Futuristic
set avatar chibi         → 🧝 Small cute
set avatar realistic     → 👩 Human-like
set avatar pixel         → 👾 Retro pixel
set avatar minimal       → 🔵 Simple circle
set avatar cat           → 🐱 Neko cat
```

#### List All Avatars
```
show avatars
```

The avatar automatically reacts to conversations with animations!

---

### 🧠 Learning Setup

#### Teach Rama Facts
```
remember my birthday is January 15
remember I live in Mumbai
remember my favorite food is biryani
remember I work at Infosys
```

#### Feed Knowledge
```
learn from url https://article.com
learn from video https://youtube.com/watch?v=...
learn from book C:\Books\textbook.txt
learn from blog https://blog.example.com
memorize this: [paste content]
```

#### Teach Software
```
learn chrome open new tab step 1: press Ctrl+T
learn word make bold step 1: select text step 2: press Ctrl+B
learn excel save file step 1: press Ctrl+S
learn vscode open terminal step 1: press Ctrl+`
```

#### Use Learned Tasks
```
do open new tab in chrome
do make bold in word
do save file in excel
```

---

### 💻 Coding Setup

No setup needed! Just ask:
```
code fibonacci in python
code REST API in javascript
code sorting algorithm in c++
explain this code: [paste code]
debug this code: [paste code]
convert code to java: [paste code]
list languages
```

---

## Commands Reference

### 👤 Profile
| Command | Description |
|---------|-------------|
| `my name is [name]` | Set your name |
| `call me bhai` | Set nickname |
| `change your name to [name]` | Rename Rama |
| `be sassy` | Sassy mode |
| `be naughty` | Naughty mode |
| `be cute` | Cute mode |
| `be professional` | Professional mode |
| `profile` | Show profile |

### 🎙️ Voice
| Command | Description |
|---------|-------------|
| `speak hindi` | Hindi mode |
| `speak marathi` | Marathi mode |
| `speak english` | English mode |
| `say something` | Talk to me |
| `setup elevenlabs [key]` | Premium voices |
| `set voice [name]` | Change voice |

### 🖥️ Apps
| Command | Description |
|---------|-------------|
| `open [app]` | Launch app |
| `close [app]` | Close app |
| `switch to [app]` | Focus window |
| `show running` | List open apps |
| `software list` | Known apps |

### 🎓 Software Learning
| Command | Description |
|---------|-------------|
| `learn [app] [task] step 1: [x]` | Teach task |
| `do [task] in [app]` | Perform task |

### 💻 Coding
| Command | Description |
|---------|-------------|
| `code [task] in [lang]` | Generate code |
| `explain code` | Explain code |
| `debug code` | Find bugs |
| `convert code to [lang]` | Convert code |
| `optimize code` | Improve code |

### 📚 Knowledge
| Command | Description |
|---------|-------------|
| `learn from url [url]` | From webpage |
| `learn from video [url]` | From YouTube |
| `learn from book [path]` | From book |
| `learn from blog [url]` | From blog |
| `memorize this: [text]` | From paste |
| `show knowledge` | View all |
| `search knowledge [topic]` | Search |

### 🧠 Thinking
| Command | Description |
|---------|-------------|
| `deep think about [problem]` | Full analysis |
| `brainstorm [topic]` | Creative ideas |
| `think about [problem]` | Quick analysis |
| `verify [action]` | Pre-check |
| `mistakes` | Error report |
| `lessons learned` | What I learned |
| `learning report` | Full stats |

### 📱 Mobile
| Command | Description |
|---------|-------------|
| `register phone` | Pair phone |
| `take photo` | Camera |
| `start vision` | Watch |
| `listen to mic` | Hear |
| `where am i` | GPS |
| `screenshot` | Screen |

### 🏠 Smart Home
| Command | Description |
|---------|-------------|
| `setup home assistant [url] [token]` | Configure |
| `discover devices` | Scan |
| `turn on [device]` | On |
| `turn off [device]` | Off |
| `set brightness [n]` | Brightness |
| `set color [color]` | Color |
| `set thermostat [temp]` | Temp |
| `good morning` | Routine |
| `good night` | Routine |

### 📅📧 Calendar & Email
| Command | Description |
|---------|-------------|
| `connect calendar` | Setup |
| `today's events` | View |
| `add event [title] [time]` | Create |
| `connect email` | Setup |
| `check email` | Read |
| `send email to [addr]` | Send |

### 🛒 Marketplace
| Command | Description |
|---------|-------------|
| `marketplace` | Browse |
| `marketplace [category]` | Filter |
| `search [query]` | Search |
| `install plugin [name]` | Install |
| `uninstall plugin [name]` | Remove |
| `my plugins` | Installed |

### 🎭 Avatar
| Command | Description |
|---------|-------------|
| `enable avatar` | Turn on |
| `disable avatar` | Turn off |
| `set avatar [style]` | Change |
| `show avatars` | List all |

### 🌍 Language
| Command | Description |
|---------|-------------|
| `set language [name]` | Switch |
| `show languages` | List 35 |

### 😏 Personality
| Command | Description |
|---------|-------------|
| `set sass [level]` | 0-5 |
| `set sass max` | 🔥 Max! |

### ⚙️ System
| Command | Description |
|---------|-------------|
| `help` | All commands |
| `skills` | List skills |
| `settings` | Settings panel |
| `remember [fact]` | Teach |
| `what have you learned` | Stats |

---

## Troubleshooting

### Build Issues

**"dotnet not found"**
- Install .NET 8 SDK
- Restart terminal/computer
- Verify: `dotnet --version`

**"NuGet restore failed"**
```bash
dotnet nuget locals all --clear
dotnet restore
```

**"UseWPF not supported"**
- Install .NET 8 SDK (not just Runtime)
- Check `.csproj` has `net8.0-windows`

### Runtime Issues

**"Voice not working"**
1. Settings → Time & Language → Speech
2. Download speech language pack
3. Set default voice
4. Restart Rama

**"Ollama not connecting"**
1. Install Ollama
2. CMD: `ollama serve`
3. CMD: `ollama pull phi3:mini`
4. In Rama: `ai status`

**"Microphone not detected"**
1. Settings → Privacy → Microphone
2. Enable microphone access
3. Restart Rama

**"Skills not loading"**
1. Validate JSON: https://jsonlint.com
2. Say `reload skills`
3. Check: `%APPDATA%\Rama\Skills\`

### Performance Issues

**"Rama is slow"**
- Use smaller model: `ollama pull gemma2:2b`
- Disable local AI in Settings
- Close other heavy apps

**"Out of memory"**
- Use smaller AI model
- Reduce context
- Close unused apps

---

## Data & Privacy

### Where Is Data Stored?
```
%APPDATA%\Rama\
```
Type in Windows Explorer to open.

### Data Structure
```
Rama/
├── settings.json          → App settings
├── profile.json           → Your profile
├── rama_memory.db         → Learning database
├── smarthome.json         → Smart home config
├── calendar_email.json    → Calendar/email
├── avatar.json            → Avatar settings
├── Memory/
│   ├── preferences.json   → Settings
│   ├── facts.json         → Facts
│   ├── patterns.json      → Patterns
│   ├── personality.json   → Personality
│   ├── context.json       → Context
│   ├── mistakes.json      → Mistakes
│   └── solutions.json     → Solutions
├── Knowledge/
│   └── knowledge.json     → Ingested content
├── Skills/
│   ├── skills.json        → Easy skills
│   └── *.skill.json       → Custom skills
├── Learning/
│   ├── concepts.json      → Concepts
│   ├── observations.json  → Observations
│   └── world_model.json   → Knowledge graph
├── Mobile/
│   └── devices.json       → Phone
├── Marketplace/
│   └── installed.json     → Plugins
└── notes.json             → Notes
```

### Privacy
- ✅ All data stays on YOUR PC
- ✅ Local AI = zero cloud
- ✅ No telemetry
- ✅ No account required
- ⚠️ Cloud AI (GPT-4) sends data to OpenAI (optional)

### Reset Everything
Delete: `%APPDATA%\Rama\`

---

## Future Roadmap

### v2.0 — Smarter AI
- [ ] Multi-model routing (use best model per task)
- [ ] RAG (search knowledge before responding)
- [ ] Fine-tuning on your data
- [ ] Multi-step agents with planning
- [ ] Offline speech recognition (Whisper)

### v2.0 — Better Vision
- [ ] Real-time object detection (YOLO)
- [ ] OCR (read text from images)
- [ ] Face recognition (opt-in)
- [ ] Scene understanding
- [ ] Screen reading (what's on your display)

### v2.0 — Better Voice
- [ ] More ElevenLabs voices
- [ ] Voice cloning (clone your voice)
- [ ] Emotion detection (from voice tone)
- [ ] Multi-speaker identification
- [ ] Background noise filtering

### v2.0 — Better Learning
- [ ] Reinforcement learning (rewards/penalties)
- [ ] Few-shot learning (2-3 examples)
- [ ] Curiosity-driven (asks questions)
- [ ] Collaborative learning (share between Ramas)
- [ ] Automatic skill creation from watching

### v2.0 — Better Integration
- [ ] Google Home integration
- [ ] Alexa skill
- [ ] Slack/Teams bot
- [ ] Discord bot
- [ ] Telegram bot
- [ ] WhatsApp integration
- [ ] Browser extension
- [ ] VS Code extension

### v2.0 — Better UI
- [ ] Light theme
- [ ] Custom themes
- [ ] Floating mini window
- [ ] Desktop widgets
- [ ] AR overlay

### v2.0 — Mobile
- [ ] Native Android app
- [ ] Native iOS app
- [ ] Background service
- [ ] Home screen widgets
- [ ] Wear OS support

### v2.0 — Enterprise
- [ ] Multi-user support
- [ ] REST API server
- [ ] Plugin marketplace (online)
- [ ] Audit logging
- [ ] Role-based access
- [ ] SSO integration

### v2.0 — Creative
- [ ] Music generation
- [ ] Video editing
- [ ] 3D modeling
- [ ] Game development
- [ ] Art generation

---

## Quick Reference Card

```
╔═══════════════════════════════════════════════════════╗
║           RAMA AI — QUICK COMMANDS                    ║
╠═══════════════════════════════════════════════════════╣
║  SETUP                                                ║
║  my name is [x]      → Your name                      ║
║  call me bhai        → Nickname                       ║
║  set sass max        → Attitude!                      ║
║  speak hindi         → Hindi mode                     ║
║                                                       ║
║  APPS                                                 ║
║  open chrome         → Launch app                     ║
║  close word          → Close app                      ║
║  show running        → List apps                      ║
║                                                       ║
║  LEARN                                                ║
║  learn [app] [task] step 1: [x]                       ║
║  do [task] in [app]  → Execute                        ║
║  remember [fact]     → Store fact                     ║
║  learn from url [x]  → From web                       ║
║                                                       ║
║  CODE                                                 ║
║  code [x] in python  → Generate                       ║
║  explain code        → Explain                        ║
║  debug code          → Find bugs                      ║
║                                                       ║
║  SMART HOME                                           ║
║  turn on light       → Control                        ║
║  good night          → Routine                        ║
║                                                       ║
║  VOICE                                                ║
║  say something       → Talk                           ║
║  take photo          → Camera                         ║
║                                                       ║
║  MARKETPLACE                                          ║
║  marketplace         → Browse                         ║
║  install plugin [x]  → Install                        ║
║                                                       ║
║  HELP                                                 ║
║  help                → All commands                   ║
║  settings            → Settings panel                 ║
║  profile             → Your profile                   ║
╚═══════════════════════════════════════════════════════╝
```

---

## Support & Community

- **GitHub:** https://github.com/Rishi114/rama-ai-assistant
- **Issues:** Report bugs on GitHub Issues
- **Features:** Request on GitHub Discussions
- **Star:** ⭐ the repo if you like it!

---

## Credits

Built with:
- .NET 8 / WPF
- System.Speech
- Microsoft.Data.Sqlite
- Newtonsoft.Json
- Ollama (local AI)
- OpenAI API (cloud AI)
- ElevenLabs (premium voices)
- Catppuccin Mocha (theme)

---

**Enjoy your AI assistant! Make her yours!** 🤖🔥

*Built with ❤️ — github.com/Rishi114/rama-ai-assistant*

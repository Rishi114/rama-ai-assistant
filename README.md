# 🧠 Rama — Self-Learning AI Assistant for Windows

Rama is a desktop AI assistant built with C# and WPF (.NET 8) that **learns from your interactions** to become more helpful over time. It features a plugin-based skill system, a modern dark-themed chat interface, and a local SQLite database for persistent memory.

![.NET 8](https://img.shields.io/badge/.NET-8.0-blue) ![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey) ![License](https://img.shields.io/badge/License-MIT-green)

## ✨ Features

- **🧠 Self-Learning** — Rama learns from every interaction. It recognizes patterns in your commands, tracks which skills you use most, remembers your preferences, and gets smarter with every conversation.
- **🔌 Skill System** — A plugin architecture where capabilities are modular skills. Add new skills as DLL plugins at runtime without restarting.
- **💬 Chat Interface** — Modern dark-themed WPF chat UI with message bubbles, typing indicator, sidebar with skills and learned patterns, and live suggestions.
- **🔒 100% Local** — All data stays on your machine. No cloud services, no API keys required for core functionality.

## 🚀 Built-in Skills

| Skill | Description | Example Commands |
|-------|-------------|-----------------|
| **App Launcher** | Launch Windows applications | "open notepad", "launch chrome" |
| **File Manager** | Manage files and folders | "list files in C:\Users", "create folder Project" |
| **Web Search** | Search the web in your browser | "search for C# tutorials", "google weather" |
| **Notes** | Take and manage personal notes | "note: buy groceries", "list notes", "search notes" |
| **Reminders** | Set timed reminders | "remind me in 10 minutes to take a break" |
| **Calculator** | Math calculations and unit conversion | "calculate (15+3)*2", "what is 15% of 200" |
| **System Info** | Computer diagnostics | "system info", "cpu", "memory", "disk" |
| **Weather** | Weather for any location | "weather in Tokyo", "forecast London" |
| **Greeting** | Friendly conversation | "hello", "how are you", "good morning" |

## 🏗️ Building from Source

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Windows 10/11 (WPF requirement)
- Visual Studio 2022 (optional, for IDE experience)

### Build

```bash
# Clone the repository
git clone https://github.com/rama-assistant/rama.git
cd rama

# Build
dotnet build Rama.sln

# Run
dotnet run --project Rama/Rama.csproj

# Publish (self-contained)
dotnet publish Rama/Rama.csproj -c Release -r win-x64 --self-contained false -o publish
```

### Install with Installer

1. Build and publish the project
2. Copy the publish output to `RamaInstaller/publish/`
3. Compile the Inno Setup script (`RamaInstaller/RamaInstaller.iss`)
4. Run the generated installer

## 📁 Project Structure

```
Rama/
├── Rama.sln                          # Visual Studio solution
├── Rama/
│   ├── Rama.csproj                   # .NET 8 WPF project
│   ├── App.xaml / App.xaml.cs        # Application entry point
│   ├── MainWindow.xaml / .cs         # Main chat UI
│   ├── Core/
│   │   ├── Brain.cs                  # Main AI engine, routes messages to skills
│   │   ├── Learner.cs                # Self-learning engine (SQLite-backed)
│   │   ├── SkillManager.cs           # Loads/discovers/manages skills
│   │   └── Memory.cs                 # Conversation memory + context
│   ├── Skills/
│   │   ├── ISkill.cs                 # Skill interface
│   │   ├── SkillBase.cs              # Base class for skills
│   │   ├── AppLauncherSkill.cs       # Launch Windows apps
│   │   ├── FileManagerSkill.cs       # File operations
│   │   ├── WebSearchSkill.cs         # Web search via default browser
│   │   ├── NoteSkill.cs              # Take/manage notes
│   │   ├── ReminderSkill.cs          # Set reminders
│   │   ├── CalculatorSkill.cs        # Math calculations
│   │   ├── SystemInfoSkill.cs        # System information
│   │   ├── WeatherSkill.cs           # Weather via wttr.in
│   │   └── GreetingSkill.cs          # Conversational responses
│   ├── Data/                         # SQLite DB + JSON files (auto-created)
│   └── Styles/
│       └── Theme.xaml                # Modern dark theme
├── RamaInstaller/
│   └── RamaInstaller.iss             # Inno Setup installer script
├── SKILLS_GUIDE.md                   # How to create custom skills
├── LEARNING_GUIDE.md                 # How self-learning works
└── README.md                         # This file
```

## 🧠 How Learning Works

Rama's learning engine tracks:

1. **Pattern Recognition** — When you repeatedly use similar commands, Rama learns to route them to the correct skill automatically.
2. **Frequency Tracking** — Skills you use more often get higher confidence scores.
3. **Preference Learning** — Rama remembers your choices (preferred apps, search engine, location).
4. **Feedback Integration** — The confidence of learned patterns adjusts based on usage success.

All learning data is stored in a local SQLite database (`Data/rama_memory.db`).

See [LEARNING_GUIDE.md](LEARNING_GUIDE.md) for details.

## 🔌 Creating Custom Skills

Rama supports adding custom skills as external DLL plugins. See [SKILLS_GUIDE.md](SKILLS_GUIDE.md) for a complete guide with examples.

Quick start:

1. Create a .NET 8 class library project
2. Reference `Rama` (or implement `ISkill` interface)
3. Build to a DLL
4. Drop the DLL into Rama's `Plugins/` folder
5. Restart Rama — your skill is loaded!

## 🛠️ Technology Stack

- **Language:** C# 12
- **Framework:** .NET 8 / WPF
- **Database:** SQLite (via Microsoft.Data.Sqlite)
- **JSON:** Newtonsoft.Json
- **Theme:** Custom dark theme in XAML
- **Weather API:** wttr.in (no API key needed)

## 📄 License

MIT License. See [LICENSE](LICENSE) for details.

## 🙏 Acknowledgments

- Weather data provided by [wttr.in](https://wttr.in)
- Inspired by the concept of AI assistants that truly learn and adapt to their users

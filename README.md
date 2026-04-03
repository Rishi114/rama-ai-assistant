# Rama Complete Setup Guide v2.0

## 📦 Download Files

The complete Rama package is at:
- `/root/.openclaw/workspace/Rama/rama-export/` (849MB when uncompressed)
- Full source: Core, Skills (33), Knowledge Bases (13), Rama 3D

---

## 🚀 Option 1: Desktop (WPF - Original)

### Requirements
- Windows 10/11
- .NET 6.0+ SDK
- Visual Studio 2022

### Build & Run
```bash
# Open solution
cd Rama
open Rama.sln

# Build in Visual Studio
# Run: Ctrl+F5

# Output: Rama/bin/Debug/Rama.exe
```

---

## 🚀 Option 2: Rama 3D (Electron + Voice + 3D)

### Requirements
- Node.js 18+
- npm

### Install & Run
```bash
# Navigate to Rama 3D
cd Rama/Rama/Data/BrainKnowledge/rama-3d

# Install
npm install

# Run
npm start
```

### Build .exe (Production)
```bash
# Install builder
npm install -D electron-builder

# Build for Windows
npx electron-builder --win

# Output: dist/Rama 3D Setup.exe
```

---

## 🌐 Option 3: Online (Web + API)

### Step 1: Host Rama 3D (Frontend)
```bash
# Build
npm run build

# Upload dist/ folder to:
# - Vercel (recommended)
# - Netlify
# - GitHub Pages
# - Cloudflare Pages
```

### Step 2: Host API (Backend)
```bash
# Compile API server
cd Rama
csc /reference:Rama.Core.dll RamaApiServer.cs

# Run
mono RamaApiServer.exe

# Or as Docker:
docker build -t rama-api .
docker run -p 5000:5000 rama-api
```

---

## 💻 Offline Desktop Build

### WPF (C#)
```bash
# Full build with dependencies
dotnet publish Rama/Rama.csproj -c Release -r win-x64 --self-contained

# Output: publish/Rama.exe
```

### Electron (Portable)
```bash
cd Rama/Rama/Data/BrainKnowledge/rama-3d
npx electron-builder --win portable

# Output: Rama-3D-portable.exe (works offline!)
```

---

## 🔌 API Integration

### Start API Server
```bash
cd Rama
dotnet run RamaApiServer.cs
# Port: 5000
```

### Endpoints
| Method | Endpoint | Usage |
|--------|----------|-------|
| POST | `/brain` | `{"message": "hello"}` |
| GET | `/skills` | List all skills |
| GET | `/memory` | Memory stats |
| GET | `/status` | Server status |

### Connect Rama 3D to API
- Rama 3D auto-detects API at `http://localhost:5000`
- Falls back to offline mode if unavailable

---

## 📚 Knowledge Bases Included (13)

```
Rama/Data/BrainKnowledge/
├── rama-3d/              # 3D UI prototype
├── minGPT/               # Karpathy's minGPT
├── godmode3/             # Multi-model chat
├── claude-code-architecture/  # 10 patterns
├── everything-claude-code/  # 156+ skills
├── agentjson/            # JSON agent spec
├── MiroFish/             # Swarm intelligence
├── agentic-context-engine/  # ACE framework
└── ... (5 more)
```

---

## ⚡ Skills Summary (33 Total)

| Category | Skills |
|----------|--------|
| **AI/ML** | ACESkill, MinGPTSkill, MiroFishSkill, ClaudeCodeArchitectureSkill |
| **Coding** | CoderSkill, DeepLearningSkill, SkillCreatorSkill, KnowledgeSkill |
| **Tools** | GitHubBrainPullerSkill, VibeCodingKnowledgeSkill, SecondBrainSkill |
| **System** | AppControllerSkill, AppLauncherSkill, FileManagerSkill, SystemInfoSkill |
| **Utility** | CalculatorSkill, WebSearchSkill, WeatherSkill, ReminderSkill, NoteSkill |
| **Voice** | VoiceAssistantSkill, MultiVoiceEngine |
| **New** | EverythingClaudeCodeSkill, AgentJSONSkill, G0DM0D3Skill, ClawCodeSkill |

---

## 🎨 Customizing the 3D Avatar

In `rama-3d/index.html`:

```javascript
// Change avatar color (line ~350)
avatarMaterial.color.setHex(0x00f5ff); // Cyan
avatarMaterial.emissive.setHex(0x00f5ff);

// Change shape (line ~320)
// Head: IcosahedronGeometry → BoxGeometry, SphereGeometry
// Body: TorusKnotGeometry → any Three.js geometry
```

---

## 🛠️ Troubleshooting

### "Microphone not working"
- Allow browser mic access
- Use HTTPS or localhost

### "API not connecting"
- Start API: `dotnet run RamaApiServer.cs`
- Check port 5000

### "Build fails"
```bash
# Clear npm
npm cache clean --force
rm -rf node_modules
npm install
```

---

## 📄 File Structure

```
Rama/
├── Rama/                      # WPF source
│   ├── Core/                 # Brain, Memory, etc (22 files)
│   ├── Skills/               # 33 skills
│   └── Data/BrainKnowledge/  # 13 knowledge bases
├── RamaApiServer.cs          # HTTP API
└── SETUP_AND_DEPLOY.md       # This guide
```

---

## ✅ What Works Offline

| Component | Offline | Notes |
|-----------|---------|-------|
| WPF Desktop | ✅ | Full functionality |
| Rama 3D (Electron) | ✅ | Uses offline responses |
| API Server | ⚠️ | Needs LLM API key |
| Voice (TTS/STT) | ✅ | Browser APIs work |

**Offline mode**: Rama 3D works without API - uses built-in responses!

---

Build, run, and enhance Rama! 🚀
# Rama 3D - Continuous Voice AI Assistant

A 3D desktop AI assistant with continuous voice conversation and holographic avatar.

## Quick Start

```bash
cd rama-3d
npm install
npm start
```

## Features

- 🎙️ **Continuous Voice Listening** - Always ready to hear you
- 🔊 **Voice Response** - Speaks answers aloud with TTS
- 🧬 **3D Holographic Avatar** - Geometric cyberpunk AI form
- ✨ **State Animations** - Avatar reacts: listening, thinking, speaking
- 🌌 **Immersive Environment** - Deep space background with particles
- 🖥️ **Frameless Window** - Clean, edge-to-edge experience
- ⚙️ **Window Controls** - Minimize, maximize, close

## How It Works

```
┌─────────────────────────────────────────────────────────────┐
│                     RAMA 3D FLOW                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   🎤 Voice Input                                           │
│       │                                                    │
│       ▼                                                    │
│   👁️ Speech Recognition (Web Speech API)                 │
│       │                                                    │
│       ▼                                                    │
│   🧠 "Thinking" State → Avatar rotates/searches            │
│       │                                                    │
│       ▼                                                    │
│   💭 Rama Brain (mock for now)                            │
│       │                                                    │
│       ▼                                                    │
│   💬 "Speaking" State → Avatar animates + TTS              │
│       │                                                    │
│       ▼                                                    │
│   🔄 Loop - Back to listening                              │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Integration with Rama Brain

Currently uses mock brain. To connect to actual Rama:

1. **Option A: HTTP API** - Add endpoint in Rama (C#) that accepts POST
2. **Option B: IPC** - Use Electron IPC to communicate with Rama process
3. **Option C: Socket** - WebSocket connection to Rama backend

Example integration code:

```javascript
// In index.html, replace mockRamaBrain() with:
async function processVoiceInput(text) {
  setAvatarState('thinking');
  
  const response = await fetch('http://localhost:5000/brain', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ input: text })
  }).then(r => r.json());
  
  speak(response.message);
}
```

## Keyboard Controls

- **Allow microphone** when browser prompts
- Click anywhere to focus for voice input

## Requirements

- Node.js 18+
- Electron 28+
- Microphone access for voice input

## File Structure

```
rama-3d/
├── main.js       # Electron main process
├── preload.js    # Secure bridge to renderer
├── index.html   # 3D UI + Voice AI logic
├── package.json # Dependencies
└── SPEC.md      # Detailed specification
```

## License

MIT
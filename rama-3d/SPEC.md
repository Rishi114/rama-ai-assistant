# Rama 3D - Specification Document

## Project Overview
- **Name:** Rama 3D
- **Type:** Desktop AI Assistant with 3D Visual Interface
- **Core:** Continuous voice conversation with 3D avatar response
- **Platform:** Electron + Three.js + Web Speech API

## UI/UX Specification

### Layout Structure
- **Fullscreen 3D canvas** - Immersive environment
- **Avatar center-stage** - 3D character responds to conversation
- **Floating UI overlay** - Minimal, non-intrusive controls
- **No traditional windows** - Pure 3D experience

### Visual Design

**Color Palette:**
- Background: Deep space gradient (#0a0a1a → #1a1a3a)
- Avatar glow: Cyan (#00f5ff)
- Accent: Electric blue (#4d9fff)
- UI elements: Semi-transparent dark (#1a1a2e, 80% opacity)
- Text: White (#ffffff) with cyan highlights

**Typography:**
- UI Text: Orbitron (futuristic)
- Chat display: Rajdhani

**Visual Effects:**
- Particle background (subtle floating particles)
- Avatar glow/bloom effect
- Smooth avatar animations
- Ambient lighting with color shifts
- Scanline/hologram overlay (subtle)

### 3D Avatar

**Design:** Cyberpunk holographic AI
- Geometric/abstract humanoid form
- Glowing edges and inner light
- Transparent/glass-like material
- Floating animation (gentle bob)

**Animations:**
- Idle: Gentle float + subtle pulse
- Listening: Focused, slightly leaned forward
- Thinking: Rotating/searching motion
- Speaking: Gesturing, expressive movement
- Happy/Excited: Brightness increase, faster movement

### UI Overlay Components

**Top-left:** Status indicator
- "LISTENING" / "THINKING" / "SPEAKING" / "IDLE"
- Pulsing dot indicator

**Bottom-center:** Minimal input prompt
- "Speak naturally..." or transcription display

**Top-right:** Settings gear (minimal)

### Interaction Flow
1. **Start:** App launches → Avatar appears → "Listening..."
2. **User speaks:** Voice detected → Avatar "listens" state → Text appears
3. **Processing:** Avatar "thinking" state → API call to Rama brain
4. **Response:** Avatar "speaking" state → TTS audio + text display
5. **Loop:** Returns to listening state

## Functionality Specification

### Core Features
1. **Continuous Voice Listening**
   - Web Speech API (SpeechRecognition)
   - Auto-restart after each utterance
   - Wake word optional ("Hey Rama")

2. **Voice Response (TTS)**
   - Web Speech API synthesis
   - Sync with avatar animations

3. **Rama Brain Integration**
   - Connect to Rama's Brain.cs via IPC or HTTP
   - Send user input → receive response

4. **3D Avatar System**
   - Three.js based
   - Procedural geometry (no external models needed)
   - Animation state machine

5. **Desktop Features**
   - Window controls (min/max/close)
   - System tray option
   - Always-on-top option

### Technical Stack
- Electron 28+
- Three.js r150+
- Web Speech API
- HTML/CSS for UI overlay

## Acceptance Criteria
- [ ] App launches to 3D environment
- [ ] Avatar renders and animates
- [ ] Voice input captures speech
- [ ] Text transcription displays
- [ ] Rama brain processes input (mock for now)
- [ ] TTS reads response aloud
- [ ] Avatar animates during response
- [ ] Continuous loop: listen → respond → listen
- [ ] Window controls work
- [ ] No crashes on extended use
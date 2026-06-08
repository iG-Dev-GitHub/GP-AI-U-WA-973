# Breath Tower Rise Builder - PRD

## Overview
Offline Android breathing practice tracker with tower-building game mechanics. No auth, no cloud — all data stored locally via AsyncStorage.

## Architecture
- **Frontend**: React Native Expo SDK 54 (Expo Router file-based routing)
- **Backend**: Minimal FastAPI (not actively used by the app; kept for platform requirements)
- **Storage**: AsyncStorage (offline, fully local)
- **Animations**: react-native-reanimated v4.1.1

## User Personas
- Adults interested in mindfulness, breathing exercises, meditation, and stress relief
- People who enjoy light gamification to build healthy habits

## Core Requirements (Static)
1. Offline-only, no authentication, no cloud sync
2. 4 breathing techniques: 4-7-8, Box Breathing, Equal Breathing, Custom
3. Session screen with animated breathing ring + tower building
4. Tower grows floor-by-floor with each completed breath cycle
5. Special block types: Perfect (gold), Calm (cyan, every 5 cycles), Focus Floor (gold+star, every 10 cycles)
6. 7 screens: Welcome (tutorial), Home, Technique Selection, Session, Summary, Tower, Settings
7. Automatic breathing mode (timer auto-advances phases)
8. English interface
9. Dark navy theme (#0D1B2A)

## What's Been Implemented (as of 2026-06-05)

### Screens
- **Welcome** (`/welcome`): 3-slide onboarding with navigation dots, crane emoji, tower preview, "Next →" / "Start Breathing" button
- **Home** (`/home`): Total floor counter, city skyline hero card, New Session + View Full Tower buttons, session history list
- **Technique** (`/technique`): 4 technique cards (4-7-8, Box, Even, Custom), custom rhythm inputs, session mode (cycles/time), session length selector
- **Session** (`/session`): Animated breathing ring (scale animation via Reanimated), crane animation (pendulum), tower block stack, phase label + countdown timer, Begin/End Session buttons, cycle counter
- **Summary** (`/summary`): Session stats (floors, cycles, perfect blocks, duration), special block breakdown, tower mini-preview, badges earned, Build Again / Go Home
- **Tower** (`/tower`): Full tower visualization with all sessions, session dividers with technique color, milestone Focus Floors highlighted
- **Settings** (`/settings`): Haptic/Sound toggles, Custom Rhythms management (add/delete), Reset All Data with confirmation

### Game Mechanics Implemented
- Auto-mode timer: 100ms interval, tracks phase elapsed time
- Block types: perfect (gold), calm (cyan, cycle %5=0), focus (gold+star, cycle %10=0)
- Block entrance animation: spring translateY from above
- Flash overlay on block placement
- Completion glow animation on session end
- Crane pendulum animation (Reanimated withRepeat)
- AsyncStorage persistence for all sessions and settings

### Data Model
- `SessionRecord`: id, date, techniqueId, techniqueName, cycles, perfectBlocks, calmBlocks, focusFloors, totalFloors, badges, durationSeconds, blocks[]
- `AppData`: totalFloors, sessions[], hasSeenTutorial, settings{}

## Prioritized Backlog

### P0 - Critical (already done)
- [x] All 7 screens implemented
- [x] Session timer with breathing phases
- [x] Tower building mechanics
- [x] AsyncStorage persistence
- [x] Tutorial onboarding fix (direct state update + onMomentumScrollEnd)

### P1 - High Priority (next iteration)
- [ ] Tower shake animation when 3+ crooked blocks (manual mode)
- [ ] Manual mode (user taps Inhale/Hold/Exhale in sync) with accuracy scoring
- [ ] Crooked blocks based on timing deviation (for manual mode)
- [ ] Sound effects (breathing guidance audio)
- [ ] Better crane visual (actual crane shape with mast, not just arm)
- [ ] Night city silhouette in session screen (currently only on home)
- [ ] Session pause/resume feature

### P2 - Nice to Have
- [ ] Custom fonts (Quicksand from @expo-google-fonts)
- [ ] More badge types
- [ ] Weekly/monthly stats view
- [ ] Widget support (Android)
- [ ] Tower comparison across sessions (graph view)
- [ ] Share session summary as image

## Next Tasks
1. Test on real Android device via Expo Go
2. Publish to Google Play via Emergent publish button
3. Add manual breathing mode (accuracy scoring)
4. Add sound effects
5. Polish crane visual design

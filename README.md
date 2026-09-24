# Ruka Desktop Assistant

Windows desktop AI companion project.

## Vision
Ruka lives on the Windows desktop as a lightweight 2D character rather than a normal app window. She can move around the desktop, converse by voice, open a dedicated chat window, and eventually perform permission-controlled PC actions.

## Development principles
- Free-first: no paid service is required for the basic desktop shell.
- Quiet by default: Ruka should not interrupt games, calls, or focused work.
- Explicit permissions: PC control is granular and dangerous actions require confirmation.
- Persistent conversation history is separate from long-term memory.
- Long-term memory is stored only when the user explicitly asks Ruka to remember something.
- Multi-monitor and per-game/per-app behavior are first-class features.

## Initial stack
- C# / .NET 8
- WPF
- Windows APIs where appropriate

## Roadmap
1. Transparent desktop character window
2. Character movement and idle behavior
3. Speech bubbles and animation
4. Dedicated chat window
5. Voice wake word, STT and TTS
6. AI conversation
7. Persistent history and explicit memory
8. Multi-monitor/app/game context
9. Granular PC permissions and safe operations
10. Packaging and installer

## Current status (v0.1.1)
The Windows desktop shell is buildable through GitHub Actions and can be published as a self-contained win-x64 EXE or portable ZIP.

Implemented now:
- Transparent 2D desktop character with drag, double-click chat, idle/walk/sleep behavior
- Persistent conversation history and explicit long-term memory
- Japanese wake-word voice input and Windows TTS
- OpenAI-compatible and generic HTTP AI providers with configurable history and encrypted API-key storage
- Personality presets and per-game behavior profiles
- Multi-monitor profiles and game-side positioning
- Permissioned PC actions with confirmation for dangerous operations
- Global shortcuts, pause/resume, and emergency stop
- Windows startup registration and diagnostics

The built-in local provider is intentionally a lightweight offline fallback; full natural-language AI requires a configured compatible provider or local AI server.

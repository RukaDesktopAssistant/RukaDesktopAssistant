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

## Repository status
The repository is being built incrementally. Early commits focus on a clean, testable desktop shell before adding AI and PC-control features.

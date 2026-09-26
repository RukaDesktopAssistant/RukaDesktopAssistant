# るか デスクトップアシスタント

Windowsデスクトップに住むAIコンパニオン「るか」のプロジェクトです。

## コンセプト
Ruka lives on the Windows desktop as a lightweight 2D character rather than a normal app window. She can move around the desktop, converse by voice, open a dedicated chat window, and eventually perform permission-controlled PC actions.

## 開発方針
- Free-first: no paid service is required for the basic desktop shell.
- Quiet by default: Ruka should not interrupt games, calls, or focused work.
- Explicit permissions: PC control is granular and dangerous actions require confirmation.
- Persistent conversation history is separate from long-term memory.
- Long-term memory is stored only when the user explicitly asks Ruka to remember something.
- Multi-monitor and per-game/per-app behavior are first-class features.

## 技術構成
- C# / .NET 8
- WPF
- Windows APIs where appropriate

## 開発ロードマップ
1. Transparent desktop character window
2. Character movement and idle behavior
3. Speech bubbles and animation
4. Dedicated chat window
5. Voice wake word, STT and TTS
6. AI conversation
7. Persistent history and explicit memory
8. Multi-monitor/app/game context
9. Granular PC permissions and safe operations
10. Avatar polish, packaging and installer

## 現在の状態（v0.3.0）
Windows版はGitHub Actionsでビルドし、自己完結型のwin-x64 EXEまたはポータブルZIPとして公開できます。

現在実装済み：
- Built-in Ruka vector avatar with drag, double-click chat, idle/talk/walk/sleep states
- Persistent conversation history and explicit long-term memory
- Japanese wake-word voice input and Windows TTS
- OpenAI-compatible and generic HTTP AI providers with configurable history and encrypted API-key storage
- Personality presets and per-game behavior profiles
- Multi-monitor profiles and game-side positioning
- Permissioned PC actions with confirmation for dangerous operations
- Global shortcuts, pause/resume, and emergency stop
- Windows startup registration and diagnostics

アバターは外部画像に依存せず、アプリ本体に内蔵したWPFベクター描画で表示されます。そのため、初回起動時に画像素材が見つからず仮アバターへ戻ることはありません。

内蔵ローカルプロバイダーは軽量なオフライン用フォールバックです。本格的な自然言語AIには対応プロバイダーまたはるかAIサーバーの設定が必要です。

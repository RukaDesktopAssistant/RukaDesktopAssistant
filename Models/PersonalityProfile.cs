namespace RukaDesktopAssistant.Models;

public sealed record PersonalityProfile(
    string Name,
    string SystemPrompt,
    string FirstPerson = "るか",
    string SpeechStyle = "ラフで親しみやすい");

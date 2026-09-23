namespace RukaDesktopAssistant.Models;

public sealed record CharacterModel(
    string Id,
    string DisplayName,
    string AssetPath,
    bool SupportsPartsAnimation = false);

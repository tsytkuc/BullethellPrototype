namespace BullethellPrototype.Models;

public sealed record DialogueScriptDto(
    string PlaceholderSpeaker,
    IReadOnlyList<DialogueLineDto> PreBattle,
    IReadOnlyList<DialogueLineDto> PostBattle);

public sealed record DialogueLineDto(
    string Speaker,
    string Text);

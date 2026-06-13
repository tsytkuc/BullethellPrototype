namespace BullethellPrototype.Models;

public sealed record CharacterProfileDto(
    string Id,
    string DisplayName,
    string PortraitImagePath,
    string PortraitAlt,
    bool PlaceholderVisual,
    string Notes);

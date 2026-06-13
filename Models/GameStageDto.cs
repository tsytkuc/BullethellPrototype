namespace BullethellPrototype.Models;

public sealed record GameStageDto(
    string Id,
    string StageLabel,
    string Description,
    string CharacterProfileFile,
    string DialogueScriptFile,
    IReadOnlyList<SceneDefinitionDto> Scenes,
    PlayerTuningDto Player,
    BattleTuningDto Battle,
    IReadOnlyList<PatternReferenceDto> Patterns);

public sealed record GameStageSummaryDto(
    string Id,
    string StageLabel,
    string Description);

public sealed record SceneDefinitionDto(
    string Scene,
    string Label,
    string Summary,
    string StandbyMessage,
    bool ShowInFlow,
    float ReadyDelaySeconds);

public sealed record PatternReferenceDto(
    string Id,
    string Label,
    string Usage,
    string PreviewPatternId);

public sealed record PlayerTuningDto(
    int Lives,
    int Bombs,
    float Radius,
    float Speed,
    float FocusSpeed,
    float ShotCooldown,
    float BombCooldown,
    float RespawnInvulnerabilitySeconds,
    float BombInvulnerabilitySeconds,
    IReadOnlyList<PlayerShotDefinitionDto> Shots);

public sealed record PlayerShotDefinitionDto(
    float OffsetX,
    float OffsetY,
    float Speed,
    float Radius,
    int Damage);

public sealed record BattleTuningDto(
    int BombBossDamage,
    int BombMobDamage,
    IReadOnlyList<WaveDefinitionDto> Waves,
    BossDefinitionDto Boss);

public sealed record WaveDefinitionDto(
    string Id,
    string Label,
    float StartSeconds,
    float EndSeconds,
    float SpawnIntervalSeconds,
    string PatternId,
    int BulletCount,
    float BulletSpeed,
    float SpreadAngleRadians,
    float FireCooldownSeconds,
    EnemyPrototypeDto Enemy,
    IReadOnlyList<float> LaneXs);

public sealed record BossDefinitionDto(
    string PatternIdAimed,
    string PatternIdRadial,
    float SpawnAfterSeconds,
    int AimedBulletCount,
    float AimedBulletSpeed,
    float AimedSpreadRadians,
    float AimedCooldownSeconds,
    int RadialBulletCount,
    float RadialBulletSpeed,
    float RadialCooldownSeconds,
    EnemyPrototypeDto Enemy);

public sealed record EnemyPrototypeDto(
    int HitPoints,
    float Width,
    float Height,
    float SpeedY,
    float Drift,
    float StartX,
    float StartY,
    float EntranceY = 0f,
    int DefeatScore = 0);

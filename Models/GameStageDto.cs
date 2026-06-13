namespace BullethellPrototype.Models;

public sealed record GameStageDto(
    string Id,
    string StageLabel,
    IReadOnlyList<FlowStepDto> Flow,
    SceneTextSetDto SceneText,
    DialogueSetDto Dialogue,
    PlayerTuningDto Player,
    BattleTuningDto Battle);

public sealed record FlowStepDto(
    string Scene,
    string Label,
    string Summary);

public sealed record SceneTextSetDto(
    IReadOnlyDictionary<string, string> Labels,
    IReadOnlyDictionary<string, string> StandbyMessages);

public sealed record DialogueSetDto(
    string PlaceholderSpeaker,
    IReadOnlyList<DialogueLineDto> PreBattle,
    IReadOnlyList<DialogueLineDto> PostBattle);

public sealed record DialogueLineDto(
    string Speaker,
    string Text);

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
    float MobSpawnDelaySeconds,
    float MobSpawnIntervalSeconds,
    float BossSpawnAfterSeconds,
    int MobBulletCount,
    float MobBulletSpeed,
    float MobSpreadAngleRadians,
    float MobFireCooldownSeconds,
    int BossAimedBulletCount,
    float BossAimedBulletSpeed,
    float BossAimedSpreadRadians,
    float BossAimedCooldownSeconds,
    int BossRadialBulletCount,
    float BossRadialBulletSpeed,
    float BossRadialCooldownSeconds,
    int BombBossDamage,
    int BombMobDamage,
    EnemyPrototypeDto Mob,
    EnemyPrototypeDto Boss);

public sealed record EnemyPrototypeDto(
    int HitPoints,
    float Width,
    float Height,
    float SpeedY,
    float Drift,
    float StartX,
    float StartY,
    float EntranceY = 0f);

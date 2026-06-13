using System.Numerics;
using BenedictBulletHell.Patterns;
using BenedictBulletHell.Patterns.Core;

namespace BullethellPrototype.Services;

public sealed class PatternCatalog
{
    private const float SampleStep = 1f / 120f;
    private static readonly Vector2 BossOrigin = new(480f, 140f);
    private static readonly Vector2 PlayerAnchor = new(480f, 540f);

    private readonly IReadOnlyDictionary<string, PatternDefinition> _patterns;

    public PatternCatalog()
    {
        _patterns = new Dictionary<string, PatternDefinition>
        {
            ["spiral-bloom"] = new(
                "spiral-bloom",
                "Spiral Bloom",
                "A foundational continuous spiral for validating density and rotation speed.",
                () => Pattern.Spiral(
                    bulletsPerRevolution: 20,
                    totalRevolutions: 6f,
                    speed: 150f,
                    rotationSpeed: 270f,
                    startAngle: -90f,
                    looping: false),
                PreviewDuration: 8f,
                LoopPreview: true),
            ["ring-pulse"] = new(
                "ring-pulse",
                "Ring Pulse",
                "A basic full-screen ring pattern layered at fixed intervals.",
                () => Pattern.Repeat(
                    Pattern.Ring(count: 18, speed: 180f, startAngle: 0f),
                    count: 16,
                    delay: 0.5f),
                PreviewDuration: 7.6f,
                LoopPreview: true),
            ["aimed-burst"] = new(
                "aimed-burst",
                "Aimed Burst",
                "An aimed burst toward the player's initial position, useful for approximating boss attacks in Unity.",
                () => Pattern.Repeat(
                    Pattern.Aimed(count: 5, speed: 220f, spreadAngle: 22f),
                    count: 18,
                    delay: 0.4f),
                PreviewDuration: 7.2f,
                LoopPreview: true,
                Target: PlayerAnchor),
            ["rotating-wave"] = new(
                "rotating-wave",
                "Rotating Wave",
                "A compound bullet hell pattern that layers a rotating modifier over a wave form.",
                () => Pattern.Rotating(
                    Pattern.Repeat(
                        Pattern.Wave(
                            bulletCount: 14,
                            baseDirection: 90f,
                            waveAmplitude: 36f,
                            waveFrequency: 1.75f,
                            speed: 170f),
                        count: 20,
                        delay: 0.22f),
                    degreesPerSecond: 32f),
                PreviewDuration: 4.5f,
                LoopPreview: true),
            ["boss-sequence"] = new(
                "boss-sequence",
                "Boss Sequence",
                "A boss-phase style sequence that rotates through Ring -> Spread -> Spiral.",
                () => Pattern.Sequence(
                    Pattern.Repeat(
                        Pattern.Ring(count: 14, speed: 150f, startAngle: -90f),
                        count: 4,
                        delay: 0.45f),
                    Pattern.Repeat(
                        Pattern.Spread(count: 9, angleSpread: 65f, speed: 210f, baseAngle: 90f),
                        count: 6,
                        delay: 0.28f),
                    Pattern.Spiral(
                        bulletsPerRevolution: 16,
                        totalRevolutions: 3f,
                        speed: 145f,
                        rotationSpeed: 320f,
                        startAngle: -90f,
                        looping: false),
                    Pattern.Repeat(
                        Pattern.Aimed(count: 3, speed: 240f, spreadAngle: 16f),
                        count: 5,
                        delay: 0.35f)),
                PreviewDuration: 7f,
                LoopPreview: true,
                Target: PlayerAnchor),
        };
    }

    public IReadOnlyList<PatternSummaryDto> GetSummaries()
    {
        return _patterns.Values
            .Select(pattern => new PatternSummaryDto(pattern.Id, pattern.Label, pattern.Description))
            .ToList();
    }

    public PatternSampleDto? TryBuildSample(string id)
    {
        if (!_patterns.TryGetValue(id, out var definition))
        {
            return null;
        }

        var pattern = definition.Factory();
        var duration = definition.PreviewDuration > 0f
            ? definition.PreviewDuration
            : pattern.IsLooping
                ? 8f
                : Math.Clamp(pattern.Duration, 2f, 10f);

        var context = new PatternContext
        {
            Origin = BossOrigin,
            Target = definition.Target,
            PatternAge = 0f,
        };

        var spawns = new List<BulletSpawnDto>();
        var lastTime = 0f;

        for (var currentTime = SampleStep; currentTime <= duration + 0.0001f; currentTime += SampleStep)
        {
            context.PatternAge = currentTime;

            foreach (var spawn in pattern.GetSpawns(lastTime, currentTime, context))
            {
                spawns.Add(new BulletSpawnDto(
                    SpawnTime: currentTime,
                    X: spawn.Position.X,
                    Y: spawn.Position.Y,
                    DirectionX: spawn.Direction.X,
                    DirectionY: spawn.Direction.Y,
                    Speed: spawn.Speed,
                    Angle: spawn.Angle));
            }

            lastTime = currentTime;
        }

        return new PatternSampleDto(
            definition.Id,
            definition.Label,
            definition.Description,
            duration,
            definition.LoopPreview,
            BossOrigin.X,
            BossOrigin.Y,
            definition.Target?.X,
            definition.Target?.Y,
            spawns);
    }
}

public sealed record PatternSummaryDto(string Id, string Label, string Description);

public sealed record PatternSampleDto(
    string Id,
    string Label,
    string Description,
    float Duration,
    bool Looping,
    float OriginX,
    float OriginY,
    float? TargetX,
    float? TargetY,
    IReadOnlyList<BulletSpawnDto> Spawns);

public sealed record BulletSpawnDto(
    float SpawnTime,
    float X,
    float Y,
    float DirectionX,
    float DirectionY,
    float Speed,
    float Angle);

internal sealed record PatternDefinition(
    string Id,
    string Label,
    string Description,
    Func<IBulletPattern> Factory,
    float PreviewDuration,
    bool LoopPreview = false,
    Vector2? Target = null);

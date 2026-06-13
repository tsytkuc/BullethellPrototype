using System.Text.Json;
using BullethellPrototype.Models;

namespace BullethellPrototype.Services;

public sealed class GameStageCatalog
{
    private readonly IReadOnlyDictionary<string, GameStageDto> _stages;
    private readonly JsonSerializerOptions _jsonOptions;

    public GameStageCatalog()
    {
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
        };

        _stages = new Dictionary<string, GameStageDto>
        {
            ["stage-1-prototype"] = BuildStage1(),
            ["stage-2-prototype"] = BuildStage2(),
        };

        ValidateOrThrow();
    }

    public IReadOnlyList<GameStageSummaryDto> GetSummaries()
    {
        return _stages.Values
            .Select(stage => new GameStageSummaryDto(stage.Id, stage.StageLabel, stage.Description))
            .ToList();
    }

    public GameStageDto? TryGet(string id)
    {
        return _stages.TryGetValue(id, out var stage) ? stage : null;
    }

    public async Task ExportUnityJsonAsync(string rootPath, CancellationToken cancellationToken = default)
    {
        var outputDirectory = Path.Combine(rootPath, "unity", "Assets", "StreamingAssets", "Stages");
        Directory.CreateDirectory(outputDirectory);

        foreach (var stage in _stages.Values)
        {
            var outputPath = Path.Combine(outputDirectory, $"{stage.Id}.json");
            var json = JsonSerializer.Serialize(stage, _jsonOptions);
            await File.WriteAllTextAsync(outputPath, json, cancellationToken);
        }
    }

    public void ValidateOrThrow()
    {
        foreach (var stage in _stages.Values)
        {
            if (string.IsNullOrWhiteSpace(stage.Id))
            {
                throw new InvalidOperationException("Stage id is required.");
            }

            if (string.IsNullOrWhiteSpace(stage.CharacterProfileFile))
            {
                throw new InvalidOperationException($"Stage '{stage.Id}' must define CharacterProfileFile.");
            }

            if (string.IsNullOrWhiteSpace(stage.DialogueScriptFile))
            {
                throw new InvalidOperationException($"Stage '{stage.Id}' must define DialogueScriptFile.");
            }

            if (stage.Scenes.Count == 0)
            {
                throw new InvalidOperationException($"Stage '{stage.Id}' must define scenes.");
            }

            if (stage.Scenes.All(scene => scene.ShowInFlow == false))
            {
                throw new InvalidOperationException($"Stage '{stage.Id}' must expose at least one flow scene.");
            }

            if (stage.Player.Shots.Count == 0)
            {
                throw new InvalidOperationException($"Stage '{stage.Id}' must define player shots.");
            }

            if (stage.Battle.Waves.Count == 0)
            {
                throw new InvalidOperationException($"Stage '{stage.Id}' must define at least one battle wave.");
            }

            foreach (var wave in stage.Battle.Waves)
            {
                if (wave.LaneXs.Count == 0)
                {
                    throw new InvalidOperationException($"Stage '{stage.Id}' wave '{wave.Id}' must define at least one lane.");
                }

                if (wave.EndSeconds <= wave.StartSeconds)
                {
                    throw new InvalidOperationException($"Stage '{stage.Id}' wave '{wave.Id}' must have EndSeconds > StartSeconds.");
                }
            }
        }
    }

    private static IReadOnlyList<SceneDefinitionDto> BuildDefaultScenes()
    {
        return
        [
            new("stage-intro", "Stage Intro", "This is the stage intro. After a short ready period, it moves into the briefing.", "This is the stage opening sequence. Once it is ready, the flow connects to the briefing.", true, 1.4f),
            new("dialogue-pre", "Briefing", "This is the pre-battle dialogue section. Finishing the lines moves into the main battle.", "Preparing the pre-battle communication sequence.", true, 0f),
            new("battle", "Battle", "This is the main gameplay section. Clearing mobs and the boss advances to the post-battle dialogue.", "The main gameplay section is active. Once the battle ends, the post-battle dialogue appears here.", true, 0f),
            new("dialogue-post", "After Talk", "This is the post-battle dialogue section. Finishing it shows the stage clear screen.", "Preparing the post-battle communication sequence.", true, 0f),
            new("stage-clear", "Stage Clear", "This is the stage clear screen. You can restart the stage flow from here.", "The stage clear screen is active. After reviewing it, you can return to the first intro step.", true, 1.4f),
            new("game-over", "Game Over", "This is the game over screen. You can retry the stage from here.", "Mission failed. You can retry the stage from here.", false, 1.2f),
        ];
    }

    private static GameStageDto BuildStage1()
    {
        return new GameStageDto(
            Id: "stage-1-prototype",
            StageLabel: "Stage 1",
            Description: "City-edge corridor prototype with one warm-up wave set and a single boss finish.",
            CharacterProfileFile: "operator-chloe.json",
            DialogueScriptFile: "stage-1-prototype.json",
            Scenes: BuildDefaultScenes(),
            Player: BuildDefaultPlayer(),
            Battle: new BattleTuningDto(
                BombBossDamage: 40,
                BombMobDamage: 999,
                Waves:
                [
                    new(
                        Id: "scrap-left-right",
                        Label: "Scrap Patrol",
                        StartSeconds: 0f,
                        EndSeconds: 7f,
                        SpawnIntervalSeconds: 1.25f,
                        PatternId: "aimed-burst",
                        BulletCount: 3,
                        BulletSpeed: 180f,
                        SpreadAngleRadians: 0.22f,
                        FireCooldownSeconds: 1.35f,
                        Enemy: new EnemyPrototypeDto(6, 34f, 34f, 110f, 60f, 140f, -30f, 0f, 120),
                        LaneXs: [140f, 330f, 520f, 710f]),
                    new(
                        Id: "tight-center-lane",
                        Label: "Center Pressure",
                        StartSeconds: 7f,
                        EndSeconds: 12f,
                        SpawnIntervalSeconds: 1.05f,
                        PatternId: "ring-pulse",
                        BulletCount: 5,
                        BulletSpeed: 190f,
                        SpreadAngleRadians: 0.14f,
                        FireCooldownSeconds: 1.1f,
                        Enemy: new EnemyPrototypeDto(7, 36f, 36f, 122f, 38f, 235f, -30f, 0f, 150),
                        LaneXs: [235f, 480f, 725f]),
                ],
                Boss: new BossDefinitionDto(
                    PatternIdAimed: "aimed-burst",
                    PatternIdRadial: "spiral-bloom",
                    SpawnAfterSeconds: 12f,
                    AimedBulletCount: 7,
                    AimedBulletSpeed: 230f,
                    AimedSpreadRadians: 0.18f,
                    AimedCooldownSeconds: 0.95f,
                    RadialBulletCount: 16,
                    RadialBulletSpeed: 170f,
                    RadialCooldownSeconds: 2.3f,
                    Enemy: new EnemyPrototypeDto(260, 140f, 100f, 80f, 210f, 480f, -120f, 120f, 2000))),
            Patterns:
            [
                new("aimed-burst", "Aimed Burst", "Used by the warm-up mobs and aimed boss volleys.", "aimed-burst"),
                new("ring-pulse", "Ring Pulse", "Used by the center pressure wave to create denser fan timing.", "ring-pulse"),
                new("spiral-bloom", "Spiral Bloom", "Used as the boss radial finisher pattern.", "spiral-bloom"),
            ]);
    }

    private static GameStageDto BuildStage2()
    {
        return new GameStageDto(
            Id: "stage-2-prototype",
            StageLabel: "Stage 2",
            Description: "Industrial channel prototype with longer wave chaining and a denser boss phase.",
            CharacterProfileFile: "operator-chloe.json",
            DialogueScriptFile: "stage-2-prototype.json",
            Scenes: BuildDefaultScenes(),
            Player: BuildDefaultPlayer() with
            {
                Bombs = 2,
            },
            Battle: new BattleTuningDto(
                BombBossDamage: 34,
                BombMobDamage: 999,
                Waves:
                [
                    new(
                        Id: "outer-lane-screen",
                        Label: "Outer Lane Screen",
                        StartSeconds: 0f,
                        EndSeconds: 8f,
                        SpawnIntervalSeconds: 1.05f,
                        PatternId: "rotating-wave",
                        BulletCount: 4,
                        BulletSpeed: 205f,
                        SpreadAngleRadians: 0.19f,
                        FireCooldownSeconds: 1.15f,
                        Enemy: new EnemyPrototypeDto(8, 34f, 34f, 126f, 82f, 120f, -30f, 0f, 160),
                        LaneXs: [120f, 300f, 660f, 840f]),
                    new(
                        Id: "mid-lane-crush",
                        Label: "Mid Lane Crush",
                        StartSeconds: 8f,
                        EndSeconds: 15f,
                        SpawnIntervalSeconds: 0.92f,
                        PatternId: "boss-sequence",
                        BulletCount: 6,
                        BulletSpeed: 220f,
                        SpreadAngleRadians: 0.16f,
                        FireCooldownSeconds: 0.95f,
                        Enemy: new EnemyPrototypeDto(10, 38f, 38f, 134f, 52f, 220f, -36f, 0f, 180),
                        LaneXs: [220f, 400f, 560f, 740f]),
                ],
                Boss: new BossDefinitionDto(
                    PatternIdAimed: "boss-sequence",
                    PatternIdRadial: "rotating-wave",
                    SpawnAfterSeconds: 15f,
                    AimedBulletCount: 9,
                    AimedBulletSpeed: 250f,
                    AimedSpreadRadians: 0.14f,
                    AimedCooldownSeconds: 0.82f,
                    RadialBulletCount: 20,
                    RadialBulletSpeed: 182f,
                    RadialCooldownSeconds: 1.95f,
                    Enemy: new EnemyPrototypeDto(340, 154f, 108f, 86f, 230f, 480f, -140f, 116f, 2600))),
            Patterns:
            [
                new("rotating-wave", "Rotating Wave", "Used for lateral pressure in the opening wave and boss radial pressure.", "rotating-wave"),
                new("boss-sequence", "Boss Sequence", "Used for tighter Stage 2 attack routing and boss aimed pressure.", "boss-sequence"),
            ]);
    }

    private static PlayerTuningDto BuildDefaultPlayer()
    {
        return new PlayerTuningDto(
            Lives: 3,
            Bombs: 3,
            Radius: 10f,
            Speed: 320f,
            FocusSpeed: 170f,
            ShotCooldown: 0.08f,
            BombCooldown: 0.5f,
            RespawnInvulnerabilitySeconds: 2f,
            BombInvulnerabilitySeconds: 2.2f,
            Shots:
            [
                new(-9f, -14f, -620f, 4f, 1),
                new(9f, -14f, -620f, 4f, 1),
                new(0f, -22f, -700f, 4f, 2),
            ]);
    }
}

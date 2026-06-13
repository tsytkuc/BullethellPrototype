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
        };
    }

    public IReadOnlyList<GameStageDto> GetAll()
    {
        return _stages.Values.ToList();
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

    private static GameStageDto BuildStage1()
    {
        return new GameStageDto(
            Id: "stage-1-prototype",
            StageLabel: "Stage 1",
            Flow:
            [
                new("stage-intro", "Stage Intro", "Stage 1 の導入表示です。短い準備のあと、会話パートへ進みます。"),
                new("dialogue-pre", "Briefing", "出撃前の会話パートです。内容を送り終えるとゲーム本編に入ります。"),
                new("battle", "Battle", "ゲーム本編です。ザコ敵とボスを突破すると戦闘後会話へ進みます。"),
                new("dialogue-post", "After Talk", "戦闘後の会話パートです。締めの会話が終わるとステージクリアを表示します。"),
                new("stage-clear", "Stage Clear", "ステージクリア表示です。ここから Stage 1 を最初から再確認できます。"),
            ],
            SceneText: new SceneTextSetDto(
                Labels: new Dictionary<string, string>
                {
                    ["stage-intro"] = "Stage Intro",
                    ["dialogue-pre"] = "Briefing",
                    ["battle"] = "Battle",
                    ["dialogue-post"] = "After Talk",
                    ["stage-clear"] = "Clear",
                    ["game-over"] = "Game Over",
                },
                StandbyMessages: new Dictionary<string, string>
                {
                    ["stage-intro"] = "Stage 1 の開始演出です。準備が整うと、ここから Briefing に接続されます。",
                    ["battle"] = "現在はゲーム本編の進行中です。戦闘が終わると、ここに戦闘後の会話が表示されます。",
                    ["stage-clear"] = "Stage 1 Clear を表示中です。確認後はここから最初の導入に戻れます。",
                    ["game-over"] = "ミッション失敗です。ここから Stage 1 を再試行できます。",
                }),
            Dialogue: new DialogueSetDto(
                PlaceholderSpeaker: "System",
                PreBattle:
                [
                    new("Operator Chloe", "Stage 1, city-edge corridor. Sensors say the air is filthy with weak hostiles, so let's use them as a warm-up."),
                    new("Operator Chloe", "Your frame is armed with a forward shot and three emergency bombs. If the screen gets ugly, don't be shy about clearing space."),
                    new("Operator Chloe", "One command unit is hiding behind the trash waves. Break the formation, drop the boss, and come back in one piece."),
                ],
                PostBattle:
                [
                    new("Operator Chloe", "Clean finish. The lane is ours, and your timing on that bomb was better than I expected."),
                    new("Operator Chloe", "We'll tighten the patterns, add proper boss scripting, and make the next stage meaner. For now, log this as a successful prototype pass."),
                ]),
            Player: new PlayerTuningDto(
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
                ]),
            Battle: new BattleTuningDto(
                MobSpawnDelaySeconds: 0.8f,
                MobSpawnIntervalSeconds: 1.3f,
                BossSpawnAfterSeconds: 12f,
                MobBulletCount: 3,
                MobBulletSpeed: 180f,
                MobSpreadAngleRadians: 0.22f,
                MobFireCooldownSeconds: 1.35f,
                BossAimedBulletCount: 7,
                BossAimedBulletSpeed: 230f,
                BossAimedSpreadRadians: 0.18f,
                BossAimedCooldownSeconds: 0.95f,
                BossRadialBulletCount: 16,
                BossRadialBulletSpeed: 170f,
                BossRadialCooldownSeconds: 2.3f,
                BombBossDamage: 40,
                BombMobDamage: 999,
                Mob: new EnemyPrototypeDto(
                    HitPoints: 6,
                    Width: 34f,
                    Height: 34f,
                    SpeedY: 110f,
                    Drift: 60f,
                    StartX: 140f,
                    StartY: -30f),
                Boss: new EnemyPrototypeDto(
                    HitPoints: 260,
                    Width: 140f,
                    Height: 100f,
                    SpeedY: 80f,
                    Drift: 210f,
                    StartX: 480f,
                    StartY: -120f,
                    EntranceY: 120f)));
    }
}

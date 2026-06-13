using System;

namespace BullethellPrototype.Unity
{
    [Serializable]
    public sealed class StageDefinition
    {
        public string id;
        public string stageLabel;
        public string description;
        public string characterProfileFile;
        public string dialogueScriptFile;
        public SceneDefinition[] scenes;
        public PlayerTuning player;
        public BattleTuning battle;
        public PatternReference[] patterns;
    }

    [Serializable]
    public sealed class SceneDefinition
    {
        public string scene;
        public string label;
        public string summary;
        public string standbyMessage;
        public bool showInFlow;
        public float readyDelaySeconds;
    }

    [Serializable]
    public sealed class PatternReference
    {
        public string id;
        public string label;
        public string usage;
        public string previewPatternId;
    }

    [Serializable]
    public sealed class PlayerTuning
    {
        public int lives;
        public int bombs;
        public float radius;
        public float speed;
        public float focusSpeed;
        public float shotCooldown;
        public float bombCooldown;
        public float respawnInvulnerabilitySeconds;
        public float bombInvulnerabilitySeconds;
        public PlayerShotDefinition[] shots;
    }

    [Serializable]
    public sealed class PlayerShotDefinition
    {
        public float offsetX;
        public float offsetY;
        public float speed;
        public float radius;
        public int damage;
    }

    [Serializable]
    public sealed class BattleTuning
    {
        public int bombBossDamage;
        public int bombMobDamage;
        public WaveDefinition[] waves;
        public BossDefinition boss;
    }

    [Serializable]
    public sealed class WaveDefinition
    {
        public string id;
        public string label;
        public float startSeconds;
        public float endSeconds;
        public float spawnIntervalSeconds;
        public string patternId;
        public int bulletCount;
        public float bulletSpeed;
        public float spreadAngleRadians;
        public float fireCooldownSeconds;
        public EnemyPrototype enemy;
        public float[] laneXs;
    }

    [Serializable]
    public sealed class BossDefinition
    {
        public string patternIdAimed;
        public string patternIdRadial;
        public float spawnAfterSeconds;
        public int aimedBulletCount;
        public float aimedBulletSpeed;
        public float aimedSpreadRadians;
        public float aimedCooldownSeconds;
        public int radialBulletCount;
        public float radialBulletSpeed;
        public float radialCooldownSeconds;
        public EnemyPrototype enemy;
    }

    [Serializable]
    public sealed class EnemyPrototype
    {
        public int hitPoints;
        public float width;
        public float height;
        public float speedY;
        public float drift;
        public float startX;
        public float startY;
        public float entranceY;
        public int defeatScore;
    }
}

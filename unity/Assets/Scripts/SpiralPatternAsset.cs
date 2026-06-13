using System.Collections.Generic;
using UnityEngine;

namespace BullethellPrototype.Unity
{
    [CreateAssetMenu(
        fileName = "SpiralPattern",
        menuName = "BullethellPrototype/Patterns/Spiral")]
    public sealed class SpiralPatternAsset : BulletPatternAsset
    {
        [SerializeField, Min(1)] private int bulletsPerRevolution = 18;
        [SerializeField, Min(0.01f)] private float bulletSpeed = 6f;
        [SerializeField, Min(1f)] private float rotationSpeedDegrees = 240f;
        [SerializeField] private float startAngleDegrees = -90f;

        public override void GetSpawns(
            float lastTime,
            float currentTime,
            BulletPatternContext context,
            List<BulletSpawnData> results)
        {
            float timePerBullet = (360f / rotationSpeedDegrees) / bulletsPerRevolution;
            if (timePerBullet <= 0f)
            {
                return;
            }

            int startIndex = Mathf.Max(0, Mathf.FloorToInt(lastTime / timePerBullet));
            int endIndex = Mathf.CeilToInt(currentTime / timePerBullet);

            for (int i = startIndex; i < endIndex; i += 1)
            {
                float spawnTime = i * timePerBullet;
                if (spawnTime <= lastTime || spawnTime > currentTime)
                {
                    continue;
                }

                float angle = startAngleDegrees + spawnTime * rotationSpeedDegrees;
                float radians = angle * Mathf.Deg2Rad;
                var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                results.Add(new BulletSpawnData(context.Origin, direction, bulletSpeed));
            }
        }
    }
}

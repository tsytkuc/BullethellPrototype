using System.Collections.Generic;
using UnityEngine;

namespace BullethellPrototype.Unity
{
    [CreateAssetMenu(
        fileName = "RingPattern",
        menuName = "BullethellPrototype/Patterns/Ring")]
    public sealed class RingPatternAsset : BulletPatternAsset
    {
        [SerializeField, Min(1)] private int bulletCount = 16;
        [SerializeField, Min(0.01f)] private float bulletSpeed = 6f;
        [SerializeField, Min(0.01f)] private float intervalSeconds = 0.45f;
        [SerializeField] private float startAngleDegrees = -90f;

        public override void GetSpawns(
            float lastTime,
            float currentTime,
            BulletPatternContext context,
            List<BulletSpawnData> results)
        {
            int previousBurstIndex = Mathf.FloorToInt(Mathf.Max(lastTime, 0f) / intervalSeconds);
            int currentBurstIndex = Mathf.FloorToInt(Mathf.Max(currentTime, 0f) / intervalSeconds);

            for (int burstIndex = previousBurstIndex; burstIndex <= currentBurstIndex; burstIndex += 1)
            {
                float burstTime = burstIndex * intervalSeconds;
                if (burstTime <= lastTime || burstTime > currentTime)
                {
                    continue;
                }

                float angleStep = 360f / bulletCount;
                for (int i = 0; i < bulletCount; i += 1)
                {
                    float angle = startAngleDegrees + angleStep * i;
                    float radians = angle * Mathf.Deg2Rad;
                    var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                    results.Add(new BulletSpawnData(context.Origin, direction, bulletSpeed));
                }
            }
        }
    }
}

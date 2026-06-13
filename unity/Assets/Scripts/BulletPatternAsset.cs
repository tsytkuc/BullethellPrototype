using System.Collections.Generic;
using UnityEngine;

namespace BullethellPrototype.Unity
{
    public abstract class BulletPatternAsset : ScriptableObject
    {
        public abstract void GetSpawns(
            float lastTime,
            float currentTime,
            BulletPatternContext context,
            List<BulletSpawnData> results);
    }
}

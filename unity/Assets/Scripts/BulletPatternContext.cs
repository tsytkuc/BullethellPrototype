using System;
using UnityEngine;

namespace BullethellPrototype.Unity
{
    [Serializable]
    public struct BulletPatternContext
    {
        public Vector2 Origin;
        public Vector2? Target;
        public float PatternAge;
    }
}

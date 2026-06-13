using System;
using UnityEngine;

namespace BullethellPrototype.Unity
{
    [Serializable]
    public struct BulletSpawnData
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float Speed;

        public BulletSpawnData(Vector2 position, Vector2 direction, float speed)
        {
            Position = position;
            Direction = direction.normalized;
            Speed = speed;
        }
    }
}

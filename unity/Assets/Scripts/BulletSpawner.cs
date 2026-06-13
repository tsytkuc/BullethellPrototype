using System.Collections.Generic;
using UnityEngine;

namespace BullethellPrototype.Unity
{
    public sealed class BulletSpawner : MonoBehaviour
    {
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private Transform bulletParent;
        [SerializeField, Min(0)] private int prewarmCount = 32;

        private readonly Queue<Bullet> _pool = new();

        private void Awake()
        {
            if (bulletParent == null)
            {
                bulletParent = transform;
            }

            Prewarm();
        }

        public void Spawn(BulletSpawnData spawnData, float speedMultiplier = 1f)
        {
            var bullet = GetBullet();
            bullet.Initialize(
                spawnData.Position,
                spawnData.Direction,
                spawnData.Speed * speedMultiplier,
                ReleaseBullet);
        }

        private void Prewarm()
        {
            for (int i = 0; i < prewarmCount; i += 1)
            {
                ReleaseBullet(CreateBullet());
            }
        }

        private Bullet GetBullet()
        {
            return _pool.Count > 0 ? _pool.Dequeue() : CreateBullet();
        }

        private Bullet CreateBullet()
        {
            var bullet = Instantiate(bulletPrefab, bulletParent);
            bullet.gameObject.SetActive(false);
            return bullet;
        }

        private void ReleaseBullet(Bullet bullet)
        {
            bullet.gameObject.SetActive(false);
            bullet.transform.SetParent(bulletParent, false);
            _pool.Enqueue(bullet);
        }
    }
}

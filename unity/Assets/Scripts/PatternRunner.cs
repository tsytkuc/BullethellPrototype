using System.Collections.Generic;
using UnityEngine;

namespace BullethellPrototype.Unity
{
    public sealed class PatternRunner : MonoBehaviour
    {
        [SerializeField] private BulletPatternAsset pattern;
        [SerializeField] private BulletSpawner bulletSpawner;
        [SerializeField] private Transform originPoint;
        [SerializeField] private Transform targetPoint;
        [SerializeField] private bool playOnStart = true;
        [SerializeField, Range(1, 100)] private int bulletDensityPercent = 100;
        [SerializeField, Range(1, 100)] private int bulletSpeedPercent = 100;

        private readonly List<BulletSpawnData> _spawnBuffer = new();

        private bool _isPlaying;
        private float _elapsed;
        private float _lastQueryTime;
        private int _spawnSequence;

        private void Start()
        {
            if (playOnStart)
            {
                Play();
            }
        }

        private void Update()
        {
            if (!_isPlaying || pattern == null || bulletSpawner == null)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            _spawnBuffer.Clear();

            var context = new BulletPatternContext
            {
                Origin = originPoint != null ? (Vector2)originPoint.position : (Vector2)transform.position,
                Target = targetPoint != null ? (Vector2?)targetPoint.position : null,
                PatternAge = _elapsed,
            };

            pattern.GetSpawns(_lastQueryTime, _elapsed, context, _spawnBuffer);

            float speedMultiplier = bulletSpeedPercent / 100f;
            foreach (var spawn in _spawnBuffer)
            {
                if (!ShouldEmitSpawn(_spawnSequence))
                {
                    _spawnSequence += 1;
                    continue;
                }

                bulletSpawner.Spawn(spawn, speedMultiplier);
                _spawnSequence += 1;
            }

            _lastQueryTime = _elapsed;
        }

        public void Play()
        {
            _elapsed = 0f;
            _lastQueryTime = 0f;
            _spawnSequence = 0;
            _isPlaying = true;
        }

        public void Stop()
        {
            _isPlaying = false;
        }

        private bool ShouldEmitSpawn(int index)
        {
            if (bulletDensityPercent >= 100)
            {
                return true;
            }

            float hash = Mathf.Abs(Mathf.Sin((index + 1) * 12.9898f) * 43758.5453f);
            return (hash - Mathf.Floor(hash)) * 100f < bulletDensityPercent;
        }
    }
}

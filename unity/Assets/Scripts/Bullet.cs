using System;
using UnityEngine;

namespace BullethellPrototype.Unity
{
    public sealed class Bullet : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float defaultLifetime = 10f;

        private Vector2 _direction;
        private float _speed;
        private float _lifeRemaining;
        private Action<Bullet>? _release;

        public void Initialize(Vector2 position, Vector2 direction, float speed, Action<Bullet> release)
        {
            transform.position = position;
            _direction = direction.normalized;
            _speed = speed;
            _lifeRemaining = defaultLifetime;
            _release = release;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            transform.position += (Vector3)(_direction * _speed * Time.deltaTime);
            _lifeRemaining -= Time.deltaTime;

            if (_lifeRemaining <= 0f)
            {
                Release();
            }
        }

        private void OnBecameInvisible()
        {
            if (Application.isPlaying)
            {
                Release();
            }
        }

        private void Release()
        {
            _release?.Invoke(this);
        }
    }
}

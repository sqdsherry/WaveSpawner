using System;
using UnityEngine;

namespace WaveSpawner.Scripts.Enemies
{
    /// <summary>
    /// Base enemy entity that can be targeted and destroyed by the player.
    /// Supports both standard instantiation and high-performance object pooling.
    /// </summary>
    [DisallowMultipleComponent]
    public class Enemy : MonoBehaviour, IDamageable
    {
        private bool _isDead;
        private Action<Enemy> _returnToPool;
        private Collider _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        /// <summary>
        /// Event triggered when the enemy is killed.
        /// </summary>
        public event Action<Enemy> OnDied;

        /// <summary>
        /// Indicates whether the enemy is already dead/in the process of destruction.
        /// </summary>
        public bool IsDead => _isDead;

        /// <summary>
        /// Initializes the enemy with an optional callback to return it to an object pool.
        /// </summary>
        /// <param name="returnToPool">Action invoked to release the enemy back to the pool.</param>
        public void Init(Action<Enemy> returnToPool)
        {
            _returnToPool = returnToPool;
        }

        /// <summary>
        /// Resets the enemy state when retrieved from the object pool.
        /// </summary>
        public void ResetEnemy()
        {
            _isDead = false;
            if (_collider != null)
            {
                _collider.enabled = true;
            }
        }

        /// <summary>
        /// Destroys the enemy, disables its collider immediately to prevent multi-hits, and notifies listeners.
        /// </summary>
        public void Kill()
        {
            if (_isDead)
            {
                return;
            }

            _isDead = true;

            // Immediately disable collider to prevent repeated raycast hits within the same frame
            if (_collider != null)
            {
                _collider.enabled = false;
            }

            OnDied?.Invoke(this);
            OnDied = null;

            if (_returnToPool != null)
            {
                _returnToPool.Invoke(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            // Clear subscriptions upon forced destruction or scene unloading
            OnDied = null;
        }
    }
}

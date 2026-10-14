using UnityEngine;
using UnityEngine.Pool;
using WaveSpawner.Scripts.Enemies;

namespace WaveSpawner.Scripts.Spawning
{
    /// <summary>
    /// Enemy spawner implementing Round-Robin spawn point selection
    /// and built-in object pooling via <see cref="UnityEngine.Pool.ObjectPool{T}"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemySpawner : MonoBehaviour, ISpawner
    {
        [SerializeField]
        [Tooltip("Default enemy prefab instantiated or pooled when no override is specified.")]
        private Enemy _defaultEnemyPrefab;

        [SerializeField]
        [Tooltip("Collection of scene spawn points.")]
        private Transform[] _spawnPoints;

        [Header("Object Pool Settings")]
        [SerializeField]
        [Tooltip("Whether to use object pooling for default enemies.")]
        private bool _useObjectPool = true;

        [SerializeField]
        [Min(1)]
        [Tooltip("Initial capacity allocated for the enemy pool.")]
        private int _defaultPoolCapacity = 10;

        [SerializeField]
        [Min(1)]
        [Tooltip("Maximum allowed size of the enemy pool.")]
        private int _maxPoolSize = 50;

        private IObjectPool<Enemy> _enemyPool;
        private int _currentSpawnPointIndex;

        private void Awake()
        {
            if (_useObjectPool)
            {
                InitializePool();
            }
        }

        private void InitializePool()
        {
            if (_enemyPool == null)
            {
                _enemyPool = new ObjectPool<Enemy>(
                    createFunc: CreateEnemyForPool,
                    actionOnGet: OnGetFromPool,
                    actionOnRelease: OnReleaseToPool,
                    actionOnDestroy: OnDestroyPoolObject,
                    collectionCheck: true,
                    defaultCapacity: _defaultPoolCapacity,
                    maxSize: _maxPoolSize
                );
            }
        }

        private Enemy CreateEnemyForPool()
        {
            Enemy enemy = Instantiate(_defaultEnemyPrefab, transform);
            enemy.Init(ReleaseEnemyToPool);
            return enemy;
        }

        private void OnGetFromPool(Enemy enemy)
        {
            enemy.gameObject.SetActive(true);
            enemy.ResetEnemy();
        }

        private void OnReleaseToPool(Enemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(Enemy enemy)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        private void ReleaseEnemyToPool(Enemy enemy)
        {
            if (_enemyPool != null && enemy.gameObject.activeSelf)
            {
                _enemyPool.Release(enemy);
            }
        }

        /// <summary>
        /// Spawns an enemy instance at the current Round-Robin spawn point.
        /// </summary>
        /// <param name="prefabOverride">Optional prefab override. If null, the default pooled prefab is used.</param>
        /// <returns>The spawned or pooled <see cref="Enemy"/> instance, or null on failure.</returns>
        public Enemy SpawnEnemy(Enemy prefabOverride = null)
        {
            Enemy targetPrefab = prefabOverride != null ? prefabOverride : _defaultEnemyPrefab;

            if (targetPrefab == null)
            {
                Debug.LogError("[EnemySpawner] Enemy prefab is not set! Spawn aborted.", this);
                return null;
            }

            Vector3 spawnPosition = transform.position;
            Quaternion spawnRotation = transform.rotation;

            if (_spawnPoints != null && _spawnPoints.Length > 0)
            {
                if (_currentSpawnPointIndex >= _spawnPoints.Length)
                {
                    _currentSpawnPointIndex = 0;
                }

                Transform spawnPoint = _spawnPoints[_currentSpawnPointIndex];
                _currentSpawnPointIndex = (_currentSpawnPointIndex + 1) % _spawnPoints.Length;

                if (spawnPoint != null)
                {
                    spawnPosition = spawnPoint.position;
                    spawnRotation = spawnPoint.rotation;
                }
            }

            if (_useObjectPool && prefabOverride == null)
            {
                if (_enemyPool == null)
                {
                    InitializePool();
                }

                Enemy enemy = _enemyPool.Get();
                enemy.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
                return enemy;
            }

            Enemy spawnedEnemy = Instantiate(targetPrefab, spawnPosition, spawnRotation);
            return spawnedEnemy;
        }

        private void OnValidate()
        {
            if (_defaultEnemyPrefab == null)
            {
                Debug.LogWarning("[EnemySpawner] Default enemy prefab (_defaultEnemyPrefab) is not assigned!", this);
            }

            if (_defaultPoolCapacity < 1)
            {
                _defaultPoolCapacity = 1;
            }

            if (_maxPoolSize < _defaultPoolCapacity)
            {
                _maxPoolSize = _defaultPoolCapacity;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_spawnPoints == null || _spawnPoints.Length == 0)
            {
                // Draw fallback position of the spawner itself if no points are set
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
                Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
                return;
            }

            for (int i = 0; i < _spawnPoints.Length; i++)
            {
                Transform point = _spawnPoints[i];
                if (point == null) continue;

                // Spawn point position
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(point.position, 0.5f);

                // Forward direction indicator
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(point.position, point.forward * 1.5f);

                // Label for designer readability
                UnityEditor.Handles.Label(point.position + Vector3.up * 0.7f, $"Spawn Point {i + 1}");
            }
        }
#endif
    }
}

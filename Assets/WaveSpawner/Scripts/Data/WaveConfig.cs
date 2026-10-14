using UnityEngine;

namespace WaveSpawner.Scripts.Data
{
    /// <summary>
    /// Configuration data for an individual enemy wave.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveConfig_01", menuName = "WaveSpawner/Wave Config")]
    public class WaveConfig : ScriptableObject
    {
        [SerializeField]
        [Min(1)]
        [Tooltip("Total number of enemies to spawn in this wave (minimum 1).")]
        private int _enemyCount = 1;

        [SerializeField]
        [Min(0.05f)]
        [Tooltip("Delay in seconds between consecutive enemy spawns (minimum 0.05s).")]
        private float _spawnInterval = 1f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Delay in seconds before the next wave begins after all current enemies are destroyed (minimum 0s).")]
        private float _delayBeforeNextWave = 2f;

        /// <summary>
        /// Total number of enemies to spawn in this wave.
        /// </summary>
        public int EnemyCount => _enemyCount;

        /// <summary>
        /// Delay in seconds between consecutive enemy spawns.
        /// </summary>
        public float SpawnInterval => _spawnInterval;

        /// <summary>
        /// Delay in seconds before starting the next wave after clearing this wave.
        /// </summary>
        public float DelayBeforeNextWave => _delayBeforeNextWave;

        private void OnValidate()
        {
            if (_enemyCount < 1)
            {
                _enemyCount = 1;
            }

            if (_spawnInterval < 0.05f)
            {
                _spawnInterval = 0.05f;
            }

            if (_delayBeforeNextWave < 0f)
            {
                _delayBeforeNextWave = 0f;
            }
        }
    }
}

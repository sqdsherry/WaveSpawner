using System.Collections.Generic;
using UnityEngine;

namespace WaveSpawner.Scripts.Data
{
    /// <summary>
    /// Level configuration defining the ordered sequence of enemy waves.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveSequenceConfig", menuName = "WaveSpawner/Wave Sequence Config")]
    public class WaveSequenceConfig : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Ordered collection of wave configurations.")]
        private List<WaveConfig> _waves = new();

        /// <summary>
        /// Read-only access to the configured wave sequence.
        /// </summary>
        public IReadOnlyList<WaveConfig> Waves => _waves;

        /// <summary>
        /// Total number of configured waves.
        /// </summary>
        public int TotalWavesCount => _waves != null ? _waves.Count : 0;

        /// <summary>
        /// Safely retrieves a wave configuration by index with boundary validation.
        /// </summary>
        /// <param name="index">Zero-based wave index.</param>
        /// <returns>Wave configuration, or null if the index is out of bounds.</returns>
        public WaveConfig GetWave(int index)
        {
            if (_waves == null || index < 0 || index >= _waves.Count)
            {
                Debug.LogWarning($"[WaveSequenceConfig] Wave index {index} is out of range [0..{TotalWavesCount - 1}].");
                return null;
            }

            return _waves[index];
        }
    }
}

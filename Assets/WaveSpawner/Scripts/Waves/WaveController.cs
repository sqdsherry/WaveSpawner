using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaveSpawner.Scripts.Data;
using WaveSpawner.Scripts.Enemies;
using WaveSpawner.Scripts.Spawning;

namespace WaveSpawner.Scripts.Waves
{
    /// <summary>
    /// Core orchestrator for the enemy wave lifecycle.
    /// Manages state transitions, spawn intervals, remaining enemy tracking, and event dispatches.
    /// </summary>
    [DisallowMultipleComponent]
    public class WaveController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Configuration defining the sequence of waves.")]
        private WaveSequenceConfig _sequenceConfig;

        [SerializeField]
        [Tooltip("Enemy spawner subsystem.")]
        private EnemySpawner _spawner;

        [SerializeField]
        [Tooltip("Whether to automatically start the first wave on Start().")]
        private bool _autoStartOnStart = true;

        private WaveState _currentState = WaveState.Idle;
        private int _currentWaveIndex = -1;
        private int _aliveEnemiesCount;
        private Coroutine _waveRoutine;
        private readonly List<Enemy> _activeEnemies = new();

        /// <summary>
        /// Triggered when a wave starts. Parameters: (currentWaveNumber [1-based], totalWavesCount).
        /// </summary>
        public event Action<int, int> OnWaveStarted;

        /// <summary>
        /// Triggered when the count of alive enemies changes. Parameter: remaining alive count.
        /// </summary>
        public event Action<int> OnEnemiesCountChanged;

        /// <summary>
        /// Triggered when a wave is cleared and the delay before the next wave begins. Parameter: delay in seconds.
        /// </summary>
        public event Action<float> OnWaveDelayStarted;

        /// <summary>
        /// Triggered when all waves in the sequence are successfully completed.
        /// </summary>
        public event Action OnAllWavesCompleted;

        /// <summary>
        /// Current lifecycle state of the wave system.
        /// </summary>
        public WaveState CurrentState => _currentState;

        /// <summary>
        /// Current wave number (1-based). Returns 0 if no wave has started.
        /// </summary>
        public int CurrentWaveNumber => _currentWaveIndex + 1;

        /// <summary>
        /// Total number of configured waves in the current sequence.
        /// </summary>
        public int TotalWavesCount => _sequenceConfig != null ? _sequenceConfig.TotalWavesCount : 0;

        /// <summary>
        /// Current number of alive enemies on the scene in the active wave.
        /// </summary>
        public int AliveEnemiesCount => _aliveEnemiesCount;

        /// <summary>
        /// Initializes controller dependencies explicitly (enables testing and dependency injection).
        /// </summary>
        /// <param name="sequenceConfig">Wave sequence configuration.</param>
        /// <param name="spawner">Enemy spawner instance.</param>
        public void Init(WaveSequenceConfig sequenceConfig, EnemySpawner spawner)
        {
            _sequenceConfig = sequenceConfig;
            _spawner = spawner;
        }

        private void Start()
        {
            if (_autoStartOnStart)
            {
                StartWaves();
            }
        }

        /// <summary>
        /// Begins wave execution.
        /// </summary>
        public void StartWaves()
        {
            if (_waveRoutine != null)
            {
                Debug.LogWarning("[WaveController] Waves are already running or in progress.", this);
                return;
            }

            _waveRoutine = StartCoroutine(WaveProcessRoutine());
        }

        private IEnumerator WaveProcessRoutine()
        {
            if (_sequenceConfig == null || TotalWavesCount == 0 || _spawner == null)
            {
                Debug.LogError("[WaveController] Cannot start waves: missing WaveSequenceConfig, no waves configured, or EnemySpawner is null!", this);
                _waveRoutine = null;
                yield break;
            }

            for (_currentWaveIndex = 0; _currentWaveIndex < _sequenceConfig.TotalWavesCount; _currentWaveIndex++)
            {
                WaveConfig waveConfig = _sequenceConfig.GetWave(_currentWaveIndex);
                if (waveConfig == null)
                {
                    Debug.LogError($"[WaveController] WaveConfig at index {_currentWaveIndex} not found!", this);
                    continue;
                }

                _currentState = WaveState.Spawning;
                OnWaveStarted?.Invoke(CurrentWaveNumber, TotalWavesCount);

                for (int i = 0; i < waveConfig.EnemyCount; i++)
                {
                    Enemy enemy = _spawner.SpawnEnemy();
                    if (enemy != null)
                    {
                        _activeEnemies.Add(enemy);
                        enemy.OnDied += HandleEnemyDied;
                        _aliveEnemiesCount++;
                        OnEnemiesCountChanged?.Invoke(_aliveEnemiesCount);
                    }

                    if (i < waveConfig.EnemyCount - 1)
                    {
                        yield return new WaitForSeconds(waveConfig.SpawnInterval);
                    }
                }

                _currentState = WaveState.WaitingForClear;
                while (_aliveEnemiesCount > 0)
                {
                    yield return null;
                }

                if (_currentWaveIndex == _sequenceConfig.TotalWavesCount - 1)
                {
                    break;
                }

                _currentState = WaveState.WaveCompletedDelay;
                OnWaveDelayStarted?.Invoke(waveConfig.DelayBeforeNextWave);
                yield return new WaitForSeconds(waveConfig.DelayBeforeNextWave);
            }

            _currentState = WaveState.AllWavesCompleted;
            OnAllWavesCompleted?.Invoke();
            _waveRoutine = null;
        }

        private void HandleEnemyDied(Enemy enemy)
        {
            if (enemy != null)
            {
                enemy.OnDied -= HandleEnemyDied;
                _activeEnemies.Remove(enemy);
            }

            _aliveEnemiesCount = Mathf.Max(0, _aliveEnemiesCount - 1);
            OnEnemiesCountChanged?.Invoke(_aliveEnemiesCount);
        }

        private void OnDestroy()
        {
            if (_waveRoutine != null)
            {
                StopCoroutine(_waveRoutine);
                _waveRoutine = null;
            }

            foreach (var enemy in _activeEnemies)
            {
                if (enemy != null)
                {
                    enemy.OnDied -= HandleEnemyDied;
                }
            }

            _activeEnemies.Clear();
        }

        private void OnValidate()
        {
            if (_sequenceConfig == null)
            {
                Debug.LogWarning("[WaveController] WaveSequenceConfig (_sequenceConfig) is not assigned!", this);
            }

            if (_spawner == null)
            {
                Debug.LogWarning("[WaveController] EnemySpawner (_spawner) is not assigned!", this);
            }
        }
    }
}

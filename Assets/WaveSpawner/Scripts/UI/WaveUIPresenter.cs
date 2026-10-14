using UnityEngine;
using WaveSpawner.Scripts.Waves;

namespace WaveSpawner.Scripts.UI
{
    /// <summary>
    /// Presenter component for the wave user interface (MVP Presenter).
    /// Mediates between the domain game state (<see cref="WaveController"/>) and the passive presentation (<see cref="WaveUIView"/>).
    /// </summary>
    [DisallowMultipleComponent]
    public class WaveUIPresenter : MonoBehaviour
    {
        [Header("Model and View Dependencies")]
        [SerializeField]
        [Tooltip("Enemy wave controller (Model).")]
        private WaveController _model;

        [SerializeField]
        [Tooltip("Passive view component (View).")]
        private WaveUIView _view;

        /// <summary>
        /// Reference to the wave controller (Model).
        /// </summary>
        public WaveController Model => _model;

        /// <summary>
        /// Reference to the passive view interface (View).
        /// </summary>
        public IWaveUIView View => _view;

        private void OnEnable()
        {
            SubscribeToModel();
            SyncStateWithModel();
        }

        private void OnDisable()
        {
            UnsubscribeFromModel();
        }

        private void OnDestroy()
        {
            UnsubscribeFromModel();
        }

        private void OnValidate()
        {
            if (_view == null)
            {
                _view = GetComponent<WaveUIView>();
            }
        }

        /// <summary>
        /// Initializes presenter dependencies explicitly (supports DI containers and code-based bootstrapping).
        /// </summary>
        /// <param name="model">Wave controller model.</param>
        /// <param name="view">Passive view component.</param>
        public void Init(WaveController model, WaveUIView view)
        {
            if (isActiveAndEnabled)
            {
                UnsubscribeFromModel();
            }

            _model = model;
            _view = view;

            if (isActiveAndEnabled)
            {
                SubscribeToModel();
                SyncStateWithModel();
            }
        }

        private void SubscribeToModel()
        {
            if (_model == null)
            {
                return;
            }

            _model.OnWaveStarted += HandleWaveStarted;
            _model.OnEnemiesCountChanged += HandleEnemiesCountChanged;
            _model.OnAllWavesCompleted += HandleAllWavesCompleted;
        }

        private void UnsubscribeFromModel()
        {
            if (_model == null)
            {
                return;
            }

            _model.OnWaveStarted -= HandleWaveStarted;
            _model.OnEnemiesCountChanged -= HandleEnemiesCountChanged;
            _model.OnAllWavesCompleted -= HandleAllWavesCompleted;
        }

        private void SyncStateWithModel()
        {
            if (_model == null || _model.CurrentState == WaveState.Idle)
            {
                return;
            }

            _view?.SetWave(_model.CurrentWaveNumber, _model.TotalWavesCount);
            _view?.SetAliveEnemies(_model.AliveEnemiesCount);

            if (_model.CurrentState == WaveState.AllWavesCompleted)
            {
                _view?.ShowVictoryScreen();
            }
        }

        private void HandleWaveStarted(int current, int total) => _view?.SetWave(current, total);

        private void HandleEnemiesCountChanged(int aliveCount) => _view?.SetAliveEnemies(aliveCount);

        private void HandleAllWavesCompleted() => _view?.ShowVictoryScreen();
    }
}

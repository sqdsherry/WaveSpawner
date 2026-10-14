using System;
using TMPro;
using UnityEngine;

namespace WaveSpawner.Scripts.UI
{
    /// <summary>
    /// Passive view implementation for the wave UI (Passive View pattern).
    /// Responsible strictly for visual presentations and contains zero domain or game logic dependencies.
    /// </summary>
    [DisallowMultipleComponent]
    public class WaveUIView : MonoBehaviour, IWaveUIView
    {
        private const string DefaultVictoryMessage = "Victory! All Waves Cleared!";

        [Header("TextMeshPro Elements")]
        [SerializeField]
        [Tooltip("Text component displaying current wave information.")]
        private TextMeshProUGUI _waveText;

        [SerializeField]
        [Tooltip("Text component displaying the count of remaining alive enemies.")]
        private TextMeshProUGUI _enemiesAliveText;

        [Header("Victory Screen")]
        [SerializeField]
        [Tooltip("Root GameObject of the victory overlay panel.")]
        private GameObject _victoryPanel;

        [SerializeField]
        [Tooltip("Text component displaying congratulatory victory message.")]
        private TextMeshProUGUI _victoryText;

        [Header("Formatting Patterns")]
        [SerializeField]
        [Tooltip("Format string for wave text ({0} - current, {1} - total).")]
        private string _waveFormat = "Wave: {0} / {1}";

        [SerializeField]
        [Tooltip("Format string for remaining enemies text ({0} - count).")]
        private string _enemiesFormat = "Enemies Alive: {0}";

        private void Awake()
        {
            ResetUI();
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_waveFormat))
            {
                _waveFormat = "Wave: {0} / {1}";
            }

            if (string.IsNullOrEmpty(_enemiesFormat))
            {
                _enemiesFormat = "Enemies Alive: {0}";
            }
        }

        /// <summary>
        /// Resets the UI to its initial state: hides victory panel and sets placeholder texts.
        /// </summary>
        public void ResetUI()
        {
            if (_victoryPanel != null)
            {
                _victoryPanel.SetActive(false);
            }

            if (_waveText != null)
            {
                try
                {
                    _waveText.text = string.Format(_waveFormat, "-", "-");
                }
                catch (FormatException)
                {
                    _waveText.text = "Wave: - / -";
                }
            }

            if (_enemiesAliveText != null)
            {
                try
                {
                    _enemiesAliveText.text = string.Format(_enemiesFormat, 0);
                }
                catch (FormatException)
                {
                    _enemiesAliveText.text = "Enemies Alive: 0";
                }
            }
        }

        /// <summary>
        /// Displays the current wave number and total waves count.
        /// </summary>
        /// <param name="currentWave">Current wave number (1-based).</param>
        /// <param name="totalWaves">Total configured wave count.</param>
        public void SetWave(int currentWave, int totalWaves)
        {
            if (_waveText != null)
            {
                _waveText.text = string.Format(_waveFormat, currentWave, totalWaves);
            }
        }

        /// <summary>
        /// Displays the current count of remaining alive enemies.
        /// </summary>
        /// <param name="count">Number of remaining alive enemies.</param>
        public void SetAliveEnemies(int count)
        {
            if (_enemiesAliveText != null)
            {
                _enemiesAliveText.text = string.Format(_enemiesFormat, count);
            }
        }

        /// <summary>
        /// Displays the victory overlay upon clearing all waves.
        /// </summary>
        public void ShowVictoryScreen()
        {
            if (_victoryPanel != null)
            {
                _victoryPanel.SetActive(true);
            }

            if (_victoryText != null)
            {
                _victoryText.text = DefaultVictoryMessage;
            }
        }
    }
}

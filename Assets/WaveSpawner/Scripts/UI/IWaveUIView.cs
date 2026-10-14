namespace WaveSpawner.Scripts.UI
{
    /// <summary>
    /// Contract for passive wave UI views (Passive View pattern).
    /// Completely isolated from gameplay and domain logic.
    /// </summary>
    public interface IWaveUIView
    {
        /// <summary>
        /// Updates the current and total wave display.
        /// </summary>
        /// <param name="currentWave">Current wave number (1-based).</param>
        /// <param name="totalWaves">Total configured wave count.</param>
        void SetWave(int currentWave, int totalWaves);

        /// <summary>
        /// Updates the remaining alive enemies count display.
        /// </summary>
        /// <param name="count">Number of remaining alive enemies.</param>
        void SetAliveEnemies(int count);

        /// <summary>
        /// Activates the victory screen upon completing all waves.
        /// </summary>
        void ShowVictoryScreen();

        /// <summary>
        /// Resets UI elements to their default initial state.
        /// </summary>
        void ResetUI();
    }
}

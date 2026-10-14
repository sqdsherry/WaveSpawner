namespace WaveSpawner.Scripts.Waves
{
    /// <summary>
    /// Lifecycle states of the enemy wave system.
    /// </summary>
    public enum WaveState
    {
        /// <summary>
        /// System is idle, awaiting initial start.
        /// </summary>
        Idle,

        /// <summary>
        /// Wave is actively spawning enemies one by one.
        /// </summary>
        Spawning,

        /// <summary>
        /// All enemies for the current wave have spawned; waiting for all of them to be destroyed.
        /// </summary>
        WaitingForClear,

        /// <summary>
        /// All enemies of the current wave were destroyed; waiting out the transition delay.
        /// </summary>
        WaveCompletedDelay,

        /// <summary>
        /// All waves in the sequence were successfully completed; game is won.
        /// </summary>
        AllWavesCompleted
    }
}

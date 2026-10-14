namespace WaveSpawner.Scripts.Enemies
{
    /// <summary>
    /// Contract for entities that can receive fatal damage and be destroyed.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Destroys or eliminates the entity.
        /// </summary>
        void Kill();
    }
}

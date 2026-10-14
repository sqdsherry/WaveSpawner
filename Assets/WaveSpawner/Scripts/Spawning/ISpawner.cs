using WaveSpawner.Scripts.Enemies;

namespace WaveSpawner.Scripts.Spawning
{
    /// <summary>
    /// Contract for enemy spawning subsystems.
    /// </summary>
    public interface ISpawner
    {
        /// <summary>
        /// Spawns an enemy instance into the scene.
        /// </summary>
        /// <param name="prefabOverride">Optional enemy prefab. If null, the default configured prefab is used.</param>
        /// <returns>The spawned <see cref="Enemy"/> instance, or null if creation failed.</returns>
        Enemy SpawnEnemy(Enemy prefabOverride = null);
    }
}

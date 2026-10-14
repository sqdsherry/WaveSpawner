# WaveSpawner Project Rules & Coding Standards

## 1. Namespace Convention
- Namespaces strictly reflect the project directory structure under `Assets/`:
  - `Assets/WaveSpawner/Scripts/Data` -> `namespace WaveSpawner.Scripts.Data`
  - `Assets/WaveSpawner/Scripts/Enemies` -> `namespace WaveSpawner.Scripts.Enemies`
  - `Assets/WaveSpawner/Scripts/Spawning` -> `namespace WaveSpawner.Scripts.Spawning`
  - `Assets/WaveSpawner/Scripts/Waves` -> `namespace WaveSpawner.Scripts.Waves`
  - `Assets/WaveSpawner/Scripts/Player` -> `namespace WaveSpawner.Scripts.Player`
  - `Assets/WaveSpawner/Scripts/UI` -> `namespace WaveSpawner.Scripts.UI`

## 2. Architecture & Code Style
- **SOLID & Loose Coupling:**
  - Data classes (`ScriptableObject`) contain only configuration data and validation (`OnValidate`).
  - Logic systems communicate via C# `Action` events, avoiding direct cyclic coupling.
  - UI never polls gameplay logic; it subscribes to events in `OnEnable` and unsubscribes in `OnDisable`.
- **Naming Conventions:**
  - Private serialized fields: `_camelCase` with `[SerializeField]` attribute.
  - Public properties: `PascalCase`.
  - Methods and Events: `PascalCase` (e.g. `event Action<Enemy> OnDied;`).
  - Constants: `PascalCase` or `ALL_CAPS`.
- **Unity Best Practices:**
  - Protect against double execution (e.g. `_isDead` flag on entities).
  - Disable collider immediately on destruction to prevent multiple raycast hits within the same frame.
  - Clean up event subscriptions in `OnDestroy` / `OnDisable` to avoid memory leaks.
  - No `FindObjectOfType` or `GameObject.Find` in gameplay loops.
  - All public members must have clear XML documentation comments (`/// <summary>`).

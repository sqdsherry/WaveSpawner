Ты выступаешь в роли Unity-разработчика (Кодера). Твоя задача — реализовать ядро логики: управление жизненным циклом волн (Wave Controller).

### Правило неймспейсов:
- Путь: `Assets/WaveSpawner/Scripts/Waves/`
- Namespace: `WaveSpawner.Scripts.Waves`

### Задача (Task 4: WaveState & WaveController):
Создать два файла в директории `Assets/WaveSpawner/Scripts/Waves/`:

1. `WaveState.cs`:
```csharp
namespace WaveSpawner.Scripts.Waves
{
    /// <summary>
    /// Состояния жизненного цикла системы волн.
    /// </summary>
    public enum WaveState
    {
        Idle,               // Ожидание запуска
        Spawning,           // Процесс появления врагов текущей волны
        WaitingForClear,    // Все враги волны появились, ожидаем их уничтожения
        WaveCompletedDelay, // Все враги уничтожены, пауза перед следующей волной
        AllWavesCompleted   // Все волны пройдены, игра завершена
    }
}
```

2. `WaveController.cs`:
- Наследуется от `MonoBehaviour`.
- Сериализованные поля:
  - `[SerializeField] private WaveSequenceConfig _sequenceConfig;` (`using WaveSpawner.Scripts.Data;`)
  - `[SerializeField] private EnemySpawner _spawner;` (`using WaveSpawner.Scripts.Spawning;`)
  - `[SerializeField] private bool _autoStartOnStart = true;` — автоматический запуск первой волны в `Start()`.
- Состояния и счетчики:
  - `private WaveState _currentState = WaveState.Idle;`
  - `private int _currentWaveIndex = -1;` (0-based)
  - `private int _aliveEnemiesCount;`
  - `private Coroutine _waveRoutine;`
  - `private readonly List<Enemy> _activeEnemies = new();` (`using WaveSpawner.Scripts.Enemies;`)
- События (C# Action, UI и внешние системы слушают их):
  - `public event Action<int, int> OnWaveStarted;` // (currentWaveNumber, totalWavesCount)
  - `public event Action<int> OnEnemiesCountChanged;` // (aliveCount)
  - `public event Action<float> OnWaveDelayStarted;` // (delayDurationSeconds)
  - `public event Action OnAllWavesCompleted;`
- Публичные свойства (read-only):
  - `public WaveState CurrentState => _currentState;`
  - `public int CurrentWaveNumber => _currentWaveIndex + 1;`
  - `public int TotalWavesCount => _sequenceConfig != null ? _sequenceConfig.TotalWavesCount : 0;`
  - `public int AliveEnemiesCount => _aliveEnemiesCount;`
- Логика работы:
  - В `Start()`: если `_autoStartOnStart`, вызываем `StartWaves()`.
  - `public void StartWaves()`:
    - Если корутина уже идет, логируем предупреждение и выходим.
    - Запускаем `_waveRoutine = StartCoroutine(WaveProcessRoutine());`.
  - Корутина `WaveProcessRoutine()`:
    1. Проверка конфигурации: если `_sequenceConfig == null` или `TotalWavesCount == 0` или `_spawner == null`, логируем `Debug.LogError` и завершаем корутину.
    2. Цикл по волнам:
       ```csharp
       for (_currentWaveIndex = 0; _currentWaveIndex < _sequenceConfig.TotalWavesCount; _currentWaveIndex++)
       {
           WaveConfig waveConfig = _sequenceConfig.GetWave(_currentWaveIndex);
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
           yield return new WaitUntil(() => _aliveEnemiesCount <= 0);

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
       ```
  - Метод `HandleEnemyDied(Enemy enemy)`:
    ```csharp
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
    ```
  - Очистка в `OnDestroy()`:
    - Если `_waveRoutine != null`, останавливаем корутину: `StopCoroutine(_waveRoutine); _waveRoutine = null;`.
    - Отписываемся от оставшихся врагов:
      ```csharp
      foreach (var enemy in _activeEnemies)
      {
          if (enemy != null)
          {
              enemy.OnDied -= HandleEnemyDied;
          }
      }
      _activeEnemies.Clear();
      ```

### Требования к коду:
- Полное соблюдение C# code style и XML-комментарии для публичных сущностей.
- Проверить сборку решения (`dotnet build WaveSpawner.sln`).

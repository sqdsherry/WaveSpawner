Ты выступаешь в роли Unity-разработчика (Кодера). Нам необходимо сделать архитектуру UI образцовой — перевести ее на строгий паттерн MVP (Model-View-Presenter) с пассивным View (Passive View).

### Правило неймспейсов:
- Путь: `Assets/WaveSpawner/Scripts/UI/`
- Namespace: `WaveSpawner.Scripts.UI`

### Что сделать:
Удалить старый файл `Assets/WaveSpawner/Scripts/UI/WaveUIController.cs` (и его .meta) и создать 3 файла:

1. `IWaveUIView.cs`:
```csharp
namespace WaveSpawner.Scripts.UI
{
    /// <summary>
    /// Контракт пассивного отображения UI волн (Passive View).
    /// Не зависит от игровой логики.
    /// </summary>
    public interface IWaveUIView
    {
        void SetWave(int currentWave, int totalWaves);
        void SetAliveEnemies(int count);
        void ShowVictoryScreen();
        void ResetUI();
    }
}
```

2. `WaveUIView.cs`:
- Наследуется от `MonoBehaviour` и реализует `IWaveUIView`.
- Добавить `[DisallowMultipleComponent]`.
- Сериализованные поля (только элементы визуала, никаких ссылок на WaveController!):
  - `[Header("TextMeshPro элементы")]`
  - `[SerializeField] private TextMeshProUGUI _waveText;`
  - `[SerializeField] private TextMeshProUGUI _enemiesAliveText;`
  - `[Header("Экран победы")]`
  - `[SerializeField] private GameObject _victoryPanel;`
  - `[SerializeField] private TextMeshProUGUI _victoryText;`
  - `[Header("Форматирование")]`
  - `[SerializeField] private string _waveFormat = "Wave: {0} / {1}";`
  - `[SerializeField] private string _enemiesFormat = "Enemies Alive: {0}";`
- Методы:
  - `Awake()`: вызываем `ResetUI();`.
  - `public void ResetUI()`:
    - Скрываем панель победы (`if (_victoryPanel != null) _victoryPanel.SetActive(false);`).
    - Выставляем базовые тексты (`if (_waveText != null) _waveText.text = string.Format(_waveFormat, "-", "-");`, `if (_enemiesAliveText != null) _enemiesAliveText.text = string.Format(_enemiesFormat, 0);`).
  - `public void SetWave(int currentWave, int totalWaves)`: обновляет `_waveText` через `string.Format(_waveFormat, currentWave, totalWaves)`.
  - `public void SetAliveEnemies(int count)`: обновляет `_enemiesAliveText` через `string.Format(_enemiesFormat, count)`.
  - `public void ShowVictoryScreen()`: активирует `_victoryPanel`, выставляет текст `if (_victoryText != null) _victoryText.text = "Victory! All Waves Cleared!";`.

3. `WaveUIPresenter.cs`:
- Наследуется от `MonoBehaviour`.
- Добавить `[DisallowMultipleComponent]`.
- Сериализованные поля:
  - `[SerializeField] private WaveController _model;` (`using WaveSpawner.Scripts.Waves;`)
  - `[SerializeField] private WaveUIView _view;`
- Логика:
  - `OnEnable()`:
    - Если `_model != null`:
      - Подписываемся на события:
        - `_model.OnWaveStarted += HandleWaveStarted;`
        - `_model.OnEnemiesCountChanged += HandleEnemiesCountChanged;`
        - `_model.OnAllWavesCompleted += HandleAllWavesCompleted;`
      - Если волны уже идут (`_model.CurrentState != WaveState.Idle`):
        - `_view?.SetWave(_model.CurrentWaveNumber, _model.TotalWavesCount);`
        - `_view?.SetAliveEnemies(_model.AliveEnemiesCount);`
        - Если `_model.CurrentState == WaveState.AllWavesCompleted`, вызываем `_view?.ShowVictoryScreen();`.
  - `OnDisable()`:
    - Отписываемся от событий `_model` (защита от утечек памяти).
  - Обработчики:
    - `private void HandleWaveStarted(int current, int total) => _view?.SetWave(current, total);`
    - `private void HandleEnemiesCountChanged(int aliveCount) => _view?.SetAliveEnemies(aliveCount);`
    - `private void HandleAllWavesCompleted() => _view?.ShowVictoryScreen();`
  - `OnValidate()`:
    - Если `_view == null`, пробуем `_view = GetComponent<WaveUIView>();`.

### Проверка:
- Убедиться, что `dotnet build WaveSpawner.sln` проходит без ошибок.

Ты выступаешь в роли Unity-разработчика (Кодера). Твоя задача — реализовать слой пользовательского интерфейса (UI) для отображения состояния волн и окончания игры.

### Правило неймспейсов:
- Путь: `Assets/WaveSpawner/Scripts/UI/`
- Namespace: `WaveSpawner.Scripts.UI`

### Требования ТЗ к UI:
- Отображать номер текущей волны (например, "Wave: 1 / 3").
- Отображать количество живых врагов (например, "Enemies Alive: 5").
- При завершении последней волны отображать окно/сообщение о победе ("All Waves Cleared! Victory!").
- Архитектура: Observer pattern (UI слушает C#-события `WaveController`, никакого опроса в `Update`).

### Задача (Task 6: WaveUIController):
Создать файл `Assets/WaveSpawner/Scripts/UI/WaveUIController.cs`:

- Наследуется от `MonoBehaviour`.
- Атрибут `[DisallowMultipleComponent]`.
- Сериализованные поля:
  - `[Header("Зависимости")]`
  - `[SerializeField] private WaveController _waveController;` (`using WaveSpawner.Scripts.Waves;`)
  - `[Header("Элементы UI (TextMeshPro)")]`
  - `[SerializeField] private TextMeshProUGUI _waveText;` (`using TMPro;`)
  - `[SerializeField] private TextMeshProUGUI _enemiesAliveText;`
  - `[Header("Экран завершения игры")]`
  - `[SerializeField] private GameObject _gameCompletedPanel;`
  - `[SerializeField] private TextMeshProUGUI _gameCompletedText;`
  - `[Header("Шаблоны форматирования")]`
  - `[SerializeField] private string _waveTextFormat = "Wave: {0} / {1}";`
  - `[SerializeField] private string _enemiesAliveFormat = "Enemies Alive: {0}";`

- Логика:
  - В `Awake()`:
    - Если `_gameCompletedPanel != null`, скрываем ее `_gameCompletedPanel.SetActive(false);`.
    - Инициализируем начальный текст (например, `Wave: - / -`, `Enemies Alive: 0`).
  - В `OnEnable()`:
    - Подписываемся на события `_waveController`:
      - `_waveController.OnWaveStarted += HandleWaveStarted;`
      - `_waveController.OnEnemiesCountChanged += HandleEnemiesCountChanged;`
      - `_waveController.OnAllWavesCompleted += HandleAllWavesCompleted;`
    - Если контроллер уже в процессе работы, синхронизируем начальное состояние:
      - `UpdateWaveText(_waveController.CurrentWaveNumber, _waveController.TotalWavesCount);`
      - `UpdateEnemiesText(_waveController.AliveEnemiesCount);`
  - В `OnDisable()`:
    - Отписываемся от всех событий `_waveController` (защита от утечек памяти).
  - Обработчики событий:
    - `private void HandleWaveStarted(int currentWave, int totalWaves) => UpdateWaveText(currentWave, totalWaves);`
    - `private void HandleEnemiesCountChanged(int aliveCount) => UpdateEnemiesText(aliveCount);`
    - `private void HandleAllWavesCompleted()`:
      - Активируем панель: `if (_gameCompletedPanel != null) _gameCompletedPanel.SetActive(true);`
      - При необходимости выставляем текст `_gameCompletedText.text = "Victory! All Waves Cleared!";`
  - Вспомогательные методы обновления:
    - `UpdateWaveText(int current, int total)`: с проверкой `_waveText != null`.
    - `UpdateEnemiesText(int count)`: с проверкой `_enemiesAliveText != null`.

### Требования к коду:
- `using TMPro;`, `using UnityEngine;`, `using WaveSpawner.Scripts.Waves;`.
- Полное соблюдение C# code style, XML-комментарии для публичных сущностей.
- Проверить сборку решения (`dotnet build WaveSpawner.sln`).

Ты выступаешь в роли Unity-разработчика (Кодера). Твоя задача — реализовать финальный шаг (Task 7: Сборка сцены и генерация ассетов).

Чтобы исключить человеческий фактор при ручной настройке инспектора и произвести впечатление на ревьюеров Mind Studios Games, мы создадим Editor-утилиту для автоматической сборки сцены и всех необходимых ассетов в один клик через меню Unity.

### Правило неймспейсов:
- Путь: `Assets/WaveSpawner/Scripts/Editor/`
- Namespace: `WaveSpawner.Scripts.Editor`

### Задача (Task 7: WaveSpawnerSceneSetup):
Создать файл `Assets/WaveSpawner/Scripts/Editor/WaveSpawnerSceneSetup.cs`:

- Класс утилиты с атрибутом `[MenuItem("Tools/Wave Spawner/Setup Assets and Scene")]`.
- Утилита должна выполнять следующие действия:

#### 1. Создание структуры папок:
- `Assets/WaveSpawner/ScriptableObjects/Waves/`
- `Assets/WaveSpawner/Prefabs/`

#### 2. Генерация ScriptableObject-ассетов волн:
- `Wave_01.asset`: EnemyCount = 3, SpawnInterval = 1.0f, DelayBeforeNextWave = 2.0f.
- `Wave_02.asset`: EnemyCount = 5, SpawnInterval = 0.8f, DelayBeforeNextWave = 2.5f.
- `Wave_03.asset`: EnemyCount = 8, SpawnInterval = 0.5f, DelayBeforeNextWave = 0.0f.
- `DefaultWaveSequence.asset`: связывает волны 1, 2 и 3 в последовательность.
- Использовать `AssetDatabase.CreateAsset()`. Если ассеты уже существуют — обновлять/перезаписывать.

#### 3. Создание префаба врага (`Enemy_Cube.prefab`):
- Создать временный GameObject примитива `GameObject.CreatePrimitive(PrimitiveType.Cube)`.
- Добавить компонент `Enemy` (`using WaveSpawner.Scripts.Enemies;`).
- Сохранить как префаб через `PrefabUtility.SaveAsPrefabAsset()` в `Assets/WaveSpawner/Prefabs/Enemy_Cube.prefab`.
- Удалить временный объект со сцены.

#### 4. Настройка текущей сцены:
- Очистить или обновить открытую сцену:
  - **Камера:** настроить `Camera.main` в позицию `(0, 7, -10)`, поворот `(35, 0, 0)`.
  - **Освещение:** создать Directional Light, если отсутствует.
  - **Окружение (Ground):** создать плоскость/куб `Ground` размера `(15, 0.1, 15)` в позиции `(0, -0.5, 0)`.
  - **Точки спавна (`--- SPAWN POINTS ---`):**
    - Создать родительский пустой объект `SpawnPoints`.
    - Создать 3 дочерние точки:
      - `SpawnPoint_1`: `(-4, 0.5, 2)`
      - `SpawnPoint_2`: `(0, 0.5, 4)`
      - `SpawnPoint_3`: `(4, 0.5, 2)`
  - **Менеджеры (`--- MANAGERS ---`):**
    - Создать GameObject `[GameManagers]`.
    - Добавить `EnemySpawner`: назначить префаб `Enemy_Cube` и массив из 3 точек спавна. Включить `_useObjectPool = true`.
    - Добавить `WaveController`: назначить `DefaultWaveSequence` и `EnemySpawner`.
    - Добавить `ClickRaycastController`: назначить `Camera.main`.
  - **Пользовательский интерфейс (`--- UI ---`):**
    - Создать `Canvas` с `CanvasScaler` (Scale With Screen Size, 1920x1080) и `GraphicRaycaster`.
    - Создать `EventSystem` с `InputSystemUIInputModule` (`using UnityEngine.InputSystem.UI;`).
    - Создать панель статуса вверху экрана:
      - Текст `WaveText` (TextMeshProUGUI): анкор Top-Left, позиция `(30, -30)`, текст "Wave: 1 / 3", размер шрифта 36.
      - Текст `EnemiesText` (TextMeshProUGUI): анкор Top-Left, позиция `(30, -80)`, текст "Enemies Alive: 0", размер шрифта 32.
    - Создать панель победы `VictoryPanel`:
      - Анкор по центру экрана, полупрозрачный темный фон.
      - Дочерний текст `VictoryText` (TextMeshProUGUI): "Victory! All Waves Cleared!", размер шрифта 54, цвет золотой/зеленый, выравнивание по центру.
      - По умолчанию отключена (`SetActive(false)`).
    - Добавить `WaveUIView` на Canvas:
      - Связать ссылки: `_waveText`, `_enemiesAliveText`, `_victoryPanel`, `_victoryText`.
    - Добавить `WaveUIPresenter` на Canvas (или на `[GameManagers]`):
      - Связать ссылки: `_model` (`WaveController`), `_view` (`WaveUIView`).
- Пометить сцену как измененную `EditorSceneManager.MarkSceneDirty()` и сохранить `EditorSceneManager.SaveOpenScenes()`.
- Вывести `Debug.Log("[WaveSpawner] Сцена и ассеты успешно настроены! Можно нажимать Play.")`.

### Требования к коду:
- Класс должен быть внутри `#if UNITY_EDITOR ... #endif` или в папке `Editor`.
- Использовать `UnityEditor`, `UnityEditor.SceneManagement`, `UnityEngine.UI`, `TMPro`, `UnityEngine.InputSystem.UI`.
- Проверить сборку решения (`dotnet build WaveSpawner.sln`).

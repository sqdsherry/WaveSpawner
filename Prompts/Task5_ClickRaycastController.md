Ты выступаешь в роли Unity-разработчика (Кодера). Твоя задача — реализовать подсистему взаимодействия игрока (клик по врагам для их уничтожения) с использованием современного Unity Input System и физического рейкаста.

### Правило неймспейсов:
- Путь: `Assets/WaveSpawner/Scripts/Player/`
- Namespace: `WaveSpawner.Scripts.Player`

### Контекст проекта:
- Проект использует Unity 6 с активным новым Input System (`activeInputHandler: 1`, `com.unity.inputsystem`).
- Враги реализуют интерфейс `IDamageable` (`using WaveSpawner.Scripts.Enemies;`).

### Задача (Task 5: ClickRaycastController):
Создать файл `Assets/WaveSpawner/Scripts/Player/ClickRaycastController.cs`:

- Наследуется от `MonoBehaviour`.
- Атрибут `[DisallowMultipleComponent]`.
- Сериализованные поля:
  - `[SerializeField] private Camera _camera;` — камера для рейкаста.
  - `[SerializeField] private LayerMask _hitLayerMask = ~0;` — маска слоев для фильтрации (по умолчанию Everything, в инспекторе можно выбрать Enemy).
  - `[SerializeField] private float _maxRaycastDistance = 100f;` — максимальная дистанция луча.
- Логика:
  - `Awake()`:
    - Если `_camera == null`, кэшируем `_camera = Camera.main;`.
  - `Update()`:
    1. Проверка нажатия через `UnityEngine.InputSystem.Pointer`:
       ```csharp
       if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
       {
           return;
       }
       ```
       *(Pointer.current автоматически поддерживает мышь, тачскрин и стилус без дублирования кода)*.
    2. Проверка клика по UI (чтобы клик по кнопке интерфейса не пробивал врагов насквозь):
       ```csharp
       if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
       {
           return;
       }
       ```
    3. Выполнение рейкаста:
       - Получаем экранную позицию указателя: `Vector2 screenPosition = Pointer.current.position.ReadValue();`
       - Строим луч: `Ray ray = _camera.ScreenPointToRay(screenPosition);`
       - Пускаем луч:
         ```csharp
         if (Physics.Raycast(ray, out RaycastHit hit, _maxRaycastDistance, _hitLayerMask))
         {
             if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
             {
                 damageable.Kill();
             }
         }
         ```
  - `OnValidate()`:
    - Если `_camera == null`, кэшируем `_camera = Camera.main;`.

### Требования к коду:
- Подключить: `using UnityEngine;`, `using UnityEngine.EventSystems;`, `using UnityEngine.InputSystem;`, `using WaveSpawner.Scripts.Enemies;`.
- Полное соблюдение C# code style, XML-комментарии для публичных сущностей.
- Проверить сборку решения (`dotnet build WaveSpawner.sln`).

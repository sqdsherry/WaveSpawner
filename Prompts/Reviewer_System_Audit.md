Ты выступаешь в роли Senior/Lead Unity Architect (Независимого Технического Аудитора). Твоя задача — провести глубокий, критический аудит кодовой базы тестового проекта «Wave Spawner» перед его отправкой в Mind Studios Games.

### Твоя цель:
Проверить качество кода, архитектурные решения, отсутствие скрытых багов, утечек памяти, производительность и строгое соответствие требованиям компании.

---

### Контекст проекта и исходные документы:
1. Исходное ТЗ от компании: `TASK.md` в корне проекта.
2. Технический дизайн-документ: `TDD.md` в корне проекта.
3. Правила проекта и стандарты неймспейсов: `GEMINI.md` и `.antigravity/rules.md`.

Все скрипты проекта находятся в папке `Assets/WaveSpawner/Scripts/`.

---

### Чек-лист для проведения аудита:

#### 1. Соответствие требованиям ТЗ (Task Compliance):
- [ ] Настройка нескольких волн через Инспектор без хардкода (количество врагов, интервал, задержка).
- [ ] Спавн врагов по одному с заданным интервалом.
- [ ] Механика уничтожения врагов игроком (клик/тап).
- [ ] Переход к следующей волне строго после того, как:
  1) все враги волны заспавнены;
  2) все заспавненные враги уничтожены;
  3) выдержана заданная для волны задержка (`DelayBeforeNextWave`).
- [ ] UI: отображение номера волны, количества живых врагов, экран победы после последней волны.
- [ ] Отсутствие сторонних ассетов (чистый Unity 6 + URP).

#### 2. Архитектура и SOLID (Code Architecture):
- [ ] **Data Layer:** `WaveConfig` и `WaveSequenceConfig` (ScriptableObjects). Наличие `[Min]`, `OnValidate()`, read-only инкапсуляция.
- [ ] **Enemy Entity:** Интерфейс `IDamageable`, класс `Enemy`. Защита от повторного вызова (`_isDead`), немедленное отключение коллайдера, событие `OnDied`.
- [ ] **Spawner & Object Pooling:** Интерфейс `ISpawner`. Использование официального `UnityEngine.Pool.ObjectPool<Enemy>` для исключения аллокаций памяти. Fallback на `Instantiate` при выключенном пуле.
- [ ] **Wave Controller:** Состояния через `WaveState`. Корутина спавна и ожидания, корректный учет живых врагов, безопасная отписка от событий в `OnDestroy()`.
- [ ] **Player Input:** `ClickRaycastController`. Использование `UnityEngine.InputSystem.Pointer.current`, проверка блокировки кликов через UI (`EventSystem.current.IsPointerOverGameObject()`), использование `LayerMask`.
- [ ] **UI Layer (Strict MVP):** Разделение на `IWaveUIView`, `WaveUIView` (Passive View) и `WaveUIPresenter`. Отсутствие связности `WaveUIView` с игровой логикой. Подписка в `OnEnable()`, отписка в `OnDisable()`.
- [ ] **Соблюдение правил неймспейсов:** Строгое совпадение со структурой папок:
  `WaveSpawner.Scripts.Data`, `WaveSpawner.Scripts.Enemies`, `WaveSpawner.Scripts.Spawning`, `WaveSpawner.Scripts.Waves`, `WaveSpawner.Scripts.Player`, `WaveSpawner.Scripts.UI`, `WaveSpawner.Scripts.Editor`.

#### 3. Безопасность и Unity Best Practices:
- [ ] Отсутствие `FindObjectOfType`, `GameObject.Find` в рантайме.
- [ ] Защита от NullReferenceException во всех публичных методах и корутинах.
- [ ] Отсутствие утечек памяти (отписка от всех событий в `OnDisable` / `OnDestroy`).
- [ ] Отсутствие спама мусора (GC Allocations) в геймплейных циклах.
- [ ] Изоляция Editor-скриптов (`#if UNITY_EDITOR` или папка `Editor`).

#### 4. Проверка автоматизации сборки сцены:
- [ ] Изучить `Assets/WaveSpawner/Scripts/Editor/WaveSpawnerSceneSetup.cs`.
- [ ] Проверить корректность настройки связей между компонентами через пункт меню `Tools -> Wave Spawner -> Setup Assets and Scene`.

---

### Формат отчета аудитора:
1. **Общий вердикт:** (Готов к отправке / Требуются доработки) с оценкой от 1 до 10.
2. **Анализ по компонентам:** Краткая оценка каждого модуля (Data, Enemies, Spawner, Waves, Input, UI, Editor Setup).
3. **Обнаруженные риски или мелкие замечания:** (если есть).
4. **Топ-3 сильных стороны проекта для защиты на техническом интервью:** (что выделит кандидата среди остальных).

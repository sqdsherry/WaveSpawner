#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using WaveSpawner.Scripts.Data;
using WaveSpawner.Scripts.Enemies;
using WaveSpawner.Scripts.Player;
using WaveSpawner.Scripts.Spawning;
using WaveSpawner.Scripts.UI;
using WaveSpawner.Scripts.Waves;

namespace WaveSpawner.Scripts.Editor
{
    /// <summary>
    /// Editor utility for one-click asset generation and game scene setup.
    /// Eliminates human error during inspector wiring and reference assignment.
    /// </summary>
    public static class WaveSpawnerSceneSetup
    {
        private const string WavesFolderPath = "Assets/WaveSpawner/ScriptableObjects/Waves";
        private const string PrefabsFolderPath = "Assets/WaveSpawner/Prefabs";
        private const string EnemyPrefabPath = PrefabsFolderPath + "/Enemy_Cube.prefab";
        private const string DefaultSequencePath = WavesFolderPath + "/DefaultWaveSequence.asset";
        private const string Wave1Path = WavesFolderPath + "/Wave_01.asset";
        private const string Wave2Path = WavesFolderPath + "/Wave_02.asset";
        private const string Wave3Path = WavesFolderPath + "/Wave_03.asset";
        private const string RedMaterialPath = "Assets/MAT/EnemyMaterial_Red.mat";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";

        /// <summary>
        /// Executes full automated setup of asset structures (folders, wave configs, prefabs)
        /// and scene hierarchy (camera, lighting, environment, spawn points, managers, and MVP UI).
        /// </summary>
        [MenuItem("Tools/Wave Spawner/Setup Assets and Scene")]
        public static void SetupAssetsAndScene()
        {
            // 1. Create directory structure
            CreateFolderStructure();

            // 2. Generate wave ScriptableObject assets
            WaveSequenceConfig sequenceConfig = GenerateWaveAssets();

            // 3. Create enemy prefab (Enemy_Cube.prefab)
            Enemy enemyPrefabComponent = CreateEnemyPrefab();

            // 4. Setup scene hierarchy
            SetupSceneHierarchy(enemyPrefabComponent, sequenceConfig);

            // Save assets and active scene
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("[WaveSpawner] Scene and assets successfully configured! Ready to press Play.");
        }

        private static void CreateFolderStructure()
        {
            EnsureFolder(WavesFolderPath);
            EnsureFolder(PrefabsFolderPath);
        }

        private static void EnsureFolder(string relativePath)
        {
            string normalized = relativePath.Replace('\\', '/').TrimEnd('/');
            string[] segments = normalized.Split('/');
            string current = segments[0];

            for (int i = 1; i < segments.Length; i++)
            {
                string next = $"{current}/{segments[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[i]);
                }
                current = next;
            }
        }

        private static WaveSequenceConfig GenerateWaveAssets()
        {
            WaveConfig wave1 = CreateOrUpdateWaveConfig(Wave1Path, 3, 1.0f, 2.0f);
            WaveConfig wave2 = CreateOrUpdateWaveConfig(Wave2Path, 5, 0.8f, 2.5f);
            WaveConfig wave3 = CreateOrUpdateWaveConfig(Wave3Path, 8, 0.5f, 0.0f);

            return CreateOrUpdateSequenceConfig(DefaultSequencePath, wave1, wave2, wave3);
        }

        private static WaveConfig CreateOrUpdateWaveConfig(string path, int enemyCount, float spawnInterval, float delayBeforeNextWave)
        {
            WaveConfig asset = AssetDatabase.LoadAssetAtPath<WaveConfig>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<WaveConfig>();
                AssetDatabase.CreateAsset(asset, path);
            }

            SerializedObject serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("_enemyCount").intValue = enemyCount;
            serializedObject.FindProperty("_spawnInterval").floatValue = spawnInterval;
            serializedObject.FindProperty("_delayBeforeNextWave").floatValue = delayBeforeNextWave;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static WaveSequenceConfig CreateOrUpdateSequenceConfig(string path, params WaveConfig[] waves)
        {
            WaveSequenceConfig sequence = AssetDatabase.LoadAssetAtPath<WaveSequenceConfig>(path);
            if (sequence == null)
            {
                sequence = ScriptableObject.CreateInstance<WaveSequenceConfig>();
                AssetDatabase.CreateAsset(sequence, path);
            }

            SerializedObject serializedObject = new SerializedObject(sequence);
            SerializedProperty wavesProp = serializedObject.FindProperty("_waves");
            wavesProp.ClearArray();
            wavesProp.arraySize = waves.Length;
            for (int i = 0; i < waves.Length; i++)
            {
                wavesProp.GetArrayElementAtIndex(i).objectReferenceValue = waves[i];
            }
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(sequence);
            return sequence;
        }

        private static Enemy CreateEnemyPrefab()
        {
            GameObject tempEnemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tempEnemy.name = "Enemy_Cube";

            if (!tempEnemy.TryGetComponent<Enemy>(out var enemyComponent))
            {
                enemyComponent = tempEnemy.AddComponent<Enemy>();
            }

            Material redMat = AssetDatabase.LoadAssetAtPath<Material>(RedMaterialPath);
            if (redMat != null && tempEnemy.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.sharedMaterial = redMat;
            }

            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(tempEnemy, EnemyPrefabPath);
            UnityEngine.Object.DestroyImmediate(tempEnemy);

            return prefabAsset.GetComponent<Enemy>();
        }

        private static void SetupSceneHierarchy(Enemy enemyPrefab, WaveSequenceConfig sequenceConfig)
        {
            Camera mainCamera = SetupCamera();
            SetupDirectionalLight();
            SetupGround();
            Transform[] spawnPoints = SetupSpawnPoints();
            WaveController waveController = SetupManagers(enemyPrefab, sequenceConfig, spawnPoints, mainCamera);
            SetupUI(waveController);
        }

        private static Camera SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                mainCamera = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                camGo.tag = "MainCamera";
            }

            mainCamera.transform.position = new Vector3(0f, 7f, -10f);
            mainCamera.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
            return mainCamera;
        }

        private static void SetupDirectionalLight()
        {
            Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            Light dirLight = null;
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    dirLight = light;
                    break;
                }
            }

            if (dirLight == null)
            {
                GameObject lightGo = new GameObject("Directional Light");
                dirLight = lightGo.AddComponent<Light>();
                dirLight.type = LightType.Directional;
                dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
        }

        private static void SetupGround()
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground == null)
            {
                ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ground.name = "Ground";
            }

            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.rotation = Quaternion.identity;
            ground.transform.localScale = new Vector3(15f, 0.1f, 15f);
        }

        private static Transform[] SetupSpawnPoints()
        {
            GameObject spawnPointsRoot = GameObject.Find("SpawnPoints");
            if (spawnPointsRoot == null)
            {
                spawnPointsRoot = new GameObject("SpawnPoints");
            }

            spawnPointsRoot.transform.position = Vector3.zero;
            spawnPointsRoot.transform.rotation = Quaternion.identity;

            Transform[] spawnPointTransforms = new Transform[3];
            Vector3[] spawnPositions =
            {
                new Vector3(-4f, 0.5f, 2f),
                new Vector3(0f, 0.5f, 4f),
                new Vector3(4f, 0.5f, 2f)
            };

            for (int i = 0; i < 3; i++)
            {
                string pointName = $"SpawnPoint_{i + 1}";
                Transform child = spawnPointsRoot.transform.Find(pointName);
                if (child == null)
                {
                    GameObject pointGo = new GameObject(pointName);
                    pointGo.transform.SetParent(spawnPointsRoot.transform, false);
                    child = pointGo.transform;
                }

                child.position = spawnPositions[i];
                child.rotation = Quaternion.identity;
                spawnPointTransforms[i] = child;
            }

            return spawnPointTransforms;
        }

        private static WaveController SetupManagers(
            Enemy enemyPrefab,
            WaveSequenceConfig sequenceConfig,
            Transform[] spawnPoints,
            Camera mainCamera)
        {
            GameObject managersGo = GameObject.Find("[GameManagers]");
            if (managersGo == null)
            {
                managersGo = new GameObject("[GameManagers]");
            }

            managersGo.transform.position = Vector3.zero;
            managersGo.transform.rotation = Quaternion.identity;

            // EnemySpawner
            if (!managersGo.TryGetComponent<EnemySpawner>(out var enemySpawner))
            {
                enemySpawner = managersGo.AddComponent<EnemySpawner>();
            }

            SerializedObject spawnerSo = new SerializedObject(enemySpawner);
            spawnerSo.FindProperty("_defaultEnemyPrefab").objectReferenceValue = enemyPrefab;
            spawnerSo.FindProperty("_useObjectPool").boolValue = true;
            SerializedProperty pointsProp = spawnerSo.FindProperty("_spawnPoints");
            pointsProp.ClearArray();
            pointsProp.arraySize = spawnPoints.Length;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                pointsProp.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];
            }
            spawnerSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(enemySpawner);

            // WaveController
            if (!managersGo.TryGetComponent<WaveController>(out var waveController))
            {
                waveController = managersGo.AddComponent<WaveController>();
            }

            SerializedObject waveControllerSo = new SerializedObject(waveController);
            waveControllerSo.FindProperty("_sequenceConfig").objectReferenceValue = sequenceConfig;
            waveControllerSo.FindProperty("_spawner").objectReferenceValue = enemySpawner;
            waveControllerSo.FindProperty("_autoStartOnStart").boolValue = true;
            waveControllerSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(waveController);

            // ClickRaycastController
            if (!managersGo.TryGetComponent<ClickRaycastController>(out var clickController))
            {
                clickController = managersGo.AddComponent<ClickRaycastController>();
            }

            SerializedObject clickSo = new SerializedObject(clickController);
            clickSo.FindProperty("_camera").objectReferenceValue = mainCamera;
            clickSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(clickController);

            return waveController;
        }

        private static void SetupUI(WaveController waveController)
        {
            // Canvas
            GameObject canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("Canvas");
            }

            Canvas canvas = canvasGo.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasGo.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvasGo.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (!canvasGo.TryGetComponent<GraphicRaycaster>(out _))
            {
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // EventSystem
            SetupEventSystem();

            // WaveText
            Transform waveTextTrans = canvasGo.transform.Find("WaveText");
            GameObject waveTextGo = waveTextTrans != null ? waveTextTrans.gameObject : new GameObject("WaveText");
            waveTextGo.transform.SetParent(canvasGo.transform, false);

            TextMeshProUGUI waveText = waveTextGo.GetComponent<TextMeshProUGUI>();
            if (waveText == null)
            {
                waveText = waveTextGo.AddComponent<TextMeshProUGUI>();
            }

            EnsureDefaultFont(waveText);

            RectTransform waveRect = waveTextGo.GetComponent<RectTransform>();
            waveRect.anchorMin = new Vector2(0f, 1f);
            waveRect.anchorMax = new Vector2(0f, 1f);
            waveRect.pivot = new Vector2(0f, 1f);
            waveRect.anchoredPosition = new Vector2(30f, -30f);
            waveRect.sizeDelta = new Vector2(500f, 50f);

            waveText.text = "Wave: 1 / 3";
            waveText.fontSize = 36f;
            waveText.color = Color.white;
            waveText.alignment = TextAlignmentOptions.TopLeft;

            // EnemiesText
            Transform enemiesTextTrans = canvasGo.transform.Find("EnemiesText");
            GameObject enemiesTextGo = enemiesTextTrans != null ? enemiesTextTrans.gameObject : new GameObject("EnemiesText");
            enemiesTextGo.transform.SetParent(canvasGo.transform, false);

            TextMeshProUGUI enemiesText = enemiesTextGo.GetComponent<TextMeshProUGUI>();
            if (enemiesText == null)
            {
                enemiesText = enemiesTextGo.AddComponent<TextMeshProUGUI>();
            }

            EnsureDefaultFont(enemiesText);

            RectTransform enemiesRect = enemiesTextGo.GetComponent<RectTransform>();
            enemiesRect.anchorMin = new Vector2(0f, 1f);
            enemiesRect.anchorMax = new Vector2(0f, 1f);
            enemiesRect.pivot = new Vector2(0f, 1f);
            enemiesRect.anchoredPosition = new Vector2(30f, -80f);
            enemiesRect.sizeDelta = new Vector2(500f, 50f);

            enemiesText.text = "Enemies Alive: 0";
            enemiesText.fontSize = 32f;
            enemiesText.color = Color.white;
            enemiesText.alignment = TextAlignmentOptions.TopLeft;

            // VictoryPanel
            Transform victoryPanelTrans = canvasGo.transform.Find("VictoryPanel");
            GameObject victoryPanelGo = victoryPanelTrans != null ? victoryPanelTrans.gameObject : new GameObject("VictoryPanel");
            victoryPanelGo.transform.SetParent(canvasGo.transform, false);

            RectTransform panelRect = victoryPanelGo.GetComponent<RectTransform>();
            if (panelRect == null)
            {
                panelRect = victoryPanelGo.AddComponent<RectTransform>();
            }
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(850f, 250f);

            Image panelImage = victoryPanelGo.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = victoryPanelGo.AddComponent<Image>();
            }
            panelImage.color = new Color(0.08f, 0.08f, 0.12f, 0.85f);

            Transform victoryTextTrans = victoryPanelGo.transform.Find("VictoryText");
            GameObject victoryTextGo = victoryTextTrans != null ? victoryTextTrans.gameObject : new GameObject("VictoryText");
            victoryTextGo.transform.SetParent(victoryPanelGo.transform, false);

            TextMeshProUGUI victoryText = victoryTextGo.GetComponent<TextMeshProUGUI>();
            if (victoryText == null)
            {
                victoryText = victoryTextGo.AddComponent<TextMeshProUGUI>();
            }

            EnsureDefaultFont(victoryText);

            RectTransform victoryTextRect = victoryTextGo.GetComponent<RectTransform>();
            victoryTextRect.anchorMin = Vector2.zero;
            victoryTextRect.anchorMax = Vector2.one;
            victoryTextRect.offsetMin = Vector2.zero;
            victoryTextRect.offsetMax = Vector2.zero;
            victoryTextRect.pivot = new Vector2(0.5f, 0.5f);

            victoryText.text = "Victory! All Waves Cleared!";
            victoryText.fontSize = 54f;
            victoryText.color = new Color(1f, 0.84f, 0.0f, 1f); // Gold accent
            victoryText.alignment = TextAlignmentOptions.Center;

            victoryPanelGo.SetActive(false);

            // WaveUIView
            if (!canvasGo.TryGetComponent<WaveUIView>(out var waveUIView))
            {
                waveUIView = canvasGo.AddComponent<WaveUIView>();
            }

            SerializedObject viewSo = new SerializedObject(waveUIView);
            viewSo.FindProperty("_waveText").objectReferenceValue = waveText;
            viewSo.FindProperty("_enemiesAliveText").objectReferenceValue = enemiesText;
            viewSo.FindProperty("_victoryPanel").objectReferenceValue = victoryPanelGo;
            viewSo.FindProperty("_victoryText").objectReferenceValue = victoryText;
            viewSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(waveUIView);

            // WaveUIPresenter
            if (!canvasGo.TryGetComponent<WaveUIPresenter>(out var presenter))
            {
                presenter = canvasGo.AddComponent<WaveUIPresenter>();
            }

            SerializedObject presenterSo = new SerializedObject(presenter);
            presenterSo.FindProperty("_model").objectReferenceValue = waveController;
            presenterSo.FindProperty("_view").objectReferenceValue = waveUIView;
            presenterSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(presenter);
        }

        private static void SetupEventSystem()
        {
            GameObject eventSystemGo = GameObject.Find("EventSystem");
            if (eventSystemGo == null)
            {
                eventSystemGo = new GameObject("EventSystem");
            }

            if (!eventSystemGo.TryGetComponent<EventSystem>(out _))
            {
                eventSystemGo.AddComponent<EventSystem>();
            }

            StandaloneInputModule legacyModule = eventSystemGo.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
            {
                UnityEngine.Object.DestroyImmediate(legacyModule);
            }

            if (!eventSystemGo.TryGetComponent<InputSystemUIInputModule>(out var inputModule))
            {
                inputModule = eventSystemGo.AddComponent<InputSystemUIInputModule>();
            }

            InputActionAsset actionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (actionsAsset != null)
            {
                SerializedObject moduleSo = new SerializedObject(inputModule);
                SerializedProperty actionsProp = moduleSo.FindProperty("m_ActionsAsset");
                if (actionsProp != null)
                {
                    actionsProp.objectReferenceValue = actionsAsset;
                    moduleSo.ApplyModifiedProperties();
                }
            }
        }

        private static void EnsureDefaultFont(TextMeshProUGUI tmpText)
        {
            if (tmpText.font == null && TMP_Settings.defaultFontAsset != null)
            {
                tmpText.font = TMP_Settings.defaultFontAsset;
            }
        }
    }
}
#endif

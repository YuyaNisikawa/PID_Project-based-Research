using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace PIDSimulator
{
    public class PIDSimulatorSetup : EditorWindow
    {
        private const string PREFABS_PATH = "Assets/Resources/Prefabs";

        [MenuItem("PID Simulator/Setup/1. Create Folders")]
        private static void CreateFolders()
        {
            string[] folders = { 
                "Assets/Scripts", "Assets/Resources", "Assets/Scenes", 
                PREFABS_PATH, PREFABS_PATH + "/Models", PREFABS_PATH + "/Managers",
                PREFABS_PATH + "/UI", "Assets/Resources/ModelConfigs" 
            };
            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            }
            AssetDatabase.Refresh();
            Debug.Log("Folders created.");
        }

        [MenuItem("PID Simulator/Setup/2. Create Model Configs")]
        private static void CreateDefaultModelConfigs()
        {
            CreateModelConfig("Inverted Pendulum", 1.0f, 0.0f);
            CreateModelConfig("Helicopter", 1.0f, 10.0f);
            CreateModelConfig("Crane", 5.0f, 0.0f);
        }

        private static void CreateModelConfig(string modelName, float defaultMass, float defaultTarget)
        {
            string path = $"Assets/Resources/ModelConfigs/{modelName}.asset";
            if (File.Exists(path)) return;
            ModelConfig config = ScriptableObject.CreateInstance<ModelConfig>();
            config.modelName = modelName;
            config.defaultMass = defaultMass;
            config.defaultTargetValue = defaultTarget;
            AssetDatabase.CreateAsset(config, path);
        }

        [MenuItem("PID Simulator/Setup/3. Create All Prefabs")]
        private static void CreateAllPrefabs()
        {
            PrefabCreator.CreateAllPrefabs();
        }

        [MenuItem("PID Simulator/Setup/4. Setup All Scenes")]
        private static void SetupAllScenes()
        {
            string[] sceneNames = { "MainMenuScene", "ParameterSettingScene", "SimulationScene", "ResultScene", "RankingScene" };
            
            foreach (string sceneName in sceneNames)
            {
                string scenePath = $"Assets/Scenes/{sceneName}.unity";
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                
                UnityEngine.SceneManagement.Scene targetScene;
                if (sceneAsset == null)
                {
                    targetScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                }
                else
                {
                    targetScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }

                // マネージャーとUIの自動配置
                if (sceneName == "MainMenuScene")
                {
                    InstantiateIfMissing("Managers/SceneController", "SceneController");
                    InstantiateIfMissing("Managers/ModelManager", "ModelManager");
                    InstantiateIfMissing("Managers/RankingManager", "RankingManager");
                    InstantiateIfMissing("UI/MainMenuUI", "MainMenuCanvas");
                }
                else if (sceneName == "ParameterSettingScene")
                {
                    InstantiateIfMissing("UI/ParameterSettingUI", "ParameterSettingCanvas");
                }
                else if (sceneName == "SimulationScene")
                {
                    InstantiateIfMissing("Managers/SimulationManager", "SimulationManager");
                    InstantiateIfMissing("UI/SimulationUI", "SimulationCanvas");
                }
                else if (sceneName == "ResultScene")
                {
                    InstantiateIfMissing("UI/ResultUI", "ResultCanvas");
                }
                else if (sceneName == "RankingScene")
                {
                    InstantiateIfMissing("UI/RankingUI", "RankingCanvas");
                }

                EditorSceneManager.SaveScene(targetScene, scenePath);
                Debug.Log($"Setup completed for scene: {scenePath}");
            }
            
            // Build Settingsへの登録
            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[sceneNames.Length];
            for (int i = 0; i < sceneNames.Length; i++)
            {
                buildScenes[i] = new EditorBuildSettingsScene($"Assets/Scenes/{sceneNames[i]}.unity", true);
            }
            EditorBuildSettings.scenes = buildScenes;
            Debug.Log("All scenes added to Build Settings.");
        }

        private static void InstantiateIfMissing(string relativePath, string gameObjectName)
        {
            if (GameObject.Find(gameObjectName) == null)
            {
                string path = $"{PREFABS_PATH}/{relativePath}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    PrefabUtility.InstantiatePrefab(prefab);
                }
                else
                {
                    Debug.LogWarning($"Prefab not found: {path}. Run 'Create All Prefabs' first.");
                }
            }
        }
    }
}
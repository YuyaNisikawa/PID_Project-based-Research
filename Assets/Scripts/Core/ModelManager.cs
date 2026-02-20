using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PIDSimulator
{
    public class ModelManager : MonoBehaviour
    {
        public static ModelManager Instance { get; private set; }

        [SerializeField] private Transform modelParent;
        private Dictionary<string, ModelConfig> modelConfigs = new Dictionary<string, ModelConfig>();
        private IPhysicsModel currentModel;
        private GameObject currentModelInstance;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Initialize()
        {
            LoadModelConfigs();
            if (modelParent == null)
            {
                GameObject parentObj = new GameObject("Models");
                modelParent = parentObj.transform;
                parentObj.transform.SetParent(transform);
            }
        }

        private void LoadModelConfigs()
        {
            ModelConfig[] configs = Resources.LoadAll<ModelConfig>("ModelConfigs");
            foreach (var config in configs)
            {
                if (config.Validate()) modelConfigs[config.modelName] = config;
            }
        }

        public IPhysicsModel GetModel(string modelName)
        {
            UnloadCurrentModel();

            if (!modelConfigs.ContainsKey(modelName))
            {
                Debug.LogError($"[ModelManager] ModelConfig for '{modelName}' not found. Make sure it's in Resources/ModelConfigs/.");
                return null;
            }
            ModelConfig config = modelConfigs[modelName];

            // PrefabLoaderが名前空間外にある可能性を考慮し、型指定なしでロード
            currentModel = PrefabLoader.LoadModel(modelName, modelParent);
            if (currentModel == null)
            {
                Debug.LogError($"[ModelManager] Failed to load prefab for '{modelName}'. Make sure it's in Resources/Models/ and has an IPhysicsModel component.");
                return null;
            }

            currentModelInstance = (currentModel as MonoBehaviour).gameObject;
            currentModel.SetMass(config.defaultMass);
            currentModel.SetTargetValue(config.defaultTargetValue);

            return currentModel;
        }

        public string[] GetAvailableModelNames() => modelConfigs.Keys.ToArray();

        public void UnloadCurrentModel()
        {
            if (currentModelInstance != null)
            {
                Destroy(currentModelInstance);
                currentModel = null;
                currentModelInstance = null;
            }
        }
    }
}

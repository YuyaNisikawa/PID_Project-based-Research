using UnityEngine;

namespace PIDSimulator
{
    /// <summary>
    /// モデルの設定データを保持するScriptableObject
    /// Resources/ModelConfigs/ に配置して使用
    /// </summary>
    [CreateAssetMenu(fileName = "ModelConfig_", menuName = "PID/Model Config")]
    public class ModelConfig : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] public string modelName;
        [SerializeField] public string prefabPath = "Prefabs/Models/";  // Resources 内のパス
        
        [Header("Physics Parameters")]
        [SerializeField] public float defaultMass = 1.0f;
        [SerializeField] public float defaultTargetValue = 10.0f;
        [SerializeField] public float controlForceMultiplier = 100f;
        
        [Header("Model-Specific Settings")]
        [SerializeField] public float initialAngle = 10f;  // InvertedPendulum用
        [SerializeField] public float initialHeight = 5f;  // Helicopter用
        [SerializeField] public float initialXPosition = 0f;  // Crane用
        
        [Header("Simulation Parameters")]
        [SerializeField] public float maxSimulationTime = 30f;
        [SerializeField] public float stabilityTolerance = 0.05f;
        [SerializeField] public float stabilityCheckDuration = 1.0f;
        
        [Header("Debug")]
        [SerializeField] public bool enableDebugLogs = true;

        /// <summary>
        /// 設定の妥当性をチェック
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(modelName))
            {
                Debug.LogError("[ModelConfig] modelName is empty!");
                return false;
            }

            if (defaultMass <= 0)
            {
                Debug.LogError("[ModelConfig] defaultMass must be greater than 0!");
                return false;
            }

            if (controlForceMultiplier <= 0)
            {
                Debug.LogError("[ModelConfig] controlForceMultiplier must be greater than 0!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// デバッグ情報を出力
        /// </summary>
        public void LogInfo()
        {
            if (!enableDebugLogs) return;

            Debug.Log($"[ModelConfig] {modelName}\n" +
                      $"  Prefab Path: {prefabPath}\n" +
                      $"  Default Mass: {defaultMass}\n" +
                      $"  Default Target: {defaultTargetValue}\n" +
                      $"  Control Force Multiplier: {controlForceMultiplier}");
        }
    }
}

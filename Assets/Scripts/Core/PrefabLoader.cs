using UnityEngine;
using System.Collections.Generic;

namespace PIDSimulator
{
    /// <summary>
    /// プレハブをロードしてモデルをインスタンス化するユーティリティ
    /// </summary>
    public static class PrefabLoader
    {
        private static Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

        /// <summary>
        /// モデルプレハブをロードしてインスタンス化
        /// </summary>
        /// <param name="modelName">モデル名（例: "Inverted Pendulum"）</param>
        /// <param name="parent">親トランスフォーム（null の場合はワールドルート）</param>
        /// <returns>IPhysicsModel インターフェース、失敗時は null</returns>
        public static IPhysicsModel LoadModel(string modelName, Transform parent = null)
        {
            if (string.IsNullOrEmpty(modelName))
            {
                Debug.LogError("[PrefabLoader] modelName is null or empty!");
                return null;
            }

            try
            {
                // プレハブをロード
                GameObject prefab = LoadPrefab(modelName);
                if (prefab == null)
                {
                    Debug.LogError($"[PrefabLoader] Failed to load prefab for model: {modelName}. Check if prefab exists in Resources/Models/ and is assigned correctly.");
                    return null;
                }

                // インスタンス化
                GameObject instance = Object.Instantiate(prefab, parent);
                instance.name = $"{modelName} (Instance)";

                // IPhysicsModel コンポーネントを取得
                IPhysicsModel model = instance.GetComponent<IPhysicsModel>();
                if (model == null)
                {
                    Debug.LogError($"[PrefabLoader] IPhysicsModel component not found on {modelName} prefab!");
                    Object.Destroy(instance);
                    return null;
                }

                Debug.Log($"[PrefabLoader] Model loaded successfully: {modelName}");
                return model;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PrefabLoader] Exception while loading model {modelName}: {ex}");
                return null;
            }
        }

        /// <summary>
        /// プレハブをキャッシュからロード（キャッシュがない場合は Resources から読み込み）
        /// </summary>
        private static GameObject LoadPrefab(string modelName)
        {
            // キャッシュを確認
            if (prefabCache.ContainsKey(modelName))
            {
                return prefabCache[modelName];
            }

            // Resources から読み込み
            string prefabPath = $"Models/{modelName}"; // Resources.LoadはResourcesフォルダからの相対パスを期待する
            GameObject prefab = Resources.Load<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"[PrefabLoader] Prefab not found at path: Resources/{prefabPath}. Make sure the prefab is in a 'Models' subfolder directly under a Resources folder.");
                return null;
            }

            // キャッシュに保存
            prefabCache[modelName] = prefab;
            Debug.Log($"[PrefabLoader] Prefab cached: {modelName}");

            return prefab;
        }

        /// <summary>
        /// キャッシュをクリア
        /// </summary>
        public static void ClearCache()
        {
            prefabCache.Clear();
            Debug.Log("[PrefabLoader] Cache cleared");
        }

        /// <summary>
        /// キャッシュ内のプレハブ数を取得
        /// </summary>
        public static int GetCachedPrefabCount()
        {
            return prefabCache.Count;
        }
    }
}

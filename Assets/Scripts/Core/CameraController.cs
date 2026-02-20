using UnityEngine;

namespace PIDSimulator
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        // 前よりZを小さく（-25f）して遠ざけ、全体が映るように変更
        [SerializeField] private Vector3 offset = new Vector3(0, 6f, -25f);
        [SerializeField] private float smoothTime = 0.3f;

        private Vector3 velocity = Vector3.zero;

        private void LateUpdate()
        {
            if (SimulationManager.Instance != null && SimulationManager.Instance.PhysicsModel != null)
            {
                // ルートオブジェクト（地面の位置）を取得
                Transform targetRoot = ((MonoBehaviour)SimulationManager.Instance.PhysicsModel).transform;

                // カメラの目標位置へ滑らかに移動
                Vector3 targetPosition = targetRoot.position + offset;
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

                // 激しく動くアームではなく、支柱の中心（Y=4あたり）を安定して見つめ続ける
                transform.LookAt(targetRoot.position + Vector3.up * 4f);
            }
        }
    }
}
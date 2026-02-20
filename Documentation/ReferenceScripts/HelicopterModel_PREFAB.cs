using UnityEngine;

/// <summary>
/// ヘリコプターモデル（プレハブ対応版）
/// </summary>
public class HelicopterModel : MonoBehaviour, IPhysicsModel
{
    [SerializeField] private Rigidbody heliRigidbody;
    [SerializeField] private float controlForceMultiplier = 10f;
    [SerializeField] private float initialHeight = 5f;

    private float targetHeight = 0f;
    private float currentMass = 1f;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool isInitialized = false;

    private void OnEnable()
    {
        if (!isInitialized)
            Initialize();
    }

    private void Initialize()
    {
        // Rigidbody を取得
        if (heliRigidbody == null)
            heliRigidbody = GetComponent<Rigidbody>();

        // 初期位置と回転を保存
        startPosition = transform.position;
        startRotation = transform.rotation;

        isInitialized = true;

        // 検証
        if (heliRigidbody == null)
            Debug.LogError("[HelicopterModel] Rigidbody not found!");

        Debug.Log("[HelicopterModel] Initialized");
    }

    public string GetModelName() => "Helicopter";

    public float GetCurrentValue()
    {
        return transform.position.y;
    }

    public float GetCurrentVelocity()
    {
        if (heliRigidbody == null) return 0;
        return heliRigidbody.linearVelocity.y;
    }

    public void SetTargetValue(float target)
    {
        targetHeight = target;
    }

    public float GetTargetValue()
    {
        return targetHeight;
    }

    public void SetControlInput(float input)
    {
        if (heliRigidbody == null) return;

        // NaN チェック
        if (float.IsNaN(input)) return;

        // 上方向への力を加える
        heliRigidbody.AddForce(Vector3.up * input * controlForceMultiplier);
    }

    public void SetMass(float mass)
    {
        currentMass = Mathf.Max(0.1f, mass);
        if (heliRigidbody != null)
            heliRigidbody.mass = currentMass;
    }

    public void Reset()
    {
        if (heliRigidbody != null)
        {
            heliRigidbody.linearVelocity = Vector3.zero;
            heliRigidbody.angularVelocity = Vector3.zero;

            // 初期位置にリセット
            transform.position = new Vector3(startPosition.x, initialHeight, startPosition.z);
            transform.rotation = startRotation;
        }
    }

    /// <summary>
    /// ModelConfig から設定を適用
    /// </summary>
    public void ApplyConfig(ModelConfig config)
    {
        if (config == null) return;

        controlForceMultiplier = config.controlForceMultiplier;
        initialHeight = config.initialHeight;

        Debug.Log($"[HelicopterModel] Config applied: Force={controlForceMultiplier}, Height={initialHeight}");
    }
}

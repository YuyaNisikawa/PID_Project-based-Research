using UnityEngine;

/// <summary>
/// クレーンモデル（プレハブ対応版）
/// </summary>
public class CraneModel : MonoBehaviour, IPhysicsModel
{
    [SerializeField] private Rigidbody trolleyRigidbody;
    [SerializeField] private Rigidbody loadRigidbody;
    [SerializeField] private string loadObjectName = "Load";
    [SerializeField] private float controlForceMultiplier = 50f;
    [SerializeField] private float initialXPosition = 0f;

    private float targetXPosition = 0f;
    private float currentMass = 1f;
    private Vector3 startTrolleyPosition;
    private Quaternion startTrolleyRotation;
    private Vector3 startLoadPosition;
    private Quaternion startLoadRotation;
    private bool isInitialized = false;

    private void OnEnable()
    {
        if (!isInitialized)
            Initialize();
    }

    private void Initialize()
    {
        // Trolley Rigidbody を取得
        if (trolleyRigidbody == null)
            trolleyRigidbody = GetComponent<Rigidbody>();

        // Load Rigidbody を子オブジェクトから取得
        if (loadRigidbody == null)
        {
            Transform loadTransform = transform.Find(loadObjectName);
            if (loadTransform != null)
                loadRigidbody = loadTransform.GetComponent<Rigidbody>();
        }

        // 初期位置と回転を保存
        if (trolleyRigidbody != null)
        {
            startTrolleyPosition = trolleyRigidbody.transform.position;
            startTrolleyRotation = trolleyRigidbody.transform.rotation;
        }

        if (loadRigidbody != null)
        {
            startLoadPosition = loadRigidbody.transform.position;
            startLoadRotation = loadRigidbody.transform.rotation;
        }

        isInitialized = true;

        // 検証
        if (trolleyRigidbody == null)
            Debug.LogError("[CraneModel] Trolley Rigidbody not found!");

        if (loadRigidbody == null)
            Debug.LogWarning("[CraneModel] Load Rigidbody not found. Load physics will not work.");

        Debug.Log("[CraneModel] Initialized");
    }

    public string GetModelName() => "Crane";

    public float GetCurrentValue()
    {
        if (trolleyRigidbody == null) return 0;
        return trolleyRigidbody.transform.position.x;
    }

    public float GetCurrentVelocity()
    {
        if (trolleyRigidbody == null) return 0;
        return trolleyRigidbody.linearVelocity.x;
    }

    public void SetTargetValue(float target)
    {
        targetXPosition = target;
    }

    public float GetTargetValue()
    {
        return targetXPosition;
    }

    public void SetControlInput(float input)
    {
        if (trolleyRigidbody == null) return;

        // NaN チェック
        if (float.IsNaN(input)) return;

        // 左右方向への力を加える
        trolleyRigidbody.AddForce(Vector3.right * input * controlForceMultiplier);
    }

    public void SetMass(float mass)
    {
        currentMass = Mathf.Max(0.1f, mass);

        if (loadRigidbody != null)
            loadRigidbody.mass = currentMass;
    }

    public void Reset()
    {
        if (trolleyRigidbody != null)
        {
            trolleyRigidbody.linearVelocity = Vector3.zero;
            trolleyRigidbody.angularVelocity = Vector3.zero;
            trolleyRigidbody.transform.position = new Vector3(initialXPosition, startTrolleyPosition.y, startTrolleyPosition.z);
            trolleyRigidbody.transform.rotation = startTrolleyRotation;
        }

        if (loadRigidbody != null)
        {
            loadRigidbody.linearVelocity = Vector3.zero;
            loadRigidbody.angularVelocity = Vector3.zero;
            loadRigidbody.transform.position = startLoadPosition;
            loadRigidbody.transform.rotation = startLoadRotation;
        }
    }

    /// <summary>
    /// ModelConfig から設定を適用
    /// </summary>
    public void ApplyConfig(ModelConfig config)
    {
        if (config == null) return;

        controlForceMultiplier = config.controlForceMultiplier;
        initialXPosition = config.initialXPosition;

        Debug.Log($"[CraneModel] Config applied: Force={controlForceMultiplier}, InitialX={initialXPosition}");
    }
}

using UnityEngine;

/// <summary>
/// 逆立ち振り子モデル（プレハブ対応版）
/// </summary>
public class InvertedPendulumModel : MonoBehaviour, IPhysicsModel
{
    [SerializeField] private Rigidbody poleRigidbody;
    [SerializeField] private HingeJoint hingeJoint;
    [SerializeField] private float controlForceMultiplier = 100f;
    [SerializeField] private float initialAngle = 10f;

    private float targetAngle = 0f;
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
        if (poleRigidbody == null)
            poleRigidbody = GetComponent<Rigidbody>();

        // HingeJoint を取得
        if (hingeJoint == null)
            hingeJoint = GetComponent<HingeJoint>();

        // 初期位置と回転を保存
        startPosition = transform.position;
        startRotation = transform.rotation;

        isInitialized = true;

        // 検証
        if (poleRigidbody == null)
            Debug.LogError("[InvertedPendulumModel] Rigidbody not found!");

        if (hingeJoint == null)
            Debug.LogError("[InvertedPendulumModel] HingeJoint not found!");

        Debug.Log("[InvertedPendulumModel] Initialized");
    }

    public string GetModelName() => "Inverted Pendulum";

    public float GetCurrentValue()
    {
        if (hingeJoint == null) return 0;

        // 角度を-180〜180の範囲に補正
        float angle = hingeJoint.angle;
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;

        return angle;
    }

    public float GetCurrentVelocity()
    {
        if (poleRigidbody == null) return 0;
        return poleRigidbody.angularVelocity.magnitude;
    }

    public void SetTargetValue(float target)
    {
        targetAngle = target;
    }

    public float GetTargetValue() => targetAngle;

    public void SetControlInput(float input)
    {
        if (poleRigidbody == null || hingeJoint == null) return;

        // NaN チェック
        if (float.IsNaN(input)) return;

        // トルクを適用
        poleRigidbody.AddTorque(hingeJoint.axis * input * controlForceMultiplier);
    }

    public void SetMass(float mass)
    {
        if (poleRigidbody != null)
            poleRigidbody.mass = Mathf.Max(0.1f, mass);
    }

    public void Reset()
    {
        if (poleRigidbody != null)
        {
            poleRigidbody.linearVelocity = Vector3.zero;
            poleRigidbody.angularVelocity = Vector3.zero;
            transform.position = startPosition;
            transform.rotation = startRotation;

            // 初期角度を設定
            transform.localRotation = Quaternion.Euler(0, 0, initialAngle);
        }
    }

    /// <summary>
    /// ModelConfig から設定を適用
    /// </summary>
    public void ApplyConfig(ModelConfig config)
    {
        if (config == null) return;

        controlForceMultiplier = config.controlForceMultiplier;
        initialAngle = config.initialAngle;

        Debug.Log($"[InvertedPendulumModel] Config applied: Force={controlForceMultiplier}, Angle={initialAngle}");
    }
}

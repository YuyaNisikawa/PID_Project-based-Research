using UnityEngine;

namespace PIDSimulator
{
    public class InvertedPendulumModel : MonoBehaviour, IPhysicsModel
    {
        [SerializeField] private Rigidbody poleRigidbody;
        [SerializeField] private HingeJoint hingeJoint;
        
        // ★修正1: モーターの力を大幅に弱める（50f -> 2f）。これが震えの根本原因でした。
        [SerializeField] private float controlForceMultiplier = 2f;

        private float targetAngle = 90f;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private bool isInitialized = false;

        private void OnEnable()
        {
            if (!isInitialized) Initialize();
        }

        private void Initialize()
        {
            if (poleRigidbody != null)
            {
                startPosition = poleRigidbody.transform.localPosition;
                startRotation = poleRigidbody.transform.localRotation;
                
                poleRigidbody.maxAngularVelocity = 50f;
                
                // ★修正2: 物理演算の「カクつき」を無くし、視覚的に滑らかに描画する
                poleRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }
            isInitialized = true;
        }

        public string GetModelName() => "Swing-up Pendulum";

        public float GetCurrentValue()
        {
            if (hingeJoint == null) return 0;
            return hingeJoint.angle;
        }

        public float GetCurrentVelocity()
        {
            if (poleRigidbody == null) return 0;
            return poleRigidbody.angularVelocity.z;
        }

        public void SetTargetValue(float target) => targetAngle = target;
        public float GetTargetValue() => targetAngle;

        public void SetControlInput(float input)
        {
            if (poleRigidbody == null || float.IsNaN(input)) return;
            // ローカル座標で安全にトルクを加える
            poleRigidbody.AddRelativeTorque(new Vector3(0, 0, 1) * input * controlForceMultiplier);
        }

        public void SetMass(float mass)
        {
            if (poleRigidbody != null) poleRigidbody.mass = Mathf.Max(0.1f, mass);
        }

        public void Reset()
        {
            if (poleRigidbody != null)
            {
                poleRigidbody.linearVelocity = Vector3.zero;
                poleRigidbody.angularVelocity = Vector3.zero;
                poleRigidbody.transform.localPosition = startPosition;
                poleRigidbody.transform.localRotation = startRotation;
            }
        }
    }
}
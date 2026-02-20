using UnityEngine;

namespace PIDSimulator
{
    public class HelicopterModel : MonoBehaviour, IPhysicsModel
    {
        [SerializeField] private Rigidbody heliRigidbody;
        [SerializeField] private float controlForceMultiplier = 10f;
        [SerializeField] private float initialHeight = 5f;

        private float targetHeight = 0f;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private bool isInitialized = false;

        private void OnEnable()
        {
            if (!isInitialized) Initialize();
        }

        private void Initialize()
        {
            if (heliRigidbody == null) heliRigidbody = GetComponent<Rigidbody>();
            startPosition = transform.position;
            startRotation = transform.rotation;
            isInitialized = true;
        }

        public string GetModelName() => "Helicopter";
        public float GetCurrentValue() => transform.position.y;
        public float GetCurrentVelocity() => heliRigidbody != null ? heliRigidbody.linearVelocity.y : 0;
        public void SetTargetValue(float target) => targetHeight = target;
        public float GetTargetValue() => targetHeight;

        public void SetControlInput(float input)
        {
            if (heliRigidbody == null || float.IsNaN(input)) return;
            heliRigidbody.AddForce(Vector3.up * input * controlForceMultiplier);
        }

        public void SetMass(float mass)
        {
            if (heliRigidbody != null) heliRigidbody.mass = Mathf.Max(0.1f, mass);
        }

        public void Reset()
        {
            if (heliRigidbody != null)
            {
                heliRigidbody.linearVelocity = Vector3.zero;
                heliRigidbody.angularVelocity = Vector3.zero;
                transform.position = new Vector3(startPosition.x, initialHeight, startPosition.z);
                transform.rotation = startRotation;
            }
        }
    }
}

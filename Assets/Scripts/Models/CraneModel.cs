using UnityEngine;

namespace PIDSimulator
{
    public class CraneModel : MonoBehaviour, IPhysicsModel
    {
        [SerializeField] private Rigidbody trolleyRigidbody;
        [SerializeField] private Rigidbody loadRigidbody;
        [SerializeField] private string loadObjectName = "Load";
        [SerializeField] private float controlForceMultiplier = 50f;
        [SerializeField] private float initialXPosition = 0f;

        private float targetXPosition = 0f;
        private Vector3 startTrolleyPosition;
        private Quaternion startTrolleyRotation;
        private Vector3 startLoadPosition;
        private Quaternion startLoadRotation;
        private bool isInitialized = false;

        private void OnEnable()
        {
            if (!isInitialized) Initialize();
        }

        private void Initialize()
        {
            if (trolleyRigidbody == null) trolleyRigidbody = GetComponent<Rigidbody>();
            if (loadRigidbody == null)
            {
                Transform loadTransform = transform.Find(loadObjectName);
                if (loadTransform != null) loadRigidbody = loadTransform.GetComponent<Rigidbody>();
            }

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
        }

        public string GetModelName() => "Crane";
        public float GetCurrentValue() => trolleyRigidbody != null ? trolleyRigidbody.transform.position.x : 0;
        public float GetCurrentVelocity() => trolleyRigidbody != null ? trolleyRigidbody.linearVelocity.x : 0;
        public void SetTargetValue(float target) => targetXPosition = target;
        public float GetTargetValue() => targetXPosition;

        public void SetControlInput(float input)
        {
            if (trolleyRigidbody == null || float.IsNaN(input)) return;
            trolleyRigidbody.AddForce(Vector3.right * input * controlForceMultiplier);
        }

        public void SetMass(float mass)
        {
            if (loadRigidbody != null) loadRigidbody.mass = Mathf.Max(0.1f, mass);
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
    }
}

using UnityEngine;

namespace PIDSimulator
{
    [System.Serializable]
    public class PIDController : MonoBehaviour
    {
        public float Kp, Ki, Kd;
        private float integral, lastError;

        public void SetGains(float kp, float ki, float kd)
        {
            Kp = kp; Ki = ki; Kd = kd;
        }

        public void Reset()
        {
            integral = 0; lastError = 0;
        }

        public float CalculatePID(float current, float target, float dt)
        {
            if (dt <= 0) return 0;

            float error = target - current;
            integral += error * dt;
            
            integral = Mathf.Clamp(integral, -100f, 100f);

            float derivative = (error - lastError) / dt;
            lastError = error;

            float output = (error * Kp) + (integral * Ki) + (derivative * Kd);

            if (float.IsNaN(output) || float.IsInfinity(output))
            {
                return 0;
            }

            return Mathf.Clamp(output, -1000f, 1000f);
        }

        public string GetDebugInfo() => $"P:{Kp:F2} I:{Ki:F2} D:{Kd:F2}";
    }
}

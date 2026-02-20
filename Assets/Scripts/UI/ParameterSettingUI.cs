using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PIDSimulator
{
    public class ParameterSettingUI : MonoBehaviour
    {
        [Header("PID Parameters")]
        [SerializeField] private TMP_InputField kpInput;
        [SerializeField] private TMP_InputField kiInput;
        [SerializeField] private TMP_InputField kdInput;
        [SerializeField] private TMP_InputField targetInput;
        [SerializeField] private TMP_InputField massInput;
        [SerializeField] private Button startButton;

        private void Start()
        {
            if (startButton != null) startButton.onClick.AddListener(OnStartButtonClick);

            // デバッグログを追加して、InputFieldが正しく紐付けられているか確認
            Debug.Log($"[ParameterSettingUI] kpInput is null: {kpInput == null}");
            Debug.Log($"[ParameterSettingUI] kiInput is null: {kiInput == null}");
            Debug.Log($"[ParameterSettingUI] kdInput is null: {kdInput == null}");
            Debug.Log($"[ParameterSettingUI] targetInput is null: {targetInput == null}");
            Debug.Log($"[ParameterSettingUI] massInput is null: {massInput == null}");
        }

        public void OnStartButtonClick()
        {
            if (SceneController.Instance == null) return;

            try
            {
                float kp = float.Parse(kpInput.text);
                float ki = float.Parse(kiInput.text);
                float kd = float.Parse(kdInput.text);
                float target = float.Parse(targetInput.text);
                float mass = float.Parse(massInput.text);

                SceneController.Instance.SetSimulationParams(
                    SceneController.Instance.SelectedModelName,
                    kp, ki, kd, target, mass
                );

                SceneController.Instance.LoadSimulation(SceneController.Instance.SelectedModelName);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ParameterSettingUI] Error: {ex.Message}");
            }
        }
    }
}

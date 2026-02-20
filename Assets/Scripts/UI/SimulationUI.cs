using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PIDSimulator
{
    public class SimulationUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text modelNameText;
        [SerializeField] private Button stopButton;
        [SerializeField] private SimpleGraph graph;

        private void Start()
        {
            if (stopButton != null) stopButton.onClick.AddListener(OnStopButtonClick);
            
            if (SimulationManager.Instance != null)
            {
                SimulationManager.Instance.OnValueUpdated.AddListener(UpdateGraph);
                SimulationManager.Instance.OnSimulationStarted.AddListener(() => { if (graph != null) graph.ClearGraph(); });
            }
        }

        private void Update()
        {
            if (SimulationManager.Instance == null) return;

            var manager = SimulationManager.Instance;
            if (statusText != null) statusText.text = $"Status: {manager.CurrentState}";
            if (timeText != null) timeText.text = $"Time: {manager.ElapsedTime:F2}s";
            
            if (modelNameText != null && manager.PhysicsModel != null)
                modelNameText.text = "Model: " + manager.PhysicsModel.GetModelName();
            else if (modelNameText != null && SceneController.Instance != null)
                modelNameText.text = "Model: " + SceneController.Instance.SelectedModelName;
        }

        private void UpdateGraph(float current, float target)
        {
            if (graph != null) graph.UpdateGraph(current, target);
        }

        public void OnStopButtonClick()
        {
            if (SimulationManager.Instance != null)
                SimulationManager.Instance.StopSimulation();
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

namespace PIDSimulator
{
    public class SceneController : MonoBehaviour
    {
        public static SceneController Instance { get; private set; }

        public string SelectedModelName { get; private set; }
        public PIDParameters PendingPIDParams { get; set; }
        public SimulationResult LastResult { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadMainMenu() => SceneManager.LoadScene("MainMenuScene");
        public void LoadRanking() => SceneManager.LoadScene("RankingScene");
        public void LoadResult(SimulationResult result)
        {
            LastResult = result;
            SceneManager.LoadScene("ResultScene");
        }

        public void SelectModel(string modelName) => SelectedModelName = modelName;

        public void SetSimulationParams(string modelName, float kp, float ki, float kd, float target, float mass)
        {
            SelectedModelName = modelName;
            PendingPIDParams = new PIDParameters { Kp = kp, Ki = ki, Kd = kd, TargetValue = target, Mass = mass };
        }

        public void LoadParameterSetting() => SceneManager.LoadScene("ParameterSettingScene");

        public void LoadSimulation(string modelName)
        {
            SelectedModelName = modelName;
            SceneManager.LoadScene("SimulationScene");
        }
    }
}

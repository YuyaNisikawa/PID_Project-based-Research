using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PIDSimulator
{
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text overshootText;
        [SerializeField] private TMP_Text evaluationText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button rankingButton;
        [SerializeField] private SimpleGraph finalGraph;

        private void Start()
        {
            if (retryButton != null) retryButton.onClick.AddListener(OnRetryButtonClick);
            if (menuButton != null) menuButton.onClick.AddListener(OnMenuButtonClick);
            if (rankingButton != null) rankingButton.onClick.AddListener(OnRankingButtonClick);

            DisplayResult();
        }

        private void DisplayResult()
        {
            if (SceneController.Instance == null || SceneController.Instance.LastResult == null)
            {
                Debug.LogWarning("[ResultUI] No simulation result found.");
                if (scoreText != null) scoreText.text = "Score: N/A";
                if (timeText != null) timeText.text = "Time: N/A";
                if (overshootText != null) overshootText.text = "Overshoot: N/A";
                if (evaluationText != null) evaluationText.text = "Evaluation: N/A";
                return;
            }

            SimulationResult result = SceneController.Instance.LastResult;

            if (scoreText != null) scoreText.text = $"Score: {result.Score:F0}";
            if (timeText != null) timeText.text = $"Time: {result.SettlingTime:F2}s";
            if (overshootText != null) overshootText.text = $"Overshoot: {result.Overshoot:F1}%";
            
            string evaluationMessage = "";
            Color evaluationColor = Color.white;

            if (result.Score >= 90) { evaluationMessage = "Excellent!"; evaluationColor = Color.green; }
            else if (result.Score >= 70) { evaluationMessage = "Good!"; evaluationColor = Color.yellow; }
            else if (result.Score >= 50) { evaluationMessage = "Fair."; evaluationColor = Color.cyan; }
            else { evaluationMessage = "Needs Improvement."; evaluationColor = Color.red; }

            if (evaluationText != null)
            {
                evaluationText.text = $"Evaluation: {evaluationMessage}";
                evaluationText.color = evaluationColor;
            }

            if (finalGraph != null && result.DataTracker != null)
            {
                finalGraph.ShowFinalGraph(result.DataTracker);
            }
        }

        public void OnRetryButtonClick()
        {
            if (SceneController.Instance != null)
                SceneController.Instance.LoadSimulation(SceneController.Instance.SelectedModelName);
        }

        public void OnMenuButtonClick()
        {
            if (SceneController.Instance != null)
                SceneController.Instance.LoadMainMenu();
        }

        public void OnRankingButtonClick()
        {
            if (SceneController.Instance != null)
                SceneController.Instance.LoadRanking();
        }
    }
}

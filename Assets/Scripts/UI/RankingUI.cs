using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PIDSimulator
{
    public class RankingUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text rankingText;
        [SerializeField] private Button backButton;

        private void Start()
        {
            if (backButton != null) backButton.onClick.AddListener(OnBackToMenu);

            DisplayRanking();
        }

        private void DisplayRanking()
        {
            if (SceneController.Instance == null) return;

            string modelName = SceneController.Instance.SelectedModelName;
            if (string.IsNullOrEmpty(modelName)) modelName = "Inverted Pendulum"; // Default to Inverted Pendulum if no model selected

            var entries = RankingManager.GetRankingEntries(modelName);
            if (rankingText != null)
            {
                rankingText.text = $"Ranking for {modelName}\n\n";
                for (int i = 0; i < entries.Count; i++)
                {
                    rankingText.text += $"{i + 1}. Score: {entries[i].Score:F0} ({entries[i].Date})\n";
                }
                if (entries.Count == 0)
                {
                    rankingText.text += "No ranking data yet.";
                }
            }
        }

        public void OnBackToMenu()
        {
            if (SceneController.Instance != null) SceneController.Instance.LoadMainMenu();
        }
    }
}

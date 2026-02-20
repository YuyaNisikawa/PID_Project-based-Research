using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PIDSimulator
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button invertedPendulumButton;
        [SerializeField] private Button helicopterButton;
        [SerializeField] private Button craneButton;
        [SerializeField] private Button rankingButton;

        private void Start()
        {
            if (invertedPendulumButton != null) invertedPendulumButton.onClick.AddListener(() => OnModelSelect("Inverted Pendulum"));
            if (helicopterButton != null) helicopterButton.onClick.AddListener(() => OnModelSelect("Helicopter"));
            if (craneButton != null) craneButton.onClick.AddListener(() => OnModelSelect("Crane"));
            if (rankingButton != null) rankingButton.onClick.AddListener(OnRankingButtonClick);
        }

        private void OnModelSelect(string modelName)
        {
            if (SceneController.Instance != null)
            {
                SceneController.Instance.SelectModel(modelName);
                SceneController.Instance.LoadParameterSetting();
            }
        }

        private void OnRankingButtonClick()
        {
            if (SceneController.Instance != null) SceneController.Instance.LoadRanking();
        }
    }
}

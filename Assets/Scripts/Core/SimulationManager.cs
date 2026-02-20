using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace PIDSimulator
{

    public class SimulationManager : MonoBehaviour
    {
        public static SimulationManager Instance { get; private set; }

        [SerializeField] private PIDController pidController;
        [SerializeField] private float maxSimulationTime = 30f;
        [SerializeField] private float stabilityTolerance = 0.05f;
        [SerializeField] private float stabilityCheckDuration = 1.0f;

        private IPhysicsModel physicsModel;
        private PIDDataTracker dataTracker;
        
        private SimulationState currentState = SimulationState.Idle;
        private float elapsedTime, stabilityTimer, settlingTime;
        private bool isStabilityAchieved;

        public SimulationState CurrentState => currentState;
        public PIDDataTracker DataTracker => dataTracker;
        public IPhysicsModel PhysicsModel => physicsModel;
        public float ElapsedTime => elapsedTime;
        public PIDController PIDController => pidController;

        public UnityEvent OnSimulationStarted, OnSimulationStopped;
        public UnityEvent<float> OnSimulationCompleted, OnTimeUpdated;
        public UnityEvent<float, float> OnValueUpdated; // グラフ更新用 (current, target)

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            
            dataTracker = new PIDDataTracker(0.05f, 2000);
            
            if (pidController == null) pidController = GetComponent<PIDController>();
            if (pidController == null) pidController = gameObject.AddComponent<PIDController>();
        }

        private void Start()
        {
            // 少し遅らせて初期化することで他のマネージャーの準備を待つ
            Invoke(nameof(DeferredStart), 0.1f);
        }

        private void DeferredStart()
        {
            if (SceneController.Instance == null)
            {
                Debug.LogWarning("[SimulationManager] SceneController not found. Using default for testing.");
                InitializeDefault();
                return;
            }

            string modelName = SceneController.Instance.SelectedModelName;
            if (string.IsNullOrEmpty(modelName))
            {
                Debug.LogWarning("[SimulationManager] No model selected. Using default.");
                modelName = "Inverted Pendulum";
            }

            if (ModelManager.Instance != null)
            {
                physicsModel = ModelManager.Instance.GetModel(modelName);
                if (physicsModel != null)
                {
                    Debug.Log($"[SimulationManager] Model '{modelName}' loaded successfully.");
                    var p = SceneController.Instance.PendingPIDParams;
                    // パラメータが未設定の場合はデフォルト値を入れる
                    if (p.TargetValue == 0 && p.Mass == 0)
                    {
                        p.Kp = 1.0f; p.Ki = 0.1f; p.Kd = 0.05f; p.TargetValue = 10.0f; p.Mass = 1.0f;
                    }
                    Initialize(physicsModel, p.Kp, p.Ki, p.Kd, p.TargetValue, p.Mass);
                    StartSimulation();
                }
                else
                {
                    Debug.LogError($"[SimulationManager] Failed to load model: {modelName}. Simulation cannot start.");
                    // モデルロード失敗時は、UIにエラーを表示するなど、ユーザーにフィードバックする処理を追加することも検討
                    // 例: statusText.text = "Error: Model not found!";
                    return; // モデルがロードできない場合はここで処理を終了
                }
            }
        }

        private void InitializeDefault()
        {
            if (ModelManager.Instance != null)
            {
                physicsModel = ModelManager.Instance.GetModel("Inverted Pendulum");
                if (physicsModel != null)
                {
                    Debug.Log($"[SimulationManager] Model 'Inverted Pendulum' loaded successfully as default.");
                    Initialize(physicsModel, 1.0f, 0.1f, 0.05f, 10.0f, 1.0f);
                    StartSimulation(); // デフォルトモデルの場合もシミュレーションを開始
                }
                else
                {
                    Debug.LogError("[SimulationManager] Failed to load default model 'Inverted Pendulum'. Simulation cannot start.");
                }
            }
        }

        public void Initialize(IPhysicsModel model, float kp, float ki, float kd, float target, float mass)
        {
            physicsModel = model;
            pidController.SetGains(kp, ki, kd);
            pidController.Reset();
            physicsModel.SetTargetValue(target);
            physicsModel.SetMass(mass);
            physicsModel.Reset();
            
            if (dataTracker == null) dataTracker = new PIDDataTracker(0.05f, 2000);
            dataTracker.Clear();
            
            elapsedTime = 0;
            stabilityTimer = 0;
            isStabilityAchieved = false;
            currentState = SimulationState.Idle;
        }

        public void StartSimulation()
        {
            currentState = SimulationState.Running;
            OnSimulationStarted?.Invoke();
        }

        public void StopSimulation()
        {
            currentState = SimulationState.Stopped;
            OnSimulationStopped?.Invoke();
        }

        private void FixedUpdate()
        {
            if (currentState != SimulationState.Running || physicsModel == null) return;

            elapsedTime += Time.fixedDeltaTime;
            OnTimeUpdated?.Invoke(elapsedTime);

            float current = physicsModel.GetCurrentValue();
            float target = physicsModel.GetTargetValue();
            float input = pidController.CalculatePID(current, target, Time.fixedDeltaTime);
            physicsModel.SetControlInput(input);

            dataTracker.Record(elapsedTime, current, target, input, pidController.GetDebugInfo());
            OnValueUpdated?.Invoke(current, target); // グラフ更新イベント

            CheckStability(current, target);

            if (elapsedTime >= maxSimulationTime) CompleteSimulation();
        }

        private void CheckStability(float current, float target)
        {
            if (isStabilityAchieved) return;

            float error = Mathf.Abs(current - target);
            float threshold = Mathf.Max(0.1f, Mathf.Abs(target) * stabilityTolerance);

            if (error <= threshold)
            {
                stabilityTimer += Time.fixedDeltaTime;
                if (stabilityTimer >= stabilityCheckDuration)
                {
                    isStabilityAchieved = true;
                    settlingTime = elapsedTime - stabilityCheckDuration;
                    CompleteSimulation();
                }
            }
            else
            {
                stabilityTimer = 0;
            }
        }

        private void CompleteSimulation()
        {
            currentState = SimulationState.Completed;
            if (!isStabilityAchieved) settlingTime = maxSimulationTime;

            var result = PerformanceEvaluator.Evaluate(dataTracker, settlingTime, stabilityTolerance);

            RankingManager.SaveScore(
                physicsModel.GetModelName(),
                result.Score,
                pidController.Kp, pidController.Ki, pidController.Kd
            );

            OnSimulationCompleted?.Invoke(settlingTime);

            if (SceneController.Instance != null)
                SceneController.Instance.LoadResult(result);
        }
    }
}

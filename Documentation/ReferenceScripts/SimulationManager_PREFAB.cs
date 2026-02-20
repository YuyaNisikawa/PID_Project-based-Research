using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// プレハブベースのシミュレーションマネージャー
/// ModelManager を使用してモデルを動的にロード
/// </summary>
public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }

    [SerializeField] private PIDController pidController = new PIDController();
    [SerializeField] private float maxSimulationTime = 30f;
    [SerializeField] private float stabilityTolerance = 0.05f;
    [SerializeField] private float stabilityCheckDuration = 1.0f;

    private IPhysicsModel physicsModel;
    private DataRecorder dataRecorder;
    private SimulationState currentState = SimulationState.Idle;
    private float elapsedTime, stabilityTimer, settlingTime;
    private bool isStabilityAchieved;

    public SimulationState CurrentState => currentState;
    public DataRecorder DataRecorder => dataRecorder;
    public IPhysicsModel PhysicsModel => physicsModel;
    public float ElapsedTime => elapsedTime;

    public UnityEvent OnSimulationStarted, OnSimulationStopped;
    public UnityEvent<float> OnSimulationCompleted, OnTimeUpdated;

    private void Awake()
    {
        // Singleton パターン
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[SimulationManager] Multiple instances detected. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        dataRecorder = new DataRecorder(0.05f, 2000);
    }

    private void Start()
    {
        // SceneController が存在するか確認
        if (SceneController.Instance == null)
        {
            Debug.LogError("[SimulationManager] SceneController not found! " +
                "Please load SimulationScene from MainMenuScene, not directly from the editor.");
            currentState = SimulationState.Stopped;
            return;
        }

        // ModelManager が存在するか確認
        if (ModelManager.Instance == null)
        {
            Debug.LogError("[SimulationManager] ModelManager not found! " +
                "Please ensure ModelManager is initialized before SimulationManager.");
            currentState = SimulationState.Stopped;
            return;
        }

        try
        {
            // SceneController からモデル名を取得
            string modelName = SceneController.Instance.SelectedModelName;

            if (string.IsNullOrEmpty(modelName))
            {
                Debug.LogError("[SimulationManager] SelectedModelName is empty!");
                currentState = SimulationState.Stopped;
                return;
            }

            Debug.Log($"[SimulationManager] Loading model: '{modelName}'");

            // ModelManager からモデルをロード
            physicsModel = ModelManager.Instance.GetModel(modelName);

            if (physicsModel == null)
            {
                Debug.LogError($"[SimulationManager] Failed to load model: {modelName}");
                Debug.Log($"[SimulationManager] Available models: " +
                    string.Join(", ", ModelManager.Instance.GetAvailableModelNames()));
                currentState = SimulationState.Stopped;
                return;
            }

            // PID パラメータを取得
            var p = SceneController.Instance.PendingPIDParams;
            Debug.Log($"[SimulationManager] Initializing with PID params: Kp={p.Kp}, Ki={p.Ki}, Kd={p.Kd}");

            // シミュレーションを初期化
            Initialize(physicsModel, p.Kp, p.Ki, p.Kd, p.TargetValue, p.Mass);

            // シミュレーションを開始
            StartSimulation();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SimulationManager] Exception during initialization: {ex}");
            currentState = SimulationState.Stopped;
        }
    }

    /// <summary>
    /// シミュレーションを初期化
    /// </summary>
    public void Initialize(IPhysicsModel model, float kp, float ki, float kd, float target, float mass)
    {
        physicsModel = model;
        pidController.SetGains(kp, ki, kd);
        pidController.Reset();
        physicsModel.SetTargetValue(target);
        physicsModel.SetMass(mass);
        physicsModel.Reset();
        dataRecorder.Clear();
        elapsedTime = 0;
        stabilityTimer = 0;
        isStabilityAchieved = false;
        currentState = SimulationState.Idle;

        Debug.Log($"[SimulationManager] Initialized: Model={physicsModel.GetModelName()}, " +
                  $"Target={target}, Mass={mass}");
    }

    /// <summary>
    /// シミュレーションを開始
    /// </summary>
    public void StartSimulation()
    {
        currentState = SimulationState.Running;
        OnSimulationStarted?.Invoke();
        Debug.Log("[SimulationManager] Simulation started");
    }

    /// <summary>
    /// シミュレーションを停止
    /// </summary>
    public void StopSimulation()
    {
        currentState = SimulationState.Stopped;
        OnSimulationStopped?.Invoke();
        Debug.Log("[SimulationManager] Simulation stopped");
    }

    private void FixedUpdate()
    {
        if (currentState != SimulationState.Running || physicsModel == null) return;

        elapsedTime += Time.fixedDeltaTime;
        OnTimeUpdated?.Invoke(elapsedTime);

        float current = physicsModel.GetCurrentValue();
        float target = physicsModel.GetTargetValue();
        float input = pidController.Update(current, target, Time.fixedDeltaTime);
        physicsModel.SetControlInput(input);

        dataRecorder.Record(elapsedTime, current, target, pidController.GetDebugInfo());
        CheckStability(current, target);

        if (elapsedTime >= maxSimulationTime)
        {
            Debug.Log($"[SimulationManager] Max simulation time reached: {elapsedTime:F2}s");
            CompleteSimulation();
        }
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
                Debug.Log($"[SimulationManager] Stability achieved at {settlingTime:F2}s");
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

        var result = PerformanceEvaluator.Evaluate(dataRecorder, settlingTime, stabilityTolerance);

        Debug.Log($"[SimulationManager] Simulation completed. Score: {result.Score:F0}, SettlingTime: {result.SettlingTime:F2}s");

        RankingManager.SaveScore(
            physicsModel.GetModelName(),
            result.Score,
            pidController.Kp, pidController.Ki, pidController.Kd
        );

        OnSimulationCompleted?.Invoke(settlingTime);

        if (SceneController.Instance != null)
            SceneController.Instance.LoadResult(result);
    }

    private void OnDestroy()
    {
        // モデルをアンロード
        if (ModelManager.Instance != null)
            ModelManager.Instance.UnloadCurrentModel();
    }
}

public enum SimulationState { Idle, Running, Paused, Stopped, Completed }

using System.Collections.Generic;

namespace PIDSimulator
{
    /// <summary>
    /// シミュレーションの状態を定義する列挙型
    /// </summary>
    public enum SimulationState
    {
        Idle,       // 待機中
        Running,    // 実行中
        Paused,     // 一時停止中
        Completed,  // 完了
        Stopped     // 強制停止
    }

    /// <summary>
    /// PIDパラメータを保持する構造体
    /// </summary>
    [System.Serializable]
    public struct PIDParameters
    {
        public float Kp, Ki, Kd, TargetValue, Mass;
    }

    /// <summary>
    /// シミュレーション結果を保持するデータ構造
    /// </summary>
    [System.Serializable]
    public class SimulationResult
    {
        public float Score;
        public float SettlingTime;
        public float Overshoot;
        public bool IsSuccess;
        public List<float> RawCurrentData;
        public List<float> RawTargetData;
        public List<float> RawTimeData;
        public PIDDataTracker DataTracker; // 追加
    }

    /// <summary>
    /// ランキングの1エントリーを保持するデータ構造
    /// </summary>
    [System.Serializable]
    public class RankingEntry
    {
        public string ModelName;
        public float Score;
        public float Kp, Ki, Kd;
        public string Date;
    }
}

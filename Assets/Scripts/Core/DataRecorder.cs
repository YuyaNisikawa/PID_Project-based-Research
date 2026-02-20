using UnityEngine;
using System.Collections.Generic;

namespace PIDSimulator
{
    /// <summary>
    /// シミュレーションデータを記録する純粋なC#クラス。
    /// [System.Serializable]を削除することで、Unityのシリアル化システム（シーンやプレハブへの保存）から
    /// 完全に隔離し、ExtensionOfNativeClassエラーを根絶します。
    /// </summary>
    public class SimulationDataRecorder
    {
        public List<float> TimeData { get; private set; }
        public List<float> CurrentValueData { get; private set; }
        public List<float> TargetValueData { get; private set; }
        public List<float> InputData { get; private set; } // グラフ表示用に制御入力も追加
        public List<string> DebugInfoData { get; private set; }

        private float sampleInterval;
        private int maxSamples;
        private float lastSampleTime;

        public SimulationDataRecorder(float interval, int max)
        {
            sampleInterval = interval;
            maxSamples = max;
            TimeData = new List<float>();
            CurrentValueData = new List<float>();
            TargetValueData = new List<float>();
            InputData = new List<float>();
            DebugInfoData = new List<string>();
        }

        public void Record(float time, float current, float target, float input, string debug)
        {
            if (TimeData.Count > 0 && time - lastSampleTime < sampleInterval) return;

            TimeData.Add(time);
            CurrentValueData.Add(current);
            TargetValueData.Add(target);
            InputData.Add(input);
            DebugInfoData.Add(debug);

            if (TimeData.Count > maxSamples)
            {
                TimeData.RemoveAt(0);
                CurrentValueData.RemoveAt(0);
                TargetValueData.RemoveAt(0);
                InputData.RemoveAt(0);
                DebugInfoData.RemoveAt(0);
            }
            lastSampleTime = time;
        }

        public void Clear()
        {
            TimeData.Clear();
            CurrentValueData.Clear();
            TargetValueData.Clear();
            InputData.Clear();
            DebugInfoData.Clear();
            lastSampleTime = 0;
        }
    }
}

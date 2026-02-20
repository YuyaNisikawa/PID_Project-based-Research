using UnityEngine;
using System.Collections.Generic;

namespace PIDSimulator
{
    /// <summary>
    /// クラス名を完全に刷新(PIDDataTracker)し、Unityの古いシリアル化参照(DataRecorder)を完全に断ち切ります。
    /// これによりExtensionOfNativeClassエラーを根絶します。
    /// </summary>
    public class PIDDataTracker
    {
        public List<float> TimeData { get; private set; }
        public List<float> CurrentValueData { get; private set; }
        public List<float> TargetValueData { get; private set; }
        public List<float> InputData { get; private set; }
        public List<string> DebugInfoData { get; private set; }

        private float sampleInterval;
        private int maxSamples;
        private float lastSampleTime;

        public PIDDataTracker(float interval, int max)
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

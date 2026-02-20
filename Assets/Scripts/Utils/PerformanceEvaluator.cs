using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PIDSimulator
{
    public class PerformanceEvaluator
    {
        public static SimulationResult Evaluate(PIDDataTracker tracker, float settlingTime, float tolerance)
        {
            var data = tracker.CurrentValueData;
            var targetData = tracker.TargetValueData;
            
            if (data.Count == 0) return new SimulationResult();

            float target = targetData.Last();
            float maxVal = data.Max();
            float minVal = data.Min();
            
            float overshoot = 0;
            if (Mathf.Abs(target) > 0.01f)
            {
                float peak = (target > 0) ? maxVal : minVal;
                overshoot = Mathf.Max(0, (Mathf.Abs(peak - target) / Mathf.Abs(target)));
            }

            float timeScore = Mathf.Max(0, 100 * (1 - settlingTime / 30f));
            float overshootScore = Mathf.Max(0, 100 * (1 - overshoot));
            float finalScore = (timeScore * 0.6f) + (overshootScore * 0.4f);

            return new SimulationResult
            {
                Score = finalScore,
                SettlingTime = settlingTime,
                Overshoot = overshoot,
                IsSuccess = settlingTime < 30f,
                RawCurrentData = new List<float>(data),
                RawTargetData = new List<float>(targetData),
                RawTimeData = new List<float>(tracker.TimeData),
                DataTracker = tracker // PIDDataTrackerを割り当てる
            };
        }
    }
}

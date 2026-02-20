using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PIDSimulator
{
    public class RankingManager : MonoBehaviour
    {
        public static RankingManager Instance { get; private set; }
        private const string PREFIX = "Ranking_";
        
        [Serializable] 
        private class ListWrapper { public List<RankingEntry> entries = new List<RankingEntry>(); }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static void SaveScore(string model, float score, float p, float i, float d)
        {
            var list = GetRanking(model);
            list.entries.Add(new RankingEntry 
            { 
                ModelName = model, 
                Score = score, 
                Kp = p, Ki = i, Kd = d,
                Date = DateTime.Now.ToString("yyyy/MM/dd") 
            });
            
            list.entries = list.entries.OrderByDescending(e => e.Score).Take(5).ToList();
            
            PlayerPrefs.SetString(PREFIX + model, JsonUtility.ToJson(list));
            PlayerPrefs.Save();
        }

        public static List<RankingEntry> GetRankingEntries(string model) => GetRanking(model).entries;

        private static ListWrapper GetRanking(string model)
        {
            string json = PlayerPrefs.GetString(PREFIX + model, "{}");
            var wrapper = JsonUtility.FromJson<ListWrapper>(json);
            return wrapper ?? new ListWrapper();
        }
    }
}

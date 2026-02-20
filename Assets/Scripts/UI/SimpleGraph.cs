using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace PIDSimulator
{
    public class SimpleGraph : MonoBehaviour
    {
        [SerializeField] private RawImage graphImage;
        [SerializeField] private Color lineColor = Color.blue;
        [SerializeField] private Color targetColor = Color.red;
        [SerializeField] private int textureWidth = 512;
        [SerializeField] private int textureHeight = 256;

        private Texture2D texture;
        private Color[] clearColors;
        private List<float> currentHistory = new List<float>();
        private List<float> targetHistory = new List<float>();
        private int maxPoints = 100;
        private bool isInitialized = false;

        private void Awake()
        {
            InitializeTexture();
        }

        private void InitializeTexture()
        {
            if (isInitialized) return;
            
            if (graphImage == null) graphImage = GetComponent<RawImage>();
            
            if (graphImage == null)
            {
                Debug.LogError("[SimpleGraph] RawImage component not found!");
                return;
            }

            texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            graphImage.texture = texture;
            
            clearColors = new Color[textureWidth * textureHeight];
            for (int i = 0; i < clearColors.Length; i++) 
                clearColors[i] = Color.black;
            
            isInitialized = true;
            ClearGraph();
        }

        public void ClearGraph()
        {
            if (!isInitialized) InitializeTexture();
            
            currentHistory.Clear();
            targetHistory.Clear();
            
            if (texture != null)
            {
                texture.SetPixels(clearColors);
                texture.Apply();
            }
        }

        public void UpdateGraph(float current, float target)
        {
            if (!isInitialized) InitializeTexture();
            
            currentHistory.Add(current);
            targetHistory.Add(target);

            if (currentHistory.Count > maxPoints)
            {
                currentHistory.RemoveAt(0);
                targetHistory.RemoveAt(0);
            }

            DrawGraph(currentHistory, targetHistory, maxPoints);
        }

        // 引数を PIDDataTracker に対応
        public void ShowFinalGraph(PIDDataTracker tracker)
        {
            if (!isInitialized) InitializeTexture();
            
            if (tracker == null || tracker.CurrentValueData.Count < 2)
            {
                Debug.LogWarning("[SimpleGraph] Invalid tracker data for final graph");
                return;
            }
            
            DrawGraph(tracker.CurrentValueData, tracker.TargetValueData, tracker.CurrentValueData.Count);
        }

        private void DrawGraph(List<float> currentList, List<float> targetList, int totalPoints)
        {
            if (texture == null) return;

            texture.SetPixels(clearColors);

            if (currentList.Count < 2) 
            {
                texture.Apply();
                return;
            }

            float min = Mathf.Min(currentList.Min(), targetList.Min(), -1f);
            float max = Mathf.Max(currentList.Max(), targetList.Max(), 1f);
            float range = Mathf.Max(0.1f, max - min);

            for (int i = 0; i < currentList.Count - 1; i++)
            {
                int x1 = (int)(i * (float)textureWidth / totalPoints);
                int x2 = (int)((i + 1) * (float)textureWidth / totalPoints);

                int y1_curr = (int)(((currentList[i] - min) / range) * (textureHeight - 1));
                int y2_curr = (int)(((currentList[i + 1] - min) / range) * (textureHeight - 1));
                DrawLine(x1, y1_curr, x2, y2_curr, lineColor);

                int y1_targ = (int)(((targetList[i] - min) / range) * (textureHeight - 1));
                int y2_targ = (int)(((targetList[i + 1] - min) / range) * (textureHeight - 1));
                DrawLine(x1, y1_targ, x2, y2_targ, targetColor);
            }

            texture.Apply();
        }

        private void DrawLine(int x1, int y1, int x2, int y2, Color color)
        {
            int dx = Mathf.Abs(x2 - x1);
            int dy = Mathf.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (x1 >= 0 && x1 < textureWidth && y1 >= 0 && y1 < textureHeight)
                    texture.SetPixel(x1, y1, color);

                if (x1 == x2 && y1 == y2) break;
                
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x1 += sx; }
                if (e2 < dx) { err += dx; y1 += sy; }
            }
        }

        private void OnDestroy()
        {
            if (texture != null)
            {
                Destroy(texture);
            }
        }
    }
}

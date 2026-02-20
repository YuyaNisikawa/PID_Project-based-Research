using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace PIDSimulator
{
    public class PrefabCreator : MonoBehaviour
    {
        private const string MANAGERS_UI_PATH = "Assets/Resources/Prefabs";
        private const string MODELS_PATH = "Assets/Resources/Models";

        [MenuItem("PID Simulator/Create Prefabs/Create All Prefabs")]
        public static void CreateAllPrefabs()
        {
            CreateInvertedPendulumPrefab();
            CreateHelicopterPrefab();
            CreateCranePrefab();
            CreateSimulationManagerPrefab();
            CreateModelManagerPrefab();
            
            CreateSimulationUIPrefab();
            CreateMainMenuUIPrefab();
            CreateParameterSettingUIPrefab();
            CreateResultUIPrefab();
            CreateRankingUIPrefab();
            
            AssetDatabase.Refresh();
            Debug.Log("All prefabs (Models, Managers, UIs) created successfully.");
        }

       public static void CreateInvertedPendulumPrefab()
        {
            GameObject root = new GameObject("InvertedPendulum");
            var modelScript = root.AddComponent<InvertedPendulumModel>();
            
            // 1. 支柱 (Stand)
            GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stand.name = "Stand";
            stand.transform.SetParent(root.transform);
            stand.transform.localPosition = new Vector3(0, 2.5f, 0); 
            stand.transform.localScale = new Vector3(0.5f, 5f, 0.2f); 
            Rigidbody standRb = stand.AddComponent<Rigidbody>();
            standRb.isKinematic = true;
            // ★修正3: 支柱のコライダー(当たり判定)を完全に消去し、物理的な引っかかりの可能性を0にする
            DestroyImmediate(stand.GetComponent<Collider>()); 

            // 2. ピン（Pin）
            GameObject pin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pin.name = "Pin";
            pin.transform.SetParent(root.transform);
            pin.transform.localPosition = new Vector3(0, 4.5f, 0.4f); 
            pin.transform.localRotation = Quaternion.Euler(90f, 0, 0); 
            pin.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f); 
            DestroyImmediate(pin.GetComponent<Collider>()); 

            // 3. アーム (Pendulum Arm)
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "Arm";
            arm.transform.SetParent(root.transform);
            arm.transform.localPosition = new Vector3(0, 2.9f, 0.8f); 
            arm.transform.localScale = new Vector3(0.3f, 4f, 0.3f);
            Rigidbody armRb = arm.AddComponent<Rigidbody>();
            armRb.mass = 1f;
            armRb.linearDamping = 0.1f;
            armRb.angularDamping = 0.1f;
            
            // 4. ヒンジジョイント (Hinge Joint)
            HingeJoint joint = arm.AddComponent<HingeJoint>();
            joint.connectedBody = standRb;
            // ★修正4: ジョイント同士の内部的な衝突判定も明示的にオフにする
            joint.enableCollision = false; 
            
            joint.anchor = new Vector3(0, 0.4f, 0); 
            joint.axis = new Vector3(0, 0, 1);
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = new Vector3(0, 0.4f, 4.0f);
            
            SerializedObject so = new SerializedObject(modelScript);
            so.FindProperty("poleRigidbody").objectReferenceValue = armRb;
            so.FindProperty("hingeJoint").objectReferenceValue = joint;
            so.ApplyModifiedProperties();
            
            SavePrefab(root, $"{MODELS_PATH}/Inverted Pendulum.prefab");
        }
        // Helicopter と Crane は前回と同じため省略せずに記述してください
        public static void CreateHelicopterPrefab()
        {
            GameObject root = new GameObject("Helicopter");
            root.AddComponent<HelicopterModel>();
            Rigidbody rootRb = root.AddComponent<Rigidbody>();
            rootRb.isKinematic = false;
            
            GameObject bodyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bodyObj.name = "Body";
            bodyObj.transform.SetParent(root.transform);
            bodyObj.transform.localPosition = Vector3.zero;
            bodyObj.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);
            Rigidbody bodyRb = bodyObj.AddComponent<Rigidbody>();
            bodyRb.isKinematic = false;
            bodyRb.mass = 2f;
            
            GameObject propellerObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            propellerObj.name = "Propeller";
            propellerObj.transform.SetParent(root.transform);
            propellerObj.transform.localPosition = new Vector3(0, 0.5f, 0);
            propellerObj.transform.localScale = new Vector3(1f, 0.05f, 1f);
            Rigidbody propellerRb = propellerObj.AddComponent<Rigidbody>();
            propellerRb.isKinematic = true;
            
            SavePrefab(root, $"{MODELS_PATH}/Helicopter.prefab");
        }

        public static void CreateCranePrefab()
        {
            GameObject root = new GameObject("Crane");
            root.AddComponent<CraneModel>();
            Rigidbody rootRb = root.AddComponent<Rigidbody>();
            rootRb.isKinematic = false;
            
            GameObject armObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armObj.name = "Arm";
            armObj.transform.SetParent(root.transform);
            armObj.transform.localPosition = new Vector3(0, 0.5f, 0);
            armObj.transform.localScale = new Vector3(0.2f, 0.2f, 2f);
            Rigidbody armRb = armObj.AddComponent<Rigidbody>();
            armRb.isKinematic = true;
            
            GameObject hookObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hookObj.name = "Hook";
            hookObj.transform.SetParent(root.transform);
            hookObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            hookObj.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            Rigidbody hookRb = hookObj.AddComponent<Rigidbody>();
            hookRb.isKinematic = false;
            hookRb.mass = 1f;
            
            GameObject cableObj = new GameObject("Cable");
            cableObj.transform.SetParent(root.transform);
            cableObj.transform.localPosition = Vector3.zero;
            LineRenderer lineRenderer = cableObj.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, armObj.transform.position);
            lineRenderer.SetPosition(1, hookObj.transform.position);
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            
            SavePrefab(root, $"{MODELS_PATH}/Crane.prefab");
        }

        // --- Managers ---
        public static void CreateSimulationManagerPrefab()
        {
            GameObject root = new GameObject("SimulationManager");
            root.AddComponent<SimulationManager>();
            root.AddComponent<PIDController>();
            SavePrefab(root, $"{MANAGERS_UI_PATH}/Managers/SimulationManager.prefab");
        }

        public static void CreateModelManagerPrefab()
        {
            GameObject root = new GameObject("ModelManager");
            root.AddComponent<ModelManager>();
            SavePrefab(root, $"{MANAGERS_UI_PATH}/Managers/ModelManager.prefab");
        }

        // --- UIs ---
        public static void CreateMainMenuUIPrefab()
        {
            GameObject canvasRoot = CreateCanvasRoot("MainMenuCanvas");
            GameObject uiRoot = new GameObject("MainMenuUI");
            uiRoot.transform.SetParent(canvasRoot.transform, false);
            MainMenuUI ui = uiRoot.AddComponent<MainMenuUI>();

            CreateTMPText(uiRoot.transform, "Title", "PID Simulator", new Vector2(0, 150)).fontSize = 36;
            Button btn1 = CreateButton(uiRoot.transform, "BtnPendulum", "Swing-up Pendulum", new Vector2(0, 50));
            Button btn2 = CreateButton(uiRoot.transform, "BtnHelicopter", "Helicopter", new Vector2(0, 0));
            Button btn3 = CreateButton(uiRoot.transform, "BtnCrane", "Crane", new Vector2(0, -50));
            Button btn4 = CreateButton(uiRoot.transform, "BtnRanking", "Ranking", new Vector2(0, -120));

            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("invertedPendulumButton").objectReferenceValue = btn1;
            so.FindProperty("helicopterButton").objectReferenceValue = btn2;
            so.FindProperty("craneButton").objectReferenceValue = btn3;
            so.FindProperty("rankingButton").objectReferenceValue = btn4;
            so.ApplyModifiedProperties();

            SavePrefab(canvasRoot, $"{MANAGERS_UI_PATH}/UI/MainMenuUI.prefab");
        }

        public static void CreateParameterSettingUIPrefab()
        {
            GameObject canvasRoot = CreateCanvasRoot("ParameterSettingCanvas");
            GameObject uiRoot = new GameObject("ParameterSettingUI");
            uiRoot.transform.SetParent(canvasRoot.transform, false);
            ParameterSettingUI ui = uiRoot.AddComponent<ParameterSettingUI>();

            CreateTMPText(uiRoot.transform, "Title", "Set Parameters", new Vector2(0, 150));
            
            TMP_InputField kp = CreateTMPInputField(uiRoot.transform, "InputKp", "2.0", new Vector2(-100, 50));
            TMP_InputField ki = CreateTMPInputField(uiRoot.transform, "InputKi", "0.0", new Vector2(0, 50));
            TMP_InputField kd = CreateTMPInputField(uiRoot.transform, "InputKd", "1.0", new Vector2(100, 50));
            TMP_InputField tgt = CreateTMPInputField(uiRoot.transform, "InputTarget", "90.0", new Vector2(-50, -20));
            TMP_InputField mass = CreateTMPInputField(uiRoot.transform, "InputMass", "1.0", new Vector2(50, -20));
            
            Button startBtn = CreateButton(uiRoot.transform, "StartButton", "Start Simulation", new Vector2(0, -100));

            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("kpInput").objectReferenceValue = kp;
            so.FindProperty("kiInput").objectReferenceValue = ki;
            so.FindProperty("kdInput").objectReferenceValue = kd;
            so.FindProperty("targetInput").objectReferenceValue = tgt;
            so.FindProperty("massInput").objectReferenceValue = mass;
            so.FindProperty("startButton").objectReferenceValue = startBtn;
            so.ApplyModifiedProperties();

            SavePrefab(canvasRoot, $"{MANAGERS_UI_PATH}/UI/ParameterSettingUI.prefab");
        }

        public static void CreateSimulationUIPrefab()
        {
            GameObject canvasRoot = CreateCanvasRoot("SimulationCanvas");
            GameObject uiRoot = new GameObject("SimulationUI");
            uiRoot.transform.SetParent(canvasRoot.transform, false);
            SimulationUI ui = uiRoot.AddComponent<SimulationUI>();

            TMP_Text status = CreateTMPText(uiRoot.transform, "StatusText", "Status: Idle", new Vector2(0, 100));
            TMP_Text time = CreateTMPText(uiRoot.transform, "TimeText", "Time: 0.00s", new Vector2(0, 70));
            TMP_Text model = CreateTMPText(uiRoot.transform, "ModelNameText", "Model: None", new Vector2(0, 130));
            Button stopBtn = CreateButton(uiRoot.transform, "StopButton", "Stop", new Vector2(0, -50));
            
            GameObject graphObj = new GameObject("Graph");
            graphObj.transform.SetParent(uiRoot.transform, false);
            graphObj.AddComponent<RawImage>();
            SimpleGraph graph = graphObj.AddComponent<SimpleGraph>();
            graphObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);
            graphObj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 200);

            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("statusText").objectReferenceValue = status;
            so.FindProperty("timeText").objectReferenceValue = time;
            so.FindProperty("modelNameText").objectReferenceValue = model;
            so.FindProperty("stopButton").objectReferenceValue = stopBtn;
            so.FindProperty("graph").objectReferenceValue = graph;
            so.ApplyModifiedProperties();

            SavePrefab(canvasRoot, $"{MANAGERS_UI_PATH}/UI/SimulationUI.prefab");
        }

        public static void CreateResultUIPrefab()
        {
            GameObject canvasRoot = CreateCanvasRoot("ResultCanvas");
            GameObject uiRoot = new GameObject("ResultUI");
            uiRoot.transform.SetParent(canvasRoot.transform, false);
            ResultUI ui = uiRoot.AddComponent<ResultUI>();

            TMP_Text score = CreateTMPText(uiRoot.transform, "ScoreText", "Score: 0", new Vector2(0, 150));
            TMP_Text time = CreateTMPText(uiRoot.transform, "TimeText", "Time: 0s", new Vector2(0, 120));
            TMP_Text over = CreateTMPText(uiRoot.transform, "OvershootText", "Overshoot: 0%", new Vector2(0, 90));
            TMP_Text eval = CreateTMPText(uiRoot.transform, "EvalText", "Evaluation: None", new Vector2(0, 60));

            Button retryBtn = CreateButton(uiRoot.transform, "RetryBtn", "Retry", new Vector2(-100, -250));
            Button menuBtn = CreateButton(uiRoot.transform, "MenuBtn", "Main Menu", new Vector2(0, -250));
            Button rankBtn = CreateButton(uiRoot.transform, "RankBtn", "Ranking", new Vector2(100, -250));

            GameObject graphObj = new GameObject("FinalGraph");
            graphObj.transform.SetParent(uiRoot.transform, false);
            graphObj.AddComponent<RawImage>();
            SimpleGraph graph = graphObj.AddComponent<SimpleGraph>();
            graphObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -80);
            graphObj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 200);

            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("scoreText").objectReferenceValue = score;
            so.FindProperty("timeText").objectReferenceValue = time;
            so.FindProperty("overshootText").objectReferenceValue = over;
            so.FindProperty("evaluationText").objectReferenceValue = eval;
            so.FindProperty("retryButton").objectReferenceValue = retryBtn;
            so.FindProperty("menuButton").objectReferenceValue = menuBtn;
            so.FindProperty("rankingButton").objectReferenceValue = rankBtn;
            so.FindProperty("finalGraph").objectReferenceValue = graph;
            so.ApplyModifiedProperties();

            SavePrefab(canvasRoot, $"{MANAGERS_UI_PATH}/UI/ResultUI.prefab");
        }

        public static void CreateRankingUIPrefab()
        {
            GameObject canvasRoot = CreateCanvasRoot("RankingCanvas");
            GameObject uiRoot = new GameObject("RankingUI");
            uiRoot.transform.SetParent(canvasRoot.transform, false);
            RankingUI ui = uiRoot.AddComponent<RankingUI>();

            TMP_Text rankText = CreateTMPText(uiRoot.transform, "RankingText", "Ranking Data...", new Vector2(0, 50));
            rankText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 300);
            Button backBtn = CreateButton(uiRoot.transform, "BackBtn", "Back to Menu", new Vector2(0, -150));

            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("rankingText").objectReferenceValue = rankText;
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.ApplyModifiedProperties();

            SavePrefab(canvasRoot, $"{MANAGERS_UI_PATH}/UI/RankingUI.prefab");
        }

        // --- Helpers ---
        private static GameObject CreateCanvasRoot(string name)
        {
            GameObject canvasRoot = new GameObject(name);
            Canvas canvas = canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasRoot.AddComponent<CanvasScaler>();
            canvasRoot.AddComponent<GraphicRaycaster>();
            return canvasRoot;
        }

        private static TMP_Text CreateTMPText(Transform parent, string name, string text, Vector2 pos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(300, 40);
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string text, Vector2 pos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>();
            Button btn = go.AddComponent<Button>();
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(250, 40);
            TMP_Text tmp = CreateTMPText(go.transform, "Text", text, Vector2.zero);
            tmp.color = Color.black;
            return btn;
        }

        private static TMP_InputField CreateTMPInputField(Transform parent, string name, string defaultText, Vector2 pos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = new Color(0.9f, 0.9f, 0.9f); 
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(80, 40);

            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(go.transform, false);
            RectTransform areaRect = textArea.AddComponent<RectTransform>();
            areaRect.anchorMin = Vector2.zero; areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(5, 5); areaRect.offsetMax = new Vector2(-5, -5);
            textArea.AddComponent<RectMask2D>();

            TMP_Text tmp = CreateTMPText(textArea.transform, "Text", defaultText, Vector2.zero);
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Left;
            RectTransform textRect = tmp.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero; textRect.offsetMax = Vector2.zero;

            TMP_InputField input = go.AddComponent<TMP_InputField>();
            input.textComponent = tmp;
            input.textViewport = areaRect;
            input.text = defaultText;

            return input;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
        }
    }
}
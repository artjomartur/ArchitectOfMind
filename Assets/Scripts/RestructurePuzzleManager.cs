using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RestructurePuzzleManager : MonoBehaviour
{
    public static RestructurePuzzleManager Instance { get; private set; }

    private GameObject puzzleCanvas;
    private Text tooltipText;
    private GameObject solveButton;
    private List<Image> darkBlocks = new List<Image>();
    private List<Image> coloredBlocks = new List<Image>();
    private int replacedCount = 0;

    private Color[] puzzleColors = new Color[] {
        new Color(0.8f, 0.2f, 0.2f), // Red
        new Color(0.2f, 0.7f, 0.2f), // Green
        new Color(0.1f, 0.5f, 0.8f)  // Blue
    };

    private string negativeThought = "KATASTROPHISIEREN:\nWenn ich die Klausur nicht schaffe, bin ich wertlos!";
    private string positiveThought = "GEDANKEN UMSTRUKTURIERT:\nEine schlechte Note ist ärgerlich, definiert aber nicht mein Können.";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (MindfulnessGameManager.Instance != null && MindfulnessGameManager.Instance.isDialogueRestructured)
        {
            Destroy(gameObject);
            return;
        }

        // Freeze player movement/interactions
        if (MindfulnessGameManager.Instance != null)
        {
            MindfulnessGameManager.Instance.isSlideNavigationActive = false; // Freeze slide
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        CreatePuzzleUI();
        UpdateProgress();
    }

    private void CreatePuzzleUI()
    {
        puzzleCanvas = new GameObject("RestructurePuzzleCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = puzzleCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 95;

        CanvasScaler scaler = puzzleCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 1. Translucent Background Panel
        GameObject bgPanel = CreateImage(puzzleCanvas.transform, "BGPanel", new Vector2(1920, 1080), Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0, 0, 0, 0.6f));
        RectTransform bgRT = bgPanel.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;

        // 2. Central Puzzle Card
        GameObject card = CreateImage(bgPanel.transform, "PuzzleCard", new Vector2(700, 750), Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.12f, 0.18f, 0.24f, 1f));

        // 3. Title Header
        GameObject titleGO = CreateText(card.transform, "GEDANKEN UMSTRUKTURIEREN", font, 28, Color.yellow);
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 0.88f);
        titleRT.anchorMax = new Vector2(1f, 0.98f);
        titleRT.sizeDelta = Vector2.zero;

        // 4. Puzzle Area (Grid representation of blocks)
        GameObject gridPanel = CreateImage(card.transform, "GridPanel", new Vector2(500, 200), new Vector2(0, 100), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.08f, 0.12f, 0.16f, 1f));

        // Spawn 3 dark/cracked blocks on the left of the grid
        for (int i = 0; i < 3; i++)
        {
            GameObject block = CreateImage(gridPanel.transform, $"DarkBlock_{i}", new Vector2(90, 90), new Vector2(-150 + i * 110, 0), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
            darkBlocks.Add(block.GetComponent<Image>());
            
            // Add a cracked look (simple cross text overlay)
            CreateText(block.transform, "X", font, 36, new Color(0.4f, 0.1f, 0.1f));
        }

        // Spawn 3 colored blocks slots on the right of the grid (initially deactivated/empty look)
        for (int i = 0; i < 3; i++)
        {
            GameObject block = CreateImage(gridPanel.transform, $"ColoredBlock_{i}", new Vector2(90, 90), new Vector2(50 + i * 110, 0), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.1f, 0.15f, 0.2f, 0.8f));
            coloredBlocks.Add(block.GetComponent<Image>());
            
            // Add slot indicator text
            CreateText(block.transform, "?", font, 24, new Color(0.4f, 0.4f, 0.4f));
        }

        // 5. Interactive Resource Blocks (Click to structure)
        GameObject selectorPanel = CreateImage(card.transform, "SelectorPanel", new Vector2(550, 120), new Vector2(0, -90), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.08f, 0.12f, 0.16f, 0.5f));
        
        string[] btnLabels = new string[] { "HOBBY", "SOCIAL", "MEDITATION" };
        for (int i = 0; i < 3; i++)
        {
            int index = i;
            GameObject btn = CreateButton(selectorPanel.transform, $"ResourceBtn_{i}", new Vector2(140, 80), new Vector2(-160 + i * 160, 0), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), puzzleColors[i]);
            CreateText(btn.transform, btnLabels[i], font, 20, Color.white);
            
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnBlockClicked(index);
            });
        }

        // 6. Tooltip Dialog Text Box
        GameObject tooltipPanel = CreateImage(card.transform, "TooltipPanel", new Vector2(600, 160), new Vector2(0, -250), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.08f, 0.12f, 0.16f, 0.8f));
        GameObject textGO = CreateText(tooltipPanel.transform, negativeThought, font, 20, Color.red);
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.offsetMin = new Vector2(15, 15);
        textRT.offsetMax = new Vector2(-15, -15);
        tooltipText = textGO.GetComponent<Text>();
        tooltipText.alignment = TextAnchor.MiddleCenter;

        // 7. Solve / Proceed Button
        solveButton = CreateButton(card.transform, "SolveButton", new Vector2(250, 70), new Vector2(0, -320), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.1f, 0.7f, 0.2f));
        CreateText(solveButton.transform, "UMSTRUKTURIEREN", font, 22, Color.white);
        solveButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            SolveAndClose();
        });
        solveButton.SetActive(false); // only show when complete
    }

    private void OnBlockClicked(int resourceIndex)
    {
        if (replacedCount >= 3) return;

        // Visual change: Replace the corresponding dark block with color, and activate empty slot
        darkBlocks[replacedCount].color = new Color(0.1f, 0.15f, 0.2f, 0.5f); // fade out dark block
        Text xText = darkBlocks[replacedCount].GetComponentInChildren<Text>();
        if (xText != null) xText.text = "";

        coloredBlocks[replacedCount].color = puzzleColors[resourceIndex]; // fill slot
        Text qText = coloredBlocks[replacedCount].GetComponentInChildren<Text>();
        if (qText != null) qText.text = "✔";

        replacedCount++;
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        if (replacedCount >= 3)
        {
            tooltipText.text = positiveThought;
            tooltipText.color = Color.green;
            if (solveButton != null) solveButton.SetActive(true);
        }
    }

    private void SolveAndClose()
    {
        // 1. Set progress state in MindfulnessGameManager
        var mgm = MindfulnessGameManager.Instance;
        if (mgm != null)
        {
            mgm.isDialogueRestructured = true;
            mgm.blockCount = Mathf.Min(mgm.blockCount + 1, mgm.maxBlocks);
            mgm.thoughtBalance = Mathf.Clamp(mgm.thoughtBalance + 0.3f, -1f, 1f);
        }

        // 2. Change world space monolith state
        GameObject monolith = GameObject.Find("DialogueMonolith");
        if (monolith != null)
        {
            // Turn monolith to white marble (supports nested prefab meshes)
            Renderer[] renderers = monolith.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                Material marbleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                marbleMat.color = new Color(0.9f, 0.9f, 0.9f); // light marble
                r.material = marbleMat;
            }

            // Change World Space text
            Transform canvasTrans = monolith.transform.Find("WorldSpaceCanvas");
            if (canvasTrans != null)
            {
                Text worldText = canvasTrans.GetComponentInChildren<Text>();
                if (worldText != null)
                {
                    worldText.text = positiveThought;
                    worldText.color = Color.green;
                }
            }

            // Destroy thorns attached to monolith
            // We placed thorns as children named "ThornVine_..."
            List<GameObject> thornsToDestroy = new List<GameObject>();
            for (int i = 0; i < monolith.transform.childCount; i++)
            {
                Transform child = monolith.transform.GetChild(i);
                if (child.name.StartsWith("Thorn"))
                {
                    thornsToDestroy.Add(child.gameObject);
                }
            }
            foreach (var t in thornsToDestroy) Destroy(t);
        }

        // 3. Reset player controller
        if (mgm != null)
        {
            mgm.SetSlideNavigation(true); // slide back to main navigation
        }

        // 4. Destroy Canvas & Manager
        Destroy(puzzleCanvas);
        Destroy(gameObject);
    }

    // --- HELPER CREATION METHODS ---
    private GameObject CreateImage(Transform parent, string name, Vector2 size, Vector2 anchoredPos, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        
        go.GetComponent<Image>().color = color;
        return go;
    }

    private GameObject CreateButton(Transform parent, string name, Vector2 size, Vector2 anchoredPos, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject go = CreateImage(parent, name, size, anchoredPos, anchorMin, anchorMax, color);
        go.AddComponent<Button>();
        return go;
    }

    private GameObject CreateText(Transform parent, string textStr, Font font, int fontSize, Color color)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        Text text = go.GetComponent<Text>();
        text.text = textStr;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false; // ensure text doesn't block raycast click

        return go;
    }
}


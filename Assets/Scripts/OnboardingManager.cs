using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OnboardingManager : MonoBehaviour
{
    public static OnboardingManager Instance { get; private set; }

    [Header("UI & Mascot Sprites")]
    public Sprite panelSprite;
    public Sprite buttonSprite;
    public Sprite foxMeditatingSprite;
    public Sprite foxHappySprite;

    private Image mascotImage;
    private int currentStep = 0;
    private int[] answers = new int[4];

    private GameObject onboardingCanvas;
    private Text questionText;
    private Text progressText;
    private List<GameObject> activeButtons = new List<GameObject>();

    private string[] questions = new string[] {
        "Wie hoch ist dein Stresslevel heute?",
        "Wie nimmst du deine Gedanken aktuell wahr?",
        "Welche Ressourcen möchtest du heute besonders stärken?",
        "Wie möchtest du deine mentale Gesundheit aufbauen?"
    };

    private string[][] options = new string[][] {
        new string[] { "Niedrig (Entspannt)", "Mittel (Etwas angespannt)", "Hoch (Sehr gestresst)" },
        new string[] { "Sehr negativ (Kritisch)", "Gemischt (Neutral)", "Sehr positiv (Optimistisch)" },
        new string[] { "Hobbys & Freizeit", "Soziale Kontakte", "Selbstfürsorge", "Familie & Freunde" },
        new string[] { "Eigenständig (Fokus auf mich)", "Gemeinsam (Mit Unterstützung)" }
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // If onboarding is already completed, we skip and destroy the onboarding system
        if (PlayerPrefs.GetInt("OnboardingCompleted", 0) == 1)
        {
            Destroy(gameObject);
            return;
        }

        // Programmatically create the Onboarding UI Canvas
        CreateOnboardingUI();

        // Show the first question
        ShowQuestion(0);
    }

    public void ResetOnboarding()
    {
        PlayerPrefs.SetInt("OnboardingCompleted", 0);
        PlayerPrefs.Save();
        Debug.Log("Onboarding reset! Please restart the scene or trigger setup again.");
    }

    private void CreateOnboardingUI()
    {
        // 1. Create Canvas
        onboardingCanvas = new GameObject("OnboardingCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = onboardingCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99; // Draw on top of everything

        CanvasScaler scaler = onboardingCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 2. Fullscreen Background Panel (translucent calming blue/grey)
        GameObject bgPanel = CreateImage(onboardingCanvas.transform, "BGPanel", new Vector2(1920, 1080), Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.12f, 0.18f, 0.24f, 0.98f));
        // Force stretch to screen
        RectTransform bgRT = bgPanel.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;

        // 3. Central card panel (widened for mascot)
        GameObject card = CreateImage(bgPanel.transform, "Card", new Vector2(1050, 750), Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Color.white, panelSprite);

        // Mascot Panel on the Left
        GameObject mascotBG = CreateImage(card.transform, "MascotBG", new Vector2(320, 320), new Vector2(-320, -20), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0f, 0f, 0f, 0.2f));
        GameObject mascotGO = CreateImage(mascotBG.transform, "MascotImage", new Vector2(300, 300), Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Color.white, foxMeditatingSprite);
        mascotImage = mascotGO.GetComponent<Image>();

        // 4. Header title (shifted right)
        GameObject header = CreateText(card.transform, "ARCHITECT OF MIND\n- Mentales Onboarding -", font, 36, Color.yellow);
        RectTransform headerRT = header.GetComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0.35f, 0.8f);
        headerRT.anchorMax = new Vector2(0.95f, 0.95f);
        headerRT.sizeDelta = Vector2.zero;

        // 5. Progress indicator (shifted right)
        GameObject progressGO = CreateText(card.transform, "Schritt 1 von 4", font, 22, new Color(0.8f, 0.8f, 0.8f));
        RectTransform progRT = progressGO.GetComponent<RectTransform>();
        progRT.anchorMin = new Vector2(0.35f, 0.72f);
        progRT.anchorMax = new Vector2(0.95f, 0.78f);
        progRT.sizeDelta = Vector2.zero;
        progressText = progressGO.GetComponent<Text>();

        // 6. Question Text (shifted right)
        GameObject questGO = CreateText(card.transform, "Frage hier...", font, 28, Color.white);
        RectTransform questRT = questGO.GetComponent<RectTransform>();
        questRT.anchorMin = new Vector2(0.35f, 0.5f);
        questRT.anchorMax = new Vector2(0.95f, 0.7f);
        questRT.sizeDelta = Vector2.zero;
        questionText = questGO.GetComponent<Text>();
        questionText.alignment = TextAnchor.MiddleCenter;
    }

    private void ShowQuestion(int step)
    {
        currentStep = step;
        progressText.text = $"Schritt {step + 1} von 4";
        questionText.text = questions[step];

        // Clean up previous buttons
        foreach (var btn in activeButtons)
        {
            Destroy(btn);
        }
        activeButtons.Clear();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Spawn option buttons vertically in the card
        string[] currentOptions = options[step];
        float buttonHeight = 70f;
        float spacing = 20f;
        float startY = 100f - (currentOptions.Length * (buttonHeight + spacing)) / 2f;

        GameObject card = onboardingCanvas.transform.Find("BGPanel/Card").gameObject;

        for (int i = 0; i < currentOptions.Length; i++)
        {
            int optionIndex = i;
            float yPos = startY + i * (buttonHeight + spacing);

            // Shift buttons right (X: 180) to keep card side-by-side with mascot
            GameObject button = CreateButton(card.transform, $"Option_{i}", new Vector2(550, buttonHeight), new Vector2(180f, yPos - 120f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Color.white, buttonSprite);
            CreateText(button.transform, currentOptions[i], font, 24, Color.white);
            
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnOptionSelected(optionIndex);
            });

            activeButtons.Add(button);
        }
    }

    private void OnOptionSelected(int index)
    {
        answers[currentStep] = index;
        Debug.Log($"Step {currentStep} answered: {index}");

        if (currentStep < 3)
        {
            ShowQuestion(currentStep + 1);
        }
        else
        {
            // Show happy fox mascot cheering upon completing onboarding questionnaire!
            if (mascotImage != null && foxHappySprite != null)
            {
                mascotImage.sprite = foxHappySprite;
            }
            
            // Wait 1.2 seconds before completing onboarding to let the player see the happy fox!
            Invoke("CompleteOnboarding", 1.2f);
        }
    }

    private void CompleteOnboarding()
    {
        // 1. Save onboarding completion state
        PlayerPrefs.SetInt("OnboardingCompleted", 1);
        PlayerPrefs.Save();
        Debug.Log("Onboarding Completed!");

        // 2. Configure baseline module states in MindfulnessGameManager based on answers
        if (MindfulnessGameManager.Instance != null)
        {
            // Stress answer (0: Low, 1: Medium, 2: High)
            int stressChoice = answers[0];
            if (stressChoice == 0)
            {
                MindfulnessGameManager.Instance.distractionSuccess = 0.8f;
                MindfulnessGameManager.Instance.treesPlanted = 2;
            }
            else if (stressChoice == 1)
            {
                MindfulnessGameManager.Instance.distractionSuccess = 0.5f;
                MindfulnessGameManager.Instance.treesPlanted = 0;
            }
            else
            {
                MindfulnessGameManager.Instance.distractionSuccess = 0.2f;
                MindfulnessGameManager.Instance.treesPlanted = 0;
            }

            // Dialogue answer (0: Very Negative, 1: Mixed, 2: Very Positive)
            int thoughtChoice = answers[1];
            if (thoughtChoice == 0) MindfulnessGameManager.Instance.thoughtBalance = -0.5f;
            else if (thoughtChoice == 1) MindfulnessGameManager.Instance.thoughtBalance = 0.0f;
            else MindfulnessGameManager.Instance.thoughtBalance = 0.5f;

            // Resource answer (0: Hobbies, 1: Social, 2: Self-care, 3: Family)
            int resourceChoice = answers[2];
            // Highlight starting platform
            MindfulnessGameManager.Instance.currentModuleIndex = resourceChoice;

            // Healing answer (0: Alone, 1: Support)
            int healingChoice = answers[3];
            MindfulnessGameManager.Instance.healingProgress = (healingChoice == 0) ? 50f : 80f;

            // Set slide navigation active and reset camera positions
            MindfulnessGameManager.Instance.SetSlideNavigation(true);
        }

        // 3. Clean up Canvas and Self
        Destroy(onboardingCanvas);
        Destroy(gameObject);
    }

    // --- HELPER CREATION METHODS ---
    private GameObject CreateImage(Transform parent, string name, Vector2 size, Vector2 anchoredPos, Vector2 anchorMin, Vector2 anchorMax, Color color, Sprite sprite = null)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        
        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = (sprite != null && color == Color.clear) ? Color.white : color;
        return go;
    }

    private GameObject CreateButton(Transform parent, string name, Vector2 size, Vector2 anchoredPos, Vector2 anchorMin, Vector2 anchorMax, Color color, Sprite sprite = null)
    {
        GameObject go = CreateImage(parent, name, size, anchoredPos, anchorMin, anchorMax, color, sprite);
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
        text.raycastTarget = false;

        return go;
    }
}


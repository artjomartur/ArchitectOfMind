using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MobileControlsManager : MonoBehaviour
{
    [Header("Sensitivity Settings")]
    public float mobileLookSensitivity = 0.05f;

    private FirstPersonController fpc;
    private MindArchitect mindArchitect;

    private GameObject canvasInstance;
    private VirtualJoystick activeJoystick;

    // UI Elements references
    private GameObject joystickBG;
    private GameObject actionButtonsGroup;
    private Text moduleTitleText;
    private Text statsText;
    private Text modeToggleText;
    private Text hudBlockCountText;

    private string[] moduleNames = new string[] {
        "ACHTSAMES WACHSEN\nSamen durch tägliche Dankbarkeit pflegen.",
        "INNERER DIALOG\nPositive innere Stimmen stärken.",
        "RESSOURCEN-PFAD\nSoziale Kontakte und Hobbys reaktivieren.",
        "STRESS-ABLENKUNG\nFokus auf Aufbau, Trigger ignorieren.",
        "GEMEINSAMES HEILEN\nGegenseitige Unterstützung im Koop-Modus."
    };

    void Start()
    {
        fpc = FindAnyObjectByType<FirstPersonController>();
        mindArchitect = FindAnyObjectByType<MindArchitect>();

        // Programmatically construct the Mobile UI Canvas
        CreateMobileUI();

        // Hide Mobile UI Canvas when running as a Standalone build on desktop (Windows, Mac, Linux)
#if (UNITY_STANDALONE || UNITY_WEBGL) && !UNITY_EDITOR
        if (canvasInstance != null)
        {
            canvasInstance.SetActive(false);
        }
#endif
    }

    void Update()
    {
        if (MindfulnessGameManager.Instance == null) return;

        // Slide look input handler
        UpdateLookInput();

        // Update UI states based on active navigation mode
        UpdateUIVisibility();

        // Update stats and texts dynamically
        UpdateUIData();
    }

    private void UpdateLookInput()
    {
        // Only allow touch-dragging camera looking when free walk mode is active
        if (MindfulnessGameManager.Instance.isSlideNavigationActive) return;

        if (Touchscreen.current != null && fpc != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.isInProgress)
                {
                    Vector2 startPos = touch.startPosition.ReadValue();
                    // Detect swipe if starting on the right half of the screen
                    if (startPos.x > Screen.width * 0.45f)
                    {
                        Vector2 delta = touch.delta.ReadValue();
                        fpc.SetMobileLook(delta * mobileLookSensitivity);
                    }
                }
            }
        }
    }

    private void UpdateUIVisibility()
    {
        bool onboardingActive = PlayerPrefs.GetInt("OnboardingCompleted", 0) == 0;
        
        if (onboardingActive)
        {
            if (canvasInstance != null && canvasInstance.activeSelf) canvasInstance.SetActive(false);
            return;
        }
        else
        {
            if (canvasInstance != null && !canvasInstance.activeSelf)
            {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
                canvasInstance.SetActive(true);
#endif
            }
        }

        bool slideActive = MindfulnessGameManager.Instance.isSlideNavigationActive;

        // Hide Joystick and Action Buttons in slide mode, show in Walk mode
        if (joystickBG != null) joystickBG.SetActive(!slideActive);
        if (actionButtonsGroup != null) actionButtonsGroup.SetActive(!slideActive);
    }

    private void UpdateUIData()
    {
        var mgm = MindfulnessGameManager.Instance;
        if (mgm == null) return;

        // Update block counter text
        if (hudBlockCountText != null)
        {
            hudBlockCountText.text = $"{mgm.blockCount}/{mgm.maxBlocks}";
        }

        // Update Title text
        if (moduleTitleText != null)
        {
            moduleTitleText.text = moduleNames[mgm.currentModuleIndex];
        }

        // Update Mode Toggle button text
        if (modeToggleText != null)
        {
            modeToggleText.text = mgm.isSlideNavigationActive ? "FREIER MODUS" : "DIASHOW MODUS";
        }

        // Update Stats text
        if (statsText != null)
        {
            switch (mgm.currentModuleIndex)
            {
                case 0: // Wachsen
                    statsText.text = $"Dankbarkeits-Zähler: {mgm.gratitudeCount} / {mgm.maxGratitude}\n" +
                                     $"Wachstum: {Mathf.RoundToInt(mgm.plantGrowth * 100f)}%\n\n" +
                                     "Tippe das Beet an, um zu gießen!";
                    break;
                case 1: // Dialog
                    float percentage = Mathf.RoundToInt((mgm.thoughtBalance + 1f) * 50f);
                    statsText.text = $"Gedanken-Wippe: {percentage}% Positiv\n\n" +
                                     "Tippe positive Gedanken (Grün) an!\n" +
                                     "Tippe negative Gedanken (Rot) an, um sie zu filtern!";
                    break;
                case 2: // Pfad
                    statsText.text = $"Kristalle gesammelt: {mgm.crystalsCollected} / {mgm.totalCrystals}\n\n" +
                                     "Tippe Memory-Kristalle zum Sammeln an!";
                    break;
                case 3: // Ablenkung
                    statsText.text = $"Gepflanzte Bäume: {mgm.treesPlanted}\n" +
                                     $"Schutzschild: {Mathf.RoundToInt(mgm.distractionSuccess * 100f)}%\n\n" +
                                     "Tippe das Beet an, um Bäume wachsen zu lassen!";
                    break;
                case 4: // Heilen
                    statsText.text = $"Heilungs-Fortschritt: {mgm.healingProgress}%\n\n" +
                                     "Tippe eingestürzte Säulen an, um sie aufzubauen!";
                    break;
            }
        }
    }

    private void CreateMobileUI()
    {
        // 1. Create Canvas
        canvasInstance = new GameObject("MobileControlsCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasInstance.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasInstance.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 2. Create Left Joystick
        CreateJoystick(canvasInstance.transform);

        // 3. Create Action Buttons Group
        CreateActionButtons(canvasInstance.transform, font);

        // 4. Create Slide Navigation Header
        CreateSlideHeader(canvasInstance.transform, font);

        // 5. Create Stats Overlay Panel
        CreateStatsOverlay(canvasInstance.transform, font);

        // 6. Reset Onboarding button (Visible in Editor/Standalone for testing)
#if UNITY_EDITOR || UNITY_STANDALONE
        GameObject resetBtn = CreateButton(canvasInstance.transform, "ResetOnboardingButton", new Vector2(240, 50), new Vector2(150, 50), new Vector2(0f, 0f), new Vector2(0f, 0f), new Color(0.6f, 0.2f, 0.2f, 0.8f));
        CreateText(resetBtn.transform, "Reset Onboarding", font, 18, Color.white);
        resetBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            PlayerPrefs.SetInt("OnboardingCompleted", 0);
            PlayerPrefs.Save();
            Debug.Log("Onboarding Reset!");
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        });
#endif
    }

    private void CreateJoystick(Transform parent)
    {
        joystickBG = CreateImage(parent, "JoystickBG", new Vector2(250, 250), new Vector2(200, 200), new Vector2(0, 0), new Vector2(0, 0), new Color(0, 0, 0, 0.3f));
        GameObject handleGO = CreateImage(joystickBG.transform, "JoystickHandle", new Vector2(100, 100), Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(1, 1, 1, 0.6f));

        activeJoystick = joystickBG.AddComponent<VirtualJoystick>();
        activeJoystick.background = joystickBG.GetComponent<RectTransform>();
        activeJoystick.handle = handleGO.GetComponent<RectTransform>();
        activeJoystick.onJoystickMoved = (moveInput) =>
        {
            if (fpc != null) fpc.SetMobileMove(moveInput);
        };
    }

    private void CreateActionButtons(Transform parent, Font font)
    {
        actionButtonsGroup = new GameObject("ActionButtonsGroup", typeof(RectTransform));
        actionButtonsGroup.transform.SetParent(parent, false);
        RectTransform rt = actionButtonsGroup.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(500, 500);

        // --- JUMP BUTTON ---
        GameObject jumpButton = CreateButton(actionButtonsGroup.transform, "JumpButton", new Vector2(140, 140), new Vector2(-150, 220), new Vector2(1, 0), new Vector2(1, 0), new Color(0.2f, 0.2f, 0.2f, 0.6f));
        CreateText(jumpButton.transform, "JUMP", font, 24, Color.white);
        jumpButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (fpc != null) fpc.TriggerJump();
        });

        // --- BUILD / SPAWN BUTTON ---
        GameObject buildButton = CreateButton(actionButtonsGroup.transform, "BuildButton", new Vector2(140, 140), new Vector2(-320, 220), new Vector2(1, 0), new Vector2(1, 0), new Color(0.1f, 0.7f, 0.1f, 0.6f));
        CreateText(buildButton.transform, "BUILD", font, 24, Color.white);
        buildButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (mindArchitect != null) mindArchitect.TriggerMobileSpawn();
        });

        // --- DELETE / DESTROY BUTTON ---
        GameObject destroyButton = CreateButton(actionButtonsGroup.transform, "DestroyButton", new Vector2(140, 140), new Vector2(-150, 390), new Vector2(1, 0), new Vector2(1, 0), new Color(0.7f, 0.1f, 0.1f, 0.6f));
        CreateText(destroyButton.transform, "DEL", font, 24, Color.white);
        destroyButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (mindArchitect != null) mindArchitect.TriggerMobileDestroy();
        });

        // --- GRAB / MIND HOLD BUTTON ---
        GameObject grabButton = CreateButton(actionButtonsGroup.transform, "GrabButton", new Vector2(140, 140), new Vector2(-320, 390), new Vector2(1, 0), new Vector2(1, 0), new Color(0.1f, 0.5f, 0.8f, 0.6f));
        CreateText(grabButton.transform, "MIND\nHOLD", font, 20, Color.white);
        
        HoldButton hold = grabButton.AddComponent<HoldButton>();
        hold.onStateChanged = (isHolding) =>
        {
            if (mindArchitect != null) mindArchitect.SetMobileGrab(isHolding);
        };
    }

    private void CreateSlideHeader(Transform parent, Font font)
    {
        // 1. Navigation Panel (Top Bar background)
        GameObject headerPanel = CreateImage(parent, "HeaderPanel", new Vector2(1800, 150), new Vector2(0, -100), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Color(0, 0, 0, 0.4f));

        // 2. Left Arrow Button
        GameObject leftArrow = CreateButton(headerPanel.transform, "LeftArrow", new Vector2(100, 100), new Vector2(80, 0), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Color(0.3f, 0.3f, 0.3f, 0.8f));
        CreateText(leftArrow.transform, "<", font, 36, Color.white);
        leftArrow.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (MindfulnessGameManager.Instance != null) MindfulnessGameManager.Instance.PrevModule();
        });

        // 3. Right Arrow Button
        GameObject rightArrow = CreateButton(headerPanel.transform, "RightArrow", new Vector2(100, 100), new Vector2(-80, 0), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Color(0.3f, 0.3f, 0.3f, 0.8f));
        CreateText(rightArrow.transform, ">", font, 36, Color.white);
        rightArrow.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (MindfulnessGameManager.Instance != null) MindfulnessGameManager.Instance.NextModule();
        });

        // 4. Module Title Text
        GameObject titleGO = new GameObject("ModuleTitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        titleGO.transform.SetParent(headerPanel.transform, false);
        RectTransform rt = titleGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.2f, 0f);
        rt.anchorMax = new Vector2(0.8f, 1f);
        rt.sizeDelta = Vector2.zero;

        moduleTitleText = titleGO.GetComponent<Text>();
        moduleTitleText.text = moduleNames[0];
        moduleTitleText.font = font;
        moduleTitleText.fontSize = 28;
        moduleTitleText.alignment = TextAnchor.MiddleCenter;
        moduleTitleText.color = Color.yellow;

        // 5. Mode Toggle Button
        GameObject toggleBtn = CreateButton(headerPanel.transform, "ModeToggleButton", new Vector2(250, 100), new Vector2(-220, 0), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Color(0.1f, 0.5f, 0.6f, 0.8f));
        modeToggleText = CreateText(toggleBtn.transform, "FREIER MODUS", font, 20, Color.white).GetComponent<Text>();
        toggleBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (MindfulnessGameManager.Instance != null)
            {
                bool active = MindfulnessGameManager.Instance.isSlideNavigationActive;
                MindfulnessGameManager.Instance.SetSlideNavigation(!active);
            }
        });

        // 6. Block Counter HUD Panel (matching concept sketch)
        GameObject blockHUD = CreateImage(parent, "BlockHUDPanel", new Vector2(180, 70), new Vector2(-110, -60), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Color(0.18f, 0.24f, 0.28f, 0.9f));
        GameObject blockTextGO = CreateText(blockHUD.transform, "📦 5/20", font, 24, Color.white);
        hudBlockCountText = blockTextGO.GetComponent<Text>();

        // 7. Energy / Leaf Badge (matching concept sketch)
        GameObject energyHUD = CreateImage(parent, "EnergyHUDPanel", new Vector2(180, 70), new Vector2(110, -60), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Color(0.12f, 0.45f, 0.35f, 0.9f));
        CreateText(energyHUD.transform, "⚡ 🍃", font, 28, Color.white);

        // 8. Settings Gear Button (matching concept sketch)
        GameObject settingsBtn = CreateButton(parent, "SettingsGearButton", new Vector2(80, 80), new Vector2(-80, -60), new Vector2(1f, 1f), new Vector2(1f, 1f), new Color(0.18f, 0.24f, 0.28f, 0.9f));
        CreateText(settingsBtn.transform, "⚙", font, 36, Color.white);
        settingsBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            Debug.Log("Settings opened!");
        });
    }

    private void CreateStatsOverlay(Transform parent, Font font)
    {
        GameObject statsPanel = CreateImage(parent, "StatsPanel", new Vector2(400, 350), new Vector2(250, -400), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0, 0, 0, 0.5f));
        
        GameObject statsGO = new GameObject("StatsText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        statsGO.transform.SetParent(statsPanel.transform, false);
        RectTransform rt = statsGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(20, 20);
        rt.offsetMax = new Vector2(-20, -20);

        statsText = statsGO.GetComponent<Text>();
        statsText.font = font;
        statsText.fontSize = 22;
        statsText.alignment = TextAnchor.UpperLeft;
        statsText.color = Color.white;
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
        text.raycastTarget = false;

        return go;
    }
}

// --- SUPPORT CLASSES FOR INTERACTION ---
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform background;
    public RectTransform handle;
    public System.Action<Vector2> onJoystickMoved;

    private Vector2 inputVector;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out pos))
        {
            float width = background.sizeDelta.x;
            float height = background.sizeDelta.y;

            pos.x = (pos.x / width) * 2f;
            pos.y = (pos.y / height) * 2f;

            inputVector = new Vector2(pos.x, pos.y);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            handle.anchoredPosition = new Vector2(inputVector.x * (width / 3f), inputVector.y * (height / 3f));
            
            onJoystickMoved?.Invoke(inputVector);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        onJoystickMoved?.Invoke(Vector2.zero);
    }
}

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public System.Action<bool> onStateChanged;

    public void OnPointerDown(PointerEventData eventData)
    {
        onStateChanged?.Invoke(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onStateChanged?.Invoke(false);
    }
}

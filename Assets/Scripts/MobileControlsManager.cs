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

    void Start()
    {
        fpc = FindAnyObjectByType<FirstPersonController>();
        mindArchitect = FindAnyObjectByType<MindArchitect>();

        // Programmatically construct the Mobile UI Canvas
        CreateMobileUI();
    }

    void Update()
    {
        UpdateLookInput();
    }

    private void UpdateLookInput()
    {
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

    private void CreateMobileUI()
    {
        // 1. Create Canvas
        canvasInstance = new GameObject("MobileControlsCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasInstance.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasInstance.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 2. Create Left Joystick
        CreateJoystick(canvasInstance.transform);

        // 3. Create Action Buttons
        CreateActionButtons(canvasInstance.transform);

        // 4. Create Color Palette
        CreateColorPalette(canvasInstance.transform);
    }

    private void CreateJoystick(Transform parent)
    {
        // Background Circle
        GameObject bgGO = CreateImage(parent, "JoystickBG", new Vector2(250, 250), new Vector2(200, 200), new Vector2(0, 0), new Vector2(0, 0), new Color(0, 0, 0, 0.3f));
        
        // Handle Circle
        GameObject handleGO = CreateImage(bgGO.transform, "JoystickHandle", new Vector2(100, 100), Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(1, 1, 1, 0.6f));

        // Add Virtual Joystick script
        activeJoystick = bgGO.AddComponent<VirtualJoystick>();
        activeJoystick.background = bgGO.GetComponent<RectTransform>();
        activeJoystick.handle = handleGO.GetComponent<RectTransform>();
        activeJoystick.onJoystickMoved = (moveInput) =>
        {
            if (fpc != null) fpc.SetMobileMove(moveInput);
        };
    }

    private void CreateActionButtons(Transform parent)
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // --- JUMP BUTTON ---
        GameObject jumpButton = CreateButton(parent, "JumpButton", new Vector2(140, 140), new Vector2(-150, 220), new Vector2(1, 0), new Vector2(1, 0), new Color(0.2f, 0.2f, 0.2f, 0.6f));
        CreateText(jumpButton.transform, "JUMP", font, 24, Color.white);
        jumpButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (fpc != null) fpc.TriggerJump();
        });

        // --- BUILD / SPAWN BUTTON ---
        GameObject buildButton = CreateButton(parent, "BuildButton", new Vector2(140, 140), new Vector2(-320, 220), new Vector2(1, 0), new Vector2(1, 0), new Color(0.1f, 0.7f, 0.1f, 0.6f));
        CreateText(buildButton.transform, "BUILD", font, 24, Color.white);
        buildButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (mindArchitect != null) mindArchitect.TriggerMobileSpawn();
        });

        // --- DELETE / DESTROY BUTTON ---
        GameObject destroyButton = CreateButton(parent, "DestroyButton", new Vector2(140, 140), new Vector2(-150, 390), new Vector2(1, 0), new Vector2(1, 0), new Color(0.7f, 0.1f, 0.1f, 0.6f));
        CreateText(destroyButton.transform, "DEL", font, 24, Color.white);
        destroyButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (mindArchitect != null) mindArchitect.TriggerMobileDestroy();
        });

        // --- GRAB / MIND HOLD BUTTON ---
        GameObject grabButton = CreateButton(parent, "GrabButton", new Vector2(140, 140), new Vector2(-320, 390), new Vector2(1, 0), new Vector2(1, 0), new Color(0.1f, 0.5f, 0.8f, 0.6f));
        CreateText(grabButton.transform, "MIND\nHOLD", font, 20, Color.white);
        
        // Use HoldButton script for E-like press-and-hold behavior
        HoldButton hold = grabButton.AddComponent<HoldButton>();
        hold.onStateChanged = (isHolding) =>
        {
            if (mindArchitect != null) mindArchitect.SetMobileGrab(isHolding);
        };
    }

    private void CreateColorPalette(Transform parent)
    {
        if (mindArchitect == null) return;

        Color[] colors = mindArchitect.GetBuildColors();
        float buttonSize = 80f;
        float spacing = 20f;
        float totalWidth = (buttonSize * colors.Length) + (spacing * (colors.Length - 1));
        float startX = -totalWidth / 2f + buttonSize / 2f;

        // Container panel for palette
        GameObject paletteContainer = new GameObject("ColorPalette", typeof(RectTransform));
        paletteContainer.transform.SetParent(parent, false);
        RectTransform rectTrans = paletteContainer.GetComponent<RectTransform>();
        rectTrans.anchorMin = new Vector2(0.5f, 0f);
        rectTrans.anchorMax = new Vector2(0.5f, 0f);
        rectTrans.anchoredPosition = new Vector2(0f, 80f);
        rectTrans.sizeDelta = new Vector2(totalWidth, buttonSize);

        for (int i = 0; i < colors.Length; i++)
        {
            int index = i;
            GameObject colorBtn = CreateButton(paletteContainer.transform, $"Color_{i}", new Vector2(buttonSize, buttonSize), new Vector2(startX + i * (buttonSize + spacing), 0f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), colors[i]);
            
            // Highlight/Border handling or simple click action
            colorBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                mindArchitect.SetColorIndex(index);
                Debug.Log($"Mobile color select: {index}");
            });
        }
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

            // Map local position to a range between -1 and 1
            pos.x = (pos.x / width) * 2f;
            pos.y = (pos.y / height) * 2f;

            inputVector = new Vector2(pos.x, pos.y);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // Update handle UI position
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

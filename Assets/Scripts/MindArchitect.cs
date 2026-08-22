using UnityEngine;
using UnityEngine.InputSystem;

public class MindArchitect : MonoBehaviour
{
    [Header("Building Settings")]
    public GameObject blockPrefab;
    public float maxReachDistance = 20.0f;
    public float spawnOffset = 0.5f;

    [Header("Physics Manipulation (Mind Power)")]
    public float grabSpeed = 10f;
    public float holdDistance = 4.0f;

    [Header("Colors")]
    public Color[] buildColors = new Color[] {
        Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan, Color.white
    };
    private int currentColorIndex = 0;

    private Camera playerCamera;
    private Rigidbody grabbedRigidbody;
    private float currentGrabDistance;

    // Mobile Input States
    private bool mobileSpawnTriggered = false;
    private bool mobileDestroyTriggered = false;
    private bool mobileGrabActive = false;

    // A simple Reticle UI element
    private Texture2D reticleTexture;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();

        // Create a basic 2x2 white texture for reticle
        reticleTexture = new Texture2D(2, 2);
        for (int y = 0; y < reticleTexture.height; y++)
        {
            for (int x = 0; x < reticleTexture.width; x++)
            {
                reticleTexture.SetPixel(x, y, Color.white);
            }
        }
        reticleTexture.Apply();
    }

    void Update()
    {
        if (playerCamera == null) return;

        // Change color with mouse scroll or number keys 1-7 (Desktop fallback)
        HandleColorSelection();

        // Mind Spawning / Deleting Blocks
        HandleBuilding();

        // Mind Physics Grab
        HandleGrab();

        // Mindfulness interactions (floating island objects)
        HandleMindInteraction();
    }

    void HandleColorSelection()
    {
        if (Keyboard.current != null)
        {
            for (int i = 0; i < buildColors.Length; i++)
            {
                Key key = (Key)((int)Key.Digit1 + i);
                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    currentColorIndex = i;
                    Debug.Log($"Selected color: {buildColors[currentColorIndex]}");
                }
            }
        }

        if (Mouse.current != null)
        {
            Vector2 scroll = Mouse.current.scroll.ReadValue();
            if (scroll.y > 0)
            {
                currentColorIndex = (currentColorIndex + 1) % buildColors.Length;
            }
            else if (scroll.y < 0)
            {
                currentColorIndex = (currentColorIndex - 1 + buildColors.Length) % buildColors.Length;
            }
        }
    }

    void HandleBuilding()
    {
        bool spawnTriggered = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) || mobileSpawnTriggered;
        mobileSpawnTriggered = false; // Consume trigger

        // Click to spawn block
        if (spawnTriggered && grabbedRigidbody == null)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, maxReachDistance))
            {
                // Calculate position on the face of the hit object
                Vector3 spawnPosition = hit.point + hit.normal * 0.5f;
                // Snap to nearest grid coordinate to make building neat
                spawnPosition.x = Mathf.Round(spawnPosition.x);
                spawnPosition.y = Mathf.Round(spawnPosition.y);
                spawnPosition.z = Mathf.Round(spawnPosition.z);

                // Check if space is already occupied to avoid duplicate stacking in the exact same spot
                if (!Physics.CheckBox(spawnPosition, new Vector3(0.45f, 0.45f, 0.45f)))
                {
                    GameObject newBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    newBlock.transform.position = spawnPosition;
                    newBlock.tag = "SpawnedBlock";
                    
                    // Add Rigidbody for physics when pushed/grabbed
                    Rigidbody rb = newBlock.AddComponent<Rigidbody>();
                    rb.mass = 2.0f;

                    // Set block color
                    Renderer renderer = newBlock.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        mat.color = buildColors[currentColorIndex];
                        renderer.material = mat;
                    }
                }
            }
        }

        bool destroyTriggered = (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) || mobileDestroyTriggered;
        mobileDestroyTriggered = false; // Consume trigger

        // Click to destroy spawned block
        if (destroyTriggered)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, maxReachDistance))
            {
                if (hit.collider.CompareTag("SpawnedBlock"))
                {
                    Destroy(hit.collider.gameObject);
                }
            }
        }
    }

    void HandleGrab()
    {
        bool grabKeyPressed = (Keyboard.current != null && Keyboard.current.eKey.isPressed) || mobileGrabActive;

        if (grabKeyPressed)
        {
            if (grabbedRigidbody == null)
            {
                // Try to grab
                Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                if (Physics.Raycast(ray, out RaycastHit hit, maxReachDistance))
                {
                    Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                    if (rb != null && !rb.isKinematic)
                    {
                        grabbedRigidbody = rb;
                        currentGrabDistance = Mathf.Max(holdDistance, hit.distance);
                        grabbedRigidbody.useGravity = false;
                    }
                }
            }
            else
            {
                // Hold object in front of camera
                Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * currentGrabDistance;
                Vector3 direction = targetPos - grabbedRigidbody.position;
                grabbedRigidbody.linearVelocity = direction * grabSpeed;
                
                // Keep rotation still or slightly damped
                grabbedRigidbody.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            // Release
            if (grabbedRigidbody != null)
            {
                grabbedRigidbody.useGravity = true;
                grabbedRigidbody = null;
            }
        }
    }

    // --- MOBILE API HOOKS ---
    public void TriggerMobileSpawn()
    {
        mobileSpawnTriggered = true;
    }

    public void TriggerMobileDestroy()
    {
        mobileDestroyTriggered = true;
    }

    public void SetMobileGrab(bool active)
    {
        mobileGrabActive = active;
    }

    public void SetColorIndex(int index)
    {
        if (index >= 0 && index < buildColors.Length)
        {
            currentColorIndex = index;
        }
    }

    public int GetColorIndex()
    {
        return currentColorIndex;
    }

    public Color[] GetBuildColors()
    {
        return buildColors;
    }

    void OnGUI()
    {
        // Draw crosshair at the center of screen
        if (reticleTexture != null)
        {
            float xMin = (Screen.width / 2) - 3;
            float yMin = (Screen.height / 2) - 3;
            GUI.color = buildColors[currentColorIndex];
            GUI.DrawTexture(new Rect(xMin, yMin, 6, 6), reticleTexture);
            
#if !UNITY_ANDROID && !UNITY_IOS
            // Draw desktop controls helper text when in editor or standalone desktop builds
            GUI.color = Color.white;
            GUI.Box(new Rect(10, 10, 320, 100), "Mind Architect Controls:\n" +
                "- WASD: Walk (Shift to Run)\n" +
                "- Mouse: Look around\n" +
                "- Left Click: Spawn Block (on surfaces)\n" +
                "- Right Click: Destroy Spawned Block\n" +
                "- Hold E: Grab and Move Block with your Mind!\n" +
                $"- Scroll / Keys 1-7: Change Block Color (Current: {buildColors[currentColorIndex]})");
#endif
        }
    }

    void HandleMindInteraction()
    {
        if (MindfulnessGameManager.Instance == null) return;

        bool clickTriggered = false;
        Vector2 screenPos = Vector2.zero;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            clickTriggered = true;
            screenPos = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            clickTriggered = true;
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }

        if (clickTriggered && playerCamera != null)
        {
            Ray ray = playerCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, maxReachDistance * 2f))
            {
                // 1. Mindful Growing (Sprout/Soil interaction)
                if (hit.collider.CompareTag("SoilBed") || hit.collider.name == "SoilBed")
                {
                    MindfulnessGameManager.Instance.plantGrowth = Mathf.Clamp01(MindfulnessGameManager.Instance.plantGrowth + 0.2f);
                    GameObject sprout = GameObject.Find("SproutPlant");
                    if (sprout != null)
                    {
                        sprout.transform.localScale = Vector3.one * (0.1f + MindfulnessGameManager.Instance.plantGrowth * 1.5f);
                    }

                    if (MindfulnessGameManager.Instance.plantGrowth >= 1.0f)
                    {
                        MindfulnessGameManager.Instance.gratitudeCount = Mathf.Min(MindfulnessGameManager.Instance.gratitudeCount + 1, MindfulnessGameManager.Instance.maxGratitude);
                        MindfulnessGameManager.Instance.plantGrowth = 0f; // reset for next seed
                    }
                }

                // 2. Inner Dialogue (Tapping thought bubbles to filter/absorb)
                else if (hit.collider.name == "ThoughtBubble" || hit.collider.transform.name == "ThoughtBubble")
                {
                    FloatingThought ft = hit.collider.GetComponent<FloatingThought>();
                    if (ft != null)
                    {
                        if (ft.isPositive)
                        {
                            MindfulnessGameManager.Instance.thoughtBalance = Mathf.Clamp(MindfulnessGameManager.Instance.thoughtBalance + 0.1f, -1f, 1f);
                        }
                        else
                        {
                            MindfulnessGameManager.Instance.thoughtBalance = Mathf.Clamp(MindfulnessGameManager.Instance.thoughtBalance + 0.05f, -1f, 1f);
                        }
                        Destroy(hit.collider.gameObject);
                    }
                }

                // Dialogue Monolith (Katastrophisieren) -> Open Restructuring Puzzle
                else if (hit.collider.CompareTag("DialogueMonolith") || hit.collider.name == "DialogueMonolith")
                {
                    if (RestructurePuzzleManager.Instance == null && MindfulnessGameManager.Instance != null && !MindfulnessGameManager.Instance.isDialogueRestructured)
                    {
                        GameObject go = new GameObject("RestructurePuzzleManager");
                        go.AddComponent<RestructurePuzzleManager>();
                    }
                }

                // 3. Resource Path (Collect crystals)
                else if (hit.collider.CompareTag("MemoryCrystal") || hit.collider.name.StartsWith("MemoryCrystal"))
                {
                    Destroy(hit.collider.gameObject);
                    MindfulnessGameManager.Instance.crystalsCollected++;
                }

                // 4. Stress Distraction (Grow trees)
                else if (hit.collider.CompareTag("TreePatch") || hit.collider.name == "TreePatch")
                {
                    GameObject tree = GameObject.Find("DistractionTree");
                    if (tree != null)
                    {
                        tree.transform.localScale += Vector3.one * 0.15f;
                        MindfulnessGameManager.Instance.treesPlanted++;
                        MindfulnessGameManager.Instance.distractionSuccess = Mathf.Clamp01(MindfulnessGameManager.Instance.distractionSuccess + 0.1f);
                    }
                }

                // 5. Healing Together (Repair temple ruins)
                else if (hit.collider.CompareTag("TempleRuin") || hit.collider.name.StartsWith("RuinColumn"))
                {
                    Transform colTrans = hit.collider.transform;
                    if (colTrans.position.y < 2.0f)
                    {
                        colTrans.position = new Vector3(colTrans.position.x, Mathf.Min(colTrans.position.y + 0.5f, 2.0f), colTrans.position.z);
                        MindfulnessGameManager.Instance.healingProgress = Mathf.Min(MindfulnessGameManager.Instance.healingProgress + 5f, 100f);
                    }
                }
            }
        }
    }
}

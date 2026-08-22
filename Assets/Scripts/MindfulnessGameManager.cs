using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class MindfulnessGameManager : MonoBehaviour
{
    public static MindfulnessGameManager Instance { get; private set; }

    [Header("Navigation Settings")]
    public bool isSlideNavigationActive = true;
    public int currentModuleIndex = 0;
    public float transitionSpeed = 5.0f;

    [Header("Module 1: Wachsen")]
    public int gratitudeCount = 2;
    public int maxGratitude = 5;
    public float plantGrowth = 0.4f; // 0 to 1

    [Header("Module 2: Dialog")]
    [Range(-1f, 1f)]
    public float thoughtBalance = 0.0f; // -1 = Negative thoughts, +1 = Positive thoughts
    public bool isDialogueRestructured = false;
    public int blockCount = 5;
    public int maxBlocks = 20;
    public List<string> positiveThoughts = new List<string> { "Ich schaffe das.", "Ich bin stark.", "Fehler sind Helfer.", "Ich bin wertvoll.", "Morgen ist ein neuer Tag." };
    public List<string> negativeThoughts = new List<string> { "Du wirst scheitern.", "Das klappt nie.", "Du bist nicht genug.", "Niemand hört dir zu.", "Warum überhaupt versuchen?" };
    private float bubbleSpawnTimer = 0f;

    [Header("Module 3: Pfad")]
    public int crystalsCollected = 0;
    public int totalCrystals = 5;

    [Header("Module 4: Ablenkung")]
    public int treesPlanted = 0;
    public float distractionSuccess = 0.5f;

    [Header("Module 5: Heilen")]
    public float healingProgress = 75f; // percentage 0 to 100

    private Camera mainCamera;
    private CharacterController playerController;
    private GameObject playerObj;

    // Defined views for slide camera positions and rotations
    private Vector3[] targetCameraPositions;
    private Quaternion[] targetCameraRotations;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<CharacterController>();
        }
        mainCamera = Camera.main;

        // Initialize target camera views for the 5 islands spaced out along the X-axis (X: 0, 25, 50, 75, 100)
        targetCameraPositions = new Vector3[5];
        targetCameraRotations = new Quaternion[5];

        for (int i = 0; i < 5; i++)
        {
            float islandX = i * 25.0f;
            // Place camera slightly back and elevated, looking slightly down at the island
            targetCameraPositions[i] = new Vector3(islandX, 5.0f, -8.0f);
            targetCameraRotations[i] = Quaternion.Euler(20f, 0f, 0f);
        }

        // Apply initial slide positions if active
        if (isSlideNavigationActive && playerObj != null && mainCamera != null)
        {
            if (playerController != null) playerController.enabled = false;
            playerObj.transform.position = new Vector3(currentModuleIndex * 25.0f, 0.5f, -8.0f); // align player to view
            mainCamera.transform.position = targetCameraPositions[currentModuleIndex];
            mainCamera.transform.rotation = targetCameraRotations[currentModuleIndex];
            
            // Unparent camera during slide transitions to allow independent movement
            mainCamera.transform.SetParent(null);
        }
    }

    void Update()
    {
        // Toggle navigation modes on Windows (Key: Escape)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetSlideNavigation(!isSlideNavigationActive);
        }

        if (isSlideNavigationActive)
        {
            // Keyboard controls for slide transition on Windows (Left/Right arrows or A/D keys)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
                {
                    PrevModule();
                }
                else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
                {
                    NextModule();
                }
            }

            // Smoothly move and rotate camera to the target module's view
            if (mainCamera != null)
            {
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetCameraPositions[currentModuleIndex], Time.deltaTime * transitionSpeed);
                mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetCameraRotations[currentModuleIndex], Time.deltaTime * transitionSpeed);
            }
        }

        // Spawn thoughts in the background for Island 2
        HandleDialogueBubbles();
    }

    public void NextModule()
    {
        if (isSlideNavigationActive)
        {
            currentModuleIndex = (currentModuleIndex + 1) % 5;
        }
    }

    public void PrevModule()
    {
        if (isSlideNavigationActive)
        {
            currentModuleIndex = (currentModuleIndex - 1 + 5) % 5;
        }
    }

    public void SetSlideNavigation(bool active)
    {
        isSlideNavigationActive = active;

        if (playerObj != null && mainCamera != null)
        {
            if (active)
            {
                // Slide mode: disable character movement controls, detach camera
                if (playerController != null) playerController.enabled = false;
                mainCamera.transform.SetParent(null);
                
                // Cursor locked off on slide mode
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // 3D Walk mode: enable player controller, attach camera back to player head
                Transform cameraPoint = playerObj.transform.Find("PlayerCamera");
                if (cameraPoint != null)
                {
                    // Snap player to the current island before walking
                    playerObj.transform.position = new Vector3(currentModuleIndex * 25.0f, 1f, 0f);
                    if (playerController != null) playerController.enabled = true;
                    
                    mainCamera.transform.SetParent(cameraPoint);
                    mainCamera.transform.localPosition = Vector3.zero;
                    mainCamera.transform.localRotation = Quaternion.identity;

#if !UNITY_ANDROID && !UNITY_IOS
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
#endif
                }
            }
        }
    }

    private void HandleDialogueBubbles()
    {
        if (PlayerPrefs.GetInt("OnboardingCompleted", 0) == 0) return;

        // Only spawn bubbles when player is viewing the dialogue module (index 1) or in 3D mode
        if (currentModuleIndex != 1 && isSlideNavigationActive) return;

        bubbleSpawnTimer += Time.deltaTime;
        if (bubbleSpawnTimer > 3f)
        {
            bubbleSpawnTimer = 0f;
            SpawnThoughtBubble();
        }
    }

    private void SpawnThoughtBubble()
    {
        // Dialogue island is located at X: 25. Spawn thoughts floating upwards.
        Vector3 spawnPos = new Vector3(25f + Random.Range(-3f, 3f), 0.5f, 3f + Random.Range(-2f, 2f));
        
        bool isPositive = Random.value > (0.5f - (thoughtBalance * 0.3f)); // bias spawning based on Gedanken-Wippe balance
        string text = isPositive 
            ? positiveThoughts[Random.Range(0, positiveThoughts.Count)]
            : negativeThoughts[Random.Range(0, negativeThoughts.Count)];

        GameObject bubble = new GameObject("ThoughtBubble");
        bubble.transform.position = spawnPos;

        // Add billboard floating text
        TextMesh tm = bubble.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 20;
        tm.characterSize = 0.1f;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = isPositive ? Color.green : Color.red;

        // Add BoxCollider so raycasting can detect it
        BoxCollider bc = bubble.AddComponent<BoxCollider>();
        bc.size = new Vector3(4f, 1f, 0.2f);

        // Add upward float script
        bubble.AddComponent<FloatingThought>().isPositive = isPositive;
        
        // Auto destroy after 5 seconds
        Destroy(bubble, 5.0f);
    }
}

// Helper component for floating speech bubbles
public class FloatingThought : MonoBehaviour
{
    public bool isPositive;
    private float floatSpeed = 1.2f;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        // Float random direction slightly
        floatSpeed += Random.Range(-0.2f, 0.4f);
    }

    void Update()
    {
        // Float up
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime, Space.World);

        // Billboard look-at camera
        if (cam != null)
        {
            transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
        }
    }
}

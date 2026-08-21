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

        // Change color with mouse scroll or number keys 1-7
        HandleColorSelection();

        // Mind Spawning / Deleting Blocks
        HandleBuilding();

        // Mind Physics Grab
        HandleGrab();
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
        if (Mouse.current == null) return;

        // Click left button to spawn block
        if (Mouse.current.leftButton.wasPressedThisFrame && grabbedRigidbody == null)
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

        // Click right button to destroy spawned block
        if (Mouse.current.rightButton.wasPressedThisFrame)
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
        if (Keyboard.current == null) return;

        bool grabKeyPressed = Keyboard.current.eKey.isPressed;

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
                grabbedRigidbody.velocity = direction * grabSpeed;
                
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

    void OnGUI()
    {
        // Draw crosshair at the center of screen
        if (reticleTexture != null)
        {
            float xMin = (Screen.width / 2) - 3;
            float yMin = (Screen.height / 2) - 3;
            GUI.color = buildColors[currentColorIndex];
            GUI.DrawTexture(new Rect(xMin, yMin, 6, 6), reticleTexture);
            
            // Draw controls helper text
            GUI.color = Color.white;
            GUI.Box(new Rect(10, 10, 320, 100), "Mind Architect Controls:\n" +
                "- WASD: Walk (Shift to Run)\n" +
                "- Mouse: Look around\n" +
                "- Left Click: Spawn Block (on surfaces)\n" +
                "- Right Click: Destroy Spawned Block\n" +
                "- Hold E: Grab and Move Block with your Mind!\n" +
                $"- Scroll / Keys 1-7: Change Block Color (Current: {buildColors[currentColorIndex]})");
        }
    }
}

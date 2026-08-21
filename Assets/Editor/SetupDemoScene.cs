using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupDemoScene : EditorWindow
{
    [MenuItem("Architect of Mind/Setup Demo Scene")]
    public static void SetupScene()
    {
        // 1. Register Tag "SpawnedBlock" in TagManager
        RegisterTag("SpawnedBlock");

        // 2. Clean up existing objects that we might duplicate
        CleanExistingDemoObjects();

        // 3. Create Ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, -0.5f, 0f);
        ground.transform.localScale = new Vector3(100f, 1f, 100f);
        
        Renderer groundRenderer = ground.GetComponent<Renderer>();
        if (groundRenderer != null)
        {
            Material groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            groundMat.color = new Color(0.2f, 0.2f, 0.2f); // Dark grey ground
            groundMat.SetFloat("_Metallic", 0.1f);
            groundMat.SetFloat("_Smoothness", 0.2f);
            groundRenderer.material = groundMat;
        }
        Undo.RegisterCreatedObjectUndo(ground, "Create Ground");

        // 4. Create Player
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0f, 0.5f, 0f);
        
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0f, 1f, 0f);
        cc.height = 2f;
        cc.radius = 0.5f;

        player.AddComponent<FirstPersonController>();
        player.AddComponent<MindArchitect>();

        // Create Player Camera
        GameObject camObj = new GameObject("PlayerCamera");
        camObj.transform.parent = player.transform;
        camObj.transform.localPosition = new Vector3(0f, 1.8f, 0f); // Eye height
        camObj.transform.localRotation = Quaternion.identity;
        
        Camera camera = camObj.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.nearClipPlane = 0.01f;
        
        camObj.AddComponent<AudioListener>();

        Undo.RegisterCreatedObjectUndo(player, "Create Player");

        // 5. Create a tower of physics blocks
        Vector3 towerBase = new Vector3(0f, 0.5f, 6f);
        int rows = 4;
        int count = 1;
        
        for (int y = 0; y < rows; y++)
        {
            int cols = rows - y;
            for (int x = 0; x < cols; x++)
            {
                GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.name = $"Interactive Block {count++}";
                block.tag = "SpawnedBlock";

                float offset = (cols - 1) * 0.5f;
                block.transform.position = towerBase + new Vector3(x - offset, y, 0f);

                Rigidbody rb = block.AddComponent<Rigidbody>();
                rb.mass = 2.0f;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                Renderer renderer = block.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.color = Color.Lerp(Color.yellow, Color.red, (float)y / rows);
                    renderer.material = mat;
                }

                Undo.RegisterCreatedObjectUndo(block, "Create Physics Block");
            }
        }

        // 6. Create EventSystem (required for UI interaction)
        GameObject eventSystem = GameObject.Find("EventSystem");
        if (eventSystem == null)
        {
            eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
            // Add InputSystemUIInputModule for modern Input System UI interaction
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
        }

        // 7. Create Mobile Controls Manager (instantiates the Touch UI)
        GameObject mobileControlsManager = new GameObject("MobileControlsManager");
        mobileControlsManager.AddComponent<MobileControlsManager>();
        Undo.RegisterCreatedObjectUndo(mobileControlsManager, "Create Mobile Controls Manager");

        // Mark the scene as modified so Unity asks to save
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        
        Debug.Log("Architect of Mind: Mobile Demo Scene setup complete! Press Play in the editor to test.");
    }

    private static void RegisterTag(string tag)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets.Length == 0) return;

        SerializedObject tagManager = new SerializedObject(assets[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
            Debug.Log($"Registered tag: '{tag}'");
        }
    }

    private static void CleanExistingDemoObjects()
    {
        // Find existing ground/player/camera and delete to prevent overlapping duplicates
        GameObject oldGround = GameObject.Find("Ground");
        if (oldGround != null) Undo.DestroyObjectImmediate(oldGround);

        GameObject oldPlayer = GameObject.Find("Player");
        if (oldPlayer != null) Undo.DestroyObjectImmediate(oldPlayer);

        GameObject oldManager = GameObject.Find("MobileControlsManager");
        if (oldManager != null) Undo.DestroyObjectImmediate(oldManager);

        GameObject oldCanvas = GameObject.Find("MobileControlsCanvas");
        if (oldCanvas != null) Undo.DestroyObjectImmediate(oldCanvas);

        // Delete any leftover cameras tag-wise if they are not in Player
        Camera[] cameras = Object.FindObjectsByType<Camera>();
        foreach (var cam in cameras)
        {
            if (cam.transform.parent == null || !cam.transform.parent.name.Contains("Player"))
            {
                Undo.DestroyObjectImmediate(cam.gameObject);
            }
        }

        // Delete any blocks tagged "SpawnedBlock" or starting with "Interactive Block"
        GameObject[] spawnedBlocks = GameObject.FindGameObjectsWithTag("SpawnedBlock");
        foreach (var block in spawnedBlocks)
        {
            Undo.DestroyObjectImmediate(block);
        }

        // Fallback for name-based lookup
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj != null && obj.name.StartsWith("Interactive Block"))
            {
                Undo.DestroyObjectImmediate(obj);
            }
        }
    }
}

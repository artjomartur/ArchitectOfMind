using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

public class SetupDemoScene : EditorWindow
{
    [MenuItem("Architect of Mind/Setup Demo Scene")]
    public static void SetupScene()
    {
        // 1. Register required Tags in TagManager
        RegisterTag("SpawnedBlock");
        RegisterTag("SoilBed");
        RegisterTag("MemoryCrystal");
        RegisterTag("TreePatch");
        RegisterTag("TempleRuin");
        RegisterTag("DialogueMonolith");

        // 2. Clean up existing objects that we might duplicate
        CleanExistingDemoObjects();

        // Setup Directional Light (warm golden hour light with soft shadows)
        Light mainLight = null;
        Light[] lights = GameObject.FindObjectsByType<Light>(FindObjectsInactive.Exclude);
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional)
            {
                mainLight = l;
                break;
            }
        }
        if (mainLight == null)
        {
            GameObject lightGO = new GameObject("DirectionalLight", typeof(Light));
            mainLight = lightGO.GetComponent<Light>();
            mainLight.type = LightType.Directional;
            Undo.RegisterCreatedObjectUndo(lightGO, "Create Directional Light");
        }
        mainLight.transform.rotation = Quaternion.Euler(35f, -45f, 0f);
        mainLight.color = new Color(1.0f, 0.94f, 0.84f); // soft golden sunset/sunrise glow
        mainLight.intensity = 1.35f;
        mainLight.shadows = LightShadows.Soft;

        // 3. Create a parent holder for all islands to keep hierarchy clean
        GameObject islandsHolder = new GameObject("FloatingIslandsHolder");
        Undo.RegisterCreatedObjectUndo(islandsHolder, "Create Islands Holder");

        // --- MATERlALS SETUP ---
        Material grassMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        grassMat.color = new Color(0.2f, 0.7f, 0.2f); // Grass green

        Material darkRockMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        darkRockMat.color = new Color(0.25f, 0.25f, 0.3f); // Dark stone

        Material soilMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        soilMat.color = new Color(0.35f, 0.2f, 0.1f); // Brown soil

        Material crystalMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        crystalMat.color = new Color(0f, 0.9f, 0.9f); // Cyan glowing crystal
        crystalMat.SetFloat("_Metallic", 0.9f);
        crystalMat.SetFloat("_Smoothness", 0.9f);

        Material woodMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        woodMat.color = new Color(0.5f, 0.35f, 0.2f); // Wood brown

        Material templeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        templeMat.color = new Color(0.85f, 0.85f, 0.85f); // Light marble

        // --- PREFABS SETUP ---
        GameObject treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Nicrom/Shaders/Wind/Prefabs/LPW_Tree_A1_6.5m_01.prefab");
        GameObject boulderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Nicrom/Shaders/Wind/Prefabs/LPW_Rock_Boulder_A1_01.prefab");
        GameObject grassPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Nicrom/Shaders/Wind/Prefabs/LPW_Grass_A1_50cm_01.prefab");
        GameObject flowerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Nicrom/Shaders/Wind/Prefabs/LPW_Flower_A1_H70cm_01.prefab");

        // --- ISLAND 1: ACHTSAMES WACHSEN (X: 0) ---
        GameObject island1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        island1.name = "Island_1_Wachsen";
        island1.transform.position = new Vector3(0f, 0f, 0f);
        island1.transform.localScale = new Vector3(12f, 1f, 12f);
        island1.GetComponent<Renderer>().material = grassMat;
        island1.transform.SetParent(islandsHolder.transform);

        GameObject soil = GameObject.CreatePrimitive(PrimitiveType.Cube);
        soil.name = "SoilBed";
        soil.tag = "SoilBed";
        soil.transform.position = new Vector3(0f, 0.55f, 0f);
        soil.transform.localScale = new Vector3(4f, 0.1f, 4f);
        soil.GetComponent<Renderer>().material = soilMat;
        soil.transform.SetParent(island1.transform);

        GameObject sprout = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sprout.name = "SproutPlant";
        sprout.transform.position = new Vector3(0f, 0.7f, 0f);
        sprout.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f); // starting size (grows when clicked)
        Material sproutMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        sproutMat.color = new Color(0.1f, 0.9f, 0.1f);
        sprout.GetComponent<Renderer>().material = sproutMat;
        sprout.transform.SetParent(island1.transform);

        // Populate Island 1 with trees, rocks, grass, and flowers
        PopulateIslandNature(treePrefab, boulderPrefab, grassPrefab, flowerPrefab, new Vector3(0f, 0f, 0f), 5f, 3, 2, 20, island1.transform, woodMat, darkRockMat);


        // --- ISLAND 2: INNERER DIALOG (X: 25) ---
        GameObject dialogueHolder = new GameObject("Island_2_Dialog");
        dialogueHolder.transform.SetParent(islandsHolder.transform);

        GameObject island2A = GameObject.CreatePrimitive(PrimitiveType.Cube);
        island2A.name = "Island_2A_Positive";
        island2A.transform.position = new Vector3(21f, 0f, 0f);
        island2A.transform.localScale = new Vector3(6f, 1f, 6f);
        island2A.GetComponent<Renderer>().material = grassMat;
        island2A.transform.SetParent(dialogueHolder.transform);

        GameObject island2B = GameObject.CreatePrimitive(PrimitiveType.Cube);
        island2B.name = "Island_2B_Negative";
        island2B.transform.position = new Vector3(29f, -0.5f, 0f); // Dark side is slightly lower
        island2B.transform.localScale = new Vector3(6f, 1.2f, 6f);
        island2B.GetComponent<Renderer>().material = darkRockMat;
        island2B.transform.SetParent(dialogueHolder.transform);

        // Connecting rope bridge
        GameObject bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridge.name = "RopeBridge";
        bridge.transform.position = new Vector3(25f, 0.1f, 0f);
        bridge.transform.localScale = new Vector3(4f, 0.1f, 1.5f);
        bridge.GetComponent<Renderer>().material = woodMat;
        bridge.transform.SetParent(dialogueHolder.transform);

        // Populate Island 2 platforms with trees, rocks, grass, and flowers
        PopulateIslandNature(treePrefab, boulderPrefab, grassPrefab, flowerPrefab, new Vector3(21f, 0f, 0f), 2.5f, 1, 1, 12, dialogueHolder.transform, woodMat, darkRockMat);
        PopulateIslandNature(null, boulderPrefab, grassPrefab, flowerPrefab, new Vector3(29f, -0.5f, 0f), 2.5f, 0, 3, 10, dialogueHolder.transform, null, darkRockMat);

        // --- DIALOGUE MONOLITH (Gedanken Umstrukturieren) ---
        // Using low-poly boulder prefab as base
        GameObject monolith = SpawnLowPolyOrPrimitive(boulderPrefab, PrimitiveType.Cube, "DialogueMonolith", new Vector3(25f, 1.0f, 3.5f), new Vector3(1.5f, 2.5f, 1.5f), Quaternion.identity, dialogueHolder.transform, darkRockMat);
        monolith.tag = "DialogueMonolith";

        // Add thorny vine components
        Material thornMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        thornMat.color = new Color(0.45f, 0.1f, 0.1f); // dark reddish brown

        GameObject thorn1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        thorn1.name = "ThornVine_1";
        thorn1.transform.position = new Vector3(24.6f, 1.5f, 2.7f);
        thorn1.transform.localScale = new Vector3(0.12f, 1.5f, 0.12f);
        thorn1.transform.rotation = Quaternion.Euler(15f, 20f, 10f);
        thorn1.GetComponent<Renderer>().material = thornMat;
        thorn1.transform.SetParent(monolith.transform);

        GameObject thorn2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        thorn2.name = "ThornVine_2";
        thorn2.transform.position = new Vector3(25.4f, 1.5f, 2.7f);
        thorn2.transform.localScale = new Vector3(0.12f, 1.5f, 0.12f);
        thorn2.transform.rotation = Quaternion.Euler(-15f, -20f, -10f);
        thorn2.GetComponent<Renderer>().material = thornMat;
        thorn2.transform.SetParent(monolith.transform);

        // World Space Canvas for Negative Thought text on the front of the monolith
        GameObject wsc = new GameObject("WorldSpaceCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        wsc.transform.SetParent(monolith.transform);
        wsc.transform.localPosition = new Vector3(0f, 0.4f, -0.6f); // placed in front of the boulder face
        wsc.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // face the player
        wsc.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f); // scale down to fit

        Canvas canvasComp = wsc.GetComponent<Canvas>();
        canvasComp.renderMode = RenderMode.WorldSpace;
        
        RectTransform wscRT = wsc.GetComponent<RectTransform>();
        wscRT.sizeDelta = new Vector2(200, 100);

        Font uifont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uifont == null) uifont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject wscTextGO = new GameObject("ThoughtText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        wscTextGO.transform.SetParent(wsc.transform, false);
        
        RectTransform textRT = wscTextGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        Text wscText = wscTextGO.GetComponent<Text>();
        wscText.font = uifont;
        wscText.fontSize = 11;
        wscText.text = "KATASTROPHISIEREN:\nWenn ich die Klausur nicht schaffe, bin ich wertlos!";
        wscText.alignment = TextAnchor.MiddleCenter;
        wscText.color = Color.red;
        wscText.raycastTarget = false;



        // --- ISLAND 3: RESSOURCEN-PFAD (X: 50) ---
        GameObject pathHolder = new GameObject("Island_3_Pfad");
        pathHolder.transform.SetParent(islandsHolder.transform);

        Vector3[] steppingStonePositions = new Vector3[] {
            new Vector3(45f, 0f, 0f),
            new Vector3(47.5f, 0.2f, 2f),
            new Vector3(50f, 0.4f, 0f),
            new Vector3(52.5f, 0.2f, -2f),
            new Vector3(55f, 0f, 0f)
        };

        for (int i = 0; i < steppingStonePositions.Length; i++)
        {
            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stone.name = $"SteppingStone_{i}";
            stone.transform.position = steppingStonePositions[i];
            stone.transform.localScale = new Vector3(3f, 0.5f, 3f);
            stone.GetComponent<Renderer>().material = darkRockMat;
            stone.transform.SetParent(pathHolder.transform);

            // Scatter small decorative low-poly boulder rocks on some stepping stones
            if (i % 2 == 1)
            {
                SpawnLowPolyOrPrimitive(boulderPrefab, PrimitiveType.Cube, "PathRock", steppingStonePositions[i] + new Vector3(1.2f, 0.2f, -1.2f), new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, pathHolder.transform, darkRockMat);
            }

            // Memory crystal floating above it
            GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crystal.name = $"MemoryCrystal_{i}";
            crystal.tag = "MemoryCrystal";
            crystal.transform.position = steppingStonePositions[i] + new Vector3(0f, 1.5f, 0f);
            crystal.transform.rotation = Quaternion.Euler(45f, 0f, 45f); // diamond look
            crystal.transform.localScale = new Vector3(0.5f, 0.8f, 0.5f);
            crystal.GetComponent<Renderer>().material = crystalMat;
            
            // Add a simple automatic rotation script to make it look active
            crystal.AddComponent<IdleRotate>();
            crystal.transform.SetParent(pathHolder.transform);
        }


        // --- ISLAND 4: STRESS-ABLENKUNG (X: 75) ---
        GameObject island4 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        island4.name = "Island_4_Ablenkung";
        island4.transform.position = new Vector3(75f, 0f, 0f);
        island4.transform.localScale = new Vector3(12f, 1f, 12f);
        island4.GetComponent<Renderer>().material = grassMat;
        island4.transform.SetParent(islandsHolder.transform);

        GameObject treePatch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        treePatch.name = "TreePatch";
        treePatch.tag = "TreePatch";
        treePatch.transform.position = new Vector3(75f, 0.55f, 0f);
        treePatch.transform.localScale = new Vector3(3f, 0.1f, 3f);
        treePatch.GetComponent<Renderer>().material = soilMat;
        treePatch.transform.SetParent(island4.transform);

        // Distraction Tree (Using LPW Low-Poly Tree Prefab)
        GameObject tree = SpawnLowPolyOrPrimitive(treePrefab, PrimitiveType.Cylinder, "DistractionTree", new Vector3(75f, 0.55f, 0f), new Vector3(0.6f, 0.6f, 0.6f), Quaternion.identity, island4.transform, woodMat);

        // Populate Island 4 with trees, rocks, grass, and flowers around the central distraction tree
        PopulateIslandNature(treePrefab, boulderPrefab, grassPrefab, flowerPrefab, new Vector3(75f, 0f, 0f), 5f, 4, 3, 25, island4.transform, woodMat, darkRockMat);


        // --- ISLAND 5: GEMEINSAMES HEILEN (X: 100) ---
        GameObject island5 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        island5.name = "Island_5_Heilen";
        island5.transform.position = new Vector3(100f, 0f, 0f);
        island5.transform.localScale = new Vector3(14f, 1f, 14f);
        island5.GetComponent<Renderer>().material = templeMat;
        island5.transform.SetParent(islandsHolder.transform);

        // 6 circular pillars representing the ruined temple dome
        int pillarCount = 6;
        float radius = 4.5f;
        for (int i = 0; i < pillarCount; i++)
        {
            float angle = i * Mathf.PI * 2f / pillarCount;
            Vector3 pillarPos = new Vector3(100f + Mathf.Cos(angle) * radius, 1.5f, Mathf.Sin(angle) * radius);
            
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = $"RuinColumn_{i}";
            pillar.tag = "TempleRuin";
            pillar.GetComponent<Renderer>().material = templeMat;

            // Damage some pillars by sinking them into the ground
            if (i == 1 || i == 4)
            {
                pillar.transform.position = new Vector3(pillarPos.x, 0.3f, pillarPos.z); // fallen/buried
            }
            else
            {
                pillar.transform.position = new Vector3(pillarPos.x, 2.0f, pillarPos.z); // intact
            }
            pillar.transform.localScale = new Vector3(0.6f, 1.5f, 0.6f);
            pillar.transform.SetParent(island5.transform);
        }

        // Populate Island 5 with trees, rocks, grass, and flowers around the ancient ruins
        PopulateIslandNature(treePrefab, boulderPrefab, grassPrefab, flowerPrefab, new Vector3(100f, 0f, 0f), 6f, 3, 4, 30, island5.transform, woodMat, templeMat);

        // --- PHYSICAL STORY BRIDGES & GATES ---
        GameObject storyHolder = new GameObject("StoryProgressObjects");
        storyHolder.transform.SetParent(islandsHolder.transform);

        // 1. Bridge from Island 1 to Island 2
        GameObject bridge1to2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridge1to2.name = "Bridge_1to2";
        bridge1to2.transform.position = new Vector3(12.5f, 0f, 0f);
        bridge1to2.transform.localScale = new Vector3(13f, 0.2f, 2.5f);
        bridge1to2.GetComponent<Renderer>().material = woodMat;
        bridge1to2.transform.SetParent(storyHolder.transform);

        // 2. Crystal Gate blocking Island 1 to 2
        GameObject gate1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gate1.name = "Gate_1to2";
        gate1.transform.position = new Vector3(12.5f, 1.5f, 0f);
        gate1.transform.localScale = new Vector3(0.5f, 3f, 3f);
        gate1.GetComponent<Renderer>().material = crystalMat;
        gate1.transform.SetParent(storyHolder.transform);

        // 3. Thorn Gate blocking Island 2 to 3
        GameObject gate2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gate2.name = "Gate_2to3";
        gate2.transform.position = new Vector3(31.5f, 1.5f, 0f);
        gate2.transform.localScale = new Vector3(0.5f, 3f, 3f);
        gate2.GetComponent<Renderer>().material = darkRockMat;
        gate2.transform.SetParent(storyHolder.transform);

        // 4. Bridge from Island 3 to Island 4
        GameObject bridge3to4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridge3to4.name = "Bridge_3to4";
        bridge3to4.transform.position = new Vector3(64.5f, 0f, 0f);
        bridge3to4.transform.localScale = new Vector3(10f, 0.2f, 2.5f);
        bridge3to4.GetComponent<Renderer>().material = woodMat;
        bridge3to4.transform.SetParent(storyHolder.transform);

        // 5. Energy Shield Gate blocking Island 3 to 4
        GameObject gate3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gate3.name = "Gate_3to4";
        gate3.transform.position = new Vector3(64.5f, 1.5f, 0f);
        gate3.transform.localScale = new Vector3(0.5f, 3f, 3f);
        gate3.GetComponent<Renderer>().material = crystalMat;
        gate3.transform.SetParent(storyHolder.transform);

        // 6. Bridge from Island 4 to Island 5
        GameObject bridge4to5 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridge4to5.name = "Bridge_4to5";
        bridge4to5.transform.position = new Vector3(87.0f, 0f, 0f);
        bridge4to5.transform.localScale = new Vector3(11f, 0.2f, 2.5f);
        bridge4to5.GetComponent<Renderer>().material = woodMat;
        bridge4to5.transform.SetParent(storyHolder.transform);

        // 7. Wood Fence Gate blocking Island 4 to 5
        GameObject gate4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gate4.name = "Gate_4to5";
        gate4.transform.position = new Vector3(87.0f, 1.5f, 0f);
        gate4.transform.localScale = new Vector3(0.5f, 3f, 3f);
        gate4.GetComponent<Renderer>().material = woodMat;
        gate4.transform.SetParent(storyHolder.transform);

        // --- CREATE PLAYER & MANAGERS ---
        // Configure and load textures as sprites (uses dynamic Texture2D fallback if AssetDatabase isn't updated)
        Sprite pH = LoadSprite("Assets/Basic/Textures/panel_horizontal.png");
        Sprite pV = LoadSprite("Assets/Basic/Textures/panel_vertical.png");
        Sprite backArr = LoadSprite("Assets/Basic/Textures/back_grey.png");
        Sprite nextArr = LoadSprite("Assets/Basic/Textures/next_grey.png");
        Sprite btnG = LoadSprite("Assets/Basic/Textures/panel_horizontal.png"); // Clean panel instead of PlayButton_grey to prevent overlapping baked text
        Sprite foxMed = LoadSprite("Assets/Basic/Textures/fox_meditating.jpg");
        Sprite foxHap = LoadSprite("Assets/Basic/Textures/fox_happy.jpg");
        Sprite foxExp = LoadSprite("Assets/Basic/Textures/fox_explaining.jpg");

        GameObject foxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Fox/Prefabs/Fox.prefab");

        // 4. Create Player
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0f, 0.5f, -3f); // Spawns looking at the first island
        
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0f, 1f, 0f);
        cc.height = 2f;
        cc.radius = 0.5f;

        player.AddComponent<FirstPersonController>();
        var ma = player.AddComponent<MindArchitect>();
        ma.panelSprite = pH;
        ma.buttonSprite = btnG;

        // Instantiate playable 3D Fox model under player
        if (foxPrefab != null)
        {
            GameObject foxInstance = PrefabUtility.InstantiatePrefab(foxPrefab) as GameObject;
            foxInstance.name = "FoxModel";
            foxInstance.transform.SetParent(player.transform, false);
            foxInstance.transform.localPosition = new Vector3(0f, 0f, 0f);
            foxInstance.transform.localRotation = Quaternion.identity;
            
            // Add custom animation script and convert materials to URP Lit using reflection to avoid Editor compilation order issues
            System.Type animType = System.Type.GetType("FoxAnimationController, Assembly-CSharp");
            if (animType != null) foxInstance.AddComponent(animType);
            ConvertMaterialsToURPLit(foxInstance);
        }

        // Create Player Camera child (Third Person view behind the Fox)
        GameObject camObj = new GameObject("PlayerCamera");
        camObj.transform.parent = player.transform;
        camObj.transform.localPosition = new Vector3(0f, 2.5f, -4.5f); // placed 4.5m behind, 2.5m high
        camObj.transform.localRotation = Quaternion.Euler(15f, 0f, 0f); // tilted down 15 degrees
        
        Camera camera = camObj.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.nearClipPlane = 0.01f;
        camObj.AddComponent<AudioListener>();

        Undo.RegisterCreatedObjectUndo(player, "Create Player");

        // 5. Create EventSystem (required for UI interaction)
        GameObject eventSystem = GameObject.Find("EventSystem");
        if (eventSystem == null)
        {
            eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
        }

        // 6. Create Mindfulness Game Manager
        GameObject gameManagerObj = new GameObject("MindfulnessGameManager");
        gameManagerObj.AddComponent<MindfulnessGameManager>();
        Undo.RegisterCreatedObjectUndo(gameManagerObj, "Create Mindfulness Game Manager");

        // 7. Create Mobile Controls Manager (Touch UI Overlay)
        GameObject mobileControlsManager = new GameObject("MobileControlsManager");
        var mcm = mobileControlsManager.AddComponent<MobileControlsManager>();
        mcm.panelSprite = pH;
        mcm.backSprite = backArr;
        mcm.nextSprite = nextArr;
        mcm.foxExplainingSprite = foxExp;
        Undo.RegisterCreatedObjectUndo(mobileControlsManager, "Create Mobile Controls Manager");

        // 8. Create Onboarding Manager (First launch assessment UI)
        GameObject onboardingManager = new GameObject("OnboardingManager");
        var om = onboardingManager.AddComponent<OnboardingManager>();
        om.panelSprite = pV;
        om.buttonSprite = btnG;
        om.foxMeditatingSprite = foxMed;
        om.foxHappySprite = foxHap;
        Undo.RegisterCreatedObjectUndo(onboardingManager, "Create Onboarding Manager");

        // 9. Create Story Manager (Handles progression gates and narrative alerts)
        GameObject storyManagerObj = new GameObject("StoryManager");
        storyManagerObj.AddComponent<StoryManager>();
        Undo.RegisterCreatedObjectUndo(storyManagerObj, "Create Story Manager");

        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        
        Debug.Log("Architect of Mind: 5 Floating Islands setup complete! Press Play to test.");
    }

    private static GameObject SpawnLowPolyOrPrimitive(GameObject prefab, PrimitiveType primitiveType, string name, Vector3 pos, Vector3 scale, Quaternion rot, Transform parent, Material fallbackMat = null)
    {
        GameObject go;
        if (prefab != null)
        {
            go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.transform.rotation = rot;
            ConvertMaterialsToURPLit(go);
        }
        else
        {
            go = GameObject.CreatePrimitive(primitiveType);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.transform.rotation = rot;
            if (fallbackMat != null) go.GetComponent<Renderer>().material = fallbackMat;
        }
        if (parent != null) go.transform.SetParent(parent);
        return go;
    }

    private static void ConvertMaterialsToURPLit(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            // Note: use sharedMaterials in editor setup to avoid instancing materials on disk unnecessarily
            Material[] mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null)
                {
                    // Create a new URP Lit material
                    Material newMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    
                    // Copy main color or guess color from name
                    if (mats[i].HasProperty("_Color"))
                    {
                        newMat.color = mats[i].color;
                    }
                    else if (mats[i].HasProperty("_BaseColor"))
                    {
                        newMat.color = mats[i].GetColor("_BaseColor");
                    }
                    else
                    {
                        string matName = mats[i].name.ToLower();
                        if (matName.Contains("grass") || matName.Contains("leaf") || matName.Contains("leaves") || matName.Contains("flower"))
                        {
                            newMat.color = new Color(0.2f, 0.65f, 0.2f);
                        }
                        else if (matName.Contains("wood") || matName.Contains("trunk") || matName.Contains("bark"))
                        {
                            newMat.color = new Color(0.45f, 0.3f, 0.15f);
                        }
                        else if (matName.Contains("rock") || matName.Contains("stone") || matName.Contains("boulder"))
                        {
                            newMat.color = new Color(0.4f, 0.4f, 0.45f);
                        }
                        else
                        {
                            newMat.color = Color.grey;
                        }
                    }

                    // Copy texture if present
                    if (mats[i].HasProperty("_MainTex") && mats[i].GetTexture("_MainTex") != null)
                    {
                        newMat.SetTexture("_BaseMap", mats[i].GetTexture("_MainTex"));
                    }
                    else if (mats[i].HasProperty("_BaseMap") && mats[i].GetTexture("_BaseMap") != null)
                    {
                        newMat.SetTexture("_BaseMap", mats[i].GetTexture("_BaseMap"));
                    }

                    mats[i] = newMat;
                }
            }
            r.sharedMaterials = mats;
        }
    }

    private static void PopulateIslandNature(GameObject treePF, GameObject rockPF, GameObject grassPF, GameObject flowerPF, Vector3 center, float radius, int treeCount, int rockCount, int floraCount, Transform parent, Material woodMat = null, Material rockMat = null)
    {
        // 1. Spawn decorative trees
        for (int i = 0; i < treeCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(3.5f, radius - 1f); // keep away from center
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * dist, 0.5f, Mathf.Sin(angle) * dist);
            
            // Adjust height based on terrain
            if (Physics.Raycast(pos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
            {
                pos.y = hit.point.y;
            }

            float scale = Random.Range(0.4f, 0.8f);
            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            SpawnLowPolyOrPrimitive(treePF, PrimitiveType.Cylinder, $"DecoTree_{i}", pos, new Vector3(scale, scale, scale), rot, parent, woodMat);
        }

        // 2. Spawn decorative rocks
        for (int i = 0; i < rockCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(2.5f, radius - 0.5f);
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * dist, 0.5f, Mathf.Sin(angle) * dist);
            
            if (Physics.Raycast(pos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
            {
                pos.y = hit.point.y;
            }

            float scale = Random.Range(0.3f, 0.7f);
            Quaternion rot = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0f, 360f), Random.Range(-10f, 10f));
            SpawnLowPolyOrPrimitive(rockPF, PrimitiveType.Cube, $"DecoRock_{i}", pos, new Vector3(scale, scale, scale), rot, parent, rockMat);
        }

        // 3. Spawn grass and flowers
        ScatterGrassAndFlowers(grassPF, flowerPF, center, radius, floraCount, parent);
    }

    private static void ScatterGrassAndFlowers(GameObject grassPrefab, GameObject flowerPrefab, Vector3 center, float radius, int count, Transform parent)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(0.5f, radius);
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * dist, 0.5f, Mathf.Sin(angle) * dist);
            
            // Raycast down to find ground level
            if (Physics.Raycast(pos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
            {
                pos.y = hit.point.y;
            }

            if (Random.value > 0.4f)
            {
                if (grassPrefab != null)
                {
                    SpawnLowPolyOrPrimitive(grassPrefab, PrimitiveType.Cube, "GrassPatch", pos, Vector3.one * Random.Range(0.8f, 1.4f), Quaternion.Euler(0, Random.Range(0, 360), 0), parent);
                }
            }
            else
            {
                if (flowerPrefab != null)
                {
                    SpawnLowPolyOrPrimitive(flowerPrefab, PrimitiveType.Cube, "FlowerPatch", pos, Vector3.one * Random.Range(0.8f, 1.2f), Quaternion.Euler(0, Random.Range(0, 360), 0), parent);
                }
            }
        }
    }

    private static void ConfigureAsSprite(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }
    }

    private static Sprite LoadSprite(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        ConfigureAsSprite(path);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }
        return sprite;
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
        // Destroy the parent holder, which automatically cleans up all islands
        GameObject islandsHolder = GameObject.Find("FloatingIslandsHolder");
        if (islandsHolder != null) Undo.DestroyObjectImmediate(islandsHolder);

        GameObject oldPlayer = GameObject.Find("Player");
        if (oldPlayer != null) Undo.DestroyObjectImmediate(oldPlayer);

        GameObject oldManager = GameObject.Find("MobileControlsManager");
        if (oldManager != null) Undo.DestroyObjectImmediate(oldManager);

        GameObject oldGameManager = GameObject.Find("MindfulnessGameManager");
        if (oldGameManager != null) Undo.DestroyObjectImmediate(oldGameManager);

        GameObject oldOnboardingManager = GameObject.Find("OnboardingManager");
        if (oldOnboardingManager != null) Undo.DestroyObjectImmediate(oldOnboardingManager);

        GameObject oldStoryManager = GameObject.Find("StoryManager");
        if (oldStoryManager != null) Undo.DestroyObjectImmediate(oldStoryManager);

        GameObject oldCanvas = GameObject.Find("MobileControlsCanvas");
        if (oldCanvas != null) Undo.DestroyObjectImmediate(oldCanvas);

        GameObject oldOnboardingCanvas = GameObject.Find("OnboardingCanvas");
        if (oldOnboardingCanvas != null) Undo.DestroyObjectImmediate(oldOnboardingCanvas);

        GameObject oldPuzzleCanvas = GameObject.Find("RestructurePuzzleCanvas");
        if (oldPuzzleCanvas != null) Undo.DestroyObjectImmediate(oldPuzzleCanvas);
        
        // Clean leftover items
        GameObject[] leftoverCrystals = GameObject.FindGameObjectsWithTag("MemoryCrystal");
        foreach (var c in leftoverCrystals) Undo.DestroyObjectImmediate(c);

        GameObject[] leftoverRuins = GameObject.FindGameObjectsWithTag("TempleRuin");
        foreach (var r in leftoverRuins) Undo.DestroyObjectImmediate(r);
    }
}

// Simple helper component to rotate crystals
public class IdleRotate : MonoBehaviour
{
    private float speed = 40.0f;
    void Update()
    {
        transform.Rotate(Vector3.up * speed * Time.deltaTime, Space.World);
    }
}

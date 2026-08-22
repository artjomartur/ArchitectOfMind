using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    [Header("Story Barriers (Gates)")]
    public GameObject gate1to2;
    public GameObject gate2to3;
    public GameObject gate3to4;
    public GameObject gate4to5;

    public int currentStoryStep = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Lock player from sliding between islands using UI buttons
        // In Story Mode, the player must physically walk!
        var mgm = MindfulnessGameManager.Instance;
        if (mgm != null)
        {
            mgm.isSlideNavigationActive = false; // Disable slide navigation
        }
    }

    void Update()
    {
        if (MindfulnessGameManager.Instance == null) return;
        var mgm = MindfulnessGameManager.Instance;

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            // Dynamically set currentModuleIndex based on player's physical position
            int physicalModuleIndex = Mathf.Clamp(Mathf.RoundToInt(player.transform.position.x / 25f), 0, 4);
            if (mgm.currentModuleIndex != physicalModuleIndex)
            {
                mgm.currentModuleIndex = physicalModuleIndex;
            }
        }

        // Story progression states
        // Step 0: Initial. Sprout not grown yet.
        if (currentStoryStep == 0)
        {
            if (mgm.plantGrowth >= 1.0f)
            {
                currentStoryStep = 1;
                UnlockGate(gate1to2, "Super! Du hast den Keimling zum Wachsen gebracht. Der Pfad zur Insel des Dialogs ist frei!");
            }
        }
        // Step 1: Wait until player crosses to Island 2
        else if (currentStoryStep == 1)
        {
            if (playerOnIsland(25f))
            {
                currentStoryStep = 2;
            }
        }
        // Step 2: Dialogue monolith not solved
        else if (currentStoryStep == 2)
        {
            if (mgm.isDialogueRestructured)
            {
                currentStoryStep = 3;
                UnlockGate(gate2to3, "Toll gelöst! Die negativen Gedanken sind umstrukturiert. Du kannst jetzt zum Kristallpfad gehen!");
            }
        }
        // Step 3: Wait until player crosses to Island 3
        else if (currentStoryStep == 3)
        {
            if (playerOnIsland(50f))
            {
                currentStoryStep = 4;
            }
        }
        // Step 4: Crystals not collected
        else if (currentStoryStep == 4)
        {
            if (mgm.crystalsCollected >= mgm.totalCrystals)
            {
                currentStoryStep = 5;
                UnlockGate(gate3to4, "Fantastisch! Du hast alle Erinnerungskristalle gesammelt. Weiter geht's zur Ablenkung!");
            }
        }
        // Step 5: Wait until player crosses to Island 4
        else if (currentStoryStep == 5)
        {
            if (playerOnIsland(75f))
            {
                currentStoryStep = 6;
            }
        }
        // Step 6: Distraction tree not grown
        else if (currentStoryStep == 6)
        {
            if (mgm.treesPlanted >= 3)
            {
                currentStoryStep = 7;
                UnlockGate(gate4to5, "Wunderschön! Der Schutzbaum steht stabil. Der Pfad zum Heilungstempel ist offen!");
            }
        }
        // Step 7: Wait until player crosses to Island 5
        else if (currentStoryStep == 7)
        {
            if (playerOnIsland(100f))
            {
                currentStoryStep = 8;
            }
        }
        // Step 8: Ruins not repaired
        else if (currentStoryStep == 8)
        {
            if (mgm.healingProgress >= 100f)
            {
                currentStoryStep = 9;
                ShowMascotPopup("Herzlichen Glückwunsch! Du hast die Tempelruinen repariert und die Reise zur mentalen Stärke vollendet!");
            }
        }
    }

    private bool playerOnIsland(float targetX)
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            return Mathf.Abs(player.transform.position.x - targetX) < 5f;
        }
        return false;
    }

    private void UnlockGate(GameObject gate, string message)
    {
        if (gate != null)
        {
            Destroy(gate);
        }
        ShowMascotPopup(message);
    }

    private void ShowMascotPopup(string message)
    {
        var mcm = MobileControlsManager.Instance;
        if (mcm != null)
        {
            mcm.ShowMascotMessage(message);
        }
    }
}

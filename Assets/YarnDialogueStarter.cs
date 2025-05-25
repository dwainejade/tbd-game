using UnityEngine;
using AC;
using Yarn.Unity;

public class YarnDialogueStarter : MonoBehaviour
{
    [Header("Yarn Dialogue Settings")]
    [SerializeField] private string defaultNodeName = "Start";
    [SerializeField] private bool pauseACGameplay = true;

    
    private DialogueRunner dialogueRunner;

    private void Start()
    {
        Debug.Log("YarnDialogueStarter: Starting initialization...");
        
        // Find the DialogueRunner in the scene
        dialogueRunner = FindAnyObjectByType<DialogueRunner>();
        
        if (dialogueRunner == null)
        {
            Debug.LogError("YarnDialogueStarter: DialogueRunner not found in scene! Make sure you have a Yarn Spinner DialogueRunner component.");
        }
        else
        {
            Debug.Log($"YarnDialogueStarter: Found DialogueRunner on GameObject: {dialogueRunner.gameObject.name}");
            Debug.Log($"YarnDialogueStarter: Default node set to: {defaultNodeName}");
            
            // Subscribe to dialogue events
            dialogueRunner.onDialogueStart.AddListener(OnDialogueStart);
            dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
            Debug.Log("YarnDialogueStarter: Event listeners registered successfully");
        }
    }

    private void OnDestroy()
    {
        Debug.Log("YarnDialogueStarter: Cleaning up event listeners...");
        
        // Unsubscribe from events
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueStart.RemoveListener(OnDialogueStart);
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
        }
    }

    /// <summary>
    /// Start the default dialogue node - call this from AC ActionList "Object: Send message"
    /// </summary>
    public void StartDialogue()
    {
        Debug.Log($"YarnDialogueStarter: StartDialogue() called with default node: {defaultNodeName}");
        StartDialogue(defaultNodeName);
    }

    /// <summary>
    /// Start a specific dialogue node - call this from AC ActionList "Object: Send message"
    /// </summary>
    public void StartDialogue(string nodeName)
    {
        Debug.Log($"YarnDialogueStarter: StartDialogue() called with node: '{nodeName}'");
        
        if (dialogueRunner == null)
        {
            Debug.LogError("YarnDialogueStarter: DialogueRunner is null! Cannot start dialogue.");
            return;
        }

        if (dialogueRunner.IsDialogueRunning)
        {
            Debug.LogWarning("YarnDialogueStarter: Dialogue is already running! Cannot start new dialogue.");
            return;
        }

        if (string.IsNullOrEmpty(nodeName))
        {
            Debug.LogError("YarnDialogueStarter: Node name is null or empty!");
            return;
        }

        Debug.Log($"YarnDialogueStarter: Attempting to start Yarn dialogue: '{nodeName}'");
        
        try
        {
            dialogueRunner.StartDialogue(nodeName);
            Debug.Log($"YarnDialogueStarter: Successfully called StartDialogue on DialogueRunner");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"YarnDialogueStarter: Exception when starting dialogue: {e.Message}\n{e.StackTrace}");
        }
    }

    // Specific dialogue starters for different NPCs - you can call these directly from ActionLists
    public void StartBarryDialogue()
    {
        Debug.Log("YarnDialogueStarter: StartBarryDialogue() called");
        StartDialogue("BarryConversation");
    }

    public void StartShopkeeperDialogue()
    {
        Debug.Log("YarnDialogueStarter: StartShopkeeperDialogue() called");
        StartDialogue("Shopkeeper");
    }

    public void StartGuardDialogue()
    {
        Debug.Log("YarnDialogueStarter: StartGuardDialogue() called");
        StartDialogue("Guard");
    }

    public void StartWizardDialogue()
    {
        Debug.Log("YarnDialogueStarter: StartWizardDialogue() called");
        StartDialogue("Wizard");
    }

    // Add more specific dialogue methods as needed for your NPCs

    private void OnDialogueStart()
    {
        Debug.Log("YarnDialogueStarter: OnDialogueStart() - Yarn dialogue started successfully!");

        if (pauseACGameplay)
        {
            Debug.Log("YarnDialogueStarter: Enforcing AC cutscene mode...");
            // Enforce cutscene mode to pause AC gameplay
            AC.KickStarter.stateHandler.EnforceCutsceneMode = true;
            Debug.Log($"YarnDialogueStarter: AC cutscene mode enforced. Current game state: {AC.KickStarter.stateHandler.gameState}");
        }
        else
        {
            Debug.Log("YarnDialogueStarter: AC gameplay pausing is disabled");
        }
    }

    private void OnDialogueComplete()
    {
        Debug.Log("YarnDialogueStarter: OnDialogueComplete() - Yarn dialogue completed!");

        if (pauseACGameplay)
        {
            Debug.Log("YarnDialogueStarter: Releasing AC cutscene mode...");
            // Release cutscene mode to resume AC gameplay
            AC.KickStarter.stateHandler.EnforceCutsceneMode = false;
            Debug.Log($"YarnDialogueStarter: AC cutscene mode released. Current game state: {AC.KickStarter.stateHandler.gameState}");
        }
        else
        {
            Debug.Log("YarnDialogueStarter: AC gameplay pausing was disabled");
        }
    }

    // Debug method to test dialogue from inspector
    [ContextMenu("Test Default Dialogue")]
    public void TestDefaultDialogue()
    {
        Debug.Log("YarnDialogueStarter: Testing default dialogue from context menu...");
        StartDialogue();
    }

    [ContextMenu("Test Barry Dialogue")]
    public void TestBarryDialogue()
    {
        Debug.Log("YarnDialogueStarter: Testing Barry dialogue from context menu...");
        StartBarryDialogue();
    }
}
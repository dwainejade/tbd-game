using UnityEngine;
using AC;
using Yarn.Unity;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionYarn : Action
{
    [SerializeField] private string nodeName = "Start";
    [SerializeField] private bool waitForDialogueToFinish = true;
    [SerializeField] private int nodeNameParameterID = -1;
    
    private bool isWaiting = false;
    private DialogueRunner dialogueRunner;

    public ActionYarn()
    {
        this.isDisplayed = true;
        category = ActionCategory.Dialogue;
        title = "Start Yarn Dialogue";
        description = "Starts a Yarn Spinner dialogue";
    }

    public override void AssignValues(System.Collections.Generic.List<ActionParameter> parameters)
    {
        nodeName = AssignString(parameters, nodeNameParameterID, nodeName);
    }

    public override float Run()
    {
        if (string.IsNullOrEmpty(nodeName))
        {
            Debug.LogWarning("No dialogue node name specified!");
            return 0f;
        }

        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        
        if (dialogueRunner == null)
        {
            Debug.LogError("DialogueRunner not found in scene!");
            return 0f;
        }

        // Check if the node exists in the Yarn project
        if (dialogueRunner.Dialogue != null && !dialogueRunner.Dialogue.NodeExists(nodeName))
        {
            Debug.LogError($"Yarn node '{nodeName}' does not exist in the current Yarn project!");
            return 0f;
        }

        // Start the dialogue
        dialogueRunner.StartDialogue(nodeName);

        if (waitForDialogueToFinish)
        {
            isWaiting = true;
            // Subscribe to dialogue completion
            dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
        }

        return 0f;
    }

    public override void Skip()
    {
        OnDialogueComplete();
    }

    private void OnDialogueComplete()
    {
        isWaiting = false;
        
        // Unsubscribe from the event
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
        }

        if (waitForDialogueToFinish)
        {
            // Continue the ActionList
            isRunning = false;
        }
    }

#if UNITY_EDITOR
    public override void ShowGUI(System.Collections.Generic.List<ActionParameter> parameters)
    {
        nodeNameParameterID = Action.ChooseParameterGUI("Node name:", parameters, nodeNameParameterID, ParameterType.String);
        if (nodeNameParameterID == -1)
        {
            nodeName = CustomGUILayout.TextField("Node name:", nodeName);
        }
        
        waitForDialogueToFinish = CustomGUILayout.Toggle("Wait until finish?", waitForDialogueToFinish);
    }

    public override string SetLabel()
    {
        return "Start Yarn: " + nodeName;
    }
#endif
}
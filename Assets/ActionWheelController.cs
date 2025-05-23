using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ActionWheelController : MonoBehaviour
{
    [Header("Wheel Container")]
    public GameObject wheelContainer; // Still needed if you want to show/hide at startup

    [Header("Action Buttons")]
    public Button perceptionButton;
    public Button physicalButton;
    public Button communicationButton;
    public Button specialButton;

    public TextMeshProUGUI perceptionLabel;
    public TextMeshProUGUI physicalLabel;
    public TextMeshProUGUI communicationLabel;
    public TextMeshProUGUI specialLabel;

    [Header("Data")]
    public List<ContextualActionSet> allActions;

    void Start()
    {
        // Show the default set of actions at startup
        Emotion currentEmotion = GameState.CurrentEmotion;
        string currentScene = SceneManager.GetActiveScene().name;

        ShowActions(currentEmotion, currentScene);
    }

    public void ShowActions(Emotion emotion, string sceneId)
    {
        var set = allActions.FirstOrDefault(x => x.emotion == emotion && x.sceneId == sceneId);
        if (set == null)
        {
            Debug.LogWarning($"No action set found for Emotion: {emotion}, Scene: {sceneId}");
            ClearButtons(); // Keep wheel visible, just clear labels
            return;
        }

        SetupButton(perceptionButton, perceptionLabel, set.perceptionAction);
        SetupButton(physicalButton, physicalLabel, set.physicalAction);
        SetupButton(communicationButton, communicationLabel, set.communicationAction);
        SetupButton(specialButton, specialLabel, set.specialAction);

        if (wheelContainer != null)
            wheelContainer.SetActive(true); // Optional, keep it visible
    }

    private void SetupButton(Button button, TextMeshProUGUI label, string actionText)
    {
        bool hasAction = !string.IsNullOrWhiteSpace(actionText);
        button.gameObject.SetActive(hasAction);
        label.text = hasAction ? actionText : "";
    }

    private void ClearButtons()
    {
        SetupButton(perceptionButton, perceptionLabel, "");
        SetupButton(physicalButton, physicalLabel, "");
        SetupButton(communicationButton, communicationLabel, "");
        SetupButton(specialButton, specialLabel, "");
    }
}
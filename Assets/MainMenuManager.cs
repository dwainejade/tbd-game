using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager _;
    [SerializeField] private bool _debugMode;
    public enum MainMenuButtons { play, options, credits, quit };
    [SerializeField] private string _sceneToLoadAfterClickingPlay;
    public void Awake()
    {
        if (_ == null)
        {
            _ = this;
        }
        else
        {
            Debug.LogError("Multiple instances of MainMenuManager found. Destroying this instance.");
        }
    }
    public void MainMenuButtonClicked(MainMenuButtons buttonClicked)
    {
        DebugMessage("Button clicked: " + buttonClicked.ToString());
        switch (buttonClicked)
        {
            case MainMenuButtons.play:
                DebugMessage("Play button clicked.");
                PlayClicked();
                // Load the game scene
                break;
            case MainMenuButtons.options:
                DebugMessage("Options button clicked.");
                // Open options menu
                break;
            case MainMenuButtons.credits:
                DebugMessage("Credits button clicked.");
                // Show credits
                break;
            case MainMenuButtons.quit:
                DebugMessage("Quit button clicked.");
                QuitGame();
                break;
            default:
                Debug.LogError("Unknown button clicked: " + buttonClicked.ToString());
                break;
        }
    }
    private void DebugMessage(string message)
    {
        if (_debugMode)
        {
            Debug.Log(message);
        }
    }

    public void PlayClicked()
    {
        SceneManager.LoadScene(_sceneToLoadAfterClickingPlay);
    }

   public void QuitGame()
    {
        DebugMessage("Quitting game...");
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
    #else
        Application.Quit();
    #endif
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager _;

    [SerializeField] private bool _debugMode;
    [SerializeField] private GameObject _MainMenuContainer;
    [SerializeField] private GameObject _OptionsMenuContainer;
    [SerializeField] private GameObject _CreditsMenuContainer;
    [SerializeField] private string _sceneToLoadAfterClickingPlay;

    public enum MainMenuButtons { play, options, credits, quit }
    public enum CreditsMenuButtons { back }
    public enum OptionsMenuButtons { back }

    private void Awake()
    {
        if (_ == null)
        {
            _ = this;
        }
        else
        {
            Debug.LogError("Multiple instances of MainMenuManager found. Destroying this instance.");
            Destroy(this);
        }
    }

    private void Start()
    {
        OpenMenu(_MainMenuContainer);
    }

    public void MainMenuButtonClicked(MainMenuButtons buttonClicked)
    {
        DebugMessage("Main menu button clicked: " + buttonClicked);
        switch (buttonClicked)
        {
            case MainMenuButtons.play:
                PlayClicked();
                break;
            case MainMenuButtons.options:
                OptionsClicked();
                break;
            case MainMenuButtons.credits:
                CreditsClicked();
                break;
            case MainMenuButtons.quit:
                QuitGame();
                break;
            default:
                Debug.LogError("Unknown button clicked: " + buttonClicked);
                break;
        }
    }

    public void CreditsButtonClicked(CreditsMenuButtons buttonClicked)
    {
        DebugMessage("Credits menu button clicked: " + buttonClicked);
        switch (buttonClicked)
        {
            case CreditsMenuButtons.back:
                OpenMenu(_MainMenuContainer);
                break;
        }
    }

    public void OptionsButtonClicked(OptionsMenuButtons buttonClicked)
    {
        DebugMessage("Options menu button clicked: " + buttonClicked);
        switch (buttonClicked)
        {
            case OptionsMenuButtons.back:
                OpenMenu(_MainMenuContainer);
                break;
        }
    }

    private void PlayClicked()
    {
        DebugMessage("Loading scene: " + _sceneToLoadAfterClickingPlay);
        SceneManager.LoadScene(_sceneToLoadAfterClickingPlay);
    }

    private void OptionsClicked()
    {
        OpenMenu(_OptionsMenuContainer);
    }

    private void CreditsClicked()
    {
        OpenMenu(_CreditsMenuContainer);
    }

    private void QuitGame()
    {
        DebugMessage("Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    private void OpenMenu(GameObject menuToOpen)
    {
        _MainMenuContainer.SetActive(menuToOpen == _MainMenuContainer);
        _OptionsMenuContainer.SetActive(menuToOpen == _OptionsMenuContainer);
        _CreditsMenuContainer.SetActive(menuToOpen == _CreditsMenuContainer);
    }

    private void DebugMessage(string message)
    {
        if (_debugMode)
        {
            Debug.Log(message);
        }
    }
}
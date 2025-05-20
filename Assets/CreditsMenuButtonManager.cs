using UnityEngine;

public class CreditsMenuButtonManager : MonoBehaviour
{
    [SerializeField] private MainMenuManager.CreditsMenuButtons _buttonType;

    public void ButtonClicked()
    {
        MainMenuManager._.CreditsButtonClicked(_buttonType);
    }
}
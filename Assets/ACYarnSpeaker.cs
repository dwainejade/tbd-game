using UnityEngine;
using Yarn.Unity;
using AC;
using System.Threading;
using Yarn;

public class ACYarnSpeaker : DialoguePresenterBase
{
    private AC.Char speaker;
    public string isTalkingParameter = "IsTalking";

    public override YarnTask OnDialogueStartedAsync()
    {
        return YarnTask.CompletedTask;
    }

    public override YarnTask OnDialogueCompleteAsync()
    {
        if (speaker != null && speaker.GetAnimator() != null)
        {
            speaker.GetAnimator().SetBool(isTalkingParameter, false);
        }
        return YarnTask.CompletedTask;
    }

    public override YarnTask RunLineAsync(LocalizedLine dialogueLine, LineCancellationToken cancellationToken)
    {
        string characterName = dialogueLine.CharacterName;
        speaker = !string.IsNullOrEmpty(characterName) ? FindCharacter(characterName) : null;

        if (speaker != null && speaker.GetAnimator() != null)
        {
            speaker.GetAnimator().SetBool(isTalkingParameter, true);
        }

        return YarnTask.CompletedTask;
    }

    public override YarnTask<Yarn.Unity.DialogueOption> RunOptionsAsync(Yarn.Unity.DialogueOption[] options, CancellationToken cancellationToken)
    {
        // We're not handling options — return completed task with null
        return YarnTask.FromResult<Yarn.Unity.DialogueOption>(null);
    }

    private AC.Char FindCharacter(string searchName)
    {
        foreach (var character in KickStarter.stateHandler.Characters)
        {
            if (character.speechLabel == searchName || character.gameObject.name == searchName)
            {
                return character;
            }
        }

        Debug.LogWarning($"ACYarnSpeaker: Couldn't find a Yarn character named '{searchName}'!");
        return null;
    }
}
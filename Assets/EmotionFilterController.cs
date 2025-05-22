using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EmotionFilterController : MonoBehaviour
{
    public Image emotionOverlay; // assign in Inspector

    public void SetEmotionFilter(int emotionId)
    {
        // Kill any existing color tween to prevent conflicts
        emotionOverlay.DOKill();
        
        Color targetColor = new Color(0, 0, 0, 0); // default: transparent

        switch (emotionId)
        {
            case 1: // Happy
                targetColor = new Color(1f, 1f, 0.5f, 0.3f);
                break;
            case 2: // Sad
                targetColor = new Color(0.3f, 0.5f, 1f, 0.3f);
                break;
            case 3: // Curious
                targetColor = new Color(0.5f, 1f, 0.5f, 0.3f);
                break;
            case 4: // Angry
                targetColor = new Color(1f, 0.3f, 0.3f, 0.3f);
                break;
        }

        emotionOverlay.gameObject.SetActive(true);
        emotionOverlay.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0f); // start transparent
        emotionOverlay.DOColor(targetColor, 0.5f); // fade in
    }

    public void ClearFilter()
    {
        // Kill any existing color tween to prevent conflicts
        emotionOverlay.DOKill();
        
        emotionOverlay.DOColor(new Color(0, 0, 0, 0), 0.5f).OnComplete(() =>
        {
            emotionOverlay.gameObject.SetActive(false);
        });
    }
}
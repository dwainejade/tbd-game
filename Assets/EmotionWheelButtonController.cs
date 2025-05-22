using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using AC;

public class EmotionWheelButtonController : MonoBehaviour
{
    public int Id;
    public string itemName;
    public TextMeshProUGUI itemText;
    public Image selectedItemImage;
    public Sprite icon;

    private bool isSelected = false;
    private Vector3 originalScale;

   public EmotionFilterController filterController; // assign via Inspector


    void Start()
    {
        originalScale = transform.localScale;
        itemText.text = itemName;
        selectedItemImage.sprite = icon;
    }

    void Update()
    {
        if (isSelected)
        {
            selectedItemImage.sprite = icon;
            // itemText.text = itemName;
        }
    }

    public void Selected()
    {
        // Prevent Adventure Creator from processing this click
        AC.KickStarter.playerInput.ResetClick();
        
        isSelected = true;
        EmotionWheelController.emotionId = Id;

        if (filterController != null)
        {
            filterController.SetEmotionFilter(Id);
        }
        
        // Hide the wheel immediately when any emotion is selected
        EmotionWheelController wheelController = FindObjectOfType<EmotionWheelController>();
        if (wheelController != null)
        {
            wheelController.HideWheelFromButton();
        }
    }

    public void Deselected()
    {
        isSelected = false;
        EmotionWheelController.emotionId = 0;
        filterController.ClearFilter();
    }

    public void HoverEnter()
    {
        // Scale up slightly
        transform.DOScale(originalScale * 1.1f, 0.25f).SetEase(Ease.OutBack);

        // Optional: Fade in text
        itemText.DOFade(1f, 0.2f);

        itemText.text = itemName;
    }

    public void HoverExit()
    {
        // Scale back to original
        transform.DOScale(originalScale, 0.25f).SetEase(Ease.InBack);

        // Optional: Fade out text
        itemText.DOFade(0f, 0.2f);

        itemText.text = "";
    }
}
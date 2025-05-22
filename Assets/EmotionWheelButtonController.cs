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

    public EmotionFilterController filterController;

    void Start()
    {
        originalScale = transform.localScale;
        itemText.text = itemName;
        selectedItemImage.sprite = icon;
        
        // Check if this is the default emotion (Curious)
        if (Id == 1) // Curious
        {
            isSelected = true;
        }
    }

    void Update()
    {
        // Update selection state based on current emotion
        isSelected = (EmotionWheelController.emotionId == Id);
        
        if (isSelected)
        {
            selectedItemImage.sprite = icon;
        }
    }

    public void Selected()
    {
        Debug.Log($"Button {Id} clicked, current emotion is {EmotionWheelController.emotionId}");
        
        // Prevent Adventure Creator from processing this click
        AC.KickStarter.playerInput.ResetClick();

        // Always hide the wheel when any button is clicked
        EmotionWheelController wheelController = FindObjectOfType<EmotionWheelController>();
        if (wheelController != null)
        {
            wheelController.HideWheelFromButton();
        }

        // If clicking the same emotion, just hide wheel (don't change emotion)
        if (EmotionWheelController.emotionId == Id)
        {
            Debug.Log("Same emotion clicked, only hiding wheel");
            return; // Same emotion selected, wheel already hidden
        }

        Debug.Log($"Switching from emotion {EmotionWheelController.emotionId} to {Id}");
        
        // Switch to new emotion
        EmotionWheelController.emotionId = Id;

        if (filterController != null)
        {
            filterController.SetEmotionFilter(Id);
        }
    }

    public void HoverEnter()
    {
        transform.DOScale(originalScale * 1.1f, 0.25f).SetEase(Ease.OutBack);
        itemText.DOFade(1f, 0.2f);
        itemText.text = itemName;
    }

    public void HoverExit()
    {
        transform.DOScale(originalScale, 0.25f).SetEase(Ease.InBack);
        itemText.DOFade(0f, 0.2f);
        itemText.text = "";
    }
}
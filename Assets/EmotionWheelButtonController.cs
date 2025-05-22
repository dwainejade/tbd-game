using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using AC;
using UnityEngine.EventSystems;

public class EmotionWheelButtonController : MonoBehaviour
{
    public int Id;
    public string itemName;
    public TextMeshProUGUI itemText;
    public Image selectedItemImage;
    public Sprite icon;

    private bool isSelected = false;
    private Vector3 originalScale;
    private UnityEngine.UI.Button button;

    public EmotionFilterController filterController;

    void Start()
    {
        originalScale = transform.localScale;
        itemText.text = itemName;
        selectedItemImage.sprite = icon;
        button = GetComponent<UnityEngine.UI.Button>();
        
        Debug.Log($"Button {Id} ({itemName}) initialized");
        
        // Check if this is the default emotion (ID 1)
        if (Id == 1) // Default emotion
        {
            isSelected = true;
            SetButtonSelected(true);
            Debug.Log($"Button {Id} set as default selected emotion");
        }
    }

    void Update()
    {
        // Update selection state based on current emotion
        bool shouldBeSelected = (EmotionWheelController.emotionId == Id);
        
        if (isSelected != shouldBeSelected)
        {
            Debug.Log($"Button {Id} selection state changing from {isSelected} to {shouldBeSelected}");
            isSelected = shouldBeSelected;
            SetButtonSelected(isSelected);
        }
        
        if (isSelected)
        {
            selectedItemImage.sprite = icon;
        }
    }

    void SetButtonSelected(bool selected)
    {
        if (button != null && selected)
        {
            // Always set this button as selected when it should be
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public void Selected()
    {
        // Debug.Log($"=== SELECTED METHOD CALLED ===");
        // Debug.Log($"Button {Id} ({itemName}) clicked");
        // Debug.Log($"Current EmotionWheelController.emotionId = {EmotionWheelController.emotionId}");
        // Debug.Log($"This button ID = {Id}");
        // Debug.Log($"Are they equal? {EmotionWheelController.emotionId == Id}");
        
        // Prevent Adventure Creator from processing this click
        AC.KickStarter.playerInput.ResetClick();

        // If clicking the same emotion, just hide wheel (don't change emotion)
        if (EmotionWheelController.emotionId == Id)
        {   
            // Hide the wheel for same emotion
            EmotionWheelController wheel = FindAnyObjectByType<EmotionWheelController>();
            if (wheel != null)
            {
                wheel.HideWheelFromButton();
            }
            else
            {
                wheel.HideWheelFromButton();
                Debug.LogError("Could not find EmotionWheelController!");
            }
            
            Debug.Log("Returning from Selected method (same emotion)");
            return; // Same emotion selected, wheel already hidden
        }
        
        // Switch to new emotion
        EmotionWheelController.emotionId = Id;

        if (filterController != null)
        {
            filterController.SetEmotionFilter(Id);
        }

        // Always hide the wheel when any button is clicked
        EmotionWheelController wheelController2 = FindAnyObjectByType<EmotionWheelController>();
        if (wheelController2 != null)
        {
            wheelController2.HideWheelFromButton();
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
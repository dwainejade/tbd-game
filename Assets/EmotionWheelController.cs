using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;

public class EmotionWheelController : MonoBehaviour
{
    public GameObject wheelContainer;
    public Image selectedItem;
    public TextMeshProUGUI selectedItemText;
    public Sprite noImage;
    public static int emotionId = 1; // Default to ID 1

    private bool emotionWheelSelected = false;
    private int lastEmotionId = -1;
    private GameObject lastSelectedButton; // Track which button should stay selected

    public GameObject raycastBlocker; // UI element to block clicks when the wheel is open

    void Start()
    {
        wheelContainer.transform.localScale = Vector3.zero;
        wheelContainer.SetActive(false);
        raycastBlocker.SetActive(false);
        
        // Set default emotion to ID 1 and update UI immediately
        emotionId = 1; // Set to default emotion ID
        lastEmotionId = -1; // Force update on first frame
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed");
            emotionWheelSelected = !emotionWheelSelected;

            if (emotionWheelSelected)
            {
                ShowWheel();
            }
            else
            {
                HideWheel();
            }
        }

        // Check for any mouse click while wheel is open
        if (emotionWheelSelected && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            Debug.Log("Mouse clicked while wheel is open");
            
            // Use a slight delay to let the button clicks process first
            StartCoroutine(CheckForOutsideClick());
        }

        // Update selected emotion icon and text
        UpdateSelectedEmotion();
    }

    System.Collections.IEnumerator CheckForOutsideClick()
    {
        yield return new WaitForEndOfFrame();
        
        // If wheel is still selected after button processing, it means we clicked outside
        if (emotionWheelSelected)
        {
            bool mouseOverWheel = IsMouseOverWheel();
            Debug.Log($"CheckForOutsideClick: wheelSelected={emotionWheelSelected}, mouseOverWheel={mouseOverWheel}");
            
            if (!mouseOverWheel)
            {
                Debug.Log("Clicked outside wheel, hiding");
                HideWheelFromOutsideClick();
                
                // Restore the correct button selection after hiding
                RestoreButtonSelection();
            }
            else
            {
                Debug.Log("Clicked on wheel, not hiding");
            }
        }
    }

    void RestoreButtonSelection()
    {
        // Find the button that should be selected and restore its selection
        EmotionWheelButtonController[] buttons = GetComponentsInChildren<EmotionWheelButtonController>();
        foreach (var button in buttons)
        {
            if (button.Id == emotionId)
            {
                EventSystem.current.SetSelectedGameObject(button.gameObject);
                break;
            }
        }
    }

    bool IsMouseOverWheel()
    {
        // Get mouse position
        Vector2 mousePos = Input.mousePosition;
        
        // Check if mouse is over any UI element using GraphicRaycaster
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = mousePos
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // Check if any of the results are our wheel buttons
        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<EmotionWheelButtonController>() != null)
            {
                return true;
            }
        }
        
        return false;
    }

    public void HideWheelFromOutsideClick()
    {
        if (emotionWheelSelected)
        {
            emotionWheelSelected = false;
            HideWheel();
        }
    }

    void UpdateSelectedEmotion()
    {
        // Check if emotion has changed (just for updating UI, not hiding wheel)
        if (lastEmotionId != emotionId)
        {
            lastEmotionId = emotionId;

            // Find the selected button and update the text
            EmotionWheelButtonController[] buttons = GetComponentsInChildren<EmotionWheelButtonController>();

            foreach (var button in buttons)
            {
                if (button.Id == emotionId)
                {
                    selectedItem.sprite = button.icon;
                    if (selectedItemText != null)
                    {
                        selectedItemText.text = button.itemName;
                    }
                    
                    // Apply the filter
                    if (button.filterController != null)
                    {
                        button.filterController.SetEmotionFilter(emotionId);
                    }
                    
                    // Store reference to the selected button
                    lastSelectedButton = button.gameObject;
                    return;
                }
            }
        }
    }

void ShowWheel()
{
    Debug.Log("ShowWheel triggered");

    // Activate the raycast blocker
    raycastBlocker.SetActive(true);

    wheelContainer.SetActive(true);
    wheelContainer.transform.localScale = Vector3.zero;
    wheelContainer.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
}

void HideWheel()
{
    Debug.Log("HideWheel triggered");

    // disable the raycast blocker
    raycastBlocker.SetActive(false);

    wheelContainer.transform.DOScale(0f, 0.3f)
        .SetEase(Ease.InBack)
        .OnComplete(() =>
        {
            wheelContainer.SetActive(false);
        });
}
    public void HideWheelFromButton()
    {
        Debug.Log($"HideWheelFromButton called - emotionWheelSelected = {emotionWheelSelected}");
        if (emotionWheelSelected)
        {
            Debug.Log("Setting emotionWheelSelected to false and calling HideWheel");
            emotionWheelSelected = false;
            HideWheel();
        }
        else
        {
            Debug.Log("Wheel was already hidden, not calling HideWheel");
        }
    }
}
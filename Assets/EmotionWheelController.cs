using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class EmotionWheelController : MonoBehaviour
{
    public GameObject wheelContainer;
    public Image selectedItem;
    public TextMeshProUGUI selectedItemText;
    public Sprite noImage;
    public static int emotionId;

    private bool emotionWheelSelected = false;
    private int lastEmotionId = -1; // Track the last emotion to detect changes

    void Start()
    {
        wheelContainer.transform.localScale = Vector3.zero;
        wheelContainer.SetActive(false);
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

        // Update selected emotion icon and text
        UpdateSelectedEmotion();
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
                if (button.Id == emotionId && emotionId != 0)
                {
                    selectedItem.sprite = button.icon;
                    if (selectedItemText != null)
                    {
                        selectedItemText.text = button.itemName;
                    }
                    return;
                }
            }

            // If no emotion is selected
            if (emotionId == 0)
            {
                selectedItem.sprite = noImage;
                if (selectedItemText != null)
                {
                    selectedItemText.text = "";
                }
            }
        }
    }
    void ShowWheel()
    {
        Debug.Log("ShowWheel triggered");
        wheelContainer.SetActive(true);
        wheelContainer.transform.localScale = Vector3.zero;
        wheelContainer.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
    }

    void HideWheel()
    {
        Debug.Log("HideWheel triggered");
        wheelContainer.transform.DOScale(0f, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => wheelContainer.SetActive(false));
    }
    public void HideWheelFromButton()
    {
        if (emotionWheelSelected)
        {
            emotionWheelSelected = false;
            HideWheel();
        }
    }
}
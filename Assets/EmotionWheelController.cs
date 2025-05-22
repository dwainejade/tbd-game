using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EmotionWheelController : MonoBehaviour
{
    public GameObject wheelContainer;           // The root UI object for the emotion wheel
    public Image selectedItem;
    public Sprite noImage;
    public static int emotionId;

    private bool emotionWheelSelected = false;

    void Start()
    {
        // Set the wheel to be hidden initially
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

    // Update selected emotion icon
    switch (emotionId)
    {
        case 1:
            Debug.Log("Happy");
            break;
        case 2:
            Debug.Log("Sad");
            break;
        case 3:
            Debug.Log("Curious");
            break;
        case 4:
            Debug.Log("Angry");
            break;
        default:
            selectedItem.sprite = noImage;
            break;
    }
    }

    void ShowWheel()
    {
        Debug.Log("ShowWheel triggered");

        wheelContainer.SetActive(true); // Enable object before animating

        // Instantly set to zero scale to ensure correct starting state
        wheelContainer.transform.localScale = Vector3.zero;

        // Animate scale up with bounce
        wheelContainer.transform.DOScale(1f, 0.4f)
            .SetEase(Ease.OutBack);
    }

    void HideWheel()
    {
        Debug.Log("HideWheel triggered");
        // Animate scale down
        wheelContainer.transform.DOScale(0f, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => wheelContainer.SetActive(false));
    }
}
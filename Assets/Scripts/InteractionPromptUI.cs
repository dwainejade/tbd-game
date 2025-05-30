using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private TextMeshProUGUI objectNameText;
    [SerializeField] private TextMeshProUGUI cycleHintText;
    
    [Header("Input Display")]
    [SerializeField] private string keyboardInteractKey = "E";
    [SerializeField] private string controllerInteractButton = "A";
    [SerializeField] private string controllerCycleButtons = "LB/RB";
    
    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private Vector3 worldOffset = Vector3.up * 2f;
    [SerializeField] private bool followTarget = true;
    
    private InteractableObject currentTarget;
    private Camera playerCamera;
    private CanvasGroup canvasGroup;
    private bool isVisible = false;
    
    void Start()
    {
        // Get references
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();
            
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
        // Start hidden
        canvasGroup.alpha = 0f;
        if (promptPanel != null)
            promptPanel.SetActive(false);
    }
    
    void Update()
    {
        // Update position if following target
        if (followTarget && currentTarget != null && isVisible)
        {
            UpdatePosition();
        }
        
        // Handle fading
        float targetAlpha = isVisible ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
    }
    
    public void ShowPrompt(InteractableObject target, bool showCycleHints, int currentIndex, int totalCount)
    {
        currentTarget = target;
        isVisible = true;
        
        if (promptPanel != null)
            promptPanel.SetActive(true);
        
        // Determine input method
        bool usingController = InteractionManager.Instance.IsUsingController() || Input.GetJoystickNames().Length > 0;
        
        // Update object name
        if (objectNameText != null)
        {
            string objectName = target.name;
            // Try to get a more friendly name from the hotspot
            var hotspot = target.GetHotspot();
            if (hotspot != null && !string.IsNullOrEmpty(hotspot.GetName(0)))
            {
                objectName = hotspot.GetName(0);
            }
            objectNameText.text = objectName;
        }
        
        // Update interaction text
        if (interactionText != null)
        {
            string inputPrompt = usingController ? controllerInteractButton : keyboardInteractKey;
            interactionText.text = $"Press {inputPrompt} to interact";
        }
        
        // Update cycle hint
        if (cycleHintText != null)
        {
            if (showCycleHints && usingController)
            {
                cycleHintText.text = $"{controllerCycleButtons} to cycle ({currentIndex}/{totalCount})";
                cycleHintText.gameObject.SetActive(true);
            }
            else
            {
                cycleHintText.gameObject.SetActive(false);
            }
        }
        
        // Update position
        if (followTarget)
        {
            UpdatePosition();
        }
    }
    
    public void HidePrompt()
    {
        isVisible = false;
        currentTarget = null;
        
        // Hide immediately or after fade
        if (canvasGroup.alpha <= 0.1f && promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }
    
    private void UpdatePosition()
    {
        if (currentTarget == null || playerCamera == null) return;
        
        Vector3 worldPosition = currentTarget.transform.position + worldOffset;
        Vector3 screenPosition = playerCamera.WorldToScreenPoint(worldPosition);
        
        // Check if the object is in front of the camera
        if (screenPosition.z > 0)
        {
            transform.position = screenPosition;
            canvasGroup.alpha = isVisible ? 1f : canvasGroup.alpha;
        }
        else
        {
            // Object is behind camera, hide the prompt
            canvasGroup.alpha = 0f;
        }
    }
    
    // Method to manually set the UI to world space mode
    public void SetWorldSpaceMode(bool enabled)
    {
        followTarget = enabled;
        
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (enabled)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = playerCamera;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }
    }
}
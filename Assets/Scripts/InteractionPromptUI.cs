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
    
    [Header("Screen Position Settings")]
    [SerializeField] private PromptPositionMode positionMode = PromptPositionMode.ProjectToScreen;
    [SerializeField] private Vector2 fixedScreenPosition = new Vector2(0.5f, 0.3f); // Normalized screen coords
    [SerializeField] private Vector2 screenOffset = new Vector2(0, 50f); // Pixel offset from projected position
    [SerializeField] private Vector2 screenMargin = new Vector2(50f, 50f); // Keep this far from screen edges
    
    [Header("Behavior Settings")]
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private bool hideWhenBehindCamera = true;
    [SerializeField] private bool hideWhenTooFar = true;
    [SerializeField] private float maxInteractionDistance = 10f;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool showDistanceIndicator = false;
    [SerializeField] private Image distanceBar;
    [SerializeField] private bool showDirectionArrow = false;
    [SerializeField] private Image directionArrow;
    
    public enum PromptPositionMode
    {
        FixedScreenPosition,     // Always at same screen position (like bottom center)
        ProjectToScreen,         // Project object to screen, clamp to screen bounds
        FollowWithConstraints    // Follow object but stay within screen boundaries
    }
    
    private InteractableObject currentTarget;
    private Camera playerCamera;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;
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
            
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        // Ensure we're using screen space overlay
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        
        // Start hidden
        canvasGroup.alpha = 0f;
        if (promptPanel != null)
            promptPanel.SetActive(false);
            
        // Hide optional elements initially
        if (distanceBar != null) distanceBar.gameObject.SetActive(false);
        if (directionArrow != null) directionArrow.gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (currentTarget != null && isVisible)
        {
            UpdatePosition();
            UpdateVisualFeedback();
        }
        
        // Handle fading
        float targetAlpha = isVisible ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
    }
    
    void UpdatePosition()
    {
        if (currentTarget == null || playerCamera == null) return;
        
        Vector3 targetWorldPos = currentTarget.transform.position;
        Vector3 screenPos = playerCamera.WorldToScreenPoint(targetWorldPos);
        
        // Check if object is behind camera
        bool isBehindCamera = screenPos.z <= 0;
        if (hideWhenBehindCamera && isBehindCamera)
        {
            canvasGroup.alpha = 0f;
            return;
        }
        
        // Check distance
        float distance = Vector3.Distance(playerCamera.transform.position, targetWorldPos);
        if (hideWhenTooFar && distance > maxInteractionDistance)
        {
            canvasGroup.alpha = 0f;
            return;
        }
        
        Vector2 finalPosition = Vector2.zero;
        
        switch (positionMode)
        {
            case PromptPositionMode.FixedScreenPosition:
                finalPosition = GetFixedScreenPosition();
                break;
                
            case PromptPositionMode.ProjectToScreen:
                finalPosition = GetProjectedScreenPosition(screenPos);
                break;
                
            case PromptPositionMode.FollowWithConstraints:
                finalPosition = GetConstrainedFollowPosition(screenPos);
                break;
        }
        
        rectTransform.position = finalPosition;
    }
    
    Vector2 GetFixedScreenPosition()
    {
        // Convert normalized position to screen coordinates
        return new Vector2(
            fixedScreenPosition.x * Screen.width,
            fixedScreenPosition.y * Screen.height
        );
    }
    
    Vector2 GetProjectedScreenPosition(Vector3 screenPos)
    {
        Vector2 position = new Vector2(screenPos.x, screenPos.y) + screenOffset;
        
        // Clamp to screen bounds with margin
        position.x = Mathf.Clamp(position.x, screenMargin.x, Screen.width - screenMargin.x);
        position.y = Mathf.Clamp(position.y, screenMargin.y, Screen.height - screenMargin.y);
        
        return position;
    }
    
    Vector2 GetConstrainedFollowPosition(Vector3 screenPos)
    {
        Vector2 position = new Vector2(screenPos.x, screenPos.y) + screenOffset;
        
        // Get UI element bounds
        Vector2 uiSize = rectTransform.sizeDelta;
        
        // Clamp more precisely based on UI size
        float leftBound = uiSize.x * 0.5f + screenMargin.x;
        float rightBound = Screen.width - (uiSize.x * 0.5f) - screenMargin.x;
        float bottomBound = uiSize.y * 0.5f + screenMargin.y;
        float topBound = Screen.height - (uiSize.y * 0.5f) - screenMargin.y;
        
        position.x = Mathf.Clamp(position.x, leftBound, rightBound);
        position.y = Mathf.Clamp(position.y, bottomBound, topBound);
        
        return position;
    }
    
    void UpdateVisualFeedback()
    {
        if (currentTarget == null || playerCamera == null) return;
        
        float distance = Vector3.Distance(playerCamera.transform.position, currentTarget.transform.position);
        
        // Update distance indicator
        if (showDistanceIndicator && distanceBar != null)
        {
            distanceBar.gameObject.SetActive(true);
            float distanceRatio = Mathf.Clamp01(1f - (distance / maxInteractionDistance));
            distanceBar.fillAmount = distanceRatio;
            
            // Color coding: green when close, red when far
            Color distanceColor = Color.Lerp(Color.red, Color.green, distanceRatio);
            distanceBar.color = distanceColor;
        }
        
        // Update direction arrow (points toward object when off-screen)
        if (showDirectionArrow && directionArrow != null)
        {
            Vector3 screenPos = playerCamera.WorldToScreenPoint(currentTarget.transform.position);
            bool isOffScreen = screenPos.x < 0 || screenPos.x > Screen.width || 
                             screenPos.y < 0 || screenPos.y > Screen.height || 
                             screenPos.z <= 0;
            
            directionArrow.gameObject.SetActive(isOffScreen);
            
            if (isOffScreen)
            {
                // Calculate direction to object
                Vector3 directionToTarget = (currentTarget.transform.position - playerCamera.transform.position).normalized;
                Vector3 forward = playerCamera.transform.forward;
                Vector3 right = playerCamera.transform.right;
                
                // Project direction onto camera plane
                Vector2 screenDirection = new Vector2(
                    Vector3.Dot(directionToTarget, right),
                    Vector3.Dot(directionToTarget, playerCamera.transform.up)
                );
                
                // Rotate arrow to point in direction
                float angle = Mathf.Atan2(screenDirection.y, screenDirection.x) * Mathf.Rad2Deg;
                directionArrow.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
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
            
            // Add distance info if enabled
            if (currentTarget != null && playerCamera != null)
            {
                float distance = Vector3.Distance(playerCamera.transform.position, currentTarget.transform.position);
                if (distance <= maxInteractionDistance)
                {
                    interactionText.text = $"Press {inputPrompt} to interact";
                }
                else
                {
                    interactionText.text = $"Move closer to interact (Distance: {distance:F1}m)";
                }
            }
            else
            {
                interactionText.text = $"Press {inputPrompt} to interact";
            }
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
        
        // Update position immediately
        UpdatePosition();
    }
    
    public void HidePrompt()
    {
        isVisible = false;
        currentTarget = null;
        
        // Hide optional elements
        if (distanceBar != null) distanceBar.gameObject.SetActive(false);
        if (directionArrow != null) directionArrow.gameObject.SetActive(false);
        
        // Hide main panel after fade
        if (canvasGroup.alpha <= 0.1f && promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }
    
    // Public methods for runtime configuration
    public void SetPositionMode(PromptPositionMode mode)
    {
        positionMode = mode;
    }
    
    public void SetFixedPosition(Vector2 normalizedPosition)
    {
        fixedScreenPosition = normalizedPosition;
    }
    
    public void SetScreenOffset(Vector2 offset)
    {
        screenOffset = offset;
    }
    
    public void SetMaxInteractionDistance(float distance)
    {
        maxInteractionDistance = distance;
    }
}
using System.Collections.Generic;
using UnityEngine;
using AC;

public class InteractionManager : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float highlightDistance = 3f;
    [SerializeField] private float interactionDistance = 2f;
    
    [Header("Controller Selection")]
    [SerializeField] private string cycleLeftInput = "CycleHotspotsLeft";
    [SerializeField] private string cycleRightInput = "CycleHotspotsRight";
    [SerializeField] private float cycleCooldown = 0.3f;
    
    [Header("UI")]
    [SerializeField] private InteractionPromptUI promptUI;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGUI = true;
    [SerializeField] private bool logDetection = false;
    
    // Singleton
    public static InteractionManager Instance { get; private set; }
    
    // Tracking
    private List<InteractableObject> allInteractables = new List<InteractableObject>();
    private List<InteractableObject> currentHighlighted = new List<InteractableObject>();
    private List<InteractableObject> interactableObjects = new List<InteractableObject>(); // Objects in interaction range
    private int selectedIndex = 0; // Index of currently selected object for controller
    private float lastCycleTime = 0f;
    
    // AC References
    private Player player;
    
    // Input tracking
    private bool isUsingController = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        Debug.Log("<color=red>[InteractionManager] Starting...</color>");
        
        // Try to get AC player
        player = KickStarter.player;
        if (player != null)
        {
            Debug.Log($"<color=red>[InteractionManager] Found AC player: {player.name}</color>");
        }
        
        // Find UI component if not assigned
        if (promptUI == null)
        {
            promptUI = FindObjectOfType<InteractionPromptUI>();
        }
        
        // Find all interactables in scene
        StartCoroutine(InitializeInteractables());
    }
    
    private System.Collections.IEnumerator InitializeInteractables()
    {
        yield return null;
        
        if (player == null)
        {
            player = KickStarter.player;
        }
        
        InteractableObject[] found = FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
        foreach (var interactable in found)
        {
            RegisterInteractable(interactable);
        }
        
        Debug.Log($"<color=red>[InteractionManager] Initialized with {allInteractables.Count} interactables</color>");
    }
    
    void Update()
    {
        if (player == null)
        {
            if (Time.frameCount % 60 == 0)
            {
                player = KickStarter.player;
                if (player != null)
                {
                    Debug.Log($"<color=red>[InteractionManager] Found AC player: {player.name}</color>");
                }
            }
            return;
        }
        
        // Handle controller input
        HandleControllerInput();
        
        // Main detection logic
        UpdateDetection();
        
        // Update UI
        UpdatePromptUI();
    }
    
    void HandleControllerInput()
    {
        // Check for cycle input
        bool cycleLeft = false;
        bool cycleRight = false;
        
        try
        {
            cycleLeft = AC.KickStarter.playerInput.InputGetButtonDown(cycleLeftInput);
            cycleRight = AC.KickStarter.playerInput.InputGetButtonDown(cycleRightInput);
        }
        catch
        {
            // Fallback to Unity input if AC input fails
            cycleLeft = Input.GetButtonDown(cycleLeftInput);
            cycleRight = Input.GetButtonDown(cycleRightInput);
        }
        
        // Handle cycling with cooldown
        if ((cycleLeft || cycleRight) && Time.time > lastCycleTime + cycleCooldown)
        {
            if (interactableObjects.Count > 1)
            {
                isUsingController = true;
                lastCycleTime = Time.time;
                
                if (cycleLeft)
                {
                    selectedIndex = (selectedIndex - 1 + interactableObjects.Count) % interactableObjects.Count;
                }
                else if (cycleRight)
                {
                    selectedIndex = (selectedIndex + 1) % interactableObjects.Count;
                }
                
                Debug.Log($"<color=red>[Controller] Cycled to: {interactableObjects[selectedIndex].name} (index {selectedIndex})</color>");
            }
        }
        
        // Reset controller mode if no input for a while (optional)
        if (isUsingController && Time.time > lastCycleTime + 3f)
        {
            // Could auto-switch back to automatic selection after no input
        }
    }
    
    void UpdateDetection()
    {
        if (allInteractables.Count == 0) return;
        
        Vector3 playerPos = player.transform.position;
        
        List<InteractableObject> objectsToHighlight = new List<InteractableObject>();
        List<InteractableObject> objectsInRange = new List<InteractableObject>();
        
        // Check all interactables for distance
        foreach (var interactable in allInteractables)
        {
            if (interactable == null) continue;
            
            float distance = Calculate2DDistance(playerPos, interactable.transform.position);
            
            // Check for highlighting (outer range)
            if (distance <= highlightDistance)
            {
                objectsToHighlight.Add(interactable);
            }
            
            // Check for interaction (inner range)
            if (distance <= interactionDistance)
            {
                objectsInRange.Add(interactable);
            }
        }
        
        // Update highlighting for multiple objects
        UpdateMultipleHighlighting(objectsToHighlight);
        
        // Sort interactable objects by priority (facing direction + distance)
        interactableObjects = SortObjectsByPriority(objectsInRange);
        
        // Reset selection if list changed significantly
        if (selectedIndex >= interactableObjects.Count)
        {
            selectedIndex = 0;
            isUsingController = false; // Reset to automatic mode
        }
        
        // Determine which object to interact with
        InteractableObject targetObject = null;
        if (interactableObjects.Count > 0)
        {
            if (isUsingController && selectedIndex < interactableObjects.Count)
            {
                targetObject = interactableObjects[selectedIndex];
            }
            else
            {
                targetObject = interactableObjects[0]; // Highest priority object
                selectedIndex = 0;
            }
        }
        
        // Update AC hotspot
        UpdateACHotspot(targetObject);
    }
    
    List<InteractableObject> SortObjectsByPriority(List<InteractableObject> objects)
    {
        if (objects.Count <= 1) return objects;
        
        Vector3 playerPos = player.transform.position;
        Vector3 playerForward = GetPlayerFacingDirection();
        
        // Create list with priority scores
        List<(InteractableObject obj, float score)> scoredObjects = new List<(InteractableObject, float)>();
        
        foreach (var obj in objects)
        {
            Vector3 toObject = (obj.transform.position - playerPos).normalized;
            
            // Calculate facing score (0 to 1, where 1 means directly in front)
            float facingScore = Vector3.Dot(playerForward, toObject);
            facingScore = (facingScore + 1f) * 0.5f; // Normalize to 0-1
            
            // Calculate distance score (closer = higher score)
            float distance = Calculate2DDistance(playerPos, obj.transform.position);
            float distanceScore = 1f - (distance / interactionDistance);
            
            // Combine scores (facing direction is more important)
            float finalScore = (facingScore * 0.7f) + (distanceScore * 0.3f);
            
            scoredObjects.Add((obj, finalScore));
        }
        
        // Sort by score (highest first)
        scoredObjects.Sort((a, b) => b.score.CompareTo(a.score));
        
        // Extract sorted objects
        List<InteractableObject> sortedObjects = new List<InteractableObject>();
        foreach (var scored in scoredObjects)
        {
            sortedObjects.Add(scored.obj);
        }
        
        return sortedObjects;
    }
    
    Vector3 GetPlayerFacingDirection()
    {
        // For 2D games, we might need to determine facing based on movement or animation
        // This is a simple implementation - you might need to adjust based on your player setup
        
        if (player.GetComponent<Char>() != null)
        {
            // Use AC's character facing direction
            return player.GetComponent<Char>().TransformForward;
        }
        
        // Fallback to transform forward
        return player.transform.forward;
    }
    
    void UpdateMultipleHighlighting(List<InteractableObject> objectsToHighlight)
    {
        // Remove highlighting from objects no longer in range
        for (int i = currentHighlighted.Count - 1; i >= 0; i--)
        {
            var highlightedObj = currentHighlighted[i];
            if (!objectsToHighlight.Contains(highlightedObj))
            {
                highlightedObj.HideOutline();
                currentHighlighted.RemoveAt(i);
            }
        }
        
        // Add highlighting to new objects in range
        Vector3 playerPos = player.transform.position;
        foreach (var obj in objectsToHighlight)
        {
            if (!currentHighlighted.Contains(obj))
            {
                currentHighlighted.Add(obj);
            }
            
            // Determine color based on selection status
            Color highlightColor;
            if (interactableObjects.Count > 0 && interactableObjects.Contains(obj))
            {
                int objIndex = interactableObjects.IndexOf(obj);
                if (objIndex == selectedIndex)
                {
                    highlightColor = Color.green; // Selected for interaction
                }
                else
                {
                    highlightColor = Color.red; // In interaction range but not selected
                }
            }
            else
            {
                highlightColor = Color.yellow; // In highlight range only
            }
            
            obj.ShowOutlineWithColor(highlightColor);
        }
    }
    
    void UpdateACHotspot(InteractableObject targetObject)
    {
        if (targetObject != null)
        {
            var hotspot = targetObject.GetHotspot();
            if (hotspot != null)
            {
                hotspot.gameObject.layer = LayerMask.NameToLayer("Default");
                KickStarter.playerInteraction.SetActiveHotspot(hotspot);
            }
        }
        else
        {
            KickStarter.playerInteraction.SetActiveHotspot(null);
        }
    }
    
    void UpdatePromptUI()
    {
        if (promptUI == null) return;
        
        if (interactableObjects.Count > 0)
        {
            InteractableObject targetObject = interactableObjects[selectedIndex];
            bool showCycleHints = interactableObjects.Count > 1;
            promptUI.ShowPrompt(targetObject, showCycleHints, selectedIndex + 1, interactableObjects.Count);
        }
        else
        {
            promptUI.HidePrompt();
        }
    }
    
    // Helper method to calculate 2D distance
    private float Calculate2DDistance(Vector3 pos1, Vector3 pos2)
    {
        return Vector2.Distance(
            new Vector2(pos1.x, pos1.y),
            new Vector2(pos2.x, pos2.y)
        );
    }
    
    // Public methods for registration
    public void RegisterInteractable(InteractableObject interactable)
    {
        if (interactable != null && !allInteractables.Contains(interactable))
        {
            allInteractables.Add(interactable);
            Debug.Log($"<color=red>[InteractionManager] Registered: {interactable.name}</color>");
        }
    }
    
    public void UnregisterInteractable(InteractableObject interactable)
    {
        if (allInteractables.Remove(interactable))
        {
            Debug.Log($"<color=red>[InteractionManager] Unregistered: {interactable.name}</color>");
            
            // Clean up if this was in any of our lists
            currentHighlighted.Remove(interactable);
            interactableObjects.Remove(interactable);
            
            // Reset selection if needed
            if (selectedIndex >= interactableObjects.Count && interactableObjects.Count > 0)
            {
                selectedIndex = 0;
            }
        }
    }
    
    // Public getters
    public InteractableObject GetCurrentInteractable()
    {
        if (interactableObjects.Count > 0 && selectedIndex < interactableObjects.Count)
        {
            return interactableObjects[selectedIndex];
        }
        return null;
    }
    
    public List<InteractableObject> GetInteractableObjects()
    {
        return new List<InteractableObject>(interactableObjects);
    }
    
    public int GetSelectedIndex()
    {
        return selectedIndex;
    }
    
    public bool IsUsingController()
    {
        return isUsingController;
    }
    
    // Debug GUI (updated)
    void OnGUI()
    {
        if (!showDebugGUI || !Application.isPlaying) return;
        
        Color originalColor = GUI.color;
        GUI.color = Color.red;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 350));
        GUILayout.Label("=== 2D Interaction Manager ===");
        GUILayout.Label($"Player Found: {player != null}");
        GUILayout.Label($"Using Controller: {isUsingController}");
        GUILayout.Label($"Selected Index: {selectedIndex}");
        
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            GUILayout.Label($"Player Pos: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})");
            Vector3 facing = GetPlayerFacingDirection();
            GUILayout.Label($"Player Facing: ({facing.x:F2}, {facing.y:F2}, {facing.z:F2})");
        }
        
        GUILayout.Label($"Total Interactables: {allInteractables.Count}");
        GUILayout.Label($"In Range: {interactableObjects.Count}");
        
        if (interactableObjects.Count > 0)
        {
            GUILayout.Label("--- Interaction Priority ---");
            for (int i = 0; i < interactableObjects.Count; i++)
            {
                string prefix = (i == selectedIndex) ? "[SELECTED] " : $"[{i + 1}] ";
                GUILayout.Label($"{prefix}{interactableObjects[i].name}");
            }
        }
        
        GUILayout.EndArea();
        GUI.color = originalColor;
    }
}
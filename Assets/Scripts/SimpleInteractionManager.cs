using System.Collections.Generic;
using UnityEngine;
using AC;

public class SimpleInteractionManager : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float highlightDistance = 3f;
    [SerializeField] private float interactionDistance = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGUI = true;
    
    // Singleton
    public static SimpleInteractionManager Instance { get; private set; }
    
    // Tracking
    private List<Hotspot> allHotspots = new List<Hotspot>();
    private List<Hotspot> nearbyHotspots = new List<Hotspot>();
    private int selectedIndex = 0;
    
    // AC References
    private Player player;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("<color=green>[SimpleInteractionManager] Initialized</color>");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Wait a frame for AC to initialize
        StartCoroutine(Initialize());
    }
    
    private System.Collections.IEnumerator Initialize()
    {
        yield return null; // Wait one frame
        
        // Get AC player
        player = KickStarter.player;
        if (player != null)
        {
            Debug.Log($"<color=green>[SimpleInteractionManager] Found AC player: {player.name}</color>");
        }
        else
        {
            Debug.LogWarning("[SimpleInteractionManager] No AC player found!");
        }
        
        // Find all hotspots in scene
        FindAllHotspots();
    }
    
    void FindAllHotspots()
    {
        Hotspot[] hotspots = FindObjectsByType<Hotspot>(FindObjectsSortMode.None);
        allHotspots.Clear();
        
        foreach (var hotspot in hotspots)
        {
            allHotspots.Add(hotspot);
        }
        
        Debug.Log($"<color=green>[SimpleInteractionManager] Found {allHotspots.Count} hotspots</color>");
    }
    
    void Update()
    {
        if (player == null)
        {
            // Try to find player again
            player = KickStarter.player;
            return;
        }
        
        // Handle input - using multiple methods for compatibility
        bool interactPressed = false;
        
        // Method 1: Direct key input (most reliable)
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("<color=cyan>[SimpleInteractionManager] E key pressed!</color>");
            interactPressed = true;
        }
        
        // Method 2: Try AC input system
        if (!interactPressed)
        {
            try
            {
                if (KickStarter.playerInput != null && KickStarter.playerInput.InputGetButtonDown("InteractionA"))
                {
                    Debug.Log("<color=cyan>[SimpleInteractionManager] AC InteractionA triggered!</color>");
                    interactPressed = true;
                }
            }
            catch (System.Exception e)
            {
                // Silently fail - AC might not be fully initialized
            }
        }
        
        // Method 3: Unity Input System (if available)
        #if ENABLE_INPUT_SYSTEM
        if (!interactPressed)
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                Debug.Log("<color=cyan>[SimpleInteractionManager] Input System E key!</color>");
                interactPressed = true;
            }
        }
        #endif
        
        if (interactPressed)
        {
            TryInteract();
        }
        
        // Handle cycling through nearby hotspots
        HandleCycling();
        
        // Update detection
        UpdateDetection();
    }
    
    void HandleCycling()
    {
        // Simple cycling with Q/R keys
        if (Input.GetKeyDown(KeyCode.Q) && nearbyHotspots.Count > 1)
        {
            selectedIndex = (selectedIndex - 1 + nearbyHotspots.Count) % nearbyHotspots.Count;
            Debug.Log($"<color=yellow>[SimpleInteractionManager] Cycled to: {nearbyHotspots[selectedIndex].name}</color>");
        }
        
        if (Input.GetKeyDown(KeyCode.R) && nearbyHotspots.Count > 1)
        {
            selectedIndex = (selectedIndex + 1) % nearbyHotspots.Count;
            Debug.Log($"<color=yellow>[SimpleInteractionManager] Cycled to: {nearbyHotspots[selectedIndex].name}</color>");
        }
    }
    
    void UpdateDetection()
    {
        if (allHotspots.Count == 0) return;
        
        Vector3 playerPos = player.transform.position;
        nearbyHotspots.Clear();
        
        // Find hotspots in range
        foreach (var hotspot in allHotspots)
        {
            if (hotspot == null) continue;
            
            float distance = Vector2.Distance(
                new Vector2(playerPos.x, playerPos.y),
                new Vector2(hotspot.transform.position.x, hotspot.transform.position.y)
            );
            
            if (distance <= interactionDistance)
            {
                nearbyHotspots.Add(hotspot);
            }
        }
        
        // Reset selection if needed
        if (selectedIndex >= nearbyHotspots.Count)
        {
            selectedIndex = 0;
        }
        
        // Update AC's active hotspot
        if (nearbyHotspots.Count > 0 && selectedIndex < nearbyHotspots.Count)
        {
            try
            {
                KickStarter.playerInteraction.SetActiveHotspot(nearbyHotspots[selectedIndex]);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SimpleInteractionManager] Failed to set active hotspot: {e.Message}");
            }
        }
        else
        {
            try
            {
                KickStarter.playerInteraction.SetActiveHotspot(null);
            }
            catch (System.Exception e)
            {
                // Silently fail
            }
        }
    }
    
    void TryInteract()
    {
        if (nearbyHotspots.Count > 0 && selectedIndex < nearbyHotspots.Count)
        {
            Hotspot targetHotspot = nearbyHotspots[selectedIndex];
            Debug.Log($"<color=green>[SimpleInteractionManager] Interacting with: {targetHotspot.name}</color>");
            
            try
            {
                targetHotspot.RunUseInteraction();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SimpleInteractionManager] Failed to interact: {e.Message}");
            }
        }
        else
        {
            Debug.Log("[SimpleInteractionManager] No hotspot to interact with");
        }
    }
    
    // Debug GUI
    void OnGUI()
    {
        if (!showDebugGUI || !Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== Simple Interaction Manager ===");
        GUILayout.Label($"Player Found: {player != null}");
        GUILayout.Label($"Total Hotspots: {allHotspots.Count}");
        GUILayout.Label($"Nearby Hotspots: {nearbyHotspots.Count}");
        GUILayout.Label($"Selected Index: {selectedIndex}");
        
        if (nearbyHotspots.Count > 0)
        {
            GUILayout.Label("--- Nearby Hotspots ---");
            for (int i = 0; i < nearbyHotspots.Count; i++)
            {
                string prefix = (i == selectedIndex) ? "[SELECTED] " : $"[{i + 1}] ";
                GUILayout.Label($"{prefix}{nearbyHotspots[i].name}");
            }
        }
        
        GUILayout.Label("");
        GUILayout.Label("Controls:");
        GUILayout.Label("E - Interact");
        GUILayout.Label("Q/R - Cycle hotspots");
        
        GUILayout.EndArea();
    }
}
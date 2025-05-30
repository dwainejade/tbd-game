using UnityEngine;
using AC;

public class SimpleDebugTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== INTERACTION SYSTEM DEBUG ===");
        
        // Check if AC player exists
        Player player = KickStarter.player;
        Debug.Log($"AC Player found: {player != null}");
        if (player != null)
            Debug.Log($"Player position: {player.transform.position}");
        
        // Check if InteractionManager exists
        InteractionManager manager = InteractionManager.Instance;
        Debug.Log($"InteractionManager found: {manager != null}");

        // Check for InteractableObjects in scene
        InteractableObject[] interactables = FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
        Debug.Log($"InteractableObjects found: {interactables.Length}");
        
        foreach (var obj in interactables)
        {
            Debug.Log($"- {obj.name} has Hotspot: {obj.GetComponent<Hotspot>() != null}");
        }
        
        // Check AC settings
        Debug.Log($"AC Hotspot detection method: {KickStarter.settingsManager.hotspotDetection}");
        Debug.Log($"AC Movement method: {KickStarter.settingsManager.movementMethod}");
    }
    
    void Update()
    {
        // Simple distance check
        Player player = KickStarter.player;
        if (player != null)
        {
            InteractableObject[] interactables = FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
            foreach (var obj in interactables)
            {
                float distance = Vector3.Distance(player.transform.position, obj.transform.position);
                if (distance < 5f) // Close enough to see some activity
                {
                    Debug.Log($"Player close to {obj.name}: distance = {distance:F2}");
                }
            }
        }
    }
}
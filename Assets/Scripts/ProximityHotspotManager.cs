using UnityEngine;
using AC;
using System.Collections.Generic;

public class ProximityHotspotManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode cycleKey = KeyCode.Tab;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Controller Support")]
    [SerializeField] private string cycleButtonName = "Fire2"; // Right mouse/controller button
    [SerializeField] private string interactButtonName = "Fire1"; // Left mouse/controller button

    [Header("Highlight Colors")]
    [SerializeField] private Color nearbyHighlightColor = Color.white;
    [SerializeField] private Color selectedHighlightColor = Color.yellow;

    [Header("Debug")]
    public bool enableDebugLogging = true;

    [Header("AC Integration")]
    [SerializeField] private bool disableACInteractionForSelectedHotspot = true;

    private List<HotspotMaterialChanger> nearbyHotspots = new List<HotspotMaterialChanger>();
    private int currentIndex = -1;

    void Update()
    {
        // Simple input detection that shouldn't conflict with AC
        bool cyclePressed = Input.GetKeyDown(cycleKey) || Input.GetButtonDown(cycleButtonName);
        bool interactPressed = Input.GetKeyDown(interactKey) || Input.GetButtonDown(interactButtonName);

        if (enableDebugLogging && cyclePressed)
            Debug.Log("Cycle input detected");
        if (enableDebugLogging && interactPressed)
            Debug.Log("Interact input detected");

        // Handle cycling
        if (cyclePressed && nearbyHotspots.Count > 1)
        {
            CycleToNextHotspot();
        }

        // Handle interaction - only if we have a selected hotspot and game is in normal state
        if (interactPressed && currentIndex >= 0)
        {
            // Check if we should handle this interaction or let AC handle it
            if (ShouldHandleInteraction())
            {
                InteractWithCurrentHotspot();
            }
        }
    }

    private bool ShouldHandleInteraction()
    {
        // Only handle if we're in normal gameplay and have a selected hotspot
        if (AC.KickStarter.stateHandler != null && AC.KickStarter.stateHandler.gameState != AC.GameState.Normal)
            return false;

        // If we're configured to disable AC interaction for our selected hotspot, handle it ourselves
        if (disableACInteractionForSelectedHotspot && currentIndex >= 0 && currentIndex < nearbyHotspots.Count)
            return true;

        return false;
    }

    private void InteractWithCurrentHotspot()
    {
        if (currentIndex >= 0 && currentIndex < nearbyHotspots.Count)
        {
            Hotspot acHotspot = nearbyHotspots[currentIndex].GetComponent<Hotspot>();
            if (acHotspot != null)
            {
                if (enableDebugLogging)
                    Debug.Log($"Manually interacting with hotspot: {acHotspot.name}");

                // Trigger the hotspot's interaction manually
                acHotspot.RunUseInteraction();
            }
        }
    }

    public void RegisterHotspot(HotspotMaterialChanger hotspot)
    {
        if (!nearbyHotspots.Contains(hotspot))
        {
            nearbyHotspots.Add(hotspot);

            if (enableDebugLogging)
                Debug.Log($"Registered hotspot: {hotspot.name} (Total: {nearbyHotspots.Count})");

            // Set this hotspot as "nearby" with the nearby color
            hotspot.SetHighlightColor(nearbyHighlightColor, false);

            // If this is the first hotspot, make it selected
            if (nearbyHotspots.Count == 1)
            {
                SetActiveHotspot(0);
            }
        }
    }

    public void UnregisterHotspot(HotspotMaterialChanger hotspot)
    {
        int index = nearbyHotspots.IndexOf(hotspot);
        if (index >= 0)
        {
            // Remove highlight
            hotspot.RemoveHighlight();

            // If this was the active hotspot, clear AC's selection
            if (index == currentIndex)
            {
                AC.KickStarter.playerInteraction.SetActiveHotspot(null);
            }

            nearbyHotspots.RemoveAt(index);

            if (enableDebugLogging)
                Debug.Log($"Unregistered hotspot: {hotspot.name} (Remaining: {nearbyHotspots.Count})");

            // Adjust current index
            if (currentIndex >= nearbyHotspots.Count)
                currentIndex = nearbyHotspots.Count - 1;

            // Set new active hotspot
            if (nearbyHotspots.Count > 0 && currentIndex >= 0)
            {
                SetActiveHotspot(currentIndex);
            }
            else
            {
                currentIndex = -1;
                AC.KickStarter.playerInteraction.SetActiveHotspot(null);
            }
        }
    }

    void CycleToNextHotspot()
    {
        if (nearbyHotspots.Count <= 1) return;

        // Set current back to just "nearby" color
        if (currentIndex >= 0 && currentIndex < nearbyHotspots.Count)
        {
            nearbyHotspots[currentIndex].SetHighlightColor(nearbyHighlightColor, false);
        }

        // Move to next
        currentIndex = (currentIndex + 1) % nearbyHotspots.Count;

        SetActiveHotspot(currentIndex);

        if (enableDebugLogging)
            Debug.Log($"Cycled to hotspot: {nearbyHotspots[currentIndex].name} ({currentIndex + 1}/{nearbyHotspots.Count})");
    }

    void SetActiveHotspot(int index)
    {
        if (index >= 0 && index < nearbyHotspots.Count)
        {
            currentIndex = index;
            nearbyHotspots[index].SetHighlightColor(selectedHighlightColor, true);

            // Tell AC about the active hotspot
            Hotspot acHotspot = nearbyHotspots[index].GetComponent<Hotspot>();
            if (acHotspot != null)
            {
                AC.KickStarter.playerInteraction.SetActiveHotspot(acHotspot);

                if (enableDebugLogging)
                    Debug.Log($"Set AC active hotspot to: {acHotspot.name}");
            }
        }
    }

    public void ClearAllHotspots()
    {
        foreach (var hotspot in nearbyHotspots)
        {
            if (hotspot != null)
                hotspot.RemoveHighlight();
        }

        nearbyHotspots.Clear();
        currentIndex = -1;

        // Clear AC's active hotspot
        AC.KickStarter.playerInteraction.SetActiveHotspot(null);
    }

    // Method to let AC handle the interaction instead of us
    public void LetACHandleInteraction()
    {
        disableACInteractionForSelectedHotspot = false;
    }

    // Method to take over interaction handling
    public void TakeOverInteractionHandling()
    {
        disableACInteractionForSelectedHotspot = true;
    }

    // Public methods to change colors at runtime
    public void SetNearbyColor(Color color)
    {
        nearbyHighlightColor = color;
        UpdateAllHighlights();
    }

    public void SetSelectedColor(Color color)
    {
        selectedHighlightColor = color;
        UpdateAllHighlights();
    }

    void UpdateAllHighlights()
    {
        for (int i = 0; i < nearbyHotspots.Count; i++)
        {
            if (i == currentIndex)
            {
                nearbyHotspots[i].SetHighlightColor(selectedHighlightColor, true);
            }
            else
            {
                nearbyHotspots[i].SetHighlightColor(nearbyHighlightColor, false);
            }
        }
    }
}
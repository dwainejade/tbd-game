using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InputSys = UnityEngine.InputSystem;
using AC;

public class DetectionCone : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private bool logDirection = true;
    [SerializeField] private float logInterval = 1f;
    [SerializeField] private bool logInteractables = true;

    [Header("Vision Cone")]
    [SerializeField] private Transform triangleSprite; // Can be any invisible box or sprite
    [SerializeField] private bool rotateTriangle = true;

    [Header("Cycling Settings")]
    [SerializeField] private string cycleLeftInput = "CycleHotspotsLeft";
    [SerializeField] private string cycleRightInput = "CycleHotspotsRight";
    [SerializeField] private float cycleCooldown = 0.3f;
    [SerializeField] private Color selectedHighlightColor = Color.yellow;
    [SerializeField] private Color unselectedHighlightColor = Color.white;
    [SerializeField] private bool autoSelectClosest = true;

    [Header("Input Systems")]
    [SerializeField] private ButtonCaptureSystem buttonCapture;
    [SerializeField] private UnityEngine.InputSystem.PlayerInput playerInput;

    // AC References
    private Player player;
    private float lastLogTime = 0f;
    private float lastCycleTime = 0f;

    // Detected interactables
    private readonly HashSet<InteractableObject> currentInteractables = new HashSet<InteractableObject>();
    private List<InteractableObject> sortedInteractables = new List<InteractableObject>();
    private int selectedIndex = -1;
    private InteractableObject selectedInteractable = null;

    void Start()
    {
        player = KickStarter.player;

        // Auto-find input systems if not assigned
        if (playerInput == null)
        {
            playerInput = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();
        }

        if (buttonCapture == null)
        {
            buttonCapture = FindObjectOfType<ButtonCaptureSystem>();
        }
    }

    void Update()
    {
        if (player == null)
        {
            if (Time.frameCount % 60 == 0)
            {
                player = KickStarter.player;
            }
            return;
        }

        Vector3 direction = GetPlayerFacingDirection();

        if (logDirection && Time.time > lastLogTime + logInterval)
        {
            Debug.Log($"Player Facing Direction: ({direction.x:F2}, {direction.y:F2}, {direction.z:F2})");

            // Log currently detected interactables
            if (logInteractables && currentInteractables.Count > 0)
            {
                string interactableNames = string.Join(", ", System.Linq.Enumerable.Select(currentInteractables, i => i.name));
                Debug.Log($"Currently detecting {currentInteractables.Count} interactables: {interactableNames}");
                if (selectedInteractable != null)
                {
                    Debug.Log($"Selected: {selectedInteractable.name} (index {selectedIndex})");
                }
            }

            lastLogTime = Time.time;
        }

        if (rotateTriangle && triangleSprite != null)
        {
            UpdateTriangleDirection(direction);
        }

        // Handle cycling input
        HandleCyclingInput();

        // Handle interaction input
        HandleInteractionInput();
    }

    void LateUpdate()
    {
        // Clean up dead/disabled interactables
        currentInteractables.RemoveWhere(i => i == null || !i.gameObject.activeInHierarchy);
    }

    Vector3 GetPlayerFacingDirection()
    {
        if (player != null && player.GetComponent<Char>() != null)
        {
            Vector3 forward = player.GetComponent<Char>().TransformForward;
            forward.y = 0; // 2D plane
            return forward.normalized;
        }

        Vector3 fallback = player.transform.forward;
        fallback.y = 0;
        return fallback.normalized;
    }

    public Vector3 GetDirection()
    {
        if (player == null) return Vector3.forward;
        return GetPlayerFacingDirection();
    }

    void UpdateTriangleDirection(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        Direction2D dir = GetCardinalDirection(direction);

        switch (dir)
        {
            case Direction2D.Right:
                triangleSprite.localPosition = new Vector3(2.0f, 1.5f, 0);
                triangleSprite.localRotation = Quaternion.Euler(0, 0, 90f);
                break;
            case Direction2D.Left:
                triangleSprite.localPosition = new Vector3(-2.0f, 1.5f, 0);
                triangleSprite.localRotation = Quaternion.Euler(0, 0, -90f);
                break;
            case Direction2D.Down:
                triangleSprite.localPosition = new Vector3(0, -2.0f, 0);
                triangleSprite.localRotation = Quaternion.Euler(0, 0, 0f);
                break;
            case Direction2D.Up:
                triangleSprite.localPosition = new Vector3(0, 2.25f, 0);
                triangleSprite.localRotation = Quaternion.Euler(0, 0, 180f);
                break;
            case Direction2D.UpRight:
                triangleSprite.localPosition = new Vector3(1.0f, 2.25f, 0);
                triangleSprite.localRotation = Quaternion.Euler(0, 0, 135f);
                break;
            case Direction2D.UpLeft:
                triangleSprite.localPosition = new Vector3(-1.0f, 2.25f, 0);
                triangleSprite.localRotation = Quaternion.Euler(0, 0, -135f);
                break;
            case Direction2D.DownRight:
                triangleSprite.localPosition = new Vector3(2.0f, 0.75f, 0);
                triangleSprite.localRotation = Quaternion.Euler(0, 0, 45f);
                break;
            case Direction2D.DownLeft:
                triangleSprite.localPosition = new Vector3(-2.0f, 0.75f, 0);
                triangleSprite.localRotation = Quaternion.Euler(0, 0, -45f);
                break;
        }
    }

    Direction2D GetCardinalDirection(Vector3 dir)
    {
        dir.Normalize();

        float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        angle = (angle + 360) % 360;

        if (angle >= 337.5 || angle < 22.5) return Direction2D.Right;
        if (angle >= 22.5 && angle < 67.5) return Direction2D.UpRight;
        if (angle >= 67.5 && angle < 112.5) return Direction2D.Up;
        if (angle >= 112.5 && angle < 157.5) return Direction2D.UpLeft;
        if (angle >= 157.5 && angle < 202.5) return Direction2D.Left;
        if (angle >= 202.5 && angle < 247.5) return Direction2D.DownLeft;
        if (angle >= 247.5 && angle < 292.5) return Direction2D.Down;
        if (angle >= 292.5 && angle < 337.5) return Direction2D.DownRight;

        return Direction2D.Down;
    }

    enum Direction2D
    {
        Up, Down, Left, Right,
        UpLeft, UpRight, DownLeft, DownRight
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        InteractableObject interactable = FindInteractableObject(other);

        if (interactable != null && currentInteractables.Add(interactable))
        {
            if (logInteractables)
            {
                Debug.Log($"[DetectionCone] Entered: {interactable.name} (detected via {other.name}) (Total: {currentInteractables.Count})");
                Debug.Log($"  - Supports outline: {interactable.SupportsOutline()}");
            }

            // Update sorted list and selection
            UpdateSortedList();

            // Show outline with appropriate color
            UpdateInteractableHighlights();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        InteractableObject interactable = FindInteractableObject(other);

        if (interactable != null && currentInteractables.Remove(interactable))
        {
            if (logInteractables)
            {
                Debug.Log($"[DetectionCone] Exited: {interactable.name} (detected via {other.name}) (Total: {currentInteractables.Count})");
            }

            // Hide outline for this specific object
            interactable.HideOutline();

            // Update sorted list and selection
            UpdateSortedList();

            // Update highlights for remaining objects
            UpdateInteractableHighlights();
        }
    }

    void HandleCyclingInput()
    {
        if (currentInteractables.Count <= 1) return;

        bool cycleLeft = false;
        bool cycleRight = false;

        // Method 1: Button Capture System (preferred)
        if (buttonCapture != null)
        {
            cycleLeft = buttonCapture.GetCycleLeftPressed();
            cycleRight = buttonCapture.GetCycleRightPressed();
        }

        // Method 2: Unity Input System
        if (!cycleLeft && !cycleRight && playerInput != null && playerInput.actions != null)
        {
            var cycleLeftAction = playerInput.actions[cycleLeftInput];
            var cycleRightAction = playerInput.actions[cycleRightInput];

            if (cycleLeftAction != null && cycleLeftAction.triggered)
            {
                cycleLeft = true;
            }
            if (cycleRightAction != null && cycleRightAction.triggered)
            {
                cycleRight = true;
            }
        }

        // Method 3: AC Input System
        if (!cycleLeft && !cycleRight)
        {
            try
            {
                if (AC.KickStarter.playerInput != null)
                {
                    if (AC.KickStarter.playerInput.InputGetButtonDown(cycleLeftInput))
                    {
                        cycleLeft = true;
                    }
                    if (AC.KickStarter.playerInput.InputGetButtonDown(cycleRightInput))
                    {
                        cycleRight = true;
                    }
                }
            }
            catch (System.Exception _)
            {
                // Silent handling of AC Input exceptions
            }
        }

        // Method 4: Unity Legacy Input (fallback)
        if (!cycleLeft && !cycleRight)
        {
            try
            {
                if (Input.GetButtonDown(cycleLeftInput))
                {
                    cycleLeft = true;
                }
                if (Input.GetButtonDown(cycleRightInput))
                {
                    cycleRight = true;
                }
            }
            catch (System.Exception _)
            {
                // Silent handling if input not defined
            }
        }

        // Method 5: Direct key input (fallback)
        if (!cycleLeft && !cycleRight)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    cycleLeft = true;
                }
                else
                {
                    cycleRight = true;
                }
            }
        }

        // Apply cycling with cooldown
        if ((cycleLeft || cycleRight) && Time.time > lastCycleTime + cycleCooldown)
        {
            lastCycleTime = Time.time;

            if (cycleLeft)
            {
                CycleSelection(-1);
            }
            else if (cycleRight)
            {
                CycleSelection(1);
            }
        }
    }

    void HandleInteractionInput()
    {
        // Test multiple input methods for interaction
        bool interactPressed = false;

        // Method 1: Button Capture System (preferred)
        if (buttonCapture != null && buttonCapture.GetInteractPressed())
        {
            interactPressed = true;
        }

        // Method 2: Unity Input System
        if (!interactPressed && playerInput != null && playerInput.actions != null)
        {
            var interactAction = playerInput.actions["Interact"];
            if (interactAction != null && interactAction.triggered)
            {
                interactPressed = true;
            }
        }

        // Method 3: AC Input System
        if (!interactPressed)
        {
            try
            {
                if (AC.KickStarter.playerInput != null && AC.KickStarter.playerInput.InputGetButtonDown("InteractionA"))
                {
                    interactPressed = true;
                }
            }
            catch (System.Exception _)
            {
                // Silent handling of AC Input exceptions
            }
        }

        // Method 4: Direct key input (fallback)
        if (!interactPressed && Input.GetKeyDown(KeyCode.E))
        {
            interactPressed = true;
        }

        // If any input method detected interaction
        if (interactPressed)
        {
            InteractWithSelected();
        }
    }

    void CycleSelection(int direction)
    {
        if (sortedInteractables.Count == 0) return;

        // Calculate new index
        if (selectedIndex == -1)
        {
            selectedIndex = direction > 0 ? 0 : sortedInteractables.Count - 1;
        }
        else
        {
            selectedIndex = (selectedIndex + direction + sortedInteractables.Count) % sortedInteractables.Count;
        }

        selectedInteractable = sortedInteractables[selectedIndex];

        if (logInteractables)
        {
            Debug.Log($"[DetectionCone] Cycled to: {selectedInteractable.name} (index {selectedIndex})");
        }

        UpdateInteractableHighlights();
    }

    void UpdateSortedList()
    {
        // Convert HashSet to List and sort by distance
        sortedInteractables = currentInteractables
            .Where(i => i != null && i.gameObject.activeInHierarchy)
            .OrderBy(i => Vector3.Distance(transform.position, i.GetPosition()))
            .ToList();

        // Update selection
        if (sortedInteractables.Count == 0)
        {
            selectedIndex = -1;
            selectedInteractable = null;
        }
        else if (autoSelectClosest && (selectedInteractable == null || !currentInteractables.Contains(selectedInteractable)))
        {
            // Auto-select closest object
            selectedIndex = 0;
            selectedInteractable = sortedInteractables[0];
        }
        else if (selectedInteractable != null && currentInteractables.Contains(selectedInteractable))
        {
            // Update index of currently selected object
            selectedIndex = sortedInteractables.IndexOf(selectedInteractable);
        }
        else
        {
            // Selected object no longer in range, select closest
            selectedIndex = 0;
            selectedInteractable = sortedInteractables.Count > 0 ? sortedInteractables[0] : null;
        }
    }

    void UpdateInteractableHighlights()
    {
        foreach (var interactable in currentInteractables)
        {
            if (interactable == selectedInteractable)
            {
                // Selected object gets selected highlight color
                interactable.ShowOutlineWithColor(selectedHighlightColor);
            }
            else
            {
                // Non-selected objects get unselected highlight color
                interactable.ShowOutlineWithColor(unselectedHighlightColor);
            }
        }
    }

    // Helper method to find InteractableObject in hierarchy
    private InteractableObject FindInteractableObject(Collider2D collider)
    {
        // Method 1: Direct component check
        InteractableObject interactable = collider.GetComponent<InteractableObject>();
        if (interactable != null) return interactable;

        // Method 2: Check if this collider belongs to a Hotspot, then find InteractableObject that references this Hotspot
        Hotspot hotspot = collider.GetComponent<Hotspot>();
        if (hotspot != null)
        {
            // Find all InteractableObjects in the scene and check if any reference this Hotspot
            InteractableObject[] allInteractables = FindObjectsOfType<InteractableObject>();
            foreach (var io in allInteractables)
            {
                if (io.GetHotspot() == hotspot)
                {
                    return io;
                }
            }
        }

        // Method 3: Check parent objects (walk up the hierarchy)
        Transform current = collider.transform.parent;
        while (current != null)
        {
            interactable = current.GetComponent<InteractableObject>();
            if (interactable != null) return interactable;
            current = current.parent;
        }

        // Method 4: Check child objects (walk down the hierarchy)
        interactable = collider.GetComponentInChildren<InteractableObject>();
        if (interactable != null) return interactable;

        // Method 5: Check siblings and parent for InteractableObject (fallback)
        Transform parent = collider.transform.parent;
        if (parent != null)
        {
            interactable = parent.GetComponent<InteractableObject>();
            if (interactable != null) return interactable;

            // Check other children of the parent
            interactable = parent.GetComponentInChildren<InteractableObject>();
            if (interactable != null) return interactable;
        }

        return null;
    }

    // Optional: Public method to get currently detected interactables
    public List<InteractableObject> GetCurrentInteractables()
    {
        return new List<InteractableObject>(sortedInteractables);
    }

    // Optional: Public method to check if a specific interactable is detected
    public bool IsDetecting(InteractableObject interactable)
    {
        return currentInteractables.Contains(interactable);
    }

    // Public method to get the currently selected interactable
    public InteractableObject GetSelectedInteractable()
    {
        return selectedInteractable;
    }

    // Public method to interact with the selected object
    public void InteractWithSelected()
    {
        if (selectedInteractable != null && selectedInteractable.GetHotspot() != null)
        {
            // Use AC's proper interaction method (same as InteractionManager)
            Hotspot hotspot = selectedInteractable.GetHotspot();
            if (hotspot != null)
            {
                hotspot.RunUseInteraction();

                if (logInteractables)
                {
                    Debug.Log($"[DetectionCone] Interacting with: {selectedInteractable.name}");
                }
            }
        }
    }

    // Public method to manually select an interactable
    public void SelectInteractable(InteractableObject interactable)
    {
        if (currentInteractables.Contains(interactable))
        {
            selectedInteractable = interactable;
            selectedIndex = sortedInteractables.IndexOf(interactable);
            UpdateInteractableHighlights();
        }
    }

    // Public method to clear selection
    public void ClearSelection()
    {
        selectedIndex = -1;
        selectedInteractable = null;
        UpdateInteractableHighlights();
    }
}
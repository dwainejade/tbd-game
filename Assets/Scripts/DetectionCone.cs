using System.Collections.Generic;
using UnityEngine;
using AC;

public class DetectionCone : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private bool logDirection = true;
    [SerializeField] private float logInterval = 1f;
    [SerializeField] private bool logInteractables = true; // New: Enable/disable interactable logging

    [Header("Vision Cone")]
    [SerializeField] private Transform triangleSprite; // Can be any invisible box or sprite
    [SerializeField] private bool rotateTriangle = true;

    // AC References
    private Player player;
    private float lastLogTime = 0f;

    // Detected interactables
    private readonly HashSet<InteractableObject> currentInteractables = new HashSet<InteractableObject>();

    void Start()
    {
        player = KickStarter.player;
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
            }

            lastLogTime = Time.time;
        }

        if (rotateTriangle && triangleSprite != null)
        {
            UpdateTriangleDirection(direction);
        }
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
                triangleSprite.localPosition = new Vector3(1.0f, 2.5f, 0);
                triangleSprite.localRotation = Quaternion.Euler(0, 0, 135f);
                break;
            case Direction2D.UpLeft:
                triangleSprite.localPosition = new Vector3(-1.0f, 2.5f, 0);
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
            interactable.ShowOutline();
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
            interactable.HideOutline();
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
    public HashSet<InteractableObject> GetCurrentInteractables()
    {
        return new HashSet<InteractableObject>(currentInteractables);
    }

    // Optional: Public method to check if a specific interactable is detected
    public bool IsDetecting(InteractableObject interactable)
    {
        return currentInteractables.Contains(interactable);
    }
}
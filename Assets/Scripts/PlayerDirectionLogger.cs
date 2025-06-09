using UnityEngine;
using AC;

public class PlayerDirectionLogger : MonoBehaviour
{
    [Header("Direction Detection Settings")]
    public float directionThreshold = 0.1f; // Minimum movement to register direction change
    public bool logContinuously = false; // Set to true for continuous logging

    private Vector3 lastPosition;
    private Vector3 currentDirection;
    private float lastDirectionAngle;

    // Reference to Adventure Creator player
    private Player player;

    void Start()
    {
        // Get the Adventure Creator player
        player = KickStarter.player;

        if (player != null)
        {
            lastPosition = player.transform.position;
            Debug.Log("Player Direction Logger initialized");
        }
        else
        {
            Debug.LogWarning("Adventure Creator Player not found!");
        }
    }

    void Update()
    {
        if (player == null) return;

        DetectPlayerDirection();
    }

    void DetectPlayerDirection()
    {
        Vector3 currentPosition = player.transform.position;
        Vector3 movement = currentPosition - lastPosition;

        // Only process if there's significant movement
        if (movement.magnitude > directionThreshold)
        {
            // Calculate direction vector (normalized)
            currentDirection = movement.normalized;

            // Calculate angle in degrees (0 = right, 90 = up, 180 = left, 270 = down)
            float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;

            // Normalize angle to 0-360 range
            if (angle < 0) angle += 360;

            // Check if direction changed significantly
            if (Mathf.Abs(Mathf.DeltaAngle(lastDirectionAngle, angle)) > 10f || logContinuously)
            {
                LogDirection(angle);
                lastDirectionAngle = angle;
            }

            lastPosition = currentPosition;
        }
    }

    void LogDirection(float angle)
    {
        string direction = GetCardinalDirection(angle);

        Debug.Log($"Player Direction: {direction} (Angle: {angle:F1}°)");
        Debug.Log($"Direction Vector: {currentDirection}");

        // You can add events here later for cone rotation
        OnDirectionChanged(angle, direction);
    }

    string GetCardinalDirection(float angle)
    {
        // Convert angle to 8-directional system
        if (angle >= 337.5f || angle < 22.5f)
            return "East";
        else if (angle >= 22.5f && angle < 67.5f)
            return "Northeast";
        else if (angle >= 67.5f && angle < 112.5f)
            return "North";
        else if (angle >= 112.5f && angle < 157.5f)
            return "Northwest";
        else if (angle >= 157.5f && angle < 202.5f)
            return "West";
        else if (angle >= 202.5f && angle < 247.5f)
            return "Southwest";
        else if (angle >= 247.5f && angle < 292.5f)
            return "South";
        else
            return "Southeast";
    }

    // Event method for when direction changes - we'll use this later for cone rotation
    void OnDirectionChanged(float angle, string direction)
    {
        // This is where we'll add cone rotation logic later
        // For now, just log the change
        Debug.Log($"Direction changed to: {direction}");
    }

    // Public method to get current direction (useful for other scripts)
    public Vector3 GetCurrentDirection()
    {
        return currentDirection;
    }

    public float GetCurrentAngle()
    {
        return lastDirectionAngle;
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using AC;

public class ButtonCaptureSystem : MonoBehaviour
{
    [Header("Button Capture Settings")]
    public bool isCapturing = false;
    public string captureTarget = ""; // What we're currently capturing for

    [Header("Captured Inputs")]
    public string interactButton = "buttonSouth"; // Cross/A
    public string cycleLeftButton = "leftShoulder"; // L1/LB
    public string cycleRightButton = "rightShoulder"; // R1/RB

    [Header("Display")]
    public bool showCaptureGUI = true;
    public bool showCurrentInputs = true;

    // Internal tracking
    private string lastDetectedInput = "";
    private float lastInputTime = 0f;
    private Dictionary<string, string> buttonMappings = new Dictionary<string, string>();

    void Start()
    {
        buttonMappings["Interact"] = interactButton;
        buttonMappings["CycleLeft"] = cycleLeftButton;
        buttonMappings["CycleRight"] = cycleRightButton;
    }

    void Update()
    {
        if (isCapturing)
            DetectButtonPress();

        DetectCurrentInputs();
    }

    void DetectButtonPress()
    {
        if (Gamepad.current == null) return;

        // All button options
        string[] possibleButtons = new string[]
        {
            "buttonSouth", "buttonEast", "buttonWest", "buttonNorth",
            "leftShoulder", "rightShoulder", "leftTrigger", "rightTrigger",
            "selectButton", "startButton", "leftStickButton", "rightStickButton",
            "dpad/up", "dpad/down", "dpad/left", "dpad/right"
        };

        foreach (string btn in possibleButtons)
        {
            var control = Gamepad.current[btn] as ButtonControl;
            if (control != null && control.wasPressedThisFrame)
            {
                CaptureInput(btn, GetDisplayName(btn));
                return;
            }
        }

        // Optional: Keyboard fallback
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            CaptureInput("Keyboard/E", "E Key");
        }
        else if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CaptureInput("Keyboard/Space", "Space Key");
        }
    }

    void DetectCurrentInputs()
    {
        if (Gamepad.current == null) return;

        string[] possibleButtons = new string[]
        {
            "buttonSouth", "buttonEast", "buttonWest", "buttonNorth",
            "leftShoulder", "rightShoulder", "leftTrigger", "rightTrigger",
            "selectButton", "startButton", "leftStickButton", "rightStickButton",
            "dpad/up", "dpad/down", "dpad/left", "dpad/right"
        };

        foreach (string btn in possibleButtons)
        {
            var control = Gamepad.current[btn] as ButtonControl;
            if (control != null && control.isPressed)
            {
                lastDetectedInput = GetDisplayName(btn);
                lastInputTime = Time.time;
                return;
            }
        }
    }

    void CaptureInput(string binding, string displayName)
    {
        if (string.IsNullOrEmpty(captureTarget)) return;

        buttonMappings[captureTarget] = binding;

        switch (captureTarget)
        {
            case "Interact": interactButton = binding; break;
            case "CycleLeft": cycleLeftButton = binding; break;
            case "CycleRight": cycleRightButton = binding; break;
        }

        Debug.Log($"<color=green>[ButtonCapture] Assigned {displayName} ({binding}) to {captureTarget}</color>");
        isCapturing = false;
        captureTarget = "";
    }

    string GetDisplayName(string button)
    {
        return button switch
        {
            "buttonSouth" => "A / Cross",
            "buttonEast" => "B / Circle",
            "buttonWest" => "X / Square",
            "buttonNorth" => "Y / Triangle",
            "leftShoulder" => "L1 / LB",
            "rightShoulder" => "R1 / RB",
            "leftTrigger" => "L2 / LT",
            "rightTrigger" => "R2 / RT",
            "selectButton" => "Select / Share",
            "startButton" => "Start / Options",
            "leftStickButton" => "L3",
            "rightStickButton" => "R3",
            "dpad/up" => "D-pad Up",
            "dpad/down" => "D-pad Down",
            "dpad/left" => "D-pad Left",
            "dpad/right" => "D-pad Right",
            _ => button
        };
    }

    // Input state checks
    public bool GetInteractPressed() => IsButtonPressed(interactButton);
    public bool GetCycleLeftPressed() => IsButtonPressed(cycleLeftButton);
    public bool GetCycleRightPressed() => IsButtonPressed(cycleRightButton);

    bool IsButtonPressed(string binding)
    {
        if (string.IsNullOrEmpty(binding) || Gamepad.current == null) return false;

        // Keyboard fallback
        if (binding.StartsWith("Keyboard/"))
        {
            if (binding == "Keyboard/E") return Keyboard.current.eKey.wasPressedThisFrame;
            if (binding == "Keyboard/Space") return Keyboard.current.spaceKey.wasPressedThisFrame;
            return false;
        }

        var control = Gamepad.current[binding] as ButtonControl;
        return control != null && control.wasPressedThisFrame;
    }

    // GUI for testing and configuration
    void OnGUI()
    {
        if (!showCaptureGUI) return;

        GUILayout.BeginArea(new Rect(10, Screen.height - 250, 500, 240));
        GUI.color = Color.cyan;

        GUILayout.Label("=== BUTTON CAPTURE SYSTEM (Input System) ===");

        if (isCapturing)
        {
            GUI.color = Color.yellow;
            GUILayout.Label($"🎮 CAPTURING FOR: {captureTarget}");
            GUILayout.Label("Press any gamepad button...");

            if (GUILayout.Button("Cancel Capture"))
                CancelCapture();
        }
        else
        {
            GUI.color = Color.white;
            GUILayout.Label("Current Button Assignments:");

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Interact: {GetDisplayName(interactButton)}", GUILayout.Width(200));
            if (GUILayout.Button("Capture", GUILayout.Width(80))) StartCapturingInteract();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Cycle Left: {GetDisplayName(cycleLeftButton)}", GUILayout.Width(200));
            if (GUILayout.Button("Capture", GUILayout.Width(80))) StartCapturingCycleLeft();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Cycle Right: {GetDisplayName(cycleRightButton)}", GUILayout.Width(200));
            if (GUILayout.Button("Capture", GUILayout.Width(80))) StartCapturingCycleRight();
            GUILayout.EndHorizontal();
        }

        if (showCurrentInputs)
        {
            GUI.color = Color.green;
            GUILayout.Label("--- LIVE INPUT DETECTION ---");
            if (Time.time - lastInputTime < 0.5f)
                GUILayout.Label($"Currently Pressed: {lastDetectedInput}");
            else
                GUILayout.Label("No input detected");

            if (GetInteractPressed())
            {
                GUI.color = Color.green;
                GUILayout.Label("✓ INTERACT PRESSED!");
            }
            if (GetCycleLeftPressed())
            {
                GUI.color = Color.green;
                GUILayout.Label("✓ CYCLE LEFT PRESSED!");
            }
            if (GetCycleRightPressed())
            {
                GUI.color = Color.green;
                GUILayout.Label("✓ CYCLE RIGHT PRESSED!");
            }
        }

        GUILayout.EndArea();
        GUI.color = Color.white;
    }

    // Public API for starting capture sessions
    public void StartCapturingInteract()
    {
        isCapturing = true;
        captureTarget = "Interact";
        Debug.Log("<color=yellow>[ButtonCapture] Press the button you want to use for INTERACT...</color>");
    }

    public void StartCapturingCycleLeft()
    {
        isCapturing = true;
        captureTarget = "CycleLeft";
        Debug.Log("<color=yellow>[ButtonCapture] Press the button you want to use for CYCLE LEFT...</color>");
    }

    public void StartCapturingCycleRight()
    {
        isCapturing = true;
        captureTarget = "CycleRight";
        Debug.Log("<color=yellow>[ButtonCapture] Press the button you want to use for CYCLE RIGHT...</color>");
    }

    public void CancelCapture()
    {
        isCapturing = false;
        captureTarget = "";
        Debug.Log("<color=red>[ButtonCapture] Capture cancelled</color>");
    }
}
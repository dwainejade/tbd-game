using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ControllerLogger : MonoBehaviour
{
    [Header("Logging Settings")]
    [SerializeField] private bool logAllButtons = true;
    [SerializeField] private bool logAxes = true;
    [SerializeField] private bool logOnlyPressed = false; // If true, only log when buttons are pressed/released
    [SerializeField] private bool useInputSystem = true; // Use new Input System vs old
    
    [Header("Specific Buttons to Log (Old Input System)")]
    [SerializeField] private List<string> buttonNames = new List<string>()
    {
        "Fire1", "Fire2", "Jump", "Submit", "Cancel",
        "joystick button 0", "joystick button 1", "joystick button 2", "joystick button 3",
        "joystick button 4", "joystick button 5", "joystick button 6", "joystick button 7"
    };
    
    [Header("Specific Axes to Log (Old Input System)")]
    [SerializeField] private List<string> axisNames = new List<string>()
    {
        "Horizontal", "Vertical", "Mouse X", "Mouse Y",
        "joystick 1 analog x", "joystick 1 analog y"
    };
    
    // New Input System
    private PlayerInput playerInput;
    private InputActionMap currentActionMap;
    
    void Start()
    {
        if (useInputSystem)
        {
            SetupNewInputSystem();
        }
        
        Debug.Log("<color=yellow>[ControllerLogger] Started logging controller input</color>");
    }
    
    void SetupNewInputSystem()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            playerInput = FindObjectOfType<PlayerInput>();
        }
        
        if (playerInput != null)
        {
            currentActionMap = playerInput.currentActionMap;
            
            // Subscribe to all actions in the current action map
            foreach (var action in currentActionMap.actions)
            {
                action.performed += OnActionPerformed;
                action.started += OnActionStarted;
                action.canceled += OnActionCanceled;
            }
            
            Debug.Log($"<color=yellow>[ControllerLogger] Subscribed to {currentActionMap.actions.Count} actions</color>");
        }
        else
        {
            Debug.LogWarning("[ControllerLogger] No PlayerInput found! Make sure you have a PlayerInput component in the scene.");
        }
    }
    
    void Update()
    {
        if (useInputSystem)
        {
            // New Input System logging is handled by events
            return;
        }
        
        // Old Input System logging
        LogOldInputSystem();
    }
    
    void LogOldInputSystem()
    {
        // Log button presses
        if (logAllButtons)
        {
            foreach (string buttonName in buttonNames)
            {
                try
                {
                    if (logOnlyPressed)
                    {
                        if (Input.GetButtonDown(buttonName))
                        {
                            Debug.Log($"<color=green>[Controller] Button DOWN: {buttonName}</color>");
                        }
                        if (Input.GetButtonUp(buttonName))
                        {
                            Debug.Log($"<color=red>[Controller] Button UP: {buttonName}</color>");
                        }
                    }
                    else
                    {
                        if (Input.GetButton(buttonName))
                        {
                            Debug.Log($"<color=blue>[Controller] Button HELD: {buttonName}</color>");
                        }
                    }
                }
                catch
                {
                    // Button not defined in Input Manager
                }
            }
        }
        
        // Log axes
        if (logAxes)
        {
            foreach (string axisName in axisNames)
            {
                try
                {
                    float axisValue = Input.GetAxis(axisName);
                    if (Mathf.Abs(axisValue) > 0.1f) // Only log if axis has significant input
                    {
                        Debug.Log($"<color=cyan>[Controller] Axis {axisName}: {axisValue:F2}</color>");
                    }
                }
                catch
                {
                    // Axis not defined in Input Manager
                }
            }
        }
        
        // Log specific controller buttons by joystick number
        LogSpecificControllerButtons();
    }
    
    void LogSpecificControllerButtons()
    {
        // Check for any joystick button (0-19 covers most controllers)
        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKeyDown($"joystick button {i}"))
            {
                Debug.Log($"<color=magenta>[Controller] Joystick Button {i} PRESSED</color>");
            }
        }
        
        // Check for D-pad (often mapped as axes or buttons depending on controller)
        if (Input.GetKeyDown(KeyCode.JoystickButton0)) Debug.Log("<color=orange>[Controller] A/Cross Button</color>");
        if (Input.GetKeyDown(KeyCode.JoystickButton1)) Debug.Log("<color=orange>[Controller] B/Circle Button</color>");
        if (Input.GetKeyDown(KeyCode.JoystickButton2)) Debug.Log("<color=orange>[Controller] X/Square Button</color>");
        if (Input.GetKeyDown(KeyCode.JoystickButton3)) Debug.Log("<color=orange>[Controller] Y/Triangle Button</color>");
    }
    
    // New Input System Event Handlers
    void OnActionPerformed(InputAction.CallbackContext context)
    {
        string deviceName = context.control?.device?.displayName ?? "Unknown Device";
        string controlName = context.control?.displayName ?? "Unknown Control";
        
        Debug.Log($"<color=green>[NewInput] {context.action.name} PERFORMED on {deviceName} ({controlName}) - Value: {context.ReadValueAsObject()}</color>");
    }
    
    void OnActionStarted(InputAction.CallbackContext context)
    {
        string deviceName = context.control?.device?.displayName ?? "Unknown Device";
        string controlName = context.control?.displayName ?? "Unknown Control";
        
        Debug.Log($"<color=yellow>[NewInput] {context.action.name} STARTED on {deviceName} ({controlName})</color>");
    }
    
    void OnActionCanceled(InputAction.CallbackContext context)
    {
        string deviceName = context.control?.device?.displayName ?? "Unknown Device";
        string controlName = context.control?.displayName ?? "Unknown Control";
        
        Debug.Log($"<color=red>[NewInput] {context.action.name} CANCELED on {deviceName} ({controlName})</color>");
    }
    
    // Method to log all connected controllers
    [ContextMenu("Log Connected Controllers")]
    public void LogConnectedControllers()
    {
        string[] joystickNames = Input.GetJoystickNames();
        Debug.Log($"<color=white>[Controller] Found {joystickNames.Length} controllers:</color>");
        
        for (int i = 0; i < joystickNames.Length; i++)
        {
            if (!string.IsNullOrEmpty(joystickNames[i]))
            {
                Debug.Log($"<color=white>[Controller] {i}: {joystickNames[i]}</color>");
            }
        }
        
        // New Input System devices
        if (useInputSystem)
        {
            Debug.Log("<color=white>[NewInput] Connected Input Devices:</color>");
            foreach (var device in InputSystem.devices)
            {
                Debug.Log($"<color=white>[NewInput] - {device.displayName} ({device.GetType().Name})</color>");
            }
        }
    }
    
    // Method to test specific AC inputs
    public void LogACInputs()
    {
        try
        {
            // Test common AC inputs
            string[] acInputs = {
                "CycleHotspotsLeft", "CycleHotspotsRight", "CycleHotspots",
                "InteractionA", "InteractionB", "Horizontal", "Vertical",
                "CursorHorizontal", "CursorVertical", "ToggleCursor"
            };
            
            foreach (string inputName in acInputs)
            {
                try
                {
                    bool buttonDown = AC.KickStarter.playerInput.InputGetButtonDown(inputName);
                    bool button = AC.KickStarter.playerInput.InputGetButton(inputName);
                    float axis = AC.KickStarter.playerInput.InputGetAxis(inputName);
                    
                    if (buttonDown)
                        Debug.Log($"<color=purple>[AC Input] {inputName} DOWN</color>");
                    if (button)
                        Debug.Log($"<color=purple>[AC Input] {inputName} HELD</color>");
                    if (Mathf.Abs(axis) > 0.1f)
                        Debug.Log($"<color=purple>[AC Input] {inputName} AXIS: {axis:F2}</color>");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[AC Input] Failed to check {inputName}: {e.Message}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AC Input] Error accessing KickStarter: {e.Message}");
        }
    }
    
    void OnDestroy()
    {
        // Clean up event subscriptions
        if (useInputSystem && currentActionMap != null)
        {
            foreach (var action in currentActionMap.actions)
            {
                action.performed -= OnActionPerformed;
                action.started -= OnActionStarted;
                action.canceled -= OnActionCanceled;
            }
        }
    }
    
    void OnGUI()
    {
        // Quick on-screen buttons for testing
        GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 150));
        
        if (GUILayout.Button("Log Controllers"))
        {
            LogConnectedControllers();
        }
        
        if (GUILayout.Button("Test AC Inputs"))
        {
            LogACInputs();
        }
        
        GUILayout.Label($"Input System: {(useInputSystem ? "New" : "Old")}");
        GUILayout.Label($"Log Mode: {(logOnlyPressed ? "Press/Release" : "All")}");
        
        GUILayout.EndArea();
    }
}
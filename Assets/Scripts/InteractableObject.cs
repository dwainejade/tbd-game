using UnityEngine;
using AC;

public class InteractableObject : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color highlightColor = Color.white;
    [SerializeField] private string outlineColorProperty = "_OutlineColor";

    [Header("Hide Method")]
    [SerializeField] private OutlineHideMethod hideMethod = OutlineHideMethod.AutoDetect;
    [SerializeField] private Color hiddenOutlineColor = Color.black; // Some shaders use black to hide

    public enum OutlineHideMethod
    {
        AutoDetect,
        UseTransparentColor,
        UseBlackColor,
        UseOriginalColor,
        SwitchMaterial
    }

    [Header("Adventure Creator")]
    [SerializeField] private Hotspot hotspot;

    [Header("Material Detection")]
    [SerializeField] private bool autoDetectOutlineMaterial = true;
    [SerializeField] private Material outlineMaterialOverride; // Manual override if needed

    private Material outlineMaterial;
    private Material originalMaterial;
    private int outlineColorPropertyID;
    private bool hasOutlineColorProperty = false;
    private bool materialsInitialized = false;
    private Color originalOutlineColor; // Store the original outline color
    private bool isOutlineVisible = false;

    void Start()
    {
        InitializeMaterials();

        // Get hotspot component if not assigned
        if (hotspot == null)
            hotspot = GetComponent<Hotspot>();

        // Start with outline hidden
        HideOutline();

        // Register with the interaction manager
        if (InteractionManager.Instance != null)
            InteractionManager.Instance.RegisterInteractable(this);
    }

    void InitializeMaterials()
    {
        // Cache the renderer
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                Debug.LogWarning($"{gameObject.name}: No Renderer found! Cannot show outlines.");
                return;
            }
        }

        // Store original material
        originalMaterial = targetRenderer.material;

        // Determine which material to use for outlines
        if (outlineMaterialOverride != null)
        {
            // Use manual override
            outlineMaterial = outlineMaterialOverride;
            Debug.Log($"{name}: Using override outline material: {outlineMaterial.name}");
        }
        else if (autoDetectOutlineMaterial)
        {
            // Try to find or create outline material
            outlineMaterial = GetOrCreateOutlineMaterial();
        }
        else
        {
            // Use current material (might not have outline properties)
            outlineMaterial = originalMaterial;
        }

        if (outlineMaterial != null)
        {
            // Cache property ID
            outlineColorPropertyID = Shader.PropertyToID(outlineColorProperty);

            // Check if property exists
            hasOutlineColorProperty = outlineMaterial.HasProperty(outlineColorPropertyID);

            Debug.Log($"{name}: Material '{outlineMaterial.name}' with Shader '{outlineMaterial.shader.name}'");
            Debug.Log($"  - Has {outlineColorProperty}: {hasOutlineColorProperty}");

            if (hasOutlineColorProperty)
            {
                // Store the original outline color
                originalOutlineColor = outlineMaterial.GetColor(outlineColorPropertyID);
                Debug.Log($"  - Original outline color: {originalOutlineColor}");

                // Auto-detect best hide method if set to AutoDetect
                if (hideMethod == OutlineHideMethod.AutoDetect)
                {
                    DetectBestHideMethod();
                }
            }

            // List all available properties for debugging
            if (!hasOutlineColorProperty)
            {
                Debug.LogWarning($"{name}: No outline color property found! Available properties:");
                LogShaderProperties(outlineMaterial);
            }

            materialsInitialized = true;
        }
        else
        {
            Debug.LogError($"{name}: No outline material available!");
        }
    }

    void DetectBestHideMethod()
    {
        // If we're using a different material for outlines, material switching is best
        if (outlineMaterial != originalMaterial)
        {
            hideMethod = OutlineHideMethod.SwitchMaterial;
            Debug.Log($"{name}: Auto-detected hide method: Switch Material (outline mat: {outlineMaterial.name}, original: {originalMaterial.name})");
            return;
        }

        // Try to determine the best hide method based on the original outline color
        if (originalOutlineColor.a == 0f)
        {
            hideMethod = OutlineHideMethod.UseTransparentColor;
            Debug.Log($"{name}: Auto-detected hide method: Transparent Color");
        }
        else if (originalOutlineColor == Color.black ||
                 (originalOutlineColor.r < 0.1f && originalOutlineColor.g < 0.1f && originalOutlineColor.b < 0.1f))
        {
            hideMethod = OutlineHideMethod.UseBlackColor;
            Debug.Log($"{name}: Auto-detected hide method: Black Color");
        }
        else
        {
            hideMethod = OutlineHideMethod.UseOriginalColor;
            Debug.Log($"{name}: Auto-detected hide method: Original Color");
        }
    }

    Material GetOrCreateOutlineMaterial()
    {
        // First, check if current material already has outline properties
        if (originalMaterial.HasProperty(outlineColorPropertyID))
        {
            Debug.Log($"{name}: Current material already has outline properties");
            return originalMaterial;
        }

        // Try to find an outline version of the material
        string materialName = originalMaterial.name;

        // Remove (Instance) suffix if present
        if (materialName.EndsWith(" (Instance)"))
        {
            materialName = materialName.Replace(" (Instance)", "");
        }

        // Try common outline material naming patterns
        string[] outlinePatterns = {
            materialName + "_Outline",
            materialName + "Outline",
            "Outline_" + materialName,
            "Outline" + materialName
        };

        foreach (string pattern in outlinePatterns)
        {
            Material outlineMat = Resources.Load<Material>(pattern);
            if (outlineMat != null)
            {
                Debug.Log($"{name}: Found outline material: {pattern}");
                return outlineMat;
            }
        }

        Debug.LogWarning($"{name}: No outline material found, using original material");
        return originalMaterial;
    }

    void LogShaderProperties(Material material)
    {
        Shader shader = material.shader;
        for (int i = 0; i < shader.GetPropertyCount(); i++)
        {
            string propName = shader.GetPropertyName(i);
            var propType = shader.GetPropertyType(i);
            Debug.Log($"    {propName} ({propType})");
        }
    }

    void OnDestroy()
    {
        // Unregister when destroyed
        if (InteractionManager.Instance != null)
            InteractionManager.Instance.UnregisterInteractable(this);
    }

    public void ShowOutline()
    {
        ShowOutlineWithColor(highlightColor);
    }

    public void ShowOutlineWithColor(Color color)
    {
        if (!materialsInitialized || outlineMaterial == null)
        {
            Debug.LogWarning($"{name}: Materials not initialized or outline material missing!");
            return;
        }

        // Always switch to outline material when showing (if different from original)
        if (outlineMaterial != originalMaterial)
        {
            targetRenderer.material = outlineMaterial;
            Debug.Log($"{name}: Switched to outline material: {outlineMaterial.name}");
        }

        if (hasOutlineColorProperty)
        {
            // Set the highlight color
            Color outlineColor = color;
            outlineColor.a = 1f; // Ensure outline is visible
            outlineMaterial.SetColor(outlineColorPropertyID, outlineColor);
            isOutlineVisible = true;
            Debug.Log($"{name}: Setting outline color to {outlineColor}");
        }
        else
        {
            // Even if no outline color property, we've switched materials so mark as visible
            isOutlineVisible = true;
            Debug.Log($"{name}: Outline shown via material switch (no color property)");
        }
    }

    public void HideOutline()
    {
        if (!materialsInitialized || !isOutlineVisible)
        {
            return; // Fail silently during initialization or if already hidden
        }

        if (hideMethod == OutlineHideMethod.SwitchMaterial || outlineMaterial != originalMaterial)
        {
            // Always switch back to original material when hiding
            targetRenderer.material = originalMaterial;
            isOutlineVisible = false;
            Debug.Log($"{name}: Switched back to original material: {originalMaterial.name}");
        }
        else if (hasOutlineColorProperty)
        {
            Color hideColor = GetHideColor();
            outlineMaterial.SetColor(outlineColorPropertyID, hideColor);
            isOutlineVisible = false;
            Debug.Log($"{name}: Hiding outline using method {hideMethod} with color {hideColor}");
        }
    }

    Color GetHideColor()
    {
        switch (hideMethod)
        {
            case OutlineHideMethod.UseTransparentColor:
                Color transparent = originalOutlineColor;
                transparent.a = 0f;
                return transparent;

            case OutlineHideMethod.UseBlackColor:
                return hiddenOutlineColor;

            case OutlineHideMethod.UseOriginalColor:
                return originalOutlineColor;

            case OutlineHideMethod.SwitchMaterial:
                // This case is handled elsewhere, but return transparent as fallback
                Color fallback = originalOutlineColor;
                fallback.a = 0f;
                return fallback;

            default:
                return originalOutlineColor;
        }
    }

    public void SetOutlineVisibility(float alpha)
    {
        if (!materialsInitialized || !hasOutlineColorProperty) return;

        Color currentColor = highlightColor;
        currentColor.a = alpha;
        outlineMaterial.SetColor(outlineColorPropertyID, currentColor);
        isOutlineVisible = alpha > 0f;
    }

    // Method to manually assign outline material
    public void SetOutlineMaterial(Material material)
    {
        outlineMaterialOverride = material;
        InitializeMaterials(); // Re-initialize with new material
    }

    // Method to change hide method at runtime
    public void SetHideMethod(OutlineHideMethod method)
    {
        hideMethod = method;
        Debug.Log($"{name}: Hide method changed to {method}");
    }

    // Method to check if outline is supported
    public bool SupportsOutline()
    {
        return materialsInitialized && hasOutlineColorProperty;
    }

    // Method to check if outline is currently visible
    public bool IsOutlineVisible()
    {
        return isOutlineVisible;
    }

    public Hotspot GetHotspot()
    {
        return hotspot;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
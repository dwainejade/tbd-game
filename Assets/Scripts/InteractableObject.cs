using UnityEngine;
using AC;

public class InteractableObject : MonoBehaviour 
{
    [Header("Outline Settings")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color highlightColor = Color.white;
    [SerializeField] private string outlineColorProperty = "_OutlineColor";
    [SerializeField] private string outlineWidthProperty = "_OutlineWidth"; // Common alternative
    
    [Header("Adventure Creator")]
    [SerializeField] private Hotspot hotspot;
    
    [Header("Material Detection")]
    [SerializeField] private bool autoDetectOutlineMaterial = true;
    [SerializeField] private Material outlineMaterialOverride; // Manual override if needed
    
    private Material outlineMaterial;
    private Material originalMaterial;
    private int outlineColorPropertyID;
    private int outlineWidthPropertyID;
    private bool hasOutlineColorProperty = false;
    private bool hasOutlineWidthProperty = false;
    private bool materialsInitialized = false;
    
    void Start() 
    {
        InitializeMaterials();
        
        // Get hotspot component if not assigned
        if (hotspot == null)
            hotspot = GetComponent<Hotspot>();
        
        // Start with outline hidden
        HideOutline();
        
        // Register with the interaction manager
        InteractionManager.Instance?.RegisterInteractable(this);
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
            // Cache property IDs
            outlineColorPropertyID = Shader.PropertyToID(outlineColorProperty);
            outlineWidthPropertyID = Shader.PropertyToID(outlineWidthProperty);
            
            // Check if properties exist
            hasOutlineColorProperty = outlineMaterial.HasProperty(outlineColorPropertyID);
            hasOutlineWidthProperty = outlineMaterial.HasProperty(outlineWidthPropertyID);
            
            Debug.Log($"{name}: Material '{outlineMaterial.name}' with Shader '{outlineMaterial.shader.name}'");
            Debug.Log($"  - Has {outlineColorProperty}: {hasOutlineColorProperty}");
            Debug.Log($"  - Has {outlineWidthProperty}: {hasOutlineWidthProperty}");
            
            // List all available properties for debugging
            if (!hasOutlineColorProperty && !hasOutlineWidthProperty)
            {
                Debug.LogWarning($"{name}: No outline properties found! Available properties:");
                LogShaderProperties(outlineMaterial);
            }
            
            materialsInitialized = true;
        }
        else
        {
            Debug.LogError($"{name}: No outline material available!");
        }
    }
    
    Material GetOrCreateOutlineMaterial()
    {
        // First, check if current material already has outline properties
        if (originalMaterial.HasProperty(outlineColorPropertyID) || 
            originalMaterial.HasProperty(outlineWidthPropertyID))
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
        
        // Try to find any material with outline shader
        Material[] allMaterials = Resources.FindObjectsOfTypeAll<Material>();
        foreach (Material mat in allMaterials)
        {
            if (mat.shader.name.ToLower().Contains("outline"))
            {
                Debug.Log($"{name}: Found potential outline material: {mat.name} with shader: {mat.shader.name}");
                // You could return this, but it might not be the right one
                // return mat;
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
        InteractionManager.Instance?.UnregisterInteractable(this);
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
        
        // Switch to outline material if it's different from current
        if (targetRenderer.material != outlineMaterial)
        {
            targetRenderer.material = outlineMaterial;
        }
        
        bool outlineShown = false;
        
        // Try to set outline color
        if (hasOutlineColorProperty)
        {
            outlineMaterial.SetColor(outlineColorPropertyID, color);
            outlineShown = true;
            Debug.Log($"{name}: Setting outline color to {color}");
        }
        
        // Try to set outline width (if we're using this approach)
        if (hasOutlineWidthProperty)
        {
            outlineMaterial.SetFloat(outlineWidthPropertyID, 1f); // or your desired width
            outlineShown = true;
            Debug.Log($"{name}: Setting outline width to 1");
        }
        
        if (!outlineShown)
        {
            Debug.LogWarning($"{name}: Could not show outline - no supported properties found!");
        }
    }
    
    public void HideOutline() 
    {
        if (!materialsInitialized || outlineMaterial == null)
        {
            return; // Fail silently during initialization
        }
        
        bool outlineHidden = false;
        
        // Try to hide using outline color (make transparent)
        if (hasOutlineColorProperty)
        {
            Color transparentColor = highlightColor;
            transparentColor.a = 0f;
            outlineMaterial.SetColor(outlineColorPropertyID, transparentColor);
            outlineHidden = true;
            Debug.Log($"{name}: Hiding outline (transparent color)");
        }
        
        // Try to hide using outline width (set to 0)
        if (hasOutlineWidthProperty)
        {
            outlineMaterial.SetFloat(outlineWidthPropertyID, 0f);
            outlineHidden = true;
            Debug.Log($"{name}: Hiding outline (width = 0)");
        }
        
        // If no outline properties, switch back to original material
        if (!outlineHidden && targetRenderer.material != originalMaterial)
        {
            targetRenderer.material = originalMaterial;
            Debug.Log($"{name}: Switched back to original material");
        }
    }
    
    public void SetOutlineVisibility(float alpha) 
    {
        if (!materialsInitialized || !hasOutlineColorProperty) return;
        
        Color currentColor = highlightColor;
        currentColor.a = alpha;
        outlineMaterial.SetColor(outlineColorPropertyID, currentColor);
    }
    
    // Method to manually assign outline material
    public void SetOutlineMaterial(Material material)
    {
        outlineMaterialOverride = material;
        InitializeMaterials(); // Re-initialize with new material
    }
    
    // Method to check if outline is supported
    public bool SupportsOutline()
    {
        return materialsInitialized && (hasOutlineColorProperty || hasOutlineWidthProperty);
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
using UnityEngine;
using AC;

public class HotspotMaterialChanger : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material normalMaterial;     // Default state
    [SerializeField] private Material highlightMaterial; // Material that can change colors

    [Header("Target Objects")]
    [SerializeField] private Renderer[] targetRenderers;

    [Header("Color Settings")]
    [SerializeField] private string colorPropertyName = "_OutlineColor"; // Shader property name

    private Hotspot hotspot;
    private bool isHighlighted = false;
    private static ProximityHotspotManager proximityManager;

    void Start()
    {
        hotspot = GetComponent<Hotspot>();

        // Find the proximity manager
        if (proximityManager == null)
            proximityManager = FindObjectOfType<ProximityHotspotManager>();

        // Set initial state
        RemoveHighlight();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerDetector") || other.CompareTag("Player"))
        {
            Debug.Log($"Player entered: {gameObject.name}");

            if (proximityManager != null)
                proximityManager.RegisterHotspot(this);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PlayerDetector") || other.CompareTag("Player"))
        {
            Debug.Log($"Player left: {gameObject.name}");

            if (proximityManager != null)
                proximityManager.UnregisterHotspot(this);
        }
    }

    public void SetHighlightColor(Color color, bool isSelected)
    {
        isHighlighted = true;
        ApplyHighlightMaterial();
        SetHighlightColorValue(color);

        Debug.Log($"{gameObject.name} highlighted with color: {color} (Selected: {isSelected})");
    }

    public void RemoveHighlight()
    {
        isHighlighted = false;
        ApplyNormalMaterial();

        Debug.Log($"{gameObject.name} highlight removed");
    }

    void ApplyHighlightMaterial()
    {
        if (targetRenderers.Length == 0 || highlightMaterial == null) return;

        foreach (Renderer renderer in targetRenderers)
        {
            if (renderer != null)
            {
                renderer.material = highlightMaterial;
            }
        }
    }

    void ApplyNormalMaterial()
    {
        if (targetRenderers.Length == 0 || normalMaterial == null) return;

        foreach (Renderer renderer in targetRenderers)
        {
            if (renderer != null)
            {
                renderer.material = normalMaterial;
            }
        }
    }

    void SetHighlightColorValue(Color color)
    {
        if (targetRenderers.Length == 0) return;

        foreach (Renderer renderer in targetRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                // Check if the material has the color property
                if (renderer.material.HasProperty(colorPropertyName))
                {
                    renderer.material.SetColor(colorPropertyName, color);
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name}: Material doesn't have property '{colorPropertyName}'");
                }
            }
        }
    }
}
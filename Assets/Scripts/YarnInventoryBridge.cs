using UnityEngine;
using AC;
using Yarn.Unity;

public class YarnInventoryBridge : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("YarnInventoryBridge: Awake() called - script is initializing!");
    }

    private void Start()
    {
        Debug.Log("YarnInventoryBridge: Start() called - script is running!");
        Debug.Log($"YarnInventoryBridge: Attached to GameObject: {gameObject.name}");
    }

    [YarnCommand("CheckWormInventory")]
    public static void CheckWormInventory()
    {
        Debug.Log("YarnInventoryBridge: CheckWormInventory called!");
        
        // Safety check for Adventure Creator
        if (KickStarter.inventoryManager == null)
        {
            Debug.LogError("YarnInventoryBridge: Adventure Creator InventoryManager not found!");
            return;
        }
        
        // Get the Worm item by its ID or label
        InvItem wormItem = KickStarter.inventoryManager.GetItem("Worm");

        bool hasWorm = false;

        if (wormItem != null)
        {
            // Check if player has the item using the runtime inventory
            hasWorm = KickStarter.runtimeInventory.GetCount(wormItem.id) > 0;
            Debug.Log($"YarnInventoryBridge: Player has {KickStarter.runtimeInventory.GetCount(wormItem.id)} worm(s)");
        }
        else
        {
            Debug.LogWarning("YarnInventoryBridge: Worm item not found in InventoryManager. Make sure the item label/ID is correct.");
        }

        // Set the Yarn variable
        var runner = FindAnyObjectByType<DialogueRunner>();
        if (runner == null)
        {
            Debug.LogError("YarnInventoryBridge: DialogueRunner not found in scene.");
            return;
        }

        var storage = runner.VariableStorage;
        storage.SetValue("$hasWorm", hasWorm);
        
        Debug.Log($"YarnInventoryBridge: Set $hasWorm to {hasWorm}");
    }

    [YarnCommand("RemoveWormFromInventory")]
    public static void RemoveWormFromInventory()
    {
        Debug.Log("YarnInventoryBridge: RemoveWormFromInventory called!");
        
        InvItem wormItem = KickStarter.inventoryManager.GetItem("Worm");

        if (wormItem == null)
        {
            Debug.LogWarning("YarnInventoryBridge: Worm item not found in InventoryManager. Make sure the item label/ID is correct.");
            return;
        }

        // Get current count
        int currentCount = KickStarter.runtimeInventory.GetCount(wormItem.id);
        
        if (currentCount > 0)
        {
            // Remove all instances of the worm from the player's inventory
            KickStarter.runtimeInventory.Remove(wormItem.id, currentCount);
            Debug.Log($"YarnInventoryBridge: Removed {currentCount} Worm(s) from inventory");
        }
        else
        {
            Debug.Log("YarnInventoryBridge: No Worm items found in inventory to remove");
        }
    }

    [YarnCommand("AddWormToInventory")]
    public static void AddWormToInventory()
    {
        Debug.Log("YarnInventoryBridge: AddWormToInventory called!");
        
        InvItem wormItem = KickStarter.inventoryManager.GetItem("Worm");

        if (wormItem == null)
        {
            Debug.LogWarning("YarnInventoryBridge: Worm item not found in InventoryManager. Make sure the item label/ID is correct.");
            return;
        }

        // Add one worm to inventory
        KickStarter.runtimeInventory.Add(wormItem.id, 1);
        Debug.Log("YarnInventoryBridge: Added Worm to inventory");
    }

    [YarnCommand("GetWormCount")]
    public static void GetWormCount()
    {
        Debug.Log("YarnInventoryBridge: GetWormCount called!");
        
        InvItem wormItem = KickStarter.inventoryManager.GetItem("Worm");

        int count = 0;
        if (wormItem != null)
        {
            count = KickStarter.runtimeInventory.GetCount(wormItem.id);
        }
        else
        {
            Debug.LogWarning("YarnInventoryBridge: Worm item not found in InventoryManager.");
        }

        // Set the count in a Yarn variable
        var runner = FindAnyObjectByType<DialogueRunner>();
        if (runner != null)
        {
            var storage = runner.VariableStorage;
            storage.SetValue("$wormCount", count);
            Debug.Log($"YarnInventoryBridge: Set $wormCount to {count}");
        }
    }
}
using UnityEngine;
using Player_Actions;
using FarrokhGames.Inventory.Examples;

public class InventoryActions : MonoBehaviour
{
    PlayerActions playerActions;
    InventoryInitialation inventoryInitialation;

    // Start is called before the first frame update
    void Start()
    {
        playerActions = GetComponent<PlayerActions>();
        inventoryInitialation = playerActions.inventory.GetComponent<InventoryInitialation>();
        playerActions.tradeInventory.gameObject.SetActive(false);
    }

    public void Activate_Inventory(bool on)
    {
        playerActions.inventory.SetActive(on);
        inventoryInitialation.Inventory_Initialization();
    }

    public void Activate_Trade()
    {
        playerActions.tradeInventory.SetActive(true);
        playerActions.Enable_Inventory(true);
    }
}

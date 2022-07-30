using UnityEngine;
using Player_Actions;
using FarrokhGames.Inventory;
using FarrokhGames.Inventory.Examples;

public class Close_Open_TradeInven : MonoBehaviour
{
    PlayerActions playerActions;
    InventoryInitialation inventoryInitialation;
    GameObject playerInventory;

    void Start()
    {
        playerActions = GetComponent<PlayerActions>();
        playerInventory = playerActions.inventory.transform.GetChild(0).gameObject;
        inventoryInitialation = playerActions.inventory.GetComponent<InventoryInitialation>();
        playerActions.tradeInventory.gameObject.SetActive(false);
    }

    public void Activate_Inventory(bool on)
    {
        if (on == true)
        {
            inventoryInitialation.Inventory_Initialization();
        }
        else
        {
            playerInventory.GetComponent<SellItem>().enabled = false;
            playerInventory.GetComponent<InventoryController>().enabled = true;
            playerActions.tradeInventory.gameObject.SetActive(false);
        }

        playerActions.inventory.SetActive(on);
    }

    public void Activate_Trade()
    {
        playerInventory.GetComponent<InventoryController>().enabled = false;
        playerInventory.GetComponent<SellItem>().enabled = true;
        playerActions.tradeInventory.SetActive(true);
        playerActions.Enable_Inventory(true);
    }
}

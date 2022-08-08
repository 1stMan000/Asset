using UnityEngine;
using Player_Actions;
using FarrokhGames.Inventory;
using FarrokhGames.Inventory.Examples;

public class Close_Open_TradeInven : MonoBehaviour
{
    PlayerActions playerActions;
    Inven_Initialation inventoryInitialation;
    GameObject playerInventory;

    void Start()
    {
        playerActions = GetComponent<PlayerActions>();
        playerInventory = playerActions.inventoriesParent.transform.GetChild(0).gameObject;
        inventoryInitialation = playerActions.inventoriesParent.GetComponent<Inven_Initialation>();
        playerActions.tradeInventory_Object.gameObject.SetActive(false);
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
            playerInventory.GetComponent<Inven_Controller>().enabled = true;
            playerActions.tradeInventory_Object.gameObject.SetActive(false);
        }

        playerActions.inventoriesParent.SetActive(on);
    }

    public void Activate_Trade()
    {
        inventoryInitialation.Inventory_Initialization();

        playerInventory.GetComponent<Inven_Controller>().enabled = false;
        playerInventory.GetComponent<SellItem>().enabled = true;
        playerActions.tradeInventory_Object.SetActive(true);
        playerActions.Enable_Inventory(true);
    }
}

using Player_Actions;
using UnityEngine;

namespace FarrokhGames.Inventory
{
    public class SellItem : InventoryController
    {
        PlayerActions playerActions;

        void Start()
        {
            onItemAdded += RecieveCoins;
            playerActions = FindObjectOfType(typeof(PlayerActions)) as PlayerActions;
        }

        void RecieveCoins(IInventoryItem item)
        {
            if (TradeManager.originalController == this)
                playerActions.totalCoins = playerActions.totalCoins + item.price;
        }
    }
}
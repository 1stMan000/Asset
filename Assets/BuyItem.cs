using Player_Actions;
using UnityEngine;

namespace FarrokhGames.Inventory
{
    public class BuyItem : InventoryController
    {
        PlayerActions playerActions;

        void Start()
        {
            onItemAdded += RemoveCoins;
            playerActions = FindObjectOfType(typeof(PlayerActions)) as PlayerActions;
        }

        void RemoveCoins(IInventoryItem item)
        {
            if (TradeManager.originalController == this)
                playerActions.totalCoins = playerActions.totalCoins - item.price;
        }

        protected override void Item_Dropped()
        {
            onItemReturned?.Invoke(_itemToDrag);
        }
    }
}
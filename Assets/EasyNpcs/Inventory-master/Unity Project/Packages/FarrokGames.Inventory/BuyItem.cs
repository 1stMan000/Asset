using Player_Actions;
using UnityEngine;

namespace FarrokhGames.Inventory
{
    public class BuyItem : Inven_Controller
    {
        PlayerActions playerActions;

        void Start()
        {
            onItemAdded += RemoveCoins;
            playerActions = FindObjectOfType(typeof(PlayerActions)) as PlayerActions;
        }

        void RemoveCoins(IInven_Item item)
        {
            if (TradeManager.originalController == this)
                playerActions.totalCoins = playerActions.totalCoins - item.price;
        }

        protected override void Item_Dropped()
        {
            inventory.TryAddAt(_itemToDrag, _draggedItem.originPoint);
            onItemReturned?.Invoke(_itemToDrag);
        }
    }
}
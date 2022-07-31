using Player_Actions;
using UnityEngine;

namespace FarrokhGames.Inventory
{
    public class SellItem : Inven_Controller
    {
        PlayerActions playerActions;

        void Start()
        {
            onItemAdded += RecieveCoins;
            playerActions = FindObjectOfType(typeof(PlayerActions)) as PlayerActions;
        }

        void RecieveCoins(IInven_Item item)
        {
            if (TradeManager.originalController == this)
                playerActions.totalCoins = playerActions.totalCoins + item.price;
        }

        protected override void Item_Dropped()
        {
            inventory.TryAddAt(_itemToDrag, _draggedItem.originPoint);
            onItemReturned?.Invoke(_itemToDrag);
        }
    }
}
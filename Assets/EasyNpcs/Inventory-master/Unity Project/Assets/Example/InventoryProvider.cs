using System.Collections.Generic;

namespace FarrokhGames.Inventory.Examples
{
    public class InventoryProvider : IInven_Provider
    {
        private List<IInven_Item> _items = new List<IInven_Item>();
        private int _maximumAlowedItemCount;
        ItemType _allowedItem;

        public InventoryProvider(InventoryRenderMode renderMode, int maximumAlowedItemCount = -1, ItemType allowedItem = ItemType.Any)
        {
            inventoryRenderMode = renderMode;
            _maximumAlowedItemCount = maximumAlowedItemCount;
            _allowedItem = allowedItem;
        }

        public int inventoryItemCount => _items.Count;

        public InventoryRenderMode inventoryRenderMode { get; private set; }

        public bool isInventoryFull
        {
            get
            {
                if (_maximumAlowedItemCount < 0)return false;
                return inventoryItemCount >= _maximumAlowedItemCount;
            }
        }

        public bool AddInventoryItem(IInven_Item item)
        {
            if (!_items.Contains(item))
            {
                _items.Add(item);
                return true;
            }

            return false;
        }

        public bool DropInventoryItem(IInven_Item item)
        {
            return RemoveInventoryItem(item);
        }

        public IInven_Item GetInventoryItem(int index)
        {
            return _items[index];
        }

        public bool CanAddInventoryItem(IInven_Item item)
        {
            if (_allowedItem == ItemType.Any)return true;
            return (item as ItemDefinition).Type == _allowedItem;
        }

        public bool CanRemoveInventoryItem(IInven_Item item)
        {
            return true;
        }

        public bool CanDropInventoryItem(IInven_Item item)
        {
            return true;
        }

        public bool RemoveInventoryItem(IInven_Item item)
        {
            return _items.Remove(item);
        }
    }
}
using System.Collections.Generic;

namespace FarrokhGames.Inventory
{
    public class TestProvider : IInven_Provider
    {
        private readonly List<IInven_Item> _items = new List<IInven_Item>();
        private readonly int _maximumAlowedItemCount;

        /// <summary>
        /// CTOR
        /// </summary>
        public TestProvider(Inven_RenderMode renderMode = Inven_RenderMode.Grid, int maximumAlowedItemCount = -1)
        {
            inventoryRenderMode = renderMode;
            _maximumAlowedItemCount = maximumAlowedItemCount;
        }

        public int inventoryItemCount => _items.Count;

        public Inven_RenderMode inventoryRenderMode { get; }

        public bool isInventoryFull
        {
            get
            {
                if (_maximumAlowedItemCount < 0) return false;
                return inventoryItemCount < _maximumAlowedItemCount;
            }
        }

        public bool AddInventoryItem(IInven_Item item)
        {
            if (_items.Contains(item)) return false;
            _items.Add(item);
            return true;
        }

        public bool DropInventoryItem(IInven_Item item) => RemoveInventoryItem(item);
        public IInven_Item GetInventoryItem(int index) => _items[index];
        public bool CanAddInventoryItem(IInven_Item item) => true;
        public bool CanRemoveInventoryItem(IInven_Item item) => true;
        public bool CanDropInventoryItem(IInven_Item item) => true;
        public bool RemoveInventoryItem(IInven_Item item) => _items.Remove(item);
    }
}
using System;
using UnityEngine;

namespace FarrokhGames.Inventory
{
    public abstract class IInventoryManager
    {
        public Action<IInventoryItem> onItemAdded { get; set; }
        public Action<IInventoryItem> onItemAddedFailed { get; set; }
        public Action<IInventoryItem> onItemRemoved { get; set; }
        public Action<IInventoryItem> onItemDropped { get; set; }
        public Action<IInventoryItem> onItemDroppedFailed { get; set; }
        public Action onRebuilt { get; set; }
        public Action onResized { get; set; }
        int width { get; }
        int height { get; }

        IInventoryItem[] allItems { get; }
    }
}
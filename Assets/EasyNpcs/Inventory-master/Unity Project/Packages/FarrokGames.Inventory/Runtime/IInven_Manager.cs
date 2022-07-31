using System;
using UnityEngine;

namespace FarrokhGames.Inventory
{
    public abstract class IInven_Manager
    {
        public Action<IInven_Item> onItemAdded { get; set; }
        public Action<IInven_Item> onItemAddedFailed { get; set; }
        public Action<IInven_Item> onItemRemoved { get; set; }
        public Action<IInven_Item> onItemDropped { get; set; }
        public Action<IInven_Item> onItemDroppedFailed { get; set; }
        public Action onRebuilt { get; set; }
        public Action onResized { get; set; }
        int width { get; }
        int height { get; }

        IInven_Item[] allItems { get; }
    }
}
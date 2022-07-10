using System;
using UnityEngine;

namespace FarrokhGames.Inventory
{
    public abstract class IInventoryManager
    {
        /// <summary>
        /// Invoked when an item is added to the inventory
        /// </summary>
        public Action<IInventoryItem> onItemAdded { get; set; }
        
        /// <summary>
        /// Invoked when an item was not able to be added to the inventory
        /// </summary>
        public Action<IInventoryItem> onItemAddedFailed { get; set; }

        /// <summary>
        /// Invoked when an item is removed to the inventory
        /// </summary>
        public Action<IInventoryItem> onItemRemoved { get; set; }

        /// <summary>
        /// Invoked when an item is removed from the inventory and should be placed on the ground.
        /// </summary>
        public Action<IInventoryItem> onItemDropped { get; set; }
        
        /// <summary>
        /// Invoked when an item was unable to be placed on the ground (most likely to its canDrop being set to false)
        /// </summary>
        public Action<IInventoryItem> onItemDroppedFailed { get; set; }
        
        /// <summary>
        /// Invoked when the inventory is rebuilt from scratch
        /// </summary>
        public Action onRebuilt { get; set; }

        /// <summary>
        /// Invoked when the inventory changes its size
        /// </summary>
        public Action onResized { get; set; }

        /// <summary>
        /// The width of the inventory
        /// </summary>
        int width { get; }

        /// <summary>
        /// The height of the inventory
        /// </summary>
        int height { get; }

        IInventoryItem[] allItems { get; }
    }
}
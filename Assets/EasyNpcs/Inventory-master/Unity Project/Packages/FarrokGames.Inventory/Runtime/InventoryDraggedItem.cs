using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UI;

namespace FarrokhGames.Inventory
{
    public class InventoryDraggedItem
    {
        public enum DropMode
        {
            Added,
            Swapped,
            Returned,
            Dropped,
        }

        public InventoryController originalController { get; private set; }

        public Vector2Int originPoint { get; private set; }

        public IInventoryItem item { get; private set; }

        public InventoryController currentController;

        private readonly Canvas _canvas;
        private readonly RectTransform _canvasRect;
        private Image _image;
        private Vector2 _offset;

        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        public InventoryDraggedItem(
            Canvas canvas,
            InventoryController originalController,
            Vector2Int originPoint,
            IInventoryItem item,
            Vector2 offset,
            InventoryRenderer renderer)
        {
            this.originalController = originalController;
            currentController = this.originalController;
            this.originPoint = originPoint;
            this.item = item;

            _canvas = canvas;
            _canvasRect = canvas.transform as RectTransform;

            _offset = offset; 

            CreateImageForDrag(renderer);
        }

        void CreateImageForDrag(InventoryRenderer renderer)
        {
            _image = new GameObject("DraggedItem").AddComponent<Image>();
            _image.raycastTarget = false;
            _image.transform.SetParent(_canvas.transform);
            _image.transform.SetAsLastSibling();
            _image.transform.localScale = new Vector3(renderer.cellSize.x / 20, renderer.cellSize.y / 20, 1);
            _image.sprite = item.sprite;
            _image.SetNativeSize();
        }

        /// <summary>
        /// Gets or sets the position of this dragged item
        /// </summary>
        public Vector2 position
        {
            set
            {
                // Move the image
                var camera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, value + _offset, camera,  out var newValue);
                _image.rectTransform.localPosition = newValue;
                
                
                // Make selections
                if (currentController != null)
                {
                    item.position = currentController.ScreenPosition_To_GridPosition(value + _offset + GetDraggedItemOffset(currentController.inventoryRenderer, item));
                    var canAdd = ItemAddable_Check.CanAddAt(item, item.position, currentController.inventory) || CanSwap();
                    currentController.inventoryRenderer.SelectItem(item, !canAdd, Color.white);
                }

                // Slowly animate the item towards the center of the mouse pointer
                _offset = Vector2.Lerp(_offset, Vector2.zero, Time.deltaTime * 10f);
            }
        }

        /// <summary>
        /// Drop this item at the given position
        /// </summary>
        public DropMode Drop(Vector2 pos)
        {
            DropMode mode;
            if (currentController != null)
            {
                var grid = currentController.ScreenPosition_To_GridPosition(pos + _offset + GetDraggedItemOffset(currentController.inventoryRenderer, item));

                // Try to add new item
                if (ItemAddable_Check.CanAddAt(item, grid, currentController.inventory))
                {
                    currentController.inventory.TryAddAt(item, grid); // Place the item in a new location
                    if (TradeManager.originalController != currentController)
                        mode = DropMode.Added;
                    else
                        mode = DropMode.Returned;
                }
                // Adding did not work, try to swap
                else if (CanSwap())
                {
                    var otherItem = currentController.inventory.allItems[0];
                    currentController.inventory.TryRemove(otherItem);
                    originalController.inventory.TryAdd(otherItem);
                    currentController.inventory.TryAdd(item);
                    mode = DropMode.Swapped;
                }
                // Could not add or swap, return the item
                else
                {
                    originalController.inventory.TryAddAt(item, originPoint); // Return the item to its previous location
                    mode = DropMode.Returned;

                }

                currentController.inventoryRenderer.ClearSelection();
            }
            else
            {
                mode = DropMode.Dropped;
                if (!originalController.inventory.TryForceDrop(item)) // Drop the item on the ground
                {
                    originalController.inventory.TryAddAt(item, originPoint);
                }
            }

            // Destroy the image representing the item
            Object.Destroy(_image.gameObject);

            return mode;
        }

        /*
         * Returns the offset between dragged item and the grid 
         */
        private Vector2 GetDraggedItemOffset(InventoryRenderer renderer, IInventoryItem item)
        {
            var scale = new Vector2(
                Screen.width / _canvasRect.sizeDelta.x,
                Screen.height / _canvasRect.sizeDelta.y
            );
            var gx = -(item.width * renderer.cellSize.x / 2f) + (renderer.cellSize.x / 2);
            var gy = -(item.height * renderer.cellSize.y / 2f) + (renderer.cellSize.y / 2);
            return new Vector2(gx, gy) * scale;
        }
        
        /* 
         * Returns true if its possible to swap
         */
        private bool CanSwap()
        {
            if (!currentController.inventory.CanSwap(item)) return false;
            var otherItem = currentController.inventory.allItems[0];
            return originalController.inventory.CanAdd(otherItem) && currentController.inventory.CanRemove(otherItem);
        }
    }
}
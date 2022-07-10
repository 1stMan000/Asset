using FarrokhGames.Shared;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FarrokhGames.Inventory
{
    [RequireComponent(typeof(RectTransform))]
    public class InventoryRenderer : BaseRenderer
    {
        [SerializeField, Tooltip("The size of the cells building up the inventory")]
        private Vector2Int _cellSize = new Vector2Int(32, 32);

        [SerializeField, Tooltip("The sprite to use for empty cells")]
        private Sprite _cellSpriteEmpty = null;

        [SerializeField, Tooltip("The sprite to use for selected cells")]
        private Sprite _cellSpriteSelected = null;

        [SerializeField, Tooltip("The sprite to use for blocked cells")]
        private Sprite _cellSpriteBlocked = null;

        internal InventoryManager inventory;
        InventoryRenderMode _renderMode;
        private bool _haveListeners;
        private Image[] _grids;
        private Dictionary<IInventoryItem, Image> _items = new Dictionary<IInventoryItem, Image>();

        void Awake()
        {
            cellSize = _cellSize;
            rectTransform = GetComponent<RectTransform>();

            var imageContainer = Set_Images.Set_Parent_For_ImageObjects(transform);
            imagePool = new Pool<Image>(
                delegate
                {
                    return Set_Images.ImageObject(imageContainer);
                });
        }

        public void SetInventory(InventoryManager inventoryManager, InventoryRenderMode renderMode)
        {
            OnDisable();
            inventory = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager)); 
            _renderMode = renderMode;
            OnEnable();
        }

        public RectTransform rectTransform { get; private set; }

        void OnEnable()
        {
            if (inventory != null && !_haveListeners)
            {
                Check_Cell_Sprites();

                inventory.onRebuilt += ReRenderAllItems;
                inventory.onItemAdded += HandleItemAdded;
                inventory.onItemRemoved += HandleItemRemoved;
                inventory.onItemDropped += HandleItemRemoved;
                inventory.onResized += HandleResized;
                _haveListeners = true;

                ReRenderGrid();
                ReRenderAllItems();
            }
        }

        void OnDisable()
        {
            if (inventory != null && _haveListeners)
            {
                inventory.onRebuilt -= ReRenderAllItems;
                inventory.onItemAdded -= HandleItemAdded;
                inventory.onItemRemoved -= HandleItemRemoved;
                inventory.onItemDropped -= HandleItemRemoved;
                inventory.onResized -= HandleResized;
                _haveListeners = false;
            }
        }

        void Check_Cell_Sprites()
        {
            if (_cellSpriteEmpty == null) { throw new NullReferenceException("Sprite for empty cell is null"); }
            if (_cellSpriteSelected == null) { throw new NullReferenceException("Sprite for selected cells is null."); }
            if (_cellSpriteBlocked == null) { throw new NullReferenceException("Sprite for blocked cells is null."); }
        }
        
        private void ReRenderGrid()
        {
            GridsRenderer gridsRenderer = new GridsRenderer(_grids, imagePool, cellSize, inventory, _cellSpriteEmpty);
            gridsRenderer.Remove_All_Grids();

            var containerSize = new Vector2(cellSize.x * inventory.width, cellSize.y * inventory.height);
            Image grid = null;
            switch (_renderMode)
            {
                case InventoryRenderMode.Single:
                    gridsRenderer.OnReRenderGrid_Single(containerSize, grid, out _grids);
                    break;
                default:
                    gridsRenderer.OnRerenderGrid_Default(containerSize, grid, out _grids);
                    break;
            }

            rectTransform.sizeDelta = containerSize;
        }

        private void ReRenderAllItems()
        {
            ClearAllItems();

            foreach (var item in inventory.allItems)
            {
                HandleItemAdded(item);
            }
        }

        void ClearAllItems()
        {
            foreach (var image in _items.Values)
            {
                image.gameObject.SetActive(false);
                Set_Image_To_Inactive(image);
            }

            _items.Clear();
        }

        private void Set_Image_To_Inactive(Image image)
        {
            image.gameObject.name = "Image";
            image.gameObject.SetActive(false);
            imagePool.Set_Image_To_Inactive(image);
        }

        private void HandleItemAdded(IInventoryItem item)
        {
            var img = CreateImage(item.sprite, false);

            if (_renderMode == InventoryRenderMode.Single)
            {
                img.rectTransform.localPosition = rectTransform.rect.center;
            }
            else
            {
                img.rectTransform.localPosition = GetItemOffset(item);
            }

            _items.Add(item, img);
        }

        internal Vector2 GetItemOffset(IInventoryItem item)
        {
            var x = (-(inventory.width * 0.5f) + item.position.x + item.width * 0.5f) * cellSize.x;
            var y = (-(inventory.height * 0.5f) + item.position.y + item.height * 0.5f) * cellSize.y;
            return new Vector2(x, y);
        }

        private void HandleItemRemoved(IInventoryItem item)
        {
            if (_items.ContainsKey(item))
            {
                var image = _items[item];
                image.gameObject.SetActive(false);
                Set_Image_To_Inactive(image);
                _items.Remove(item);
            }
        }

        private void HandleResized()
        {
            ReRenderGrid();
            ReRenderAllItems();
        }

        public void SelectItem(IInventoryItem item, bool blocked, Color color)
        {
            if (item == null) { return; }
            ClearSelection();

            switch (_renderMode)
            {
                case InventoryRenderMode.Single:
                    ColorGrid(0, blocked, color);
                    break;
                default:
                    Color_All_Grids_Item_Is_On(item, blocked, color);
                    break;
            }
        }

        private void Color_All_Grids_Item_Is_On(IInventoryItem item, bool blocked, Color color)
        {
            for (var x = 0; x < item.width; x++)
            {
                for (var y = 0; y < item.height; y++)
                {
                    if (item.IsPartOfShape(new Vector2Int(x, y)))
                    {
                        var p = item.position + new Vector2Int(x, y);
                        if (p.x >= 0 && p.x < inventory.width && p.y >= 0 && p.y < inventory.height)
                        {
                            var index = p.y * inventory.width + ((inventory.width - 1) - p.x);
                            ColorGrid(index, blocked, color);
                        }
                    }
                }
            }
        }

        private void ColorGrid(int index, bool blocked, Color color)
        {
            _grids[index].sprite = blocked ? _cellSpriteBlocked : _cellSpriteSelected;
            _grids[index].color = color;
        }

        public void ClearSelection()
        {
            for (var i = 0; i < _grids.Length; i++)
            {
                _grids[i].sprite = _cellSpriteEmpty;
                _grids[i].color = Color.white;
            }
        }
    }
}
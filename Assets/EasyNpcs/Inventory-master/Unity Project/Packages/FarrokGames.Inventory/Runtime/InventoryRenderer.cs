using FarrokhGames.Shared;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FarrokhGames.Inventory
{
    [RequireComponent(typeof(RectTransform))]
    public class InventoryRenderer : MonoBehaviour
    {
        [SerializeField, Tooltip("The size of the cells building up the inventory")]
        private Vector2Int _cellSize = new Vector2Int(32, 32);

        [SerializeField, Tooltip("The sprite to use for empty cells")]
        private Sprite _cellSpriteEmpty = null;

        [SerializeField, Tooltip("The sprite to use for selected cells")]
        private Sprite _cellSpriteSelected = null;

        [SerializeField, Tooltip("The sprite to use for blocked cells")]
        private Sprite _cellSpriteBlocked = null;

        internal IInventoryManager inventory;
        InventoryRenderMode _renderMode;
        private bool _haveListeners;
        private Pool<Image> _imagePool;
        private Image[] _grids;
        private Dictionary<IInventoryItem, Image> _items = new Dictionary<IInventoryItem, Image>();

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            var imageContainer = Set_Parent_For_ImageObjects();
            _imagePool = new Pool<Image>(
                delegate
                {
                    return ImageObject(imageContainer);
                });
        }

        RectTransform Set_Parent_For_ImageObjects()
        {
            var parent_for_ImageObject = new GameObject("Image Pool").AddComponent<RectTransform>();
            parent_for_ImageObject.transform.SetParent(transform);
            parent_for_ImageObject.transform.localPosition = Vector3.zero;
            parent_for_ImageObject.transform.localScale = Vector3.one;

            return parent_for_ImageObject;
        }

        Image ImageObject(RectTransform parent_for_ImageObject)
        {
            var image = new GameObject("Image").AddComponent<Image>();
            image.transform.SetParent(parent_for_ImageObject);
            image.transform.localScale = Vector3.one;

            return image;
        }

        public void SetInventory(IInventoryManager inventoryManager, InventoryRenderMode renderMode)
        {
            OnDisable();
            inventory = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager)); 
            _renderMode = renderMode;
            OnEnable();
        }

        public RectTransform rectTransform { get; private set; }
        public Vector2 cellSize => _cellSize;

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
            GridsRenderer gridsRenderer = new GridsRenderer(_grids, _imagePool);
            gridsRenderer.Remove_All_Grids();

            var containerSize = new Vector2(cellSize.x * inventory.width, cellSize.y * inventory.height);
            Image grid = null;
            switch (_renderMode)
            {
                case InventoryRenderMode.Single:
                    OnReRenderGrid_Single(containerSize, grid);
                    break;
                default:
                    OnRerenderGrid_Default(containerSize, grid);
                    break;
            }

            rectTransform.sizeDelta = containerSize;
        }

        void OnRerenderGrid_Default(Vector2 containerSize, Image grid)
        {
            var move_to_origin_symmetry = new Vector3(-containerSize.x / 2, -containerSize.y / 2, 0);
            var move_to_halfCellSize = new Vector3(cellSize.x / 2, cellSize.y / 2, 0);
            var adjust = move_to_origin_symmetry + move_to_halfCellSize;
            _grids = new Image[inventory.width * inventory.height];
            var num = 0;
            for (int y = 0; y < inventory.height; y++)
            {
                for (int x = 0; x < inventory.width; x++)
                {
                    grid = CreateImage(_cellSpriteEmpty, true);
                    grid.gameObject.name = "Grid " + num;
                    grid.rectTransform.SetAsFirstSibling();
                    grid.type = Image.Type.Sliced;
                    grid.rectTransform.localPosition = new Vector3(cellSize.x * ((inventory.width - 1) - x), cellSize.y * y, 0) + adjust;
                    grid.rectTransform.sizeDelta = cellSize;
                    _grids[num] = grid;
                    num++;
                }
            }
        }

        void OnReRenderGrid_Single(Vector2 containerSize, Image grid)
        {
            grid = CreateImage(_cellSpriteEmpty, true);
            grid.rectTransform.SetAsFirstSibling();
            grid.type = Image.Type.Sliced;
            grid.rectTransform.localPosition = Vector3.zero;
            grid.rectTransform.sizeDelta = containerSize;
            _grids = new[] { grid };
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
            _imagePool.Set_Image_To_Inactive(image);
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

        private Image CreateImage(Sprite sprite, bool raycastTarget)
        {
            var img = _imagePool.Activate_ImageObject_In_Pool();
            img.gameObject.SetActive(true);
            img.sprite = sprite;
            img.rectTransform.sizeDelta = new Vector2(img.sprite.rect.width / (40 / _cellSize.x), img.sprite.rect.height / (40 / _cellSize.y));
            img.transform.SetAsLastSibling();
            img.type = Image.Type.Simple;
            img.raycastTarget = raycastTarget;
            return img;
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
                    _grids[0].sprite = blocked ? _cellSpriteBlocked : _cellSpriteSelected;
                    _grids[0].color = color;
                    break;
                default:
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
                                    _grids[index].sprite = blocked ? _cellSpriteBlocked : _cellSpriteSelected;
                                    _grids[index].color = color;
                                }
                            }
                        }
                    }
                    break;
            }
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
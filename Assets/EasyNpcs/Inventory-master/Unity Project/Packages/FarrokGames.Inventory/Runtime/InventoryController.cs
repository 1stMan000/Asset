using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FarrokhGames.Inventory
{
    public interface IInventoryController
    {
        Action<IInventoryItem> onItemHovered { get; set; }
        Action<IInventoryItem> onItemPickedUp { get; set; }
        Action<IInventoryItem> onItemAdded { get; set; }
        Action<IInventoryItem> onItemSwapped { get; set; }
        Action<IInventoryItem> onItemReturned { get; set; }
        Action<IInventoryItem> onItemDropped { get; set; }
    }

    [RequireComponent(typeof(InventoryRenderer))]
    public class InventoryController : MonoBehaviour,
        IPointerDownHandler, IBeginDragHandler, IDragHandler,
        IEndDragHandler, IPointerExitHandler, IPointerEnterHandler,
        IInventoryController
    {
            protected static InventoryDraggedItem _draggedItem;

            public Action<IInventoryItem> onItemHovered { get; set; }
            public Action<IInventoryItem> onItemPickedUp { get; set; }
            public Action<IInventoryItem> onItemAdded { get; set; }
            public Action<IInventoryItem> onItemSwapped { get; set; }
            public Action<IInventoryItem> onItemReturned { get; set; }
            public Action<IInventoryItem> onItemDropped { get; set; }

            private Canvas _canvas;
            internal InventoryRenderer inventoryRenderer;
            internal InventoryManager inventory => (InventoryManager) inventoryRenderer.inventory;

            private IInventoryItem _itemToDrag;
            private PointerEventData _currentEventData;
            private IInventoryItem _lastHoveredItem;

            public GameObject player;

            void Awake()
            {
                inventoryRenderer = GetComponent<InventoryRenderer>();
                if (inventoryRenderer == null) { throw new NullReferenceException("Could not find a renderer. This is not allowed!"); }

                var canvases = GetComponentsInParent<Canvas>();
                if (canvases.Length == 0) { throw new NullReferenceException("Could not find a canvas."); }
                _canvas = canvases[canvases.Length - 1];

                onItemDropped += DropItemToWorld;
                onItemPickedUp += Store_OriginalController;
            }   

            void DropItemToWorld(IInventoryItem item)
            {
                _draggedItem = null;

                Instantiate(item.dropObject);
                item.dropObject.transform.position = player.transform.position + Vector3.forward;
            }

            void Store_OriginalController(IInventoryItem item)
            {
                TradeManager.originalController = _draggedItem.currentController;
            }

            public void OnPointerDown(PointerEventData eventData)
            {
                if (_draggedItem != null) return;
                
                var grid = ScreenPosition_To_GridPosition(eventData.position);
                _itemToDrag = inventory.GetAtPoint(grid);
            }

            public void OnBeginDrag(PointerEventData eventData)
            {
                inventoryRenderer.ClearSelection();

                if (_itemToDrag == null || _draggedItem != null) return;
                
                var localPosition = ScreenToLocalPositionInRenderer(eventData.position);
                var itemOffest = inventoryRenderer.GetItemOffset(_itemToDrag);
                var offset = itemOffest - localPosition;

                _draggedItem = new InventoryDraggedItem(
                    _canvas,
                    this,
                    _itemToDrag.position,
                    _itemToDrag,
                    offset,
                    inventoryRenderer
                );

                inventory.TryRemove(_itemToDrag);

                onItemPickedUp?.Invoke(_itemToDrag);
            }

            public void OnDrag(PointerEventData eventData)
            {
                _currentEventData = eventData;
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                if (_draggedItem == null) return;
                
                var mode = _draggedItem.Drop(eventData.position);

                switch (mode)
                {
                    case InventoryDraggedItem.DropMode.Added:
                        onItemAdded?.Invoke(_itemToDrag);
                        break;
                    case InventoryDraggedItem.DropMode.Swapped:
                        onItemSwapped?.Invoke(_itemToDrag);
                        break;
                    case InventoryDraggedItem.DropMode.Returned:
                        onItemReturned?.Invoke(_itemToDrag);
                        break;
                    case InventoryDraggedItem.DropMode.Dropped:
                        onItemDropped?.Invoke(_itemToDrag);
                        ClearHoveredItem();
                        break;
                }

                _draggedItem = null;
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                if (_draggedItem != null)
                {
                    // Clear the item as it leaves its current controller
                    _draggedItem.currentController = null;
                    inventoryRenderer.ClearSelection();
                }
                else { ClearHoveredItem(); }
                _currentEventData = null;
            }

            public void OnPointerEnter(PointerEventData eventData)
            {
                if (_draggedItem != null)
                {
                    // Change which controller is in control of the dragged item
                    _draggedItem.currentController = this;
                }
                _currentEventData = eventData;
            }

            void Update()
            {
                if (_currentEventData == null) return;
                
                if (_draggedItem == null)
                {
                    // Detect hover
                    var grid = ScreenPosition_To_GridPosition(_currentEventData.position);
                    var item = inventory.GetAtPoint(grid);
                    if (item == _lastHoveredItem) return;
                    onItemHovered?.Invoke(item);
                    _lastHoveredItem = item;
                }
                else
                {
                    // Update position while dragging
                    _draggedItem.position = _currentEventData.position;
                }
            }

            private void ClearHoveredItem()
            {
                if (_lastHoveredItem != null)
                {
                    onItemHovered?.Invoke(null);
                }
                _lastHoveredItem = null;
            }

            internal Vector2Int ScreenPosition_To_GridPosition(Vector2 screenPoint)
            {
                var pos = ScreenToLocalPositionInRenderer(screenPoint);
                var sizeDelta = inventoryRenderer.rectTransform.sizeDelta;
                pos.x += sizeDelta.x / 2;
                pos.y += sizeDelta.y / 2;
                return new Vector2Int(Mathf.FloorToInt(pos.x / inventoryRenderer.cellSize.x), Mathf.FloorToInt(pos.y / inventoryRenderer.cellSize.y));
            }

            private Vector2 ScreenToLocalPositionInRenderer(Vector2 screenPosition)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    inventoryRenderer.rectTransform,
                    screenPosition,
                    _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera,
                    out var localPosition
                );
                return localPosition;
            }
    }
}
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FarrokhGames.Inventory
{
    public interface IInven_Controller
    {
        Action<IInven_Item> onItemHovered { get; set; }
        Action<IInven_Item> onItemPickedUp { get; set; }
        Action<IInven_Item> onItemAdded { get; set; }
        Action<IInven_Item> onItemSwapped { get; set; }
        Action<IInven_Item> onItemReturned { get; set; }
        Action<IInven_Item> onItemDropped { get; set; }
    }

    [RequireComponent(typeof(Inven_Renderer))]
    public class Inven_Controller : MonoBehaviour,
        IPointerDownHandler, IBeginDragHandler, IDragHandler,
        IEndDragHandler, IPointerExitHandler, IPointerEnterHandler,
        IInven_Controller
    {
        protected static Inven_DraggedItem _draggedItem;

            public Action<IInven_Item> onItemHovered { get; set; }
            public Action<IInven_Item> onItemPickedUp { get; set; }
            public Action<IInven_Item> onItemAdded { get; set; }
            public Action<IInven_Item> onItemSwapped { get; set; }
            public Action<IInven_Item> onItemReturned { get; set; }
            public Action<IInven_Item> onItemDropped { get; set; }

            private Canvas _canvas;
            internal Inven_Renderer inventoryRenderer;
            public Inven_Manager inventory => (Inven_Manager) inventoryRenderer.inventory;

            protected IInven_Item _itemToDrag;
            private PointerEventData _currentEventData;
            private IInven_Item _lastHoveredItem;

            void Awake()
            {
                inventoryRenderer = GetComponent<Inven_Renderer>();
                if (inventoryRenderer == null) { throw new NullReferenceException("Could not find a renderer. This is not allowed!"); }

                var canvases = GetComponentsInParent<Canvas>();
                if (canvases.Length == 0) { throw new NullReferenceException("Could not find a canvas."); }
                _canvas = canvases[canvases.Length - 1];

                onItemDropped += DropItemToWorld;
                onItemPickedUp += Store_OriginalController;
            }   

            void DropItemToWorld(IInven_Item item)
            {
                _draggedItem = null;

                Instantiate(item.dropObject);
                item.dropObject.transform.position = GameObject.Find("Player").transform.position + Vector3.forward;
            }

            void Store_OriginalController(IInven_Item item)
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

                _draggedItem = new Inven_DraggedItem(
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
                    case Inven_DraggedItem.DropMode.Added:
                        onItemAdded?.Invoke(_itemToDrag);
                        break;
                    case Inven_DraggedItem.DropMode.Swapped:
                        onItemSwapped?.Invoke(_itemToDrag);
                        break;
                    case Inven_DraggedItem.DropMode.Returned:
                        onItemReturned?.Invoke(_itemToDrag);
                        break;
                    case Inven_DraggedItem.DropMode.Dropped:
                        Item_Dropped();
                        break;
                }

                _draggedItem = null;
            }

        protected virtual void Item_Dropped()
        {
            onItemDropped?.Invoke(_itemToDrag);
            ClearHoveredItem();
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
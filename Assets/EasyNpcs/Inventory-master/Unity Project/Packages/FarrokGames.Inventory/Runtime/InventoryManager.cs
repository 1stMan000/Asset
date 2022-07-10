using System;
using System.Linq;
using UnityEngine;

namespace FarrokhGames.Inventory
{
    public class InventoryManager : IInventoryManager
    {
        private Vector2Int _size = Vector2Int.one;
        private IInventoryProvider _provider;
        private Rect _fullRect;

        public InventoryManager(IInventoryProvider provider, int width, int height)
        {
            _provider = provider;
            Rebuild();
            Resize(width, height);
        }

        public void Rebuild()
        {
            Rebuild(false);
        }

        public IInventoryItem[] allItems { get; private set; }

        private void Rebuild(bool notFromScratch)
        {
            allItems = new IInventoryItem[_provider.inventoryItemCount];
            for (var i = 0; i < _provider.inventoryItemCount; i++)
            {
                allItems[i] = _provider.GetInventoryItem(i);
            }

            if (!notFromScratch) onRebuilt?.Invoke();
        }

        public int width => _size.x;
        public int height => _size.y;

        public void Resize(int newWidth, int newHeight)
        {
            _size.x = newWidth;
            _size.y = newHeight;
            RebuildRect();
        }
        
        private void RebuildRect()
        {
            _fullRect = new Rect(0, 0, _size.x, _size.y);
            Check_If_Items_Fit();
            onResized?.Invoke();
        }

        private void Check_If_Items_Fit()
        {
            for (int i = 0; i < allItems.Length;)
            {
                var item = allItems[i];
                var padding = Vector2.one * 0.01f;

                if (!_fullRect.Contains(item.GetLowerLeftPoint() + padding) || !_fullRect.Contains(item.GetTopRightPoint() - padding))
                {
                    TryDrop(item);
                }
                else
                {
                    i++;
                }
            }
        }

        public void Dispose()
        {
            _provider = null;
            allItems = null;
        }

        public bool isFull
        {
            get
            {
                if (_provider.isInventoryFull)return true;

                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        if (GetAtPoint(new Vector2Int(x, y)) == null) { return false; }
                    }
                }
                return true;
            }
        }

        public IInventoryItem GetAtPoint(Vector2Int point)
        {
            if (GetAtPoint_SingleRender_Inventory())
            {
                return allItems[0];
            }

            foreach (var item in allItems)
            {
                if (item.Contains(point)) { return item; }
            }

            return null;
        }

        bool GetAtPoint_SingleRender_Inventory()
        {
            if (_provider.inventoryRenderMode == InventoryRenderMode.Single && _provider.isInventoryFull && allItems.Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryRemove(IInventoryItem item)
        {
            if (!CanRemove(item)) return false;
            if (!_provider.RemoveInventoryItem(item)) return false;

            Rebuild(true);
            onItemRemoved?.Invoke(item);
            return true;
        }

        public bool TryDrop(IInventoryItem item)
        {
            if (!CanDrop(item) || !_provider.DropInventoryItem(item)) 
			{
				onItemDroppedFailed?.Invoke(item);
				return false;
			}

            Rebuild(true);
            onItemDropped?.Invoke(item);
            return true;
        }

		internal bool TryForceDrop(IInventoryItem item)
		{
			if(!item.canDrop)
			{
				onItemDroppedFailed?.Invoke(item);
				return false;
			}

			onItemDropped?.Invoke(item);
			return true;
		}

        public bool CanAddAt(IInventoryItem item, Vector2Int point)
        {
            if (!_provider.CanAddInventoryItem(item) || _provider.isInventoryFull)
            {
                return false;
            }

            if (_provider.inventoryRenderMode == InventoryRenderMode.Single)
            {
                return true;
            }

            var previousPoint = item.position;
            item.position = point;
            var padding = Vector2.one * 0.01f;

            if (!_fullRect.Contains(item.GetLowerLeftPoint() + padding) || !_fullRect.Contains(item.GetTopRightPoint() - padding))
            {
                item.position = previousPoint;
                return false;
            }

            if (!allItems.Any(otherItem => item.Overlaps(otherItem))) return true; 
            item.position = previousPoint;
            return false;
        }

        public bool TryAddAt(IInventoryItem item, Vector2Int point)
        {
            if (!CanAddAt(item, point) || !_provider.AddInventoryItem(item)) 
			{
				onItemAddedFailed?.Invoke(item);
				return false;
			}
            switch (_provider.inventoryRenderMode)
            {
                case InventoryRenderMode.Single:
                    item.position = GetCenterPosition(item);
                    break;
                case InventoryRenderMode.Grid:
                    item.position = point;
                    break;
                default:
                    throw new NotImplementedException($"InventoryRenderMode.{_provider.inventoryRenderMode.ToString()} have not yet been implemented");
            }
            Rebuild(true);
            onItemAdded?.Invoke(item);
            return true;
        }

        public bool CanAdd(IInventoryItem item)
        {
            Vector2Int point;
            if (!Contains(item) && GetFirstPointThatFitsItem(item, out point))
            {
                return CanAddAt(item, point);
            }
            return false;
        }

        public bool TryAdd(IInventoryItem item)
        {
            if (!CanAdd(item))return false;
            Vector2Int point;
            return GetFirstPointThatFitsItem(item, out point) && TryAddAt(item, point);
        }

        public bool CanSwap(IInventoryItem item)
        {
            return _provider.inventoryRenderMode == InventoryRenderMode.Single &&
                DoesItemFit(item) &&
                _provider.CanAddInventoryItem(item);
        }

        public void DropAll()
        {
            var itemsToDrop = allItems.ToArray();
            foreach (var item in itemsToDrop)
            {
                TryDrop(item);
            }
        }

        public void Clear()
        {
            foreach (var item in allItems)
            {
                TryRemove(item);
            }
        }

        public bool Contains(IInventoryItem item) => allItems.Contains(item);
        
        public bool CanRemove(IInventoryItem item) => Contains(item) && _provider.CanRemoveInventoryItem(item);

        public bool CanDrop(IInventoryItem item) => Contains(item) && _provider.CanDropInventoryItem(item) && item.canDrop;
        
        private bool GetFirstPointThatFitsItem(IInventoryItem item, out Vector2Int point)
        {
            if (DoesItemFit(item))
            {
                for (var x = 0; x < width - (item.width - 1); x++)
                {
                    for (var y = 0; y < height - (item.height - 1); y++)
                    {
                        point = new Vector2Int(x, y);
                        if (CanAddAt(item, point))return true;
                    }
                }
            }
            point = Vector2Int.zero;
            return false;
        }

        private bool DoesItemFit(IInventoryItem item) => item.width <= width && item.height <= height;

        private Vector2Int GetCenterPosition(IInventoryItem item)
        {
            return new Vector2Int(
                (_size.x - item.width) / 2,
                (_size.y - item.height) / 2
            );
        }
    }
}
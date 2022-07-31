using System;
using System.Linq;
using UnityEngine;

namespace FarrokhGames.Inventory
{
    public class Inven_Manager : IInven_Manager
    {
        private Vector2Int _size = Vector2Int.one;
        public IInven_Provider _provider { get; private set;}
        public Rect _fullRect { get; private set; }

        public Inven_Manager(IInven_Provider provider, int width, int height)
        {
            _provider = provider;
            Rebuild();
            Resize(width, height);
        }

        public void Rebuild()
        {
            Rebuild(false);
        }

        public IInven_Item[] allItems { get; private set; }

        private void Rebuild(bool notFromScratch)
        {
            allItems = new IInven_Item[_provider.inventoryItemCount];
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

        public IInven_Item GetAtPoint(Vector2Int point)
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
            if (_provider.inventoryRenderMode == Inven_RenderMode.Single && _provider.isInventoryFull && allItems.Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryRemove(IInven_Item item)
        {
            if (!CanRemove(item)) return false;
            if (!_provider.RemoveInventoryItem(item)) return false;

            Rebuild(true);
            onItemRemoved?.Invoke(item);
            return true;
        }

        public bool TryDrop(IInven_Item item)
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

		internal bool TryForceDrop(IInven_Item item)
		{
			if(!item.canDrop)
			{
				onItemDroppedFailed?.Invoke(item);
				return false;
			}

			onItemDropped?.Invoke(item);
			return true;
		}

        public bool TryAddAt(IInven_Item item, Vector2Int point)
        {
            if (!ItemAddable_Check.CanAddAt(item, point, this) || !_provider.AddInventoryItem(item)) 
			{
				onItemAddedFailed?.Invoke(item);
				return false;
			}
            switch (_provider.inventoryRenderMode)
            {
                case Inven_RenderMode.Single:
                    item.position = GetCenterPosition(item);
                    break;
                case Inven_RenderMode.Grid:
                    item.position = point;
                    break;
                default:
                    throw new NotImplementedException($"InventoryRenderMode.{_provider.inventoryRenderMode.ToString()} have not yet been implemented");
            }
            Rebuild(true);
            onItemAdded?.Invoke(item);
            return true;
        }

        public bool CanAdd(IInven_Item item)
        {
            Vector2Int point;
            if (!Contains(item) && GetFirstPointThatFitsItem(item, out point))
            {
                return ItemAddable_Check.CanAddAt(item, point, this);
            }
            return false;
        }

        public bool TryAdd(IInven_Item item)
        {
            if (!CanAdd(item))return false;
            Vector2Int point;
            return GetFirstPointThatFitsItem(item, out point) && TryAddAt(item, point);
        }

        public bool CanSwap(IInven_Item item)
        {
            return _provider.inventoryRenderMode == Inven_RenderMode.Single &&
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

        public bool Contains(IInven_Item item) => allItems.Contains(item);
        
        public bool CanRemove(IInven_Item item) => Contains(item) && _provider.CanRemoveInventoryItem(item);

        public bool CanDrop(IInven_Item item) => Contains(item) && _provider.CanDropInventoryItem(item) && item.canDrop;
        
        private bool GetFirstPointThatFitsItem(IInven_Item item, out Vector2Int point)
        {
            if (DoesItemFit(item))
            {
                for (var x = 0; x < width - (item.width - 1); x++)
                {
                    for (var y = 0; y < height - (item.height - 1); y++)
                    {
                        point = new Vector2Int(x, y);
                        if (ItemAddable_Check.CanAddAt(item, point, this))return true;
                    }
                }
            }
            point = Vector2Int.zero;
            return false;
        }

        private bool DoesItemFit(IInven_Item item) => item.width <= width && item.height <= height;

        private Vector2Int GetCenterPosition(IInven_Item item)
        {
            return new Vector2Int(
                (_size.x - item.width) / 2,
                (_size.y - item.height) / 2
            );
        }
    }
}
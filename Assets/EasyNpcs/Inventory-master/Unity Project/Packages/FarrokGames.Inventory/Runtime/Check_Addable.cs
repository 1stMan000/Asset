using System.Linq;
using UnityEngine;

namespace FarrokhGames.Inventory
{
    public class Check_Addable : MonoBehaviour
    {
        private IInventoryProvider _provider;
        private Rect _fullRect;
        private IInventoryItem[] _allItems;

        public Check_Addable(IInventoryProvider provider, Rect fullRect, IInventoryItem[] allItems)
        {
            _provider = provider;
            _fullRect = fullRect;
            _allItems = allItems;
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

            if (!_allItems.Any(otherItem => item.Overlaps(otherItem))) return true;
            item.position = previousPoint;
            return false;
        }
    }
}

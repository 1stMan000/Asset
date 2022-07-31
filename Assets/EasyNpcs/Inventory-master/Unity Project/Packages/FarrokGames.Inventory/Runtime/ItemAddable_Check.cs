using System.Linq;
using UnityEngine;

namespace FarrokhGames.Inventory
{
    public static class ItemAddable_Check 
    {
        public static bool CanAddAt(IInven_Item item, Vector2Int point, InventoryManager manager)
        {
            if (!manager._provider.CanAddInventoryItem(item) || manager._provider.isInventoryFull)
            {
                return false;
            }

            if (manager._provider.inventoryRenderMode == InventoryRenderMode.Single)
            {
                return true;
            }

            var previousPoint = item.position;
            item.position = point;
            var padding = Vector2.one * 0.01f;

            if (!manager._fullRect.Contains(item.GetLowerLeftPoint() + padding) || !manager._fullRect.Contains(item.GetTopRightPoint() - padding))
            {
                item.position = previousPoint;
                return false;
            }

            if (!manager.allItems.Any(otherItem => item.Overlaps(otherItem))) return true;
            item.position = previousPoint;
            return false;
        }
    }
}
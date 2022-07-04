using UnityEngine;

namespace FarrokhGames.Inventory
{
    public interface IInventoryItem
    {
        Sprite sprite { get; }

        Vector2Int position { get; set; }

        int width { get; }

        int height { get; }

        bool IsPartOfShape(Vector2Int localPosition);
        
        bool canDrop { get; }

        GameObject dropObject { get; }
    }

    internal static class InventoryItemExtensions
    {
        internal static Vector2Int GetLowerLeftPoint(this IInventoryItem item)
        {
            return item.position;
        }

        internal static Vector2Int GetTopRightPoint(this IInventoryItem item)
        {
            return item.position + new Vector2Int(item.width, item.height);
        }

        internal static bool Contains(this IInventoryItem item, Vector2Int inventoryPoint)
        {
            for (var iX = 0; iX < item.width; iX++)
            {
                for (var iY = 0; iY < item.height; iY++)
                {
                    var iPoint = item.position + new Vector2Int(iX, iY);
                    if (iPoint == inventoryPoint) { return true; }
                }
            }

            return false;
        }

        internal static bool Overlaps(this IInventoryItem item, IInventoryItem otherItem)
        {
            for (var iX = 0; iX < item.width; iX++)
            {
                for (var iY = 0; iY < item.height; iY++)
                {
                    if (item.IsPartOfShape(new Vector2Int(iX, iY)))
                    {
                        var iPoint = item.position + new Vector2Int(iX, iY);
                        for (var oX = 0; oX < otherItem.width; oX++)
                        {
                            for (var oY = 0; oY < otherItem.height; oY++)
                            {
                                if (otherItem.IsPartOfShape(new Vector2Int(oX, oY)))
                                {
                                    var oPoint = otherItem.position + new Vector2Int(oX, oY);
                                    if (oPoint == iPoint) { return true; } 
                                }
                            }
                        }
                    }
                }
            }

            return false; 
        }
    }
}
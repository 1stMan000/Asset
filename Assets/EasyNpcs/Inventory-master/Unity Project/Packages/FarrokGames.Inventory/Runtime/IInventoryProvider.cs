namespace FarrokhGames.Inventory
{
    public interface IInventoryProvider
    {
        InventoryRenderMode inventoryRenderMode { get; }
        int inventoryItemCount { get; }
        bool isInventoryFull { get; }
        IInventoryItem GetInventoryItem(int index);
        bool CanAddInventoryItem(IInventoryItem item);
        bool CanRemoveInventoryItem(IInventoryItem item);
        bool CanDropInventoryItem(IInventoryItem item);
        bool AddInventoryItem(IInventoryItem item);
        bool RemoveInventoryItem(IInventoryItem item);
        bool DropInventoryItem(IInventoryItem item);
    }
}
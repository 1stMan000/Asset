namespace FarrokhGames.Inventory
{
    public interface IInven_Provider
    {
        InventoryRenderMode inventoryRenderMode { get; }
        int inventoryItemCount { get; }
        bool isInventoryFull { get; }
        IInven_Item GetInventoryItem(int index);
        bool CanAddInventoryItem(IInven_Item item);
        bool CanRemoveInventoryItem(IInven_Item item);
        bool CanDropInventoryItem(IInven_Item item);
        bool AddInventoryItem(IInven_Item item);
        bool RemoveInventoryItem(IInven_Item item);
        bool DropInventoryItem(IInven_Item item);
    }
}
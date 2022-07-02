using UnityEngine;

namespace FarrokhGames.Inventory.Examples
{
    [RequireComponent(typeof(InventoryRenderer))]
    public class SizeInventoryExample : MonoBehaviour
    {
        [SerializeField] private InventoryRenderMode _renderMode = InventoryRenderMode.Grid;
        [SerializeField] private int _maximumAlowedItemCount = -1;
        [SerializeField] private ItemType _allowedItem = ItemType.Any;
        [SerializeField] private int _width = 8;
        [SerializeField] private int _height = 4;
        [SerializeField] private ItemDefinition[] _definitions = null;
        [SerializeField] private bool _fillRandomly = true; 
        [SerializeField] private bool _fillEmpty = false; 
        public InventoryManager inventory;

        void Start()
        {
            var provider = new InventoryProvider(_renderMode, _maximumAlowedItemCount, _allowedItem);
            inventory = new InventoryManager(provider, _width, _height);

            FillRandomly();
            FillEmpty();

            GetComponent<InventoryRenderer>().SetInventory(inventory, provider.inventoryRenderMode);

            Tests();
        }

        void FillRandomly()
        {
            if (_fillRandomly)
            {
                var tries = (_width * _height) / 3;
                for (var i = 0; i < tries; i++)
                {
                    inventory.TryAdd(_definitions[Random.Range(0, _definitions.Length)].CreateInstance());
                }
            }
        }

        void FillEmpty()
        {
            if (_fillEmpty)
            {
                for (var i = 0; i < _width * _height; i++)
                {
                    inventory.TryAdd(_definitions[0].CreateInstance());
                }
            }
        }

        void Tests()
        {
            inventory.onItemDropped += (item) =>
            {
                Debug.Log((item as ItemDefinition).Name + " was dropped on the ground");
            };

            inventory.onItemDroppedFailed += (item) =>
            {
                Debug.Log($"You're not allowed to drop {(item as ItemDefinition).Name} on the ground");
            };

            inventory.onItemAddedFailed += (item) =>
            {
                Debug.Log($"You can't put {(item as ItemDefinition).Name} there!");
            };
        }
    }
}
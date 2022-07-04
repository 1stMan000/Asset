using UnityEngine;

namespace FarrokhGames.Inventory.Examples
{
    [CreateAssetMenu(fileName = "Item", menuName = "Inventory/Item", order = 1)]
    public class ItemDefinition : ScriptableObject, IInventoryItem
    {
        [SerializeField] private Sprite _sprite = null;
        [SerializeField] private InventoryShape _shape = null;
        [SerializeField] private ItemType _type = ItemType.Utility;
        [SerializeField] private bool _canDrop = true;
        [SerializeField] private GameObject _gameObject;
        [SerializeField, HideInInspector] private Vector2Int _position = Vector2Int.zero;

        public string Name => this.name;
        public ItemType Type => _type;
        public Sprite sprite => _sprite;
        public int width => _shape.width;
        public int height => _shape.height;

        public GameObject dropObject => _gameObject;

        public Vector2Int position
        {
            get => _position;
            set => _position = value;
        }

        public bool IsPartOfShape(Vector2Int localPosition)
        {
            return _shape.IsPartOfShape(localPosition);
        }

        public bool canDrop => _canDrop;

        public IInventoryItem CreateInstance()
        {
            var clone = ScriptableObject.Instantiate(this);
            clone.name = clone.name.Substring(0, clone.name.Length - 7); 
            return clone;
        }
    }
}
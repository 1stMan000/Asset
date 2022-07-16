using UnityEngine;
using UnityEngine.UI;

namespace FarrokhGames.Inventory.Examples
{
    public class InventoryInitialation : MonoBehaviour
    {
        bool isFirst_Time;
        public HorizontalLayoutGroup horizontalLayout;

        private void Start()
        {
            isFirst_Time = false;
        }

        public void Inventory_Initialization()
        {
            if (isFirst_Time == false)
            {
                SizeInventoryExample[] InventoryExamples = gameObject.GetComponentsInChildren<SizeInventoryExample>();
                foreach (SizeInventoryExample inventoryExample in InventoryExamples)
                {
                    inventoryExample.RenderInventory();
                }

                horizontalLayout.enabled = true;
                isFirst_Time = true;
            }
        }
    }
}

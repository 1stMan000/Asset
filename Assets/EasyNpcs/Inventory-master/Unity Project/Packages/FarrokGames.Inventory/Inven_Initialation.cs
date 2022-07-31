using UnityEngine;
using UnityEngine.UI;
using Player_Actions;

namespace FarrokhGames.Inventory.Examples
{
    public class Inven_Initialation : MonoBehaviour
    {
        bool isFirst_Time;
        HorizontalLayoutGroup horizontalLayout;
        public GameObject Footer;

        PlayerActions playerActions;

        private void Start()
        {
            isFirst_Time = false;
            horizontalLayout = GetComponent<HorizontalLayoutGroup>();
            playerActions = FindObjectOfType<PlayerActions>();

            gameObject.SetActive(false);
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

                Footer.SetActive(true);
                horizontalLayout.enabled = true;
                isFirst_Time = true;
            }
        }
    }
}

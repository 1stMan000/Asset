using FarrokhGames.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace FarrokhGames.Inventory
{
    public class GridsRenderer : MonoBehaviour
    {
        private Image[] grids;
        private Pool<Image> imagePool;

        public GridsRenderer(Image[] _grids, Pool<Image> _imagePool)
        {
            grids = _grids;
            imagePool = _imagePool;
        }

        public void Remove_All_Grids()
        {
            if (grids != null)
            {
                for (var i = 0; i < grids.Length; i++)
                {
                    grids[i].gameObject.SetActive(false);
                    Set_Image_To_Inactive(grids[i]);
                    grids[i].transform.SetSiblingIndex(i);
                }
            }
            grids = null;
        }

        private void Set_Image_To_Inactive(Image image)
        {
            image.gameObject.name = "Image";
            image.gameObject.SetActive(false);
            imagePool.Set_Image_To_Inactive(image);
        }
    }
}

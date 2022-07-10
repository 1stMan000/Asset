using FarrokhGames.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace FarrokhGames.Inventory
{
    public class GridsRenderer : BaseRenderer
    {
        private Image[] grids;
        InventoryManager inventory;
        Sprite cellSpriteEmpty;

        public GridsRenderer(Image[] _grids, Pool<Image> _imagePool, Vector2 _cellSize, InventoryManager _inventory, Sprite _cellSpriteEmpty)
        {
            grids = _grids;
            imagePool = _imagePool;
            cellSize = _cellSize;
            inventory = _inventory;
            cellSpriteEmpty = _cellSpriteEmpty;
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

        public void OnRerenderGrid_Default(Vector2 containerSize, Image grid, out Image[] grids)
        {
            var move_to_origin_symmetry = new Vector3(-containerSize.x / 2, -containerSize.y / 2, 0);
            var move_to_halfCellSize = new Vector3(cellSize.x / 2, cellSize.y / 2, 0);
            var adjust = move_to_origin_symmetry + move_to_halfCellSize;
            grids = new Image[inventory.width * inventory.height];
            var num = 0;
            for (int y = 0; y < inventory.height; y++)
            {
                for (int x = 0; x < inventory.width; x++)
                {
                    grid = CreateImage(cellSpriteEmpty, true);
                    grid.gameObject.name = "Grid " + num;
                    grid.rectTransform.SetAsFirstSibling();
                    grid.type = Image.Type.Sliced;
                    grid.rectTransform.localPosition = new Vector3(cellSize.x * ((inventory.width - 1) - x), cellSize.y * y, 0) + adjust;
                    grid.rectTransform.sizeDelta = cellSize;
                    grids[num] = grid;
                    num++;
                }
            }
        }

        public void OnReRenderGrid_Single(Vector2 containerSize, Image grid, out Image[] grids)
        {
            grid = CreateImage(cellSpriteEmpty, true);
            grid.rectTransform.SetAsFirstSibling();
            grid.type = Image.Type.Sliced;
            grid.rectTransform.localPosition = Vector3.zero;
            grid.rectTransform.sizeDelta = containerSize;
            grids = new[] { grid };
        }
    }
}

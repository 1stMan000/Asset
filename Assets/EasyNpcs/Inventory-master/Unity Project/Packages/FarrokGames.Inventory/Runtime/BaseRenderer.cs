using UnityEngine;
using UnityEngine.UI;
using FarrokhGames.Shared;

namespace FarrokhGames.Inventory
{
    public class BaseRenderer : MonoBehaviour
    {
        public Pool<Image> imagePool;
        public Vector2 cellSize;

        protected Image CreateImage(Sprite sprite, bool raycastTarget)
        {
            var img = imagePool.Activate_ImageObject_In_Pool();
            img.gameObject.SetActive(true);
            img.sprite = sprite;
            img.rectTransform.sizeDelta = new Vector2(img.sprite.rect.width / (40 / cellSize.x), img.sprite.rect.height / (40 / cellSize.y));
            img.transform.SetAsLastSibling();
            img.type = Image.Type.Simple;
            img.raycastTarget = raycastTarget;
            return img;
        }
    }
}
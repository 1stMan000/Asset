using UnityEngine;
using UnityEngine.UI;

namespace FarrokhGames.Inventory
{
    public static class Set_Images 
    {
        public static RectTransform Set_Parent_For_ImageObjects(Transform transform)
        {
            var parent_for_ImageObject = new GameObject("Image Pool").AddComponent<RectTransform>();
            parent_for_ImageObject.transform.SetParent(transform);
            parent_for_ImageObject.transform.localPosition = Vector3.zero;
            parent_for_ImageObject.transform.localScale = Vector3.one;

            return parent_for_ImageObject;
        }

        public static Image ImageObject(RectTransform parent_for_ImageObject)
        {
            var image = new GameObject("Image").AddComponent<Image>();
            image.transform.SetParent(parent_for_ImageObject);
            image.transform.localScale = Vector3.one;

            return image;
        }
    }
}

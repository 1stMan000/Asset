using System.Collections.Generic;
using System;
using System.Linq;

namespace FarrokhGames.Shared
{
    public sealed class Pool<T> where T : class
    {
        private List<T> inactive_imageObjects = new List<T>();
        private List<T> active_imageObjects = new List<T>();
        private Func<T> default_image_object;
        private bool _allowTakingWhenEmpty;

        public Pool(Func<T> imageObject, int initialCount = 0, bool allowTakingWhenEmpty = true)
        {
            if (imageObject == null) throw new ArgumentNullException("pCreator");
            if (initialCount < 0) throw new ArgumentOutOfRangeException("pInitialCount", "Initial count cannot be negative");

            default_image_object = imageObject;
            inactive_imageObjects.Capacity = initialCount;
            _allowTakingWhenEmpty = allowTakingWhenEmpty;

            Create_DefaultImageObject_Much_As_InitialCount(initialCount);
        }

        void Create_DefaultImageObject_Much_As_InitialCount(int initialCount)
        {
            while (inactive_imageObjects.Count < initialCount)
            {
                inactive_imageObjects.Add(default_image_object());
            }
        }

        public int Count => inactive_imageObjects.Count;
        public bool IsEmpty => Count == 0;
        public bool CanTake => _allowTakingWhenEmpty || !IsEmpty;

        public T Activate_ImageObject_In_Pool()
        {
            if (IsEmpty)
            {
                if (_allowTakingWhenEmpty)
                {
                    var obj = default_image_object();
                    inactive_imageObjects.Add(obj);
                    return Activated_ImageObject();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return Activated_ImageObject();
            }
        }

        T Activated_ImageObject()
        {
            T obj = inactive_imageObjects[inactive_imageObjects.Count - 1];
            inactive_imageObjects.RemoveAt(inactive_imageObjects.Count - 1);
            active_imageObjects.Add(obj);

            return obj;
        }

        public void Set_Image_To_Inactive(T item)
        {
            if (!active_imageObjects.Contains(item)) { throw new InvalidOperationException("An item was recycled even though it was not part of the pool"); }
            inactive_imageObjects.Add(item);
            active_imageObjects.Remove(item);
        }

        public List<T> GetInactive()
        {
            return inactive_imageObjects.ToList();
        }

        public List<T> GetActive()
        {
            return active_imageObjects.ToList();
        }
    }
}
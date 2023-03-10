using UnityEngine;

namespace Universe
{
    public static class TransformUtility
    {
        public static T Find<T>(this Transform trans, string name) where T : class
        {
            if (string.IsNullOrEmpty(name))
            {
                return trans as T;
            }

            Transform node = trans.Find(name);
            if (node == null)
            {
                return default;
            }

            return node.GetComponent<T>();
        }

        public static T Find<T>(this GameObject go, string name) where T : class
        {
            if (string.IsNullOrEmpty(name))
            {
                return go as T;
            }

            Transform node = go.transform.Find(name);
            if (node == null)
            {
                return default;
            }

            return node.GetComponent<T>();
        }
        
        public static void StretchHorizontalAndVerticle(this RectTransform transform)
        {
            transform.anchorMax = Vector2.one;
            transform.anchorMin = Vector2.zero;
        }

        public static void StretchTop(this RectTransform transform)
        {
            transform.anchorMax = new(0, 1);
            transform.anchorMin = new(1, 1);
        }

        public static void StretchHorizontal(this RectTransform transform)
        {
            transform.anchorMax = new(1, 0.5f);
            transform.anchorMin = new(0, 0.5f);
        }

        public static void StretchBottom(this RectTransform transform)
        {
            transform.anchorMax = new(0, 0);
            transform.anchorMin = new(1, 0);
        }

        public static void StretchRight(this RectTransform transform)
        {
            transform.anchorMax = new(1, 0);
            transform.anchorMin = new(1, 1);
        }

        public static void StretchVerticle(this RectTransform transform)
        {
            transform.anchorMax = new(0.5f, 1);
            transform.anchorMin = new(0.5f, 0);
        }

        public static void StretchLeft(this RectTransform transform)
        {
            transform.anchorMax = new(0, 1);
            transform.anchorMin = new(0, 0);
        }

        public static void AnchorLeftTop(this RectTransform transform)
        {
            transform.anchorMax = new(0, 1);
            transform.anchorMin = new(0, 1);
        }

        public static void AnchorTop(this RectTransform transform)
        {
            transform.anchorMax = new(0.5f, 1);
            transform.anchorMin = new(0.5f, 1);
        }

        public static void AnchorRightTop(this RectTransform transform)
        {
            transform.anchorMax = new(1, 1);
            transform.anchorMin = new(1, 1);
        }

        public static void AnchorLeft(this RectTransform transform)
        {
            transform.anchorMax = new(0, 0.5f);
            transform.anchorMin = new(0, 0.5f);
        }

        public static void AnchorCenter(this RectTransform transform)
        {
            transform.anchorMax = new(0.5f, 0.5f);
            transform.anchorMin = new(0.5f, 0.5f);
        }

        public static void AnchorRight(this RectTransform transform)
        {
            transform.anchorMax = new(1, 0.5f);
            transform.anchorMin = new(1, 0.5f);
        }

        public static void AnchorLeftBottom(this RectTransform transform)
        {
            transform.anchorMax = new(0, 0);
            transform.anchorMin = new(0, 0);
        }

        public static void AnchorBottom(this RectTransform transform)
        {
            transform.anchorMax = new(0.5f, 0);
            transform.anchorMin = new(0.5f, 0);
        }

        public static void AnchorRightBottom(this RectTransform transform)
        {
            transform.anchorMax = new(1, 0);
            transform.anchorMin = new(1, 0);
        }
    }
}
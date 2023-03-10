using System.Collections.Generic;
using UnityEngine;

namespace Universe
{
    public static class LayerUtility
    {
        static readonly List<Transform> s_TempTransList = new();
        
        public static bool BelongsToLayerMask(int layer, int layerMask)
        {
            return (layerMask & 1 << layer) > 0;
        }
        
        public static void SetLayerRecursively(Transform parent, int layer, int ignoreMask = 0)
        {
            if (parent == null)
            {
                return;
            }

            s_TempTransList.Clear();
            parent.GetComponentsInChildren(true, s_TempTransList);
            for (int i = 0; i < s_TempTransList.Count; i++)
            {
                Transform transform = s_TempTransList[i];
                if (((1 << transform.gameObject.layer) & ignoreMask) == 0)
                {
                    transform.gameObject.layer = layer;
                }
            }
        }
    }
}
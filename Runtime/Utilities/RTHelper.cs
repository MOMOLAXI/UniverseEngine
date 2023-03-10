using UnityEngine;

namespace Universe
{
    public static class RTHelper
    {
        public static void EnsureArray<T>(ref T[] array, int size, T initialValue = default(T))
        {
            if (array != null && array.Length == size)
            {
                return;
            }

            array = new T[size];
            for (int i = 0; i != size; i++)
            {
                array[i] = initialValue;
            }
        }

        public static bool EnsureRenderTarget(ref RenderTexture rt, 
                                              int width,
                                              int height,
                                              RenderTextureFormat format, 
                                              FilterMode filterMode,
                                              string name,
                                              int depthBits = 0,
                                              int antiAliasing = 1)
        {
            if (rt == null)
            {
                rt = RenderTexture.GetTemporary(width, height, depthBits, format, RenderTextureReadWrite.Default, antiAliasing);
                rt.name = name;
                rt.filterMode = filterMode;
                rt.wrapMode = TextureWrapMode.Repeat;
                return true;
            }
            
            bool exist = rt != null;
            bool sizeDontMatch = rt.width != width;
            bool heightDontMatch = rt.height != height;
            bool formatDontMatch = rt.format != format;
            bool modeDontMatch = rt.filterMode != filterMode;
            bool aliasingDontMatch = rt.antiAliasing != antiAliasing;
            
            if (exist && (sizeDontMatch || heightDontMatch || formatDontMatch || modeDontMatch || aliasingDontMatch))
            {
                RenderTexture.ReleaseTemporary(rt);
                rt = null;
                rt = RenderTexture.GetTemporary(width, height, depthBits, format, RenderTextureReadWrite.Default, antiAliasing);
                rt.name = name;
                rt.filterMode = filterMode;
                rt.wrapMode = TextureWrapMode.Repeat;
                
                //new target
                return true; 
            }
            
        #if UNITY_ANDROID || UNITY_IPHONE
            rt.DiscardContents();
        #endif

            //same target
            return false; 
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Universe
{
    public class ObjectCache<T> where T : Component
    {

        Transform m_Parent;
        T m_Template;

        bool m_IsInited;
        bool m_AutoActive = true;
        bool m_AutoUnactive = true;
        bool m_SafeCheck = true;
        int m_MaxCount = 256;

        // 获取时是否自动SetActive(true)
        public bool AutoActive { set => m_AutoActive = value; }
        // 释放时是否自动SetActive(false)
        public bool AutoUnactive { set => m_AutoUnactive = value; }

        // 是否执行安全检测，执行安全检测会损失一定的性能
        // 一旦关闭安全检测，需要使用方保证对象安全
        public bool SafeCheck { set => m_SafeCheck = value; }

        public int Count => CacheList.Count;

        public int MaxCount
        {
            get => m_MaxCount;
            set
            {
                m_MaxCount = value;
                if (m_MaxCount > 0 && m_MaxCount < CacheList.Count)
                {
                    for (int i = m_MaxCount; i < CacheList.Count; ++i)
                    {
                        if (CacheList[i] != null)
                        {
                            Object.Destroy(CacheList[i]);
                        }
                    }

                    CacheList.RemoveRange(m_MaxCount, CacheList.Count - m_MaxCount);
                }
            }
        }

        public List<T> CacheList { get; } = new();

        public void Start(Transform parent, T template)
        {
            if (null == parent || null == template)
            {
                return;
            }

            m_Parent = parent;
            m_Template = template;

            m_IsInited = true;
        }

        public T Get()
        {
            if (!m_IsInited)
            {
                Debug.LogWarning("ObjectCache is not start.");
                return null;
            }

            T obj = null;

            if (CacheList.Count > 0)
            {
                if (m_SafeCheck)
                {
                    for (int i = CacheList.Count - 1; i >= 0; --i)
                    {
                        if (CacheList[i] == null)
                        {
                            CacheList.RemoveAt(i);
                        }
                        else
                        {
                            obj = CacheList[i];
                            CacheList.RemoveAt(i);
                            break;
                        }
                    }
                }
                else
                {
                    int index = CacheList.Count - 1;
                    obj = CacheList[index];
                    CacheList.RemoveAt(index);
                }
            }

            if (obj == null)
            {
                T temp = Object.Instantiate(m_Template);
                obj = temp.TryGetComponent(out T component) ? component : temp.gameObject.AddComponent<T>();

            }

            if (obj != null)
            {
                Transform trans = obj.transform;
                Transform templateTras = m_Template.transform;
                if (m_Parent != null)
                {
                    trans.SetParent(m_Parent, false);
                    trans.localPosition = Vector3.zero;
                    trans.localRotation = templateTras.localRotation;
                    trans.localScale = templateTras.localScale;
                }
                else
                {
                    trans.SetParent(m_Parent, false);
                    trans.localPosition = Vector3.zero;
                    trans.localRotation = templateTras.localRotation;
                    trans.localScale = templateTras.localScale;
                }

                if (m_AutoActive)
                {
                    obj.gameObject.SetActive(true);
                }
            }

            return obj;
        }

        public void Release(T obj)
        {
            if (!m_IsInited)
            {
                Debug.LogWarning("ObjectCache is not start.");
                return;
            }

            if (obj == null)
            {
                return;
            }

            if (m_SafeCheck && CacheList.Contains(obj))
            {
                return;
            }

            if (m_MaxCount > 0 && CacheList.Count >= m_MaxCount)
            {
                Object.Destroy(obj);
                return;
            }

            obj.transform.SetParent(m_Parent, false);

            if (m_AutoUnactive)
            {
                obj.gameObject.SetActive(false);
            }
            else
            {
                obj.transform.localScale = Vector3.zero;
            }

            CacheList.Add(obj);
        }

        public void DestroyAll()
        {
            if (!m_IsInited)
            {
                Log.Warning("ObjectCache is not start. no thing to destroy");
                return;
            }

            for (int i = CacheList.Count - 1; i >= 0; --i)
            {
                T obj = CacheList[i];
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }

            CacheList.Clear();
        }

        public void Destroy(int capacity)
        {
            if (capacity <= 0)
            {
                DestroyAll();
                return;
            }

            if (capacity < CacheList.Count)
            {
                int removeCount = CacheList.Count - capacity;
                for (int i = 0; i < removeCount; ++i)
                {
                    if (CacheList[i] != null)
                    {
                        Object.Destroy(CacheList[i]);
                    }
                }

                CacheList.RemoveRange(0, removeCount);
            }
        }
    }
}
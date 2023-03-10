using System;
using System.Collections.Generic;
using UnityEngine;

namespace Universe
{
    /// <summary>
    /// 属性变化回调
    /// </summary>
    /// <param name="self"></param>
    /// <param name="property"></param>
    /// <param name="last"></param>
    /// <param name="cur"></param>
    public delegate void PropChangeFunction(EntityID self, string property, Var last, Var cur);

    [Serializable]
    public class EntityProperty : ICacheAble
    {
        [SerializeField]
        Var m_Value;

        Dictionary<string, PropChangeFunction> m_PropListener;
        Dictionary<string, PropChangeFunction> PropListener => m_PropListener ??= new();
        public string Name { get; set; }

        public static EntityProperty Empty = new();

        public Var Value
        {
            get => m_Value;
            set
            {
                Var last = m_Value;
                m_Value = value;
                foreach (PropChangeFunction function in PropListener.Values)
                {
                    function?.Invoke(Owner, Name, last, value);
                }
            }
        }

        public EntityID Owner { get; set; }

        public void AddPropHook(string callbackName, PropChangeFunction function)
        {
            if (string.IsNullOrEmpty(callbackName) || function == null)
            {
                return;
            }

            PropListener.Add(callbackName, function);
        }

        public void RemovePropHook(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                return;
            }

            m_PropListener?.Remove(callbackName);
        }

        public void Reset()
        {
            Name = string.Empty;
            Value = Var.None;
            Owner = EntityID.None;
            m_PropListener?.Clear();
        }

        public bool IsInCache { get; set; }
    }
}
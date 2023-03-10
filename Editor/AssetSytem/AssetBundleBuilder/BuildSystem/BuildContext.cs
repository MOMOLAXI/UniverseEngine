using System;
using System.Collections.Generic;

namespace Universe
{
    public class BuildContext
    {
        readonly Dictionary<Type, IContextObject> m_ContextObjects = new();

        /// <summary>
        /// 清空所有情景对象
        /// </summary>
        public void ClearAllContext()
        {
            m_ContextObjects.Clear();
        }

        /// <summary>
        /// 设置情景对象
        /// </summary>
        public void SetContextObject(IContextObject contextObject)
        {
            if (contextObject == null)
            {
                throw new ArgumentNullException(nameof(contextObject));
            }

            Type type = contextObject.GetType();
            if (m_ContextObjects.ContainsKey(type))
            {
                throw new($"Context object {type} is already existed.");
            }

            m_ContextObjects.Add(type, contextObject);
        }

        /// <summary>
        /// 获取情景对象
        /// </summary>
        public T GetContextObject<T>() where T : IContextObject
        {
            Type type = typeof(T);
            if (m_ContextObjects.TryGetValue(type, out IContextObject contextObject))
            {
                return (T)contextObject;
            }
            
            throw new($"Not found context object : {type}");
        }
    }
}
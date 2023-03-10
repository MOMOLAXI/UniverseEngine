using System.Collections.Generic;

namespace Universe
{
    /// <summary>
    /// 类事件中心
    /// </summary>
    internal class EntityClassEventLibrary
    {
        readonly ObjectPool<EntityClassEventContext> m_Cache = ObjectPool<EntityClassEventContext>.Create();
        readonly Dictionary<string, EntityClassEventContext> m_ClassContexts = new();

        public EntityClassEventContext Get(string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                Log.Error("class name is null or empty while find class event context");
                return null;
            }

            if (m_ClassContexts.TryGetValue(className, out EntityClassEventContext context))
            {
                return context;
            }

            context = Alloc(className);
            return context;
        }

        public EntityClassEventContext Alloc(string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                Log.Error("class name is null or empty while alloc class event context");
                return null;
            }

            if (m_ClassContexts.TryGetValue(className, out EntityClassEventContext context))
            {
                return context;
            }

            context = m_Cache.Get();
            m_ClassContexts.Add(className, context);
            return context;
        }

        public void Release(EntityClassEventContext context)
        {
            if (context == null)
            {
                Log.Error("context is destroyed somewhere, it shouldn't happen , plz check this out");
                return;
            }

            context.Reset();
            m_Cache.Release(context);
        }
    }
}
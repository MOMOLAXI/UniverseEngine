using System.Collections.Generic;

namespace Universe
{
    internal class EntityPropertyLibrary
    {
        readonly Queue<EntityPropertyCollection> m_Cache = new();

        /// <summary>
        /// 申请对象的属性
        /// </summary>
        /// <returns></returns>
        public EntityPropertyCollection Alloc()
        {
            if (m_Cache.Count <= 0)
            {
                return new();
            }

            EntityPropertyCollection collection = m_Cache.Dequeue();
            collection.Reset();
            return collection;
        }

        /// <summary>
        /// 对象删除时回收
        /// </summary>
        /// <param name="collection"></param>
        public void Release(EntityPropertyCollection collection)
        {
            if (collection == null)
            {
                Log.Error("PropertyCollection was destroyed somewhere, it shouldn't happen , plz check this out");
                return;
            }

            collection.Reset();
            m_Cache.Enqueue(collection);
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Universe
{
    /// <summary>
    /// 非ECS架构Entity系统
    /// 数据容器型Entity系统(支持一维离散数据(K,V), 二维表格型属性)
    /// 方便制定通用网络协议 (eg. 对象ID + 字段名 + 字段值...)
    /// 可Dump出一份Xml文件，根据对象继承关系决定Xml标签缩进，方便可视化地表达对象树
    /// </summary>
    internal class EntitySystem : EngineSystem
    {
        const string ENTITY_NAME_FORMAT = "{0}{1}";

        /// <summary>
        /// 根节点
        /// </summary>
        static Entity Root { get; set; }

        /// <summary>
        /// 实例
        /// </summary>
        readonly List<Entity> m_Instance = new();

        /// <summary>
        /// Entity 查询
        /// </summary>
        readonly Dictionary<EntityID, int> m_EntityIDMapping = new();
        
        public EntityPropertyLibrary PropertyLibrary { get; } = new();

        public EntityClassEventLibrary EventLibrary { get; } = new();

        public override void Init()
        {
            Root = CreateRoot();
        }

        /// <summary>
        /// 查找Entity
        /// </summary>
        /// <param name="entityID"></param>
        /// <returns></returns>
        public Entity Find(EntityID entityID)
        {
            if (entityID == EntityID.None)
            {
                return null;
            }

            if (!m_EntityIDMapping.TryGetValue(entityID, out int index))
            {
                return null;
            }

            Entity entity = m_Instance[index];
            return entity;
        }

        /// <summary>
        /// 创建Entity
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="identity">类型标识</param>
        /// <param name="parent">父类</param>
        /// <param name="isStatic">是否是静态对象</param>
        /// <returns></returns>
        public Entity CreateEntity(string className, uint identity, Entity parent = null, bool isStatic = false)
        {
            if (string.IsNullOrEmpty(className))
            {
                return null;
            }

            if (parent == null)
            {
                parent = Root;
            }


            EntityID id = EntityIDGenerator.Get(identity);
            GameObject go = new()
            {
                name = ENTITY_NAME_FORMAT.SafeFormat(id.ToString(), className)
            };

            if (isStatic)
            {
                go.transform.SetParent(Root.transform);
            }

            if (!go.TryGetComponent(out Entity entity))
            {
                entity = go.AddComponent<Entity>();
            }

            entity.ID = id;
            entity.ClassName = className;
            entity.OnDestroyEntity = OnDestroyEntity;
            entity.InitializeProperty(PropertyLibrary.Alloc());
            entity.SetEventContext(EventLibrary.Alloc(className));

            m_Instance.Add(entity);
            m_EntityIDMapping[entity.ID] = m_Instance.Count - 1;

            entity.SendClassEvent(EntityEvent.OnCreate, id, Variables.AllocNonHold());

            if (parent != null)
            {
                parent.AddChild(entity);
            }

            return entity;
        }

        public override void Update(float deltaTime)
        {
            int count = m_Instance.Count;
            for (int i = 0; i < count; ++i)
            {
                Entity entity = m_Instance[i];
                if (entity == null)
                {
                    continue;
                }

                try
                {
                    entity.OnUpdate();
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
        }

        public override void LateUpdate(float dt)
        {
            int count = m_Instance.Count;

            for (int i = 0; i < count; ++i)
            {
                Entity entity = m_Instance[i];
                if (entity == null)
                {
                    continue;
                }

                try
                {
                    entity.OnLateUpdate();
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
        }

        public override void FixedUpdate(float dt)
        {
            int count = m_Instance.Count;

            for (int i = 0; i < count; ++i)
            {
                Entity entity = m_Instance[i];
                if (entity == null)
                {
                    continue;
                }

                try
                {
                    entity.OnFixedUpdate();
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
        }

        /// <summary>
        /// 移除对象
        /// </summary>
        /// <param name="entity"></param>
        void OnDestroyEntity(Entity entity)
        {
            if (m_EntityIDMapping.TryGetValue(entity.ID, out int index))
            {
                m_Instance.RemoveAt(index);
                m_EntityIDMapping.Remove(entity.ID);
            }

            //释放类事件上下文
            EventLibrary.Release(entity.EventContext);

            //释放属性列表
            PropertyLibrary.Release(entity.Properties);
        }

        /// <summary>
        /// 销毁对象
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="immediate"></param>
        public void DestroyEntity(EntityID entityID, bool immediate)
        {
            Entity entity = Find(entityID);
            if (entity == null)
            {
                return;
            }

            DestroyEntity(entity, immediate);
        }

        /// <summary>
        /// 销毁对象
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="immediate"></param>
        public void DestroyEntity(Entity entity, bool immediate)
        {
            if (entity == null)
            {
                return;
            }

            entity.OnDestroySelf();

            if (immediate)
            {
                Object.DestroyImmediate(entity.gameObject);
            }
            else
            {
                Object.Destroy(entity.gameObject);
            }
        }

        Entity CreateRoot()
        {
            Engine.GetGlobalGameObject(nameof(EntitySystem), out GameObject go);
            Entity root = go.AddComponent<Entity>();
            root.ID = new(uint.MaxValue, uint.MaxValue);
            root.ClassName = nameof(EntitySystem);
            root.SetEventContext(EventLibrary.Alloc(nameof(EntitySystem)));
            m_Instance.Add(root);
            m_EntityIDMapping[root.ID] = m_Instance.Count - 1;
            return root;
        }
    }
}
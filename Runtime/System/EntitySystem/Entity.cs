using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Universe
{
    public class Entity : MonoBehaviour
    {
        bool m_IsDestroying;
        bool m_NeedUpdateComponent;
        readonly List<EntityComponent> m_Components = new();
        static readonly PolyObjectPool<EntityComponent> s_ComponentPool = new();

        /// <summary>
        /// 对象ID
        /// </summary>
        public EntityID ID;

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName;

        /// <summary>
        /// 父类
        /// </summary>
        public Entity Parent;

        /// <summary>
        /// 销毁时通知
        /// </summary>
        public Run<Entity> OnDestroyEntity;

        /// <summary>
        /// 子类
        /// </summary>
        public List<Entity> Children = new();

        /// <summary>
        /// 类事件触发器 
        /// </summary>
        public EntityClassEventContext EventContext;

        /// <summary>
        /// 一维离散属性
        /// </summary>
        public EntityPropertyCollection Properties = EntityPropertyCollection.Empty;

        //TODO 二维Table属性

        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public Vector3 LocalPosition
        {
            get => transform.localPosition;
            set => transform.localPosition = value;
        }

        public Vector3 EulerAngles
        {
            get => transform.eulerAngles;
            set => transform.eulerAngles = value;
        }

        public Vector3 LocalEulerAngles
        {
            get => transform.localEulerAngles;
            set => transform.localEulerAngles = value;
        }

        public Vector3 Right
        {
            get => transform.right;
            set => transform.right = value;
        }

        public Vector3 Up
        {
            get => transform.up;
            set => transform.up = value;
        }

        public Vector3 Forward
        {
            get => transform.forward;
            set => transform.forward = value;
        }

        public Quaternion Rotation
        {
            get => transform.rotation;
            set => transform.rotation = value;
        }

        public Quaternion LocalRotation
        {
            get => transform.localRotation;
            set => transform.localRotation = value;
        }

        public Vector3 LocalScale
        {
            get => transform.localScale;
            set => transform.localScale = value;
        }

        public Matrix4x4 WorldToLocalMatrix => transform.worldToLocalMatrix;

        public Matrix4x4 LocalToWorldMatrix => transform.localToWorldMatrix;

        public void Translate(Vector3 translation, Space relativeTo)
        {
            transform.Translate(translation, relativeTo);
        }

        public void Translate(Vector3 translation)
        {
            transform.Translate(translation);
        }

        public void Translate(float x, float y, float z, Space relativeTo)
        {
            transform.Translate(x, y, z, relativeTo);
        }

        public void Translate(float x, float y, float z)
        {
            transform.Translate(x, y, z);
        }

        public void Translate(Vector3 translation, Transform relativeTo)
        {
            transform.Translate(translation, relativeTo);
        }

        public void Translate(float x, float y, float z, Transform relativeTo)
        {
            transform.Translate(x, y, z, relativeTo);
        }

        public void Rotate(Vector3 eulers, Space relativeTo)
        {
            transform.Rotate(eulers, relativeTo);
        }

        public void Rotate(Vector3 eulers)
        {
            transform.Rotate(eulers);
        }

        public void Rotate(float xAngle, float yAngle, float zAngle, Space relativeTo)
        {
            transform.Rotate(xAngle, yAngle, zAngle, relativeTo);
        }

        public void Rotate(float xAngle, float yAngle, float zAngle)
        {
            transform.Rotate(xAngle, yAngle, zAngle);
        }

        public void Rotate(Vector3 axis, float angle, Space relativeTo)
        {
            transform.Rotate(axis, angle, relativeTo);
        }

        public void Rotate(Vector3 axis, float angle)
        {
            transform.Rotate(axis, angle);
        }

        public void RotateAround(Vector3 point, Vector3 axis, float angle)
        {
            transform.RotateAround(point, axis, angle);
        }

        public void LookAt(Transform target, Vector3 worldUp)
        {
            transform.LookAt(target, worldUp);
        }

        public void LookAt(Transform target)
        {
            transform.LookAt(target);
        }

        public void LookAt(Vector3 worldPosition, Vector3 worldUp)
        {
            transform.LookAt(worldPosition, worldUp);
        }

        public void LookAt(Vector3 worldPosition)
        {
            transform.LookAt(worldPosition);
        }

        public Vector3 TransformDirection(Vector3 direction)
        {
            return transform.TransformDirection(direction);
        }

        public Vector3 TransformDirection(float x, float y, float z)
        {
            return transform.TransformDirection(x, y, z);
        }

        public Vector3 InverseTransformDirection(Vector3 direction)
        {
            return transform.InverseTransformDirection(direction);
        }

        public Vector3 InverseTransformDirection(float x, float y, float z)
        {
            return transform.InverseTransformDirection(x, y, z);
        }

        public Vector3 TransformVector(Vector3 vector)
        {
            return transform.TransformVector(vector);
        }

        public Vector3 TransformVector(float x, float y, float z)
        {
            return transform.TransformVector(x, y, z);
        }

        public Vector3 InverseTransformVector(Vector3 vector)
        {
            return transform.InverseTransformVector(vector);
        }

        public Vector3 InverseTransformVector(float x, float y, float z)
        {
            return transform.InverseTransformVector(x, y, z);
        }

        public Vector3 TransformPoint(Vector3 position)
        {
            return transform.TransformPoint(position);
        }

        public Vector3 TransformPoint(float x, float y, float z)
        {
            return transform.TransformPoint(x, y, z);
        }

        public Vector3 InverseTransformPoint(Vector3 position)
        {
            return transform.InverseTransformPoint(position);
        }

        public Vector3 InverseTransformPoint(float x, float y, float z)
        {
            return transform.InverseTransformPoint(x, y, z);
        }

        /// <summary>
        /// 订阅属性变化
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="callbackName"></param>
        /// <param name="function"></param>
        public void AddPropHook(string propName, string callbackName, PropChangeFunction function)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::SubscribePropChange]property name not exist : {propName}");
                return;
            }

            property.AddPropHook(callbackName, function);
        }

        /// <summary>
        /// 移除属性变化监听
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="callbackName"></param>
        public void RemovePropHook(string propName, string callbackName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                return;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::SubscribePropChange]property name not exist : {propName}");
                return;
            }

            property.RemovePropHook(callbackName);
        }

        public bool GetPropList(Variables variables)
        {
            if (variables == null)
            {
                return false;
            }

            variables.Clear();

            foreach (EntityProperty property in Properties.Properties.Values)
            {
                variables.AddObject(property);
            }

            return true;
        }

        public bool GetPropList(List<EntityProperty> variableList)
        {
            if (variableList == null)
            {
                return false;
            }

            variableList.Clear();
            variableList.AddRange(Properties.Properties.Values);
            return true;
        }

        public void SetBool(string propName, bool value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::SetBool]property name not exist : {propName}");
                return;
            }

            property.Value = new(value);
        }

        public void SetInt(string propName, int value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::SetInt]property name not exist : {propName}");
                return;
            }

            property.Value = new(value);
        }

        public void SetInt64(string propName, long value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::SetInt64]property name not exist : {propName}");
                return;
            }

            property.Value = new(value);
        }

        public void SetFloat(string propName, float value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::SetFloat]property name not exist : {propName}");
                return;
            }

            property.Value = new(value);
        }

        public void SetDouble(string propName, double value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::SetDouble]property name not exist : {propName}");
                return;
            }

            property.Value = new(value);
        }

        public void SetString(string propName, string value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::SetString]property name not exist : {propName}");
                return;
            }

            property.Value = new(value);
        }

        public void SetEntityID(string propName, EntityID value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::SetEntityID]property name not exist : {propName}");
                return;
            }

            property.Value = new(value);
        }

        public void SetObject(string propName, object value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::SetObject]property name not exist : {propName}");
                return;
            }

            property.Value = new(value);
        }

        public void SetClass<T>(string propName, T value) where T : class
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::SetClass<T>]property name not exist : {propName}");
                return;
            }

            property.Value = new(value);
        }

        public void SetBinary(string propName, byte[] value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::SetBinary]property name not exist : {propName}");
                return;
            }

            property.Value = new(value);
        }

        public void SetProp(string propName, Var value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::SetBinary]property name not exist : {propName}");
                return;
            }

            property.Value = value;
        }

        public bool GetBool(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::GetBool]property name not exist : {propName}");
                return default;
            }

            return property.Value.GetBool();
        }

        public int GetInt(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::GetInt]property name not exist : {propName}");
                return default;
            }

            return property.Value.GetInt();
        }

        public long GetInt64(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::GetInt64]property name not exist : {propName}");
                return default;
            }

            return property.Value.GetInt64();
        }

        public float GetFloat(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::GetFloat]property name not exist : {propName}");
                return default;
            }

            return property.Value.GetFloat();
        }

        public double GetDouble(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::GetDouble]property name not exist : {propName}");
                return default;
            }

            return property.Value.GetDouble();
        }

        public string GetString(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::GetString]property name not exist : {propName}");
                return default;
            }

            return property.Value.GetString();
        }

        public EntityID GetEntityID(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return EntityID.None;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::GetEntityID]property name not exist : {propName}");
                return EntityID.None;
            }

            return property.Value.GetEntity();
        }

        public byte[] GetBinary(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::GetBinary]property name not exist : {propName}");
                return default;
            }

            return property.Value.GetBinary();
        }

        public object GetObject(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::GetObject]property name not exist : {propName}");
                return default;
            }

            return property.Value.GetObject();
        }

        public Var GetProp(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::GetObject]property name not exist : {propName}");
                return default;
            }

            return property.Value;
        }

        public bool HasProp(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            return Properties.Contains(propName);
        }

        public VarType GetVariableType(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            return Properties.GetVarType(propName);
        }

        public T GetClass<T>(string propName) where T : class
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            EntityProperty property = Properties.Get(propName);
            if (property == null)
            {
                Log.Error($"[Entity::GetClass]property name not exist : {propName}");
                return default;
            }

            return property.Value.GetObject() as T;
        }

        /// <summary>
        /// 发送类消息
        /// </summary>
        /// <param name="logicEvent"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        internal void SendClassEvent(EntityEvent logicEvent, EntityID sender, Variables args)
        {
            if (EventContext == null)
            {
                Log.Error($"{ClassName} parent's event context is missing");
                return;
            }

            EventContext.ExecuteEvent(logicEvent, ID, sender, args);
        }

        /// <summary>
        /// 初始化属性集合
        /// </summary>
        /// <param name="collection"></param>
        internal void InitializeProperty(EntityPropertyCollection collection)
        {
            if (collection == null)
            {
                Log.Error($"{ClassName} parent's property context is missing");
                return;
            }

            Properties = collection;
            foreach (KeyValuePair<string, EntityProperty> property in Properties.Properties)
            {
                property.Value.Owner = ID;
            }
        }

        /// <summary>
        /// 设置类消息上下文
        /// </summary>
        /// <param name="eventContext"></param>
        internal void SetEventContext(EntityClassEventContext eventContext)
        {
            if (eventContext == null)
            {
                Log.Error($"{ClassName} parent's event context is missing");
                return;
            }

            EventContext = eventContext;
        }

        /// <summary>
        /// 继承父类事件
        /// </summary>
        /// <param name="parentContext"></param>
        internal void Inherit(EntityClassEventContext parentContext)
        {
            if (parentContext == null)
            {
                Log.Error($"{ClassName} inherit error, parent context is null, check parent alloc");
                return;
            }

            EventContext?.Inherit(parentContext);
        }

        /// <summary>
        /// 添加子对象
        /// </summary>
        /// <param name="entitys"></param>
        internal void AddChildren(List<Entity> entitys)
        {
            if (entitys == null)
            {
                Log.Error($"Entity list is null while add children to {ClassName}");
                return;
            }

            foreach (Entity entity in entitys)
            {
                entity.Parent = this;
                entity.SendClassEvent(EntityEvent.OnEntryOther, ID, Variables.Empty);
            }

            Children.AddRange(entitys);
            SendClassEvent(EntityEvent.OnAddChildren, ID, Variables.Empty);
            SendClassEvent(EntityEvent.OnCapacityChange, ID, Variables.Empty);
        }

        /// <summary>
        /// 添加子对象
        /// </summary>
        /// <param name="entity"></param>
        internal void AddChild(Entity entity)
        {
            if (entity == null)
            {
                Log.Error($"Entity is null while add child to {ClassName}");
                return;
            }

            entity.Parent = this;
            entity.Inherit(EventContext);
            Children.Add(entity);
            SendClassEvent(EntityEvent.OnAddChild, entity.ID, Variables.Empty);
            SendClassEvent(EntityEvent.OnCapacityChange, entity.ID, Variables.Empty);
            entity.SendClassEvent(EntityEvent.OnEntryOther, ID, Variables.Empty);
        }

        /// <summary>
        /// 移除子对象
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="destroy"></param>
        internal void RemoveChild(Entity entity, bool destroy = false)
        {
            if (entity == null)
            {
                return;
            }

            Children.Remove(entity);

            SendClassEvent(EntityEvent.OnRemoveChild, entity.ID, Variables.Empty);
            SendClassEvent(EntityEvent.OnCapacityChange, entity.ID, Variables.Empty);
            entity.SendClassEvent(EntityEvent.OnLeaveOther, ID, Variables.Empty);

            if (destroy)
            {
                OnDestroyEntity?.Invoke(entity);
            }
        }

        /// <summary>
        /// 通过索引获取子对象
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public EntityID GetChildAtIndex(int index)
        {
            return Utilities.IsValidIndex(index, Children.Count) ? Children[index].ID : EntityID.None;
        }

        /// <summary>
        /// 获取所有子对象
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public int GetChildren(Variables result)
        {
            result.Clear();

            for (int i = 0; i < Children.Count; ++i)
            {
                Entity child = Children[i];
                result.AddEntityID(child.ID);
            }

            return result.Count;
        }

        /// <summary>
        /// 获取所有子对象
        /// </summary>
        /// <param name="children"></param>
        public void GetChildren(List<EntityID> children)
        {
            children?.Clear();
            children?.AddRange(Children.Select(child => child.ID));
        }

        public void ClearChildren(bool destroy)
        {
            if (destroy)
            {
                foreach (Entity child in Children)
                {
                    OnDestroyEntity?.Invoke(child);
                }
            }

            Children.Clear();
            SendClassEvent(EntityEvent.OnCapacityChange, ID, Variables.Empty);
        }

        public void OnDestroySelf()
        {
            if (m_IsDestroying)
            {
                Log.Error("[Entity:OnDestroySelf]object is in destroying.");
                return;
            }

            m_IsDestroying = true;
            // 清理组件
            RemoveAllComponent();
            ClearChildren(true);
            OnDestroyEntity?.Invoke(this);
            SendClassEvent(EntityEvent.OnDestroy, ID, Variables.Empty);
        }

        internal void OnUpdate()
        {
            if (m_NeedUpdateComponent)
            {
                UpdateComponent();
            }
        }

        internal void OnLateUpdate()
        {
            if (m_IsDestroying)
            {
                return;
            }

            if (m_NeedUpdateComponent)
            {
                LateUpdateComponent();
            }
        }

        internal void OnFixedUpdate()
        {
            if (m_IsDestroying)
            {
                return;
            }

            if (m_NeedUpdateComponent)
            {
                FixedUpdateComponent();
            }
        }

        public T AddEntityComponent<T>(bool findExisted = true) where T : EntityComponent, new()
        {
            T component = null;

            if (findExisted)
            {
                component = GetEntityComponent<T>();

                if (component != null)
                {
                    component.IsActive = true;
                    return component;
                }
            }

            component = s_ComponentPool.Get<T>();
            component.Init(ID);
            component.IsActive = true;

            m_Components.Add(component);

            try
            {
                component.Awake();
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            m_NeedUpdateComponent = true;

            return component;
        }

        public EntityComponent AddEntityComponent(Type type, bool findExisted = true)
        {
            EntityComponent entityComponent = null;

            if (findExisted)
            {
                entityComponent = GetEntityComponent(type);

                if (entityComponent != null)
                {
                    return entityComponent;
                }
            }

            entityComponent = s_ComponentPool.Get(type);
            entityComponent.Init(ID);

            m_Components.Add(entityComponent);

            try
            {
                entityComponent.Awake();
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            m_NeedUpdateComponent = true;

            return entityComponent;
        }

        public T AddComponent<T>() where T : Component
        {
            return gameObject.AddComponent<T>();
        }

        public T GetEntityComponent<T>() where T : EntityComponent
        {
            for (int i = 0; i < m_Components.Count; ++i)
            {
                if (m_Components[i] != null &&
                    m_Components[i].IsDestroyed == false &&
                    m_Components[i] is T)
                {
                    return m_Components[i] as T;
                }
            }

            return null;
        }

        public EntityComponent GetEntityComponent(Type type)
        {
            for (int i = 0; i < m_Components.Count; ++i)
            {
                if (m_Components[i] != null &&
                    m_Components[i].IsDestroyed == false &&
                    m_Components[i].GetType() == type)
                {
                    return m_Components[i];
                }
            }

            return null;
        }

        public bool RemoveEntityComponent<T>() where T : EntityComponent
        {
            for (int i = 0; i < m_Components.Count; ++i)
            {
                EntityComponent component = m_Components[i];
                if (!(component is T))
                {
                    continue;
                }

                component.DestroySelf();
                return true;
            }

            return false;
        }

        public bool RemoveEntityComponent(Type type)
        {
            for (int i = 0; i < m_Components.Count; ++i)
            {
                EntityComponent component = m_Components[i];
                if (component == null || component.GetType() != type)
                {
                    continue;
                }

                component.DestroySelf();
                return true;
            }

            return false;
        }

        public bool RemoveEntityComponent(EntityComponent entityComponent)
        {
            for (int i = 0; i < m_Components.Count; ++i)
            {
                if (m_Components[i] != entityComponent)
                {
                    continue;
                }

                entityComponent.DestroySelf();
                return true;
            }

            return false;
        }

        void RemoveAllComponent()
        {
            for (int i = 0; i < m_Components.Count; ++i)
            {
                if (m_Components[i] != null)
                {
                    m_Components[i].DestroySelf();
                }
            }
        }

        void UpdateComponent()
        {
            bool hasDestroyed = false;
            int count = m_Components.Count;

            for (int i = 0; i < count; ++i)
            {
                EntityComponent entityComponent = m_Components[i];

                if (entityComponent.IsDestroyed)
                {
                    hasDestroyed = true;
                    continue;
                }

                if (!entityComponent.IsActive)
                {
                    continue;
                }

                if (!entityComponent.IsStarted)
                {
                    entityComponent.IsStarted = true;

                    try
                    {
                        entityComponent.Start();
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex);
                    }

                    if (entityComponent.IsDestroyed || !entityComponent.IsActive)
                    {
                        continue;
                    }
                }

                try
                {
                    entityComponent.Update();
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }

            if (!hasDestroyed)
            {
                return;
            }

            for (int i = m_Components.Count - 1; i >= 0; --i)
            {
                EntityComponent component = m_Components[i];
                if (!component.IsDestroyed)
                {
                    continue;
                }

                s_ComponentPool.Release(component);
                m_Components.RemoveAt(i);
            }
        }

        void LateUpdateComponent()
        {
            int count = m_Components.Count;
            for (int i = 0; i < count; ++i)
            {
                EntityComponent entityComponent = m_Components[i];

                if (entityComponent.IsDestroyed || !entityComponent.IsActive)
                {
                    continue;
                }

                try
                {
                    entityComponent.LateUpdate();
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
        }

        void FixedUpdateComponent()
        {
            for (int i = 0; i < m_Components.Count; ++i)
            {
                EntityComponent entityComponent = m_Components[i];

                if (entityComponent.IsDestroyed || !entityComponent.IsActive)
                {
                    continue;
                }

                try
                {
                    entityComponent.FixedUpdate();
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
        }
    }
}
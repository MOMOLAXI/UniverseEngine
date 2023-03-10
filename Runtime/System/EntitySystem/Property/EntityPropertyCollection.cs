using System.Collections.Generic;

namespace Universe
{
    public class EntityPropertyCollection
    {
        static readonly ObjectPool<EntityProperty> s_Pool = ObjectPool<EntityProperty>.Create();

        public readonly Dictionary<string, EntityProperty> Properties = new();

        public static EntityPropertyCollection Empty = new();

        /// <summary>
        /// 是否存在属性
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public bool Contains(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name is null or empty while add property");
                return false;
            }

            return Properties.ContainsKey(propName);
        }

        /// <summary>
        /// 添加属性
        /// </summary>
        /// <param name="propName"></param>
        public EntityProperty Add(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name is null or empty while add property");
                return null;
            }

            EntityProperty property = s_Pool.Get();
            property.Name = propName;
            Properties.Add(property.Name, property);
            return property;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public EntityProperty Get(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                return null;
            }

            if (Properties.TryGetValue(propName, out EntityProperty property))
            {
                return property;
            }

            property = Add(propName);
            return property;
        }

        public VarType GetVarType(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                return VarType.None;
            }

            if (Properties.TryGetValue(propName, out EntityProperty property))
            {
                return property.Value.VariableType;
            }

            return VarType.None;
        }

        public void Reset()
        {
            foreach (KeyValuePair<string, EntityProperty> property in Properties)
            {
                EntityProperty prop = property.Value;
                prop.Reset();
                s_Pool.Release(prop);
            }

            Properties.Clear();
        }
    }
}
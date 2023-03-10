using System.Collections.Generic;
using System.Text;

namespace Universe
{
    public class Variables
    {
        static readonly StringBuilder s_YouFuckingSb = new();
        
        readonly List<Var> m_Variables = new();

        internal Variables() { }

        /// <summary>
        /// 默认空
        /// </summary>
        public static Variables Empty { get; } = new();

        public static Variables AllocNonHold()
        {
            return AutoVariableSystem.AllocNonHold();
        }
        
        /// <summary>
        /// 数量
        /// </summary>
        public int Count => m_Variables.Count;

        public Variables Clone()
        {
            Variables result = new();
            result.AddVariableList(this);
            return result;
        }

        public void Clear()
        {
            m_Variables.Clear();
        }

        public bool Insert(int index, Var value)
        {
            if (index < 0 || index > m_Variables.Count)
            {
                return false;
            }

            m_Variables.Insert(index, value);
            return true;
        }

        public void RemoveAt(int index)
        {
            m_Variables.RemoveAt(index);
        }

        public void AddVariable(Var value)
        {
            m_Variables.Add(value);
        }

        public void AddBool(bool value)
        {
            m_Variables.Add(new(value));
        }

        public void AddInt(int value)
        {
            m_Variables.Add(new(value));
        }

        public void AddInt64(long value)
        {
            m_Variables.Add(new(value));
        }

        public void AddFloat(float value)
        {
            m_Variables.Add(new(value));
        }

        public void AddDouble(double value)
        {
            m_Variables.Add(new(value));
        }

        public void AddString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Log.Error("[VarList:AddBinary] string is null.");
            }

            m_Variables.Add(new(value));
        }

        public void AddEntityID(EntityID value)
        {
            m_Variables.Add(new(value));
        }

        public void AddObject(object value)
        {
            m_Variables.Add(new(value));
        }

        public void AddBinary(byte[] value)
        {
            if (value == null)
            {
                Log.Error("[VarList:AddBinary] binary is null.");
            }

            m_Variables.Add(new(value));
        }

        public void AddVariableList(Variables varList)
        {
            if (varList == null)
            {
                return;
            }

            if (this == varList)
            {
                Log.Error("[VarList:AddVarList] VarList want to add Owner, please check it.");
            }

            int count = varList.Count;
            for (int i = 0; i < count; ++i)
            {
                m_Variables.Add(varList.GetVar(i));
            }
        }

        public VarType GetVarType(int index)
        {
            if (index < 0 || index >= m_Variables.Count)
            {
                return VarType.None;
            }

            return m_Variables[index].VariableType;
        }

        public Var GetVar(int index)
        {
            if (index < 0 || index >= m_Variables.Count)
            {
                return Var.None;
            }

            return m_Variables[index];
        }

        public bool GetBool(int index)
        {
            if (index < 0 || index >= m_Variables.Count)
            {
                return false;
            }

            return m_Variables[index].GetBool();
        }

        public int GetInt32(int index)
        {
            if (index < 0 || index >= m_Variables.Count)
            {
                return 0;
            }

            return m_Variables[index].GetInt();
        }

        public long GetInt64(int index)
        {
            if (index < 0 || index >= m_Variables.Count)
            {
                return 0;
            }

            return m_Variables[index].GetInt64();
        }

        public float GetFloat(int index)
        {
            if (index < 0 || index >= m_Variables.Count)
            {
                return 0;
            }

            return m_Variables[index].GetFloat();
        }

        public double GetDouble(int index)
        {
            if (index < 0 || index >= m_Variables.Count)
            {
                return 0;
            }

            return m_Variables[index].GetDouble();
        }

        public string GetString(int index)
        {
            if (index < 0 || index >= m_Variables.Count)
            {
                return string.Empty;
            }

            return m_Variables[index].GetString();
        }

        public EntityID GetEntity(int index)
        {
            if (index < 0 || index >= m_Variables.Count)
            {
                return new();
            }

            return m_Variables[index].GetEntity();
        }

        public T GetObject<T>(int index) where T : class
        {
            if (index < 0 || index >= m_Variables.Count)
            {
                return null;
            }

            Var obj = m_Variables[index];
            return obj.GetObject() as T;
        }

        public byte[] GetBinary(int index)
        {
            if (index < 0 || index >= m_Variables.Count)
            {
                return null;
            }

            return m_Variables[index].GetBinary();
        }

        public static Variables operator <(Variables variables, object value)
        {
            variables.AddObject(value);
            return variables;
        }

        public static Variables operator <(Variables variables, Var value)
        {
            variables.AddVariable(value);
            return variables;
        }

        public static Variables operator <(Variables variables, bool value)
        {
            variables.AddBool(value);
            return variables;
        }

        public static Variables operator <(Variables variables, int value)
        {
            variables.AddInt(value);
            return variables;
        }

        public static Variables operator <(Variables variables, long value)
        {
            variables.AddInt64(value);
            return variables;
        }

        public static Variables operator <(Variables variables, float value)
        {
            variables.AddFloat(value);
            return variables;
        }

        public static Variables operator <(Variables variables, double value)
        {
            variables.AddDouble(value);
            return variables;
        }

        public static Variables operator <(Variables variables, string value)
        {
            variables.AddString(value);
            return variables;
        }

        public static Variables operator <(Variables variables, EntityID value)
        {
            variables.AddEntityID(value);
            return variables;
        }

        public static Variables operator <(Variables variables, byte[] value)
        {
            variables.AddBinary(value);
            return variables;
        }

        public static Variables operator <(Variables variables, Variables value)
        {
            variables.AddVariableList(value);
            return variables;
        }

        public static Variables operator >(Variables variables, Var value)
        {
            variables.AddVariable(value);
            return variables;
        }

        public static Variables operator >(Variables variables, bool value)
        {
            variables.AddBool(value);
            return variables;
        }

        public static Variables operator >(Variables variables, int value)
        {
            variables.AddInt(value);
            return variables;
        }

        public static Variables operator >(Variables variables, long value)
        {
            variables.AddInt64(value);
            return variables;
        }

        public static Variables operator >(Variables variables, float value)
        {
            variables.AddFloat(value);
            return variables;
        }

        public static Variables operator >(Variables variables, double value)
        {
            variables.AddDouble(value);
            return variables;
        }

        public static Variables operator >(Variables variables, string value)
        {
            variables.AddString(value);
            return variables;
        }

        public static Variables operator >(Variables variables, EntityID value)
        {
            variables.AddEntityID(value);
            return variables;
        }

        public static Variables operator >(Variables variables, byte[] value)
        {
            variables.AddBinary(value);
            return variables;
        }

        public static Variables operator >(Variables variables, Variables value)
        {
            variables.AddVariableList(value);
            return variables;
        }

        public static Variables operator >(Variables variables, object value)
        {
            variables.AddObject(value);
            return variables;
        }

        public override string ToString()
        {
            s_YouFuckingSb.Clear();

            for (int i = 0; i < m_Variables.Count; ++i)
            {
                s_YouFuckingSb.Append(m_Variables[i].ToString());
                s_YouFuckingSb.Append(";");
            }

            return s_YouFuckingSb.ToString();
        }
    }
}
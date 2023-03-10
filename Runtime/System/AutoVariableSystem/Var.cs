using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Universe
{
    [StructLayout(LayoutKind.Explicit, Size = 1)]
    internal struct Value
    {
        [FieldOffset(0)]
        public bool ValueBool;

        [FieldOffset(0)]
        public int ValueInt32;

        [FieldOffset(0)]
        public long ValueInt64;

        [FieldOffset(0)]
        public float ValueFloat;

        [FieldOffset(0)]
        public double ValueDouble;

        [FieldOffset(0)]
        public EntityID m_ValueEntity;
    }

    [Serializable]
    public struct Var
    {
        object m_RefData;
        Value m_Value;

        public static Var None => new();

        public bool IsNone => VariableType == VarType.None;

        public VarType VariableType { get; set; }

        internal VarFlag Flag { get; }

        public bool GetBool()
        {
            return VariableType == VarType.Bool && m_Value.ValueBool;
        }

        public bool BoolValue
        {
            set
            {
                VariableType = VarType.Bool;
                m_Value.ValueBool = value;
            }
        }

        public int GetInt()
        {
            return VariableType != VarType.Int32 ? 0 : m_Value.ValueInt32;
        }

        public int IntValue
        {
            set
            {
                VariableType = VarType.Int32;
                m_Value.ValueInt32 = value;
            }
        }

        public long GetInt64()
        {
            return VariableType != VarType.Int64 ? 0 : m_Value.ValueInt64;
        }

        public long LongValue
        {
            set
            {
                VariableType = VarType.Int64;
                m_Value.ValueInt64 = value;
            }
        }

        public float GetFloat()
        {
            return VariableType != VarType.Float ? 0 : m_Value.ValueFloat;
        }

        public float FloatValue
        {
            set
            {
                VariableType = VarType.Float;
                m_Value.ValueFloat = value;
            }
        }

        public double GetDouble()
        {
            return VariableType != VarType.Double ? 0 : m_Value.ValueDouble;
        }

        public double DoubleValue
        {
            set
            {
                VariableType = VarType.Double;
                m_Value.ValueDouble = value;
            }
        }

        public string GetString()
        {
            if (VariableType != VarType.String)
            {
                return string.Empty;
            }

            if (Flag != VarFlag.StringInContainer)
            {
                return m_RefData as string;
            }

            if (!(m_RefData is StringBuffer sc))
            {
                return m_RefData as string;
            }

            int strID = m_Value.ValueInt32;
            return sc[strID];
        }

        public string StringValue
        {
            set
            {
                VariableType = VarType.String;
                m_RefData = value;
            }
        }

        public EntityID GetEntity()
        {
            return VariableType != VarType.Object ? EntityID.None : m_Value.m_ValueEntity;
        }

        public EntityID EntityValue
        {
            set
            {
                VariableType = VarType.Object;
                m_Value.m_ValueEntity = value;
            }
        }

        public byte[] GetBinary()
        {
            return VariableType != VarType.Binary ? null : m_RefData as byte[];
        }

        public byte[] BinaryValue
        {
            set
            {
                VariableType = VarType.Binary;
                m_RefData = value;
            }
        }

        public object GetObject()
        {
            return VariableType != VarType.UserData ? null : m_RefData;
        }

        public object UserDataValue
        {
            set
            {
                VariableType = VarType.UserData;
                m_RefData = value;
            }
        }

        public bool StringEquals(Var other)
        {
            if (Flag == VarFlag.StringInContainer && other.Flag == VarFlag.StringInContainer)
            {
                if (m_RefData != other.m_RefData)
                {
                    return false;
                }

                return m_RefData is StringBuffer sc && sc.StrEquals(m_Value.ValueInt32, other.m_Value.ValueInt32);
            }

            bool isEmpty1 = IsEmptyString();
            bool isEmpty2 = other.IsEmptyString();

            if (isEmpty1 != isEmpty2)
            {
                return false;
            }

            if (isEmpty1)
            {
                return true;
            }

            return GetString() == other.GetString();
        }

        public Var(bool value)
        {
            VariableType = VarType.Bool;
            Flag = VarFlag.None;
            m_RefData = null;
            m_Value = new()
                { ValueBool = value };
        }

        public Var(int value)
        {
            VariableType = VarType.Int32;
            Flag = VarFlag.None;
            m_RefData = null;
            m_Value = new()
                { ValueInt32 = value };
        }

        public Var(long value)
        {
            VariableType = VarType.Int64;
            Flag = VarFlag.None;
            m_RefData = null;
            m_Value = new()
                { ValueInt64 = value };
        }

        public Var(float value)
        {
            VariableType = VarType.Float;
            Flag = VarFlag.None;
            m_RefData = null;
            m_Value = new()
                { ValueFloat = value };
        }

        public Var(double value)
        {
            VariableType = VarType.Double;
            Flag = VarFlag.None;
            m_RefData = null;
            m_Value = new()
                { ValueDouble = value };
        }

        public Var(string value)
        {
            VariableType = VarType.String;
            Flag = VarFlag.None;
            m_RefData = value ?? string.Empty;
            m_Value = new();
        }

        public Var(EntityID value)
        {
            VariableType = VarType.Object;
            Flag = VarFlag.None;
            m_RefData = null;
            m_Value = new()
                { m_ValueEntity = value };
        }

        public Var(byte[] value)
        {
            VariableType = VarType.Binary;
            Flag = VarFlag.None;
            m_RefData = value;
            m_Value = new();
        }

        public Var(object value)
        {
            VariableType = VarType.UserData;
            Flag = VarFlag.None;
            m_RefData = value;
            m_Value = new();
        }

        internal Var(StringBuffer sc, int strID)
        {
            VariableType = VarType.String;
            Flag = VarFlag.StringInContainer;
            m_RefData = sc;
            m_Value = new()
                { ValueInt32 = strID };
        }

        public override string ToString()
        {
            switch (VariableType)
            {
                case VarType.Bool:
                {
                    return m_Value.ValueBool.ToString();
                }
                case VarType.Int32:
                {
                    return m_Value.ValueInt32.ToString();
                }
                case VarType.Int64:
                {
                    return m_Value.ValueInt64.ToString();
                }
                case VarType.Float:
                {
                    return m_Value.ValueFloat.ToString(CultureInfo.InvariantCulture);
                }
                case VarType.Double:
                {
                    return m_Value.ValueDouble.ToString(CultureInfo.InvariantCulture);
                }
                case VarType.String:
                case VarType.WideStr:
                {
                    return GetString();
                }
                case VarType.Object:
                {
                    return m_Value.m_ValueEntity.ToString();
                }
                case VarType.None:
                case VarType.UserData:
                case VarType.Binary:
                case VarType.Max:
                default:
                {
                    return m_RefData == null ? string.Empty : m_RefData.ToString();
                }
            }
        }

        bool IsEmptyString()
        {
            if (VariableType != VarType.String)
            {
                return true;
            }

            if (Flag != VarFlag.StringInContainer)
            {
                return GetString().Length <= 0;
            }

            return !(m_RefData is StringBuffer sc) || sc.IsEmpty(m_Value.ValueInt32);
        }
    }
}
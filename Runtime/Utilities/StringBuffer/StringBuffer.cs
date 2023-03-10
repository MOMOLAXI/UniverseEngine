using System;
using System.Collections.Generic;
using System.Text;

namespace Universe
{
    /// <summary>
    /// 字符串容器
    /// </summary>
    public partial class StringBuffer
    {
        byte[] m_Buffer;
        int m_Pos;
        readonly List<StringData> m_DataList = new();

        public const int INVALID_ID = -1;
        public int Capacity => m_Buffer.Length;
        public int UseCount { get; private set; }

        public string this[int id]
        {
            get
            {
                if (id < 0 || id >= m_DataList.Count)
                {
                    return string.Empty;
                }

                StringData data = m_DataList[id];
                if (!data.Used)
                {
                    return string.Empty;
                }

                if (string.IsNullOrEmpty(data.Str))
                {
                    switch (data.Encoding)
                    {
                        case EncodingType.Default:
                        {
                            data.Str = Encoding.UTF8.GetString(m_Buffer, data.Pos, data.Count);
                            break;
                        }
                        case EncodingType.UTF8:
                        {
                            data.Str = Encoding.UTF8.GetString(m_Buffer, data.Pos, data.Count);
                            break;
                        }
                        case EncodingType.Unicode:
                        {
                            data.Str = Encoding.Unicode.GetString(m_Buffer, data.Pos, data.Count);
                            break;
                        }
                        default:
                        {
                            data.Str = string.Empty;
                            Log.Error("[StringBuffer:this[]]invalid encoding type " + (int)data.Encoding);
                            break;
                        }
                    }

                    m_DataList[id] = data;
                }

                return data.Str;
            }
        }

        public StringBuffer(int capacity = Const.N_2048)
        {
            capacity = Math.Max(capacity, Const.N_256);
            m_Buffer = new byte[capacity];
        }

        public int Create(byte[] data, int startIndex, int count)
        {
            return Create(EncodingType.Default, data, startIndex, count);
        }

        public int Create(EncodingType encoding, byte[] data, int startIndex, int count)
        {
            if (startIndex < 0 || count <= 0 || startIndex + count >= data.Length)
            {
                return INVALID_ID;
            }

            int index = AllocNew(count);
            if (index == INVALID_ID)
            {
                return INVALID_ID;
            }

            StringData stringData = m_DataList[index];
            Array.Copy(data, startIndex, m_Buffer, stringData.Pos, count);
            stringData.Used = true;
            stringData.Count = count;
            stringData.Encoding = encoding;
            m_DataList[index] = stringData;

            ++UseCount;

            return index;
        }

        public void Release(int id)
        {
            if (id < 0 || id >= m_DataList.Count)
            {
                return;
            }

            StringData data = m_DataList[id];
            if (data.Used)
            {
                --UseCount;

                data.Used = false;
                data.Encoding = EncodingType.Default;
                data.Str = null;

                m_DataList[id] = data;
            }
        }

        public void Clear()
        {
            m_Pos = 0;
            m_DataList.Clear();
            UseCount = 0;
        }

        public bool IsEmpty(int id)
        {
            if (id < 0 || id >= m_DataList.Count)
            {
                return true;
            }

            StringData data = m_DataList[id];
            if (!data.Used)
            {
                return true;
            }

            return data.Count <= 0;
        }

        public bool StrEquals(int id1, int id2)
        {
            if (id1 < 0 || id1 >= m_DataList.Count ||
                id2 < 0 || id2 >= m_DataList.Count)
            {
                return false;
            }

            if (id1 == id2)
            {
                return true;
            }

            StringData data1 = m_DataList[id1];
            StringData data2 = m_DataList[id2];

            if (!data1.Used && !data2.Used)
            {
                return true;
            }

            if (data1.Count != data2.Count)
            {
                return false;
            }

            for (int i = 0; i < data1.Count; ++i)
            {
                if (m_Buffer[data1.Pos + i] != m_Buffer[data2.Pos + i])
                {
                    return false;
                }
            }

            return true;
        }

        int AllocNew(int newCount)
        {
            // 缓存不足，拓展
            if (m_Pos + newCount >= m_Buffer.Length)
            {
                Expand(m_Pos + newCount);
            }

            StringData data = new()
                { Pos = m_Pos, MaxCount = newCount };

            m_Pos += newCount;
            m_DataList.Add(data);

            return m_DataList.Count - 1;
        }

        void Expand(int newLength)
        {
            // 修剪长度到2的幂次方
            newLength = TrimLength(newLength);

            if (newLength <= m_Buffer.Length)
            {
                return;
            }

            byte[] newBuffer = new byte[newLength];
            Array.Copy(m_Buffer, newBuffer, m_Buffer.Length);

            m_Buffer = newBuffer;
        }

        // 修剪长度到2的幂次方
        static int TrimLength(int length, int trimTime = 20)
        {
            int count = 1;
            int times = 0;

            while (times++ < trimTime)
            {
                count *= Const.N_2;
                if (count >= length)
                {
                    return count;
                }
            }

            return length;
        }
    }
}
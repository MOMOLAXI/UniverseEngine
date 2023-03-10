using System;
using System.IO;
using System.Text;

namespace Universe
{
    public class RawFileOperationHandle : OperationHandleBase, IDisposable
    {
        Action<RawFileOperationHandle> m_Callback;

        internal RawFileOperationHandle(ProviderBase provider) : base(provider)
        {
        }

        internal override void InvokeCallback()
        {
            m_Callback?.Invoke(this);
        }

        /// <summary>
        /// 完成委托
        /// </summary>
        public event Action<RawFileOperationHandle> Completed
        {
            add
            {
                if (!IsValidWithWarning)
                {
                    throw new($"{nameof(RawFileOperationHandle)} is invalid");
                }

                if (Provider.IsDone)
                {
                    value.Invoke(this);
                }
                else
                {
                    m_Callback += value;
                }
            }
            remove => m_Callback -= IsValidWithWarning ? value : throw new($"{nameof(RawFileOperationHandle)} is invalid");
        }

        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        public void WaitForAsyncComplete()
        {
            if (IsValidWithWarning)
            {
                Provider.WaitForAsyncComplete();
            }
        }

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        public void Release()
        {
            ReleaseInternal();
        }

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        public void Dispose()
        {
            ReleaseInternal();
        }


        /// <summary>
        /// 获取原生文件的二进制数据
        /// </summary>
        public byte[] GetRawFileData()
        {
            if (IsValidWithWarning == false)
            {
                return Array.Empty<byte>();
            }

            string filePath = Provider.RawFilePath;
            if (!File.Exists(filePath))
            {
                return Array.Empty<byte>();
            }

            return File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// 获取原生文件的文本数据
        /// </summary>
        public string GetRawFileText()
        {
            if (IsValidWithWarning == false)
            {
                return string.Empty;
            }

            string filePath = Provider.RawFilePath;
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }

            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        /// <summary>
        /// 获取原生文件的路径
        /// </summary>
        public string GetRawFilePath()
        {
            if (IsValidWithWarning == false)
            {
                return string.Empty;
            }

            return Provider.RawFilePath;
        }
    }
}
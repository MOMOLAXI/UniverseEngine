using System;
using System.Collections.Generic;

namespace Universe
{
    /// <summary>
    /// 动态消息回调
    /// </summary>
    /// <param name="args"></param>
    public delegate void MessageEventCallback(Variables args = null);

    internal class MessageEvent : ICacheAble
    {
        readonly List<MessageEventCallback> m_Callbacks = new();

        public void Register(MessageEventCallback function)
        {
            if (function == null)
            {
                return;
            }

            m_Callbacks.Add(function);
        }

        public void Invoke(Variables args)
        {
            args ??= Variables.Empty;
            for (int i = 0; i < m_Callbacks.Count; i++)
            {
                MessageEventCallback callback = m_Callbacks[i];
                try
                {
                    callback?.Invoke(args);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        public bool IsInCache { get; set; }

        public void Reset()
        {
            m_Callbacks.Clear();
        }
    }

    internal class MessageSystem : EngineSystem
    {
        readonly Dictionary<ulong, MessageEvent> m_Events = new();

        readonly ObjectPool<MessageEvent> m_EventPool = ObjectPool<MessageEvent>.Create();

        /// <summary>
        /// 注册动态消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        public void Subscribe(ulong id, MessageEventCallback callback)
        {
            if (callback == null)
            {
                Log.Error($"callback for {id} is null. can not register in FunctionLibrary");
                return;
            }

            if (m_Events.TryGetValue(id, out MessageEvent message))
            {
                message.Register(callback);
                return;
            }

            message = m_EventPool.Get();
            message.Register(callback);
            m_Events[id] = message;
        }

        /// <summary>
        /// 消息是否注册
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsMessage(ulong id)
        {
            return m_Events.ContainsKey(id);
        }

        /// <summary>
        /// 移除消息
        /// </summary>
        /// <param name="id"></param>
        public void RemoveMessage(ulong id)
        {
            if (!m_Events.TryGetValue(id, out MessageEvent message))
            {
                Log.Warning($"Message Id : {id} is not found while remove message");
                return;
            }

            m_Events.Remove(id);
            m_EventPool.Release(message);
        }

        /// <summary>
        /// 广播消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        public void BroadCast(ulong id, Variables args = null)
        {
            if (!m_Events.TryGetValue(id, out MessageEvent message))
            {
                return;
            }

            message.Invoke(args);
        }
    }
}
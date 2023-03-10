using System.Collections.Generic;
using UnityEngine;

namespace Universe
{
    public delegate void GlobalHeartBeatFunction(float duration);

    public delegate void GlobalCountBeatFunction(int count);

    public enum EHearBeatState
    {
        Idle,
        Beating,
    }

    public class CountBeat : ICacheAble
    {
        /// <summary>
        /// 心跳名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 心跳计数函数
        /// </summary>
        public GlobalCountBeatFunction Function;

        /// <summary>
        /// 记录开始的时间点
        /// </summary>
        public float LastTime = -1;

        /// <summary>
        /// 心跳次数
        /// </summary>
        public int BeatCount = -1;

        /// <summary>
        /// 当前计数
        /// </summary>
        public int CurCount;

        /// <summary>
        /// 心跳间隔
        /// </summary>
        public float Interval;

        /// <summary>
        /// 已经走过的间隔时间
        /// </summary>
        public float IntervalRunningTime;

        /// <summary>
        /// 执行状态
        /// </summary>
        public EHearBeatState State = EHearBeatState.Idle;

        public void Invoke()
        {
            Function?.Invoke(CurCount);
        }

        public bool IsInCache { get; set; }

        public void Reset()
        {
            Name = string.Empty;
            Function = null;
            LastTime = -1;
            BeatCount = -1;
            CurCount = default;
            Interval = default;
            IntervalRunningTime = default;
            State = EHearBeatState.Idle;
        }
    }

    public class HeartBeat : ICacheAble
    {
        /// <summary>
        /// 心跳名称
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// 心跳函数
        /// </summary>
        public GlobalHeartBeatFunction Function;

        /// <summary>
        /// 记录开始的时间点
        /// </summary>
        public float LastTime = -1;

        /// <summary>
        /// 运行总时长
        /// </summary>
        public float Duration;

        /// <summary>
        /// 心跳间隔
        /// </summary>
        public float Interval;

        /// <summary>
        /// 已经走过的间隔时间
        /// </summary>
        public float IntervalRunningTime;

        /// <summary>
        /// 执行状态
        /// </summary>
        public EHearBeatState State = EHearBeatState.Idle;

        public void Invoke()
        {
            Function?.Invoke(Time.time - LastTime);
        }

        public bool IsInCache { get; set; }
        public void Reset()
        {
            Name = string.Empty;
            Function = null;
            LastTime = -1;
            Duration = default;
            Interval = default;
            IntervalRunningTime = default;
            State = EHearBeatState.Idle;
        }
    }

    internal class HearBeatSystem : EngineSystem
    {
        readonly ObjectPool<HeartBeat> m_HeartBeatPool = ObjectPool<HeartBeat>.Create();
        readonly ObjectPool<CountBeat> m_CountBeatPool = ObjectPool<CountBeat>.Create();
        readonly List<string> m_DeleteHeartBeatNextFrame = new();
        readonly List<string> m_DeleteCountBeatNextFrame = new();
        readonly Dictionary<string, HeartBeat> m_HeartBeats = new();
        readonly Dictionary<string, CountBeat> m_CountBeats = new();

        /// <summary>
        /// 注册全局心跳函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <param name="function"></param>
        /// <param name="count">心跳次数</param>
        /// <param name="intervalSeconds">间隔时间(秒)</param>
        public void RegisterCountBeat(
            string callbackName,
            GlobalCountBeatFunction function,
            int count,
            float intervalSeconds)
        {
            if (string.IsNullOrEmpty(callbackName) || function == null)
            {
                Log.Error("callback name is null or empty, try to apply an invalid name");
                return;
            }

            CountBeat countBeat = m_CountBeatPool.Get();
            countBeat.Name = callbackName;
            countBeat.Function = function;
            countBeat.Interval = intervalSeconds;
            countBeat.BeatCount = count;
            countBeat.State = EHearBeatState.Beating;
            m_CountBeats.Add(callbackName, countBeat);
        }

        /// <summary>
        /// 注册全局心跳函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <param name="function"></param>
        /// <param name="interval">间隔时间(秒)</param>
        /// <param name="durationSecond">总时长(秒)</param>
        public void RegisterHeartBeat(
            string callbackName,
            GlobalHeartBeatFunction function,
            float interval,
            float durationSecond)
        {
            if (string.IsNullOrEmpty(callbackName) || function == null)
            {
                Log.Error("callback name is null or empty, try to apply an invalid name");
                return;
            }

            HeartBeat heartBeat = m_HeartBeatPool.Get();
            heartBeat.Name = callbackName;
            heartBeat.Function = function;
            heartBeat.Interval = interval;
            heartBeat.Duration = durationSecond;
            heartBeat.State = EHearBeatState.Beating;
            m_HeartBeats.Add(callbackName, heartBeat);
        }

        /// <summary>
        /// 查找心跳函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <returns></returns>
        public HeartBeat FindHeartBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                return default;
            }

            if (m_HeartBeats.TryGetValue(callbackName, out HeartBeat heartBeat))
            {
                return heartBeat;
            }

            Log.Error($"Heart beat name {callbackName} is not Registered");
            return default;
        }

        /// <summary>
        /// 查找心跳函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <returns></returns>
        public CountBeat FindCountBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                return null;
            }

            if (m_CountBeats.TryGetValue(callbackName, out CountBeat countBeat))
            {
                return countBeat;
            }

            Log.Error($"Count beat name {callbackName} is not Registered");
            return null;
        }

        /// <summary>
        /// 存在心跳函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <returns></returns>
        public bool ContainsHeartBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                return false;
            }

            return m_HeartBeats.ContainsKey(callbackName);
        }

        /// <summary>
        /// 存在计数函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <returns></returns>
        public bool ContainsCountBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                return false;
            }

            return m_CountBeats.ContainsKey(callbackName);
        }

        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="callbackName"></param>
        public void PauseHeartBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error($"Heart beat name {callbackName} is not Registered");
                return;
            }

            if (!m_HeartBeats.TryGetValue(callbackName, out HeartBeat heartBeat))
            {
                return;
            }

            heartBeat.State = EHearBeatState.Idle;
        }

        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="callbackName"></param>
        public void PauseCountBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error($"Count beat name {callbackName} is not Registered");
                return;
            }

            if (m_CountBeats.TryGetValue(callbackName, out CountBeat countBeat))
            {
                return;
            }

            countBeat.State = EHearBeatState.Idle;
        }

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="callbackName"></param>
        public void RemoveHeartBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error($"Heart beat name {callbackName} is not Registered");
                return;
            }

            if (!m_HeartBeats.TryGetValue(callbackName, out HeartBeat heartBeat))
            {
                return;
            }

            m_DeleteHeartBeatNextFrame.Add(heartBeat.Name);
        }

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="callbackName"></param>
        public void RemoveCountBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error($"Count beat name {callbackName} is not Registered");
                return;
            }

            if (m_CountBeats.TryGetValue(callbackName, out CountBeat countBeat))
            {
                return;
            }

            m_DeleteCountBeatNextFrame.Add(countBeat.Name);
        }

        public override void Update(float dt)
        {
            //检查删除
            if (m_DeleteHeartBeatNextFrame.Count > 0)
            {
                for (int i = 0; i < m_DeleteHeartBeatNextFrame.Count; i++)
                {
                    string callback = m_DeleteHeartBeatNextFrame[i];
                    if (m_HeartBeats.TryGetValue(callback, out HeartBeat heartBeat))
                    {
                        m_HeartBeats.Remove(callback);
                        m_HeartBeatPool.Release(heartBeat);
                    }
                }

                m_DeleteHeartBeatNextFrame.Clear();
            }

            if (m_DeleteCountBeatNextFrame.Count > 0)
            {
                for (int i = 0; i < m_DeleteCountBeatNextFrame.Count; i++)
                {
                    string callback = m_DeleteCountBeatNextFrame[i];
                    if (m_CountBeats.TryGetValue(callback, out CountBeat countBeat))
                    {
                        m_CountBeats.Remove(callback);
                        m_CountBeatPool.Release(countBeat);
                    }
                }

                m_DeleteCountBeatNextFrame.Clear();
            }

            UpdateHeartBeat(dt);
            UpdateCountBeat(dt);
        }

        void UpdateHeartBeat(float deltaTime)
        {
            foreach (HeartBeat heartBeat in m_HeartBeats.Values)
            {
                //Idle不更新
                if (heartBeat.State == EHearBeatState.Idle)
                {
                    continue;
                }

                //记录开始时间
                if (heartBeat.LastTime < 0)
                {
                    heartBeat.LastTime = Time.time;
                    continue;
                }

                //计算时间差
                float diff = Time.time - heartBeat.LastTime;

                //超过时长删除
                if (heartBeat.Duration >= 0 && diff >= heartBeat.Duration)
                {
                    m_DeleteHeartBeatNextFrame.Add(heartBeat.Name);
                    heartBeat.State = EHearBeatState.Idle;
                    continue;
                }

                //大于interval执行回调
                heartBeat.IntervalRunningTime += deltaTime;
                if (heartBeat.IntervalRunningTime >= heartBeat.Interval)
                {
                    heartBeat.IntervalRunningTime = 0;
                    heartBeat.Invoke();
                }
            }
        }

        void UpdateCountBeat(float deltaTime)
        {
            foreach (CountBeat countBeat in m_CountBeats.Values)
            {
                //Idle不更新
                if (countBeat.State == EHearBeatState.Idle)
                {
                    continue;
                }

                //记录开始时间
                if (countBeat.LastTime <= 0)
                {
                    countBeat.LastTime = Time.time;
                    continue;
                }

                //超过计数删除
                if (countBeat.BeatCount > 0 && countBeat.CurCount >= countBeat.BeatCount)
                {
                    m_DeleteCountBeatNextFrame.Add(countBeat.Name);
                    countBeat.State = EHearBeatState.Idle;
                    continue;
                }

                //大于interval执行回调
                countBeat.IntervalRunningTime += deltaTime;
                if (countBeat.IntervalRunningTime >= countBeat.Interval && countBeat.CurCount < countBeat.BeatCount)
                {
                    countBeat.IntervalRunningTime = 0;
                    countBeat.CurCount += 1;
                    countBeat.Invoke();
                }
            }
        }
    }
}
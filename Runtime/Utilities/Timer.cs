namespace Universe
{
    using System;
    using System.Collections.Generic;

    #region 计时器类对象

    internal class TimerItem
    {
        public string Name;
        public float LastTime;
        public float Interval;
        public int Count = -1;
        public OnTimer OnElapsed;

        public bool isRemoved;

        public void Run(float deltaTime)
        {
            try
            {
                OnElapsed?.Invoke(deltaTime);
            }
            catch (Exception ex)
            {
                Log.Error("Timer Error! Timer name : " + Name + "\n" + ex.StackTrace);
            }
        }
    }

    #endregion

    /// <summary>
    /// 计时器回调
    /// </summary>
    /// <param name="deltaTime"></param>
    public delegate void OnTimer(float deltaTime);

    /// <summary>
    /// 简单计时器
    /// </summary>
    public class Timer
    {
        float m_NowTime;
        readonly List<TimerItem> m_AllTimerList = new();
        readonly List<string> m_RemoveList = new();
        readonly Dictionary<string, TimerItem> m_AllTimerDict = new();

        /// <summary>
        /// 添加计时器
        /// </summary>
        /// <param name="timerName">计时器名字</param>
        /// <param name="cb">计时器回调</param>
        /// <param name="interval">计时器间隔时间</param>
        public void Add(string timerName, OnTimer cb, float interval)
        {
            AddCount(timerName, cb, interval, -1);
        }

        /// <summary>
        /// 添加有执行次数的计时器
        /// </summary>
        /// <param name="timerName">计时器名字</param>
        /// <param name="cb">计时器回调</param>
        /// <param name="interval">计时器间隔时间</param>
        /// <param name="count">执行次数</param>
        public bool AddCount(string timerName, OnTimer cb, float interval, int count)
        {
            if (string.IsNullOrEmpty(timerName))
            {
                Log.Warning("[Timer Info]Add Timer name is invalid.");
                return false;
            }

            TimerItem timer = InnerFindTimer(timerName);

            if (timer is { isRemoved: false })
            {
                Log.Warning("[Timer Info]Add Timer name is already existed.");
                return false;
            }

            timer ??= new();
            timer.LastTime = m_NowTime;
            timer.Name = timerName;
            timer.Interval = interval;
            timer.Count = count;
            timer.OnElapsed = cb;
            timer.isRemoved = false;

            InnerAddTimer(timerName, timer);

            return true;
        }

        /// 删除计时器
        public bool RemoveTimer(string timerName)
        {
            if (string.IsNullOrEmpty(timerName))
            {
                Log.Warning("[Timer Info]Remove Timer name is invalid.");
                return false;
            }

            TimerItem timer = InnerFindTimer(timerName);

            if (null == timer)
            {
                Log.Warning("[Timer Info]Remove Timer is not existed." + timerName);
                return false;
            }

            timer.isRemoved = true;

            return true;
        }

        /// 计时器是否存在
        public bool FindTimer(string timerName)
        {
            if (string.IsNullOrEmpty(timerName))
            {
                return false;
            }

            TimerItem timer = InnerFindTimer(timerName);

            return timer is { isRemoved: false };
        }

        public void ClearTimer()
        {
            m_AllTimerList.Clear();
            m_AllTimerDict.Clear();
            m_RemoveList.Clear();
            m_NowTime = 0;
        }

        // 执行计时器事件
        public void Update(float nowTime)
        {
            m_NowTime = nowTime;

            int count = m_AllTimerList.Count;

            //避免在timer.Run(difference)又对m_AllTimerList进行了操作，导致数组越界
            //倒序遍历只能避免index错位，不能解决越界的问题，这里再加一个标记位来保护越界
            for (int i = count - 1; i >= 0; --i)
            {
                TimerItem timer = m_AllTimerList[i];

                if (timer.isRemoved)
                {
                    m_RemoveList.Add(timer.Name);
                    continue;
                }

                if (timer.LastTime <= 0)
                {
                    timer.LastTime = nowTime;
                    continue;
                }

                float difference = nowTime - timer.LastTime;

                if (difference >= timer.Interval)
                {
                    timer.LastTime = nowTime;

                    if (timer.Count > 0 && --timer.Count <= 0)
                    {
                        timer.isRemoved = true;
                    }

                    // 不要在timer里去remove timer，除了页面关闭整体清空
                    timer.Run(difference);

                    if (timer.isRemoved)
                    {
                        m_RemoveList.Add(timer.Name);
                    }

                    //倒序遍历解决了大部分问题，只剩下整体清空，这里做个保护
                    if (m_AllTimerList.Count == 0)
                    {
                        break;
                    }
                }
            }

            if (m_RemoveList.Count > 0)
            {
                for (int i = 0; i < m_RemoveList.Count; ++i)
                {
                    InnerRemoveTimer(m_RemoveList[i]);
                }

                m_RemoveList.Clear();
            }
        }

        TimerItem InnerFindTimer(string timerName)
        {
            return m_AllTimerDict.ContainsKey(timerName) ? m_AllTimerDict[timerName] : null;
        }

        void InnerAddTimer(string timerName, TimerItem item)
        {
            if (m_AllTimerDict.ContainsKey(timerName))
            {
                return;
            }

            m_AllTimerDict[timerName] = item;
            m_AllTimerList.Add(item);
        }

        void InnerRemoveTimer(string timerName)
        {
            TimerItem timer = InnerFindTimer(timerName);

            if (null == timer)
            {
                return;
            }

            m_AllTimerList.Remove(timer);
            m_AllTimerDict.Remove(timerName);
        }
    }
}
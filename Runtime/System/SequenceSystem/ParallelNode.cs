using System.Collections.Generic;

namespace Universe
{
    sealed internal class ParallelNode : WorkNode
    {
        readonly List<WorkNode> m_NodeList = new();
        readonly List<int> m_NodeStateList = new();
        bool m_IsRunning;
        
        public ParallelNode()
        {
            m_IsRunning = false;
        }

        public override bool IsDone
        {
            get
            {
                if (!m_IsRunning)
                {
                    return false;
                }

                bool isAllNodeDone = true;

                for (int i = 0; i < m_NodeList.Count; ++i)
                {
                    WorkNode node = m_NodeList[i];
                    int state = m_NodeStateList[i];
                    bool isNodeDone = node.IsDone;

                    if (state == 0 && isNodeDone)
                    {
                        m_NodeStateList[i] = 1;
                        node.End();
                    }

                    if (!isNodeDone)
                    {
                        isAllNodeDone = false;
                    }
                }

                return isAllNodeDone;
            }
        }

        internal void Set(WorkNode node)
        {
            m_NodeList.Add(node);
            m_NodeStateList.Add(0);
        }

        protected override void OnStart()
        {
            base.OnStart();

            m_IsRunning = true;

            for (int i = 0; i < m_NodeList.Count; ++i)
            {
                m_NodeList[i].Start();
            }
        }

        protected override void OnEnd()
        {
            if (m_IsRunning)
            {
                for (int i = 0; i < m_NodeList.Count; ++i)
                {
                    if (m_NodeStateList[i] == 0)
                    {
                        m_NodeList[i].End();
                    }
                }
            }

            m_IsRunning = false;

            base.OnEnd();
        }

        protected override void ResetToCache()
        {
            m_IsRunning = false;
            m_NodeList.Clear();
            m_NodeStateList.Clear();
            OnResetToCache();
        }

        public void OnResetToCache() { }
    }
}
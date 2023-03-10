using System.Collections.Generic;

namespace Universe
{
    sealed internal class SequenceNode : WorkNode
    {
        readonly List<WorkNode> m_NodeList = new();
        int m_CurIndex;
        
        public SequenceNode()
        {
            m_CurIndex = -1;
        }

        public override bool IsDone
        {
            get
            {
                if (m_CurIndex == -1)
                {
                    return false;
                }

                if (!Utilities.IsValidIndex(m_CurIndex, m_NodeList.Count))
                {
                    return m_CurIndex >= m_NodeList.Count;
                }

                WorkNode node = m_NodeList[m_CurIndex];
                if (node == null)
                {
                    Log.Error($"{WorkerName} has invalid node, stop sequencer");
                    return true;
                }

                if (node.IsDone)
                {
                    MoveToNextNode();
                }

                return m_CurIndex >= m_NodeList.Count;
            }
        }

        internal void Append(WorkNode node)
        {
            m_NodeList.Add(node);
        }

        protected override void OnStart()
        {
            m_CurIndex = -1;
            MoveToNextNode();
        }

        protected override void OnEnd()
        {
            m_CurIndex = -1;
            base.OnEnd();
        }

        protected override void ResetToCache()
        {
            m_CurIndex = -1;
            m_NodeList.Clear();
        }

        void MoveToNextNode()
        {
            if (Utilities.IsValidIndex(m_CurIndex, m_NodeList.Count))
            {
                WorkNode node = m_NodeList[m_CurIndex];
                node.End();
            }

            ++m_CurIndex;

            if (Utilities.IsValidIndex(m_CurIndex, m_NodeList.Count))
            {
                m_NodeList[m_CurIndex].Start();
            }
        }
    }
}
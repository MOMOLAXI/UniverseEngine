namespace Universe
{
    public interface ICondition
    {
        bool OnMatching();
    }

    internal class ConditionNode : WorkNode
    {
        ICondition m_Condition;
        WorkNode m_TrueNode;
        WorkNode m_FalseNode;
        bool m_Value;
        bool m_IsRunning;
        
        public override bool IsDone
        {
            get
            {
                if (!m_IsRunning)
                {
                    return false;
                }

                return m_Value ? m_TrueNode.IsDone : m_FalseNode.IsDone;
            }
        }

        public void SetBranch(ICondition condition, WorkNode ifTrue, WorkNode ifFalse)
        {
            m_Condition = condition;
            m_TrueNode = ifTrue;
            m_FalseNode = ifFalse;
            m_Value = false;
            m_IsRunning = false;
        }

        protected override void OnStart()
        {
            if (m_Condition == null)
            {
                m_Value = false;
                m_IsRunning = false;
                return;
            }

            m_Value = m_Condition.OnMatching();
            m_IsRunning = true;

            if (m_Value)
            {
                m_TrueNode?.Start();
            }
            else
            {
                m_FalseNode?.Start();
            }
        }

        protected override void OnEnd()
        {
            if (m_IsRunning)
            {
                if (m_Value)
                {
                    m_TrueNode?.End();
                }
                else
                {
                    m_FalseNode?.End();
                }
            }

            m_IsRunning = false;

            base.OnEnd();
        }

        protected override void ResetToCache()
        {
            m_Condition = null;
            m_TrueNode = null;
            m_FalseNode = null;
            m_Value = false;
            m_IsRunning = false;
        }
    }
}
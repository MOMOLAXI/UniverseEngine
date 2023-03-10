using System.Collections.Generic;

namespace Universe
{
    internal class AutoVariableSystem : EngineSystem
    {
        static readonly Queue<Variables> s_Using = new(100);
        static readonly Queue<Variables> s_Free = new(100);

        /// <summary>
        /// 分配一个自动释放的VarList，会在本帧末尾释放，不能持有
        /// </summary>
        /// <returns></returns>
        internal static Variables AllocNonHold()
        {
            if (s_Free.Count == 0)
            {
                Variables variables = new();
                s_Using.Enqueue(variables);
                return variables;
            }
            else
            {
                Variables variables = s_Free.Dequeue();
                s_Using.Enqueue(variables);
                return variables;
            }
        }
        
        public override void LateUpdate(float dt)
        {
            if (s_Using.Count == 0)
            {
                return;
            }

            while (s_Using.Count > 0)
            {
                Variables variables = s_Using.Dequeue();
                variables.Clear();
                s_Free.Enqueue(variables);
            }
        }
    }
}
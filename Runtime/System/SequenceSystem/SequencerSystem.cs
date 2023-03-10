// ******************************************************************
//       /\ /|              
//       \ V/        @author     BuXinYuan 935160739@qq.com
//       | "")       
//       /  |        @Modified   2022-5-14 3:55:33            
//      /  \\       
//    *(__\_\        @Copyright  Copyright (c) BuXinYuan
// ******************************************************************

using System.Collections.Generic;

namespace Universe
{
    public readonly struct SequencerID
    {
        static int s_Serial;
        public readonly int ID;
        public readonly WorkNode Node;

        public SequencerID(WorkNode node) : this()
        {
            s_Serial++;
            ID = s_Serial;
            Node = node;
        }

        public static bool operator ==(SequencerID lhs, SequencerID rhs)
        {
            return lhs.ID == rhs.ID && lhs.Node == rhs.Node;
        }

        public static bool operator !=(SequencerID lhs, SequencerID rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(SequencerID other)
        {
            return ID == other.ID && Equals(Node, other.Node);
        }

        public override bool Equals(object obj)
        {
            return obj is SequencerID other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ID * 397 ^ (Node != null ? Node.GetHashCode() : 0);
            }
        }
    }

    /// <summary>
    /// 工作序列库
    /// </summary>
    internal class SequencerSystem : EngineSystem
    {
        readonly Dictionary<SequencerID, SequenceNode> m_Sequencers = new();
        readonly Dictionary<SequencerID, List<ConditionNode>> m_SequenceConditions = new();
        readonly Dictionary<SequencerID, List<ParallelNode>> m_SequenceParallels = new();
        readonly PolyObjectPool<WorkNode> m_NodePool = new();

        /// <summary>
        /// 开启串行工作序列
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SequencerID Sequence(string name)
        {
            SequenceNode sequence = Alloc<SequenceNode>();
            sequence.WorkerName = name;
            SequencerID id = new(sequence);
            m_Sequencers[id] = sequence;
            Log.Info($"Create Sequencer {name}");
            return id;
        }

        /// <summary>
        /// 开始工作
        /// </summary>
        /// <param name="id"></param>
        public void Start(SequencerID id)
        {
            if (m_Sequencers.TryGetValue(id, out var sequenceNode))
            {
                sequenceNode.Start();
            }
        }

        /// <summary>
        /// 开启并行工作序列
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public SequencerID Parallel(SequencerID id, string name, params WorkNode[] nodes)
        {
            if (nodes == null)
            {
                return id;
            }

            if (!m_Sequencers.TryGetValue(id, out SequenceNode sequenceNode))
            {
                return id;
            }

            ParallelNode parallel = Alloc<ParallelNode>();
            parallel.WorkerName = name;
            foreach (WorkNode node in nodes)
            {
                parallel.Set(node);
            }

            if (m_SequenceParallels.TryGetValue(id, out List<ParallelNode> parallelNodes))
            {
                parallelNodes.Add(parallel);
            }
            else
            {
                parallelNodes = new()
                {
                    parallel
                };

                m_SequenceParallels[id] = parallelNodes;
            }

            sequenceNode.Append(parallel);
            return id;
        }

        /// <summary>
        /// 添加串行分支
        /// </summary>
        /// <param name="id"></param>
        /// <param name="conditionName"></param>
        /// <param name="condition"></param>
        /// <param name="ifTrue"></param>
        /// <param name="ifFalse"></param>
        /// <returns></returns>
        public SequencerID Branch(SequencerID id, string conditionName, ICondition condition, WorkNode ifTrue, WorkNode ifFalse)
        {
            if (string.IsNullOrEmpty(conditionName))
            {
                Log.Error("ConditionName is not valid");
                return id;
            }

            if (!m_Sequencers.TryGetValue(id, out SequenceNode sequence))
            {
                return id;
            }

            ConditionNode node = Alloc<ConditionNode>();
            node.WorkerName = conditionName;
            node.SetBranch(condition, ifTrue, ifFalse);
            if (m_SequenceConditions.TryGetValue(id, out List<ConditionNode> conditions))
            {
                conditions.Add(node);
            }
            else
            {
                conditions = new()
                {
                    node
                };

                m_SequenceConditions[id] = conditions;
            }

            sequence.Append(node);
            return id;
        }

        /// <summary>
        /// 添加串行节点
        /// </summary>
        /// <param name="id"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public SequencerID Append(SequencerID id, WorkNode node)
        {
            if (node == null)
            {
                return id;
            }

            if (m_Sequencers.TryGetValue(id, out SequenceNode sequenceNode))
            {
                sequenceNode.Append(node);
            }

            return id;
        }

        public override void Update(float deltaTime)
        {
            foreach (KeyValuePair<SequencerID, SequenceNode> node in m_Sequencers)
            {
                if (!node.Value.IsDone)
                {
                    continue;
                }

                node.Value.End();
                Release(node.Key, out int parallelsCount, out int conditionsCount);
                Log.Info($"Release Sequencer : {node.Value.WorkerName}, parallels [{parallelsCount}], conditions [{conditionsCount}]");
            }
        }

        internal T Alloc<T>() where T : WorkNode, new()
        {
            return m_NodePool.Get<T>();
        }

        internal void Release(SequencerID id, out int parallelsCount, out int conditionsCount)
        {
            parallelsCount = 0;
            conditionsCount = 0;
            if (m_Sequencers.TryGetValue(id, out SequenceNode sequenceNode))
            {
                m_NodePool.Release(sequenceNode);
            }

            if (m_SequenceParallels.TryGetValue(id, out List<ParallelNode> parallelNodes))
            {
                foreach (ParallelNode node in parallelNodes)
                {
                    parallelsCount++;
                    m_NodePool.Release(node);
                }

                m_SequenceParallels.Remove(id);
            }

            if (m_SequenceConditions.TryGetValue(id, out List<ConditionNode> conditionNodes))
            {
                foreach (ConditionNode node in conditionNodes)
                {
                    conditionsCount++;
                    m_NodePool.Release(node);
                }

                m_SequenceConditions.Remove(id);
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Universe
{
    public static class Engine
    {
        public const string ENGINE_PREFIX = "[UniverseEngine::{0}]";

        static UniverseEngine s_EngineInstance;
        static readonly Dictionary<string, GameObject> s_Globals = new();

        public static GameObject UnityObject;

        public static UniverseEngine Root => s_EngineInstance ??= UnityObject.AddComponent<UniverseEngine>();

        /// <summary>
        /// 获取全局游戏对象
        /// </summary>
        /// <param name="gName"></param>
        /// <param name="result"></param>
        /// <param name="createIfAbsent"></param>
        /// <returns></returns>
        public static bool GetGlobalGameObject(string gName, out GameObject result, bool createIfAbsent = true)
        {
            return GetNode(gName, out result, createIfAbsent);
        }

        public static bool GetOrAddGlobalComponent<T>(string gName, out T result, bool createIfAbsent = true) where T : Component
        {
            GetNode(gName, out GameObject go, createIfAbsent);
            if (go.TryGetComponent(out result))
            {
                return true;
            }

            result = go.AddComponent<T>();
            return true;
        }

        public static Coroutine StartGlobalCoroutine(IEnumerator routine)
        {
            return Root.StartCoroutine(routine);
        }

        public static void StopGlobalCoroutine(Coroutine routine)
        {
            Root.StopCoroutine(routine);
        }

        /// <summary>
        /// 注册游戏系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RegisterGameSystem<T>() where T : GameSystem, new()
        {
            EngineSystem<GameplaySystem>.System.Register<T>();
        }

        public static void RegisterGameSystem<T>(T system) where T : GameSystem, new()
        {
            EngineSystem<GameplaySystem>.System.Register(system);
        }

        /// <summary>
        /// 注册游戏系统
        /// </summary>
        /// <param name="systems"></param>
        public static void RegisterGameSystems(List<GameSystem> systems)
        {
            if (systems == null)
            {
                return;
            }

            EngineSystem<GameplaySystem>.System.Register(systems);
        }

        /// <summary>
        /// 获取游戏系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetSystem<T>() where T : GameSystem, new()
        {
            return GameSystem<T>.System;
        }

        internal static T GetEngineSystem<T>() where T : EngineSystem, new()
        {
            return EngineSystem<T>.System;
        }

    #region Beats

        /// <summary>
        /// 添加心跳时长函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <param name="function"></param>
        /// <param name="interval">间隔时间(秒)</param>
        /// <param name="duration">总时长(秒)</param>
        /// <returns></returns>
        public static void StartGlobalHeartBeat(
            string callbackName,
            GlobalHeartBeatFunction function,
            float interval,
            float duration = -1)
        {
            EngineSystem<HearBeatSystem>.System.RegisterHeartBeat(callbackName, function, interval, duration);
        }

        /// <summary>
        /// 添加心跳计数函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <param name="function"></param>
        /// <param name="interval"></param>
        /// <param name="count"></param>
        public static void StartGlobalCountBeat(
            string callbackName,
            GlobalCountBeatFunction function,
            float interval,
            int count = -1)
        {
            EngineSystem<HearBeatSystem>.System.RegisterCountBeat(callbackName, function, count, interval);
        }

        /// <summary>
        /// 查找心跳函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <returns></returns>
        public static HeartBeat FindHeartBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("callback is can not be null or empty");
                return null;
            }

            return EngineSystem<HearBeatSystem>.System.FindHeartBeat(callbackName);
        }

        /// <summary>
        /// 查找心跳计数函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <returns></returns>
        public static CountBeat FindCountBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("callback is can not be null or empty");
                return null;
            }


            return EngineSystem<HearBeatSystem>.System.FindCountBeat(callbackName);
        }

        /// <summary>
        /// 存在心跳函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <returns></returns>
        public static bool ContainsHeartBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                return false;
            }

            return EngineSystem<HearBeatSystem>.System.ContainsHeartBeat(callbackName);
        }

        /// <summary>
        /// 查找心跳计数函数
        /// </summary>
        /// <param name="callbackName"></param>
        /// <returns></returns>
        public static bool ContainsCountBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                return false;
            }

            return EngineSystem<HearBeatSystem>.System.ContainsCountBeat(callbackName);
        }

        /// <summary>
        /// 暂停心跳函数
        /// </summary>
        /// <param name="callbackName"></param>
        public static void PauseHeartBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("callback is can not be null or empty");
                return;
            }

            EngineSystem<HearBeatSystem>.System.PauseHeartBeat(callbackName);
        }

        /// <summary>
        /// 暂停心跳计数函数
        /// </summary>
        /// <param name="callbackName"></param>
        public static void PauseCountBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("callback is can not be null or empty");
                return;
            }

            EngineSystem<HearBeatSystem>.System.PauseCountBeat(callbackName);
        }

        /// <summary>
        /// 移除心跳函数
        /// </summary>
        /// <param name="callbackName"></param>
        public static void RemoveHeartBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("callback is can not be null or empty");
                return;
            }

            EngineSystem<HearBeatSystem>.System.RemoveHeartBeat(callbackName);
        }

        /// <summary>
        /// 移除心跳计数函数
        /// </summary>
        /// <param name="callbackName"></param>
        public static void RemoveCountBeat(string callbackName)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("callback is can not be null or empty");
                return;
            }

            EngineSystem<HearBeatSystem>.System.RemoveCountBeat(callbackName);
        }

    #endregion

    #region Message

        /// <summary>
        /// 订阅全局消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        public static void Subscribe(ulong id, MessageEventCallback callback)
        {
            if (callback == null)
            {
                Log.Error($"callback is null while subscribe dynamic message for {id.ToString()}");
                return;
            }

            EngineSystem<MessageSystem>.System.Subscribe(id, callback);
        }

        /// <summary>
        /// 广播全局消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        public static void BroadCast(ulong id, Variables args = null)
        {
            EngineSystem<MessageSystem>.System.BroadCast(id, args);
        }

        /// <summary>
        /// 移除全局消息
        /// </summary>
        /// <param name="id"></param>
        public static void RemoveDynamicMessage(ulong id)
        {
            EngineSystem<MessageSystem>.System.RemoveMessage(id);
        }

    #endregion

    #region Sequencer

        public static void Start(this SequencerID sequencerID)
        {
            EngineSystem<SequencerSystem>.System.Start(sequencerID);
        }

        /// <summary>
        /// 开启串行工作序列
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SequencerID Sequencer(string name)
        {
            return EngineSystem<SequencerSystem>.System.Sequence(name);
        }

        /// <summary>
        /// 添加串行分支
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="conditionName"></param>
        /// <param name="condition"></param>
        /// <param name="ifTrue"></param>
        /// <param name="ifFalse"></param>
        /// <returns></returns>
        public static SequencerID Branch(this SequencerID sequence, string conditionName, ICondition condition, WorkNode ifTrue, WorkNode ifFalse)
        {
            if (string.IsNullOrEmpty(conditionName))
            {
                Log.Error("Condition name is null or empty, can not create branch");
                return sequence;
            }

            if (condition == null)
            {
                Log.Error($"Condition is null : {conditionName}");
                return sequence;
            }

            return EngineSystem<SequencerSystem>.System.Branch(sequence, conditionName, condition, ifTrue, ifFalse);
        }

        /// <summary>
        /// 开启串行工作序列
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static SequencerID AppendSingle(this SequencerID sequence, WorkNode node)
        {
            return EngineSystem<SequencerSystem>.System.Append(sequence, node);
        }

        public static SequencerID AppendSingle<T>(this SequencerID sequence) where T : WorkNode, new()
        {
            return EngineSystem<SequencerSystem>.System.Append(sequence, new T());
        }

        /// <summary>
        /// 开启串行工作序列
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static SequencerID AppendSingle(this SequencerID sequence, params WorkNode[] args)
        {
            if (args == null)
            {
                return sequence;
            }

            for (int i = 0; i < args.Length; i++)
            {
                WorkNode node = args[i];
                if (node == null)
                {
                    continue;
                }

                sequence.AppendSingle(node);
            }

            return sequence;
        }

        /// <summary>
        /// 串行工作中开启一段并行序列
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="parallelName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static SequencerID AppendParallel(this SequencerID sequence, string parallelName, params WorkNode[] args)
        {
            if (string.IsNullOrEmpty(parallelName))
            {
                Log.Error("Parallel name is null or empty");
                return sequence;
            }

            if (args == null)
            {
                return sequence;
            }

            return EngineSystem<SequencerSystem>.System.Parallel(sequence, parallelName, args);
        }

    #endregion

    #region Entity

        /// <summary>
        /// 创建Entity
        /// </summary>
        /// <param name="className"></param>
        /// <param name="parentID"></param>
        /// <param name="identity"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        public static EntityID CreateEntity(string className, EntityID parentID, uint identity = 0, bool isStatic = false)
        {
            if (parentID == EntityID.None)
            {
                Log.Error("Entity id is not valid");
                return EntityID.None;
            }

            Entity parent = EngineSystem<EntitySystem>.System.Find(parentID);
            Entity entity = EngineSystem<EntitySystem>.System.CreateEntity(className, identity, parent, isStatic);
            return entity.ID;
        }

        /// <summary>
        /// 创建Entity
        /// </summary>
        /// <param name="className"></param>
        /// <param name="identity"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        public static EntityID CreateEntity(string className, uint identity = 0, bool isStatic = false)
        {
            Entity entity = EngineSystem<EntitySystem>.System.CreateEntity(className, identity, null, isStatic);
            return entity.ID;
        }

        /// <summary>
        /// 删除Entity
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="immediate"></param>
        public static void DestroyEntity(this EntityID entityID, bool immediate = false)
        {
            if (entityID == EntityID.None)
            {
                Log.Error("Entity id is not valid");
                return;
            }

            EngineSystem<EntitySystem>.System.DestroyEntity(entityID, immediate);
        }

        /// <summary>
        /// 获取游戏对象
        /// </summary>
        /// <param name="entityID"></param>
        /// <returns></returns>
        public static GameObject GetEntityGameObject(this EntityID entityID)
        {
            if (entityID == EntityID.None)
            {
                Log.Error("Entity id is not valid");
                return null;
            }

            return entityID.GetEntity().gameObject;
        }

        /// <summary>
        /// Entity添加Mono组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AddComponent<T>(this EntityID entityID) where T : Component
        {
            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.AddComponent<T>();
        }

        /// <summary>
        /// Entity添加逻辑组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AddEntityComponent<T>(this EntityID entityID) where T : EntityComponent, new()
        {
            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.AddEntityComponent<T>();
        }

        /// <summary>
        /// Entity添加逻辑组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static EntityComponent AddEntityComponent(this EntityID entityID, Type type)
        {
            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.AddEntityComponent(type);
        }

        /// <summary>
        /// 获取逻辑组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetEntityComponent<T>(this EntityID entityID) where T : EntityComponent
        {
            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetEntityComponent<T>();
        }

        /// <summary>
        /// 获取Mono组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetComponent<T>(this EntityID entityID) where T : Component
        {
            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetComponent<T>();
        }

        /// <summary>
        /// 移除Entity逻辑组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool RemoveEntityComponent<T>(this EntityID entityID) where T : EntityComponent
        {
            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return false;
            }

            return entity.RemoveEntityComponent<T>();
        }

        /// <summary>
        /// 移除Entity逻辑组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool RemoveEntityComponent(this EntityID entityID, Type type)
        {
            if (type == null)
            {
                Log.Error("can not remove null type");
                return false;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return false;
            }

            return entity.RemoveEntityComponent(type);
        }

        /// <summary>
        /// 移除Entity逻辑组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        public static bool RemoveEntityComponent(this EntityID entityID, EntityComponent component)
        {
            if (component == null)
            {
                Log.Error("can not remove null type");
                return false;
            }

            Entity id = GetEntity(entityID);
            if (id == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return false;
            }

            return id.RemoveEntityComponent(component);
        }

        public static Vector3 GetPosition(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.Position;
        }

        public static void SetPotision(this EntityID id, Vector3 position)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Position = position;
        }

        public static Vector3 GetLocalPosition(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.LocalPosition;
        }

        public static void SetLocalPosition(this EntityID id, Vector3 position)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LocalPosition = position;
        }

        public static Vector3 GetEulerAngles(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.EulerAngles;
        }

        public static void SetEulerAngles(this EntityID id, Vector3 eulers)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.EulerAngles = eulers;
        }

        public static Vector3 GetLocalEulerAngles(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.LocalEulerAngles;
        }

        public static void SetLocalEulerAngles(this EntityID id, Vector3 eulers)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LocalEulerAngles = eulers;
        }

        public static Vector3 GetRight(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.Right;
        }

        public static void SetRight(this EntityID id, Vector3 right)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Right = right;
        }

        public static Vector3 GetUp(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.Up;
        }

        public static void SetUp(this EntityID id, Vector3 up)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Up = up;
        }

        public static Vector3 GetForward(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.Forward;
        }

        public static void SetForward(this EntityID id, Vector3 forward)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Forward = forward;
        }

        public static Quaternion GetRotation(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Quaternion.identity;
            }

            return entity.Rotation;
        }

        public static void SetRotation(this EntityID id, Quaternion rotation)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Rotation = rotation;
        }

        public static Quaternion GetLocalRotation(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Quaternion.identity;
            }

            return entity.LocalRotation;
        }

        public static void SetLocalRotation(this EntityID id, Quaternion localRotation)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LocalRotation = localRotation;
        }

        public static Vector3 GetLocalScale(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.LocalScale;
        }

        public static void SetLocalScale(this EntityID id, Vector3 scale)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LocalScale = scale;
        }

        public static Matrix4x4 GetWorldToLocalMatrix(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Matrix4x4.zero;
            }

            return entity.WorldToLocalMatrix;
        }


        public static Matrix4x4 GetLocalToWorldMatrix(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Matrix4x4.zero;
            }

            return entity.LocalToWorldMatrix;
        }

        public static void Translate(this EntityID id, Vector3 translation, Space relativeTo)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Translate(translation, relativeTo);
        }

        public static void Translate(this EntityID id, Vector3 translation)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Translate(translation);
        }

        public static void Translate(this EntityID id, float x, float y, float z, Space relativeTo)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Translate(x, y, z, relativeTo);
        }

        public static void Translate(this EntityID id, float x, float y, float z)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }


            entity.Translate(x, y, z);
        }

        public static void Translate(this EntityID id, Vector3 translation, Transform relativeTo)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }


            entity.Translate(translation, relativeTo);
        }

        public static void Translate(this EntityID id, float x, float y, float z, Transform relativeTo)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }


            entity.Translate(x, y, z, relativeTo);
        }

        public static void Rotate(this EntityID id, Vector3 eulers, Space relativeTo)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }


            entity.Rotate(eulers, relativeTo);
        }

        public static void Rotate(this EntityID id, Vector3 eulers)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Rotate(eulers);
        }

        public static void Rotate(this EntityID id, float xAngle, float yAngle, float zAngle, Space relativeTo)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Rotate(xAngle, yAngle, zAngle, relativeTo);
        }

        public static void Rotate(this EntityID id, float xAngle, float yAngle, float zAngle)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Rotate(xAngle, yAngle, zAngle);
        }

        public static void Rotate(this EntityID id, Vector3 axis, float angle, Space relativeTo)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Rotate(axis, angle, relativeTo);
        }

        public static void Rotate(this EntityID id, Vector3 axis, float angle)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Rotate(axis, angle);
        }

        public static void RotateAround(this EntityID id, Vector3 point, Vector3 axis, float angle)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.RotateAround(point, axis, angle);
        }

        public static void LookAt(this EntityID id, Transform target, Vector3 worldUp)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LookAt(target, worldUp);
        }

        public static void LookAt(this EntityID id, Transform target)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LookAt(target);
        }

        public static void LookAt(this EntityID id, Vector3 worldPosition, Vector3 worldUp)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LookAt(worldPosition, worldUp);
        }

        public static void LookAt(this EntityID id, Vector3 worldPosition)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LookAt(worldPosition);
        }

        public static Vector3 TransformDirection(this EntityID id, Vector3 direction)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.TransformDirection(direction);
        }

        public static Vector3 TransformDirection(this EntityID id, float x, float y, float z)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }


            return entity.TransformDirection(x, y, z);
        }

        public static Vector3 InverseTransformDirection(this EntityID id, Vector3 direction)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.InverseTransformDirection(direction);
        }

        public static Vector3 InverseTransformDirection(this EntityID id, float x, float y, float z)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.InverseTransformDirection(x, y, z);
        }

        public static Vector3 TransformVector(this EntityID id, Vector3 vector)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.TransformVector(vector);
        }

        public static Vector3 TransformyVector(this EntityID id, float x, float y, float z)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.TransformVector(x, y, z);
        }

        public static Vector3 InverseTransformVector(this EntityID id, Vector3 vector)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.InverseTransformVector(vector);
        }

        public static Vector3 InverseTransformVector(this EntityID id, float x, float y, float z)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.InverseTransformVector(x, y, z);
        }

        public static Vector3 TransformPoint(this EntityID id, Vector3 position)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.TransformPoint(position);
        }

        public static Vector3 TransformPoint(this EntityID id, float x, float y, float z)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.TransformPoint(x, y, z);
        }

        public static Vector3 InverseTransformPoint(this EntityID id, Vector3 position)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.InverseTransformPoint(position);
        }

        public static Vector3 InverseTransformPoint(this EntityID id, float x, float y, float z)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.InverseTransformPoint(x, y, z);
        }

        /// <summary>
        /// 索引获取子Entity
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static EntityID GetChildAtIndex(this EntityID entityID, int index)
        {
            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                return default;
            }

            return entity.GetChildAtIndex(index);
        }

        /// <summary>
        /// 获取子对象
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="result"></param>
        public static void GetChildren(this EntityID entityID, List<EntityID> result)
        {
            if (result == null)
            {
                Log.Error("input list is null");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.GetChildren(result);
        }

        /// <summary>
        /// 注册类事件Hook
        /// </summary>
        /// <param name="className"></param>
        /// <param name="eventType"></param>
        /// <param name="callBack"></param>
        public static void RegisterClassEventHook(string className, EntityEvent eventType, EntityEventCallBack callBack)
        {
            if (string.IsNullOrEmpty(className))
            {
                Log.Error($"class name can not be null while {nameof(RegisterClassEventHook)}");
                return;
            }

            if (callBack == null)
            {
                Log.Error("callback is null");
                return;
            }


            EntityClassEventContext context = EngineSystem<EntitySystem>.System.EventLibrary.Get(className);
            context?.RegisterEvent(eventType, callBack);
        }

        /// <summary>
        /// 添加属性变化回调
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="callbackName"></param>
        /// <param name="function"></param>
        public static void AddPropertyHook(
            this EntityID entityID,
            string propName,
            string callbackName,
            PropChangeFunction function)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("[AddPropHook]property name must not be null or empty");
                return;
            }

            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("[AddPropHook]callback name must not be null or empty");
                return;
            }

            if (function == null)
            {
                Log.Error("[AddPropHook] function is null");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.AddPropHook(propName, callbackName, function);
        }

        /// <summary>
        /// 移除属性变化回调
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="callbackName"></param>
        public static void RemovePropertyHook(this EntityID entityID, string propName, string callbackName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("[RemovePropHook]property name must not be null or empty");
                return;
            }

            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("[RemovePropHook]callback name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.RemovePropHook(propName, callbackName);
        }

        /// <summary>
        /// 获取所有离散属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="variableList"></param>
        /// <returns></returns>
        public static bool GetAllProperties(this EntityID entityID, List<EntityProperty> variableList)
        {
            if (variableList == null)
            {
                return false;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return false;
            }

            return entity.GetPropList(variableList);
        }

        /// <summary>
        /// Entity是否有该属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static bool HasProp(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return false;
            }

            return entity.HasProp(propName);
        }

        /// <summary>
        /// 获取Entity属性的数据类型
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static VarType GetVariableType(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return VarType.None;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return VarType.None;
            }

            return entity.GetVariableType(propName);
        }

        /// <summary>
        /// 获取Entity的bool属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static bool GetBool(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetBool(propName);
        }

        public static string GetClassName(this EntityID entityID)
        {
            if (entityID == EntityID.None)
            {
                return string.Empty;
            }

            Entity entity = EngineSystem<EntitySystem>.System.Find(entityID);
            if (entity == null)
            {
                Log.Error($"Can not find Entity with ID : {entityID.ToString()}");
                return string.Empty;
            }

            return entity.ClassName;
        }

        /// <summary>
        /// 获取Entity的Int属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static int GetInt(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetInt(propName);
        }

        /// <summary>
        /// 获取Entity的long属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static long GetInt64(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetInt64(propName);
        }

        /// <summary>
        /// 获取Entity的float属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static float GetFloat(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetFloat(propName);
        }

        /// <summary>
        /// 获取Entity的double属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static double GetDouble(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetDouble(propName);
        }

        /// <summary>
        /// 获取角色身上的字符串属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static string GetString(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetString(propName);
        }

        /// <summary>
        /// 获取Entity的EntityID属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static EntityID GetEntityID(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return EntityID.None;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return EntityID.None;
            }

            return entity.GetEntityID(propName);
        }

        /// <summary>
        /// 获取Entity的二进制属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static byte[] GetBinary(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return null;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return null;
            }

            return entity.GetBinary(propName);
        }

        /// <summary>
        /// 获取Entity的对象属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static object GetObject(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetObject(propName);
        }

        /// <summary>
        /// 获取Entity的对象属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static T GetClass<T>(this EntityID entityID, string propName) where T : class
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetClass<T>(propName);
        }

        /// <summary>
        /// 获取Entity的属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static Var GetProp(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetProp(propName);
        }

        /// <summary>
        /// 设置Entity的属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetProp(this EntityID entityID, string propName, Var value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetProp(propName, value);
        }

        /// <summary>
        /// 设置Entity的bool属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetBool(this EntityID entityID, string propName, bool value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetBool(propName, value);
        }

        /// <summary>
        /// 设置Entity的int属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetInt(this EntityID entityID, string propName, int value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetInt(propName, value);
        }

        /// <summary>
        /// 设置Entity的long属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetInt64(this EntityID entityID, string propName, long value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetInt64(propName, value);
        }

        /// <summary>
        /// 设置Entity的float属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetFloat(this EntityID entityID, string propName, float value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetFloat(propName, value);
        }

        /// <summary>
        /// 设置Entity的double属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetDouble(this EntityID entityID, string propName, double value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetDouble(propName, value);
        }

        /// <summary>
        /// 设置Entity的string属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetString(this EntityID entityID, string propName, string value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetString(propName, value);
        }

        /// <summary>
        /// 设置Entity的EntityID属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetEntityID(this EntityID entityID, string propName, EntityID value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetEntityID(propName, value);
        }

        /// <summary>
        /// 设置Entity的二进制属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetBinary(this EntityID entityID, string propName, byte[] value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetBinary(propName, value);
        }

        /// <summary>
        /// 设置Entity的对象属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetObject(this EntityID entityID, string propName, object value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetObject(propName, value);
        }

        /// <summary>
        /// 设置Entity的对象属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetClass<T>(this EntityID entityID, string propName, T value) where T : class
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetClass(propName, value);
        }

    #endregion

    #region AsyncOperations

        /// <summary>
        /// 开启一个异步操作
        /// </summary>
        /// <param name="operation">异步操作对象</param>
        public static void StartAsyncOperation(AsyncOperationBase operation)
        {
            EngineSystem<OperationSystem>.System.StartOperation(operation);
        }

        /// <summary>
        /// 异步操作的最小时间片段(毫秒)
        /// </summary>
        public static long AsyncOperationMaxSliceTimeMs
        {
            get => EngineSystem<OperationSystem>.System.MaxSliceTimeMs;
            set => EngineSystem<OperationSystem>.System.MaxSliceTimeMs = value;
        }

        /// <summary>
        /// 处理器是否繁忙
        /// </summary>
        public static bool IsOperationBusy => EngineSystem<OperationSystem>.System.IsBusy;

    #endregion

    #region Assets

        /// <summary>
        /// 创建资源包
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static bool CreateAssetsPackage(string packageName)
        {
            return EngineSystem<AssetSystem>.System.CreateAssetsPackage(packageName);
        }

        /// <summary>
        /// 获取资源包
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        public static bool GetAssetsPackage(string packageName, out AssetsPackage package)
        {
            return EngineSystem<AssetSystem>.System.TryGetAssetsPackage(packageName, out package);
        }

        /// <summary>
        /// 检查资源包是否存在
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static bool ExistAssetsPackage(string packageName)
        {
            return EngineSystem<AssetSystem>.System.ExistAssetsPackage(packageName);
        }

        /// <summary>
        /// 资源是否需要从远端更新下载
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <param name="location">资源的定位地址</param>
        /// <returns></returns>
        public static bool IsNeedDownloadFromRemote(string packageName, string location)
        {
            return EngineSystem<AssetSystem>.System.IsNeedDownloadFromRemote(packageName, location);
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="tag">资源标签</param>
        public static AssetInfo[] GetAssetInfos(string packageName, string tag)
        {
            return EngineSystem<AssetSystem>.System.GetAssetInfos(packageName, tag);
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="tags">资源标签列表</param>
        public static AssetInfo[] GetAssetInfos(string packageName, string[] tags)
        {
            return EngineSystem<AssetSystem>.System.GetAssetInfos(packageName, tags);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public static AssetInfo GetAssetInfo(string packageName, string location)
        {
            return EngineSystem<AssetSystem>.System.GetAssetInfo(packageName, location);
        }

        /// <summary>
        /// 检查资源定位地址是否有效
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public static bool CheckLocationValid(string packageName, string location)
        {
            return EngineSystem<AssetSystem>.System.CheckLocationValid(packageName, location);
        }

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfo">资源信息</param>
        public static RawFileOperationHandle LoadRawFileSync(string packageName, AssetInfo assetInfo)
        {
            return EngineSystem<AssetSystem>.System.LoadRawFileSync(packageName, assetInfo);
        }

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public static RawFileOperationHandle LoadRawFileSync(string packageName, string location)
        {
            return EngineSystem<AssetSystem>.System.LoadRawFileSync(packageName, location);
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfo">资源信息</param>
        public static RawFileOperationHandle LoadRawFileAsync(string packageName, AssetInfo assetInfo)
        {
            return EngineSystem<AssetSystem>.System.LoadRawFileAsync(packageName, assetInfo);
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public static RawFileOperationHandle LoadRawFileAsync(string packageName, string location)
        {
            return EngineSystem<AssetSystem>.System.LoadRawFileAsync(packageName, location);
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">场景的定位地址</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="activateOnLoad">加载完毕时是否主动激活</param>
        /// <param name="priority">优先级</param>
        public static SceneOperationHandle LoadSceneAsync(string packageName,
                                                          string location,
                                                          LoadSceneMode sceneMode = LoadSceneMode.Single,
                                                          bool activateOnLoad = true,
                                                          int priority = 100)
        {
            return EngineSystem<AssetSystem>.System.LoadSceneAsync(packageName, location, sceneMode, activateOnLoad, priority);
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfo">场景的资源信息</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="activateOnLoad">加载完毕时是否主动激活</param>
        /// <param name="priority">优先级</param>
        public static SceneOperationHandle LoadSceneAsync(string packageName,
                                                          AssetInfo assetInfo,
                                                          LoadSceneMode sceneMode = LoadSceneMode.Single,
                                                          bool activateOnLoad = true,
                                                          int priority = 100)
        {
            return EngineSystem<AssetSystem>.System.LoadSceneAsync(packageName, assetInfo, sceneMode, activateOnLoad, priority);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfo">资源信息</param>
        public static AssetOperationHandle LoadAssetSync(string packageName, AssetInfo assetInfo)
        {
            return EngineSystem<AssetSystem>.System.LoadAssetSync(packageName, assetInfo);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public static AssetOperationHandle LoadAssetSync<TObject>(string packageName, string location) where TObject : UnityEngine.Object
        {
            return EngineSystem<AssetSystem>.System.LoadAssetSync<TObject>(packageName, location);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        public static AssetOperationHandle LoadAssetSync(string packageName, string location, Type type)
        {
            return EngineSystem<AssetSystem>.System.LoadAssetSync(packageName, location, type);
        }


        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfo">资源信息</param>
        public static AssetOperationHandle LoadAssetAsync(string packageName, AssetInfo assetInfo)
        {
            return EngineSystem<AssetSystem>.System.LoadAssetAsync(packageName, assetInfo);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public static AssetOperationHandle LoadAssetAsync<TObject>(string packageName, string location) where TObject : UnityEngine.Object
        {
            return EngineSystem<AssetSystem>.System.LoadAssetAsync<TObject>(packageName, location);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        public static AssetOperationHandle LoadAssetAsync(string packageName, string location, Type type)
        {
            return EngineSystem<AssetSystem>.System.LoadAssetAsync(packageName, location, type);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfo">资源信息</param>
        public static SubAssetsOperationHandle LoadSubAssetsSync(string packageName, AssetInfo assetInfo)
        {
            return EngineSystem<AssetSystem>.System.LoadSubAssetsSync(packageName, assetInfo);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public static SubAssetsOperationHandle LoadSubAssetsSync<TObject>(string packageName, string location) where TObject : UnityEngine.Object
        {
            return EngineSystem<AssetSystem>.System.LoadSubAssetsSync<TObject>(packageName, location);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        public static SubAssetsOperationHandle LoadSubAssetsSync(string packageName, string location, Type type)
        {
            return EngineSystem<AssetSystem>.System.LoadSubAssetsSync(packageName, location, type);
        }


        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfo">资源信息</param>
        public static SubAssetsOperationHandle LoadSubAssetsAsync(string packageName, AssetInfo assetInfo)
        {
            return EngineSystem<AssetSystem>.System.LoadSubAssetsAsync(packageName, assetInfo);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        public static SubAssetsOperationHandle LoadSubAssetsAsync<TObject>(string packageName, string location) where TObject : UnityEngine.Object
        {
            return EngineSystem<AssetSystem>.System.LoadSubAssetsAsync<TObject>(packageName, location);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        public static SubAssetsOperationHandle LoadSubAssetsAsync(string packageName, string location, Type type)
        {
            return EngineSystem<AssetSystem>.System.LoadSubAssetsAsync(packageName, location, type);
        }

        /// <summary>
        /// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="tag">资源标签</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public static PatchDownloaderOperation CreatePatchDownloader(string packageName, string tag, int downloadingMaxNumber, int failedTryAgain)
        {
            FixedArray<string> array = FixedArray<string>.Get(1);
            array[0] = tag;
            return EngineSystem<AssetSystem>.System.CreatePatchDownloader(packageName, array, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="tags">资源标签列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public static PatchDownloaderOperation CreatePatchDownloader(string packageName, string[] tags, int downloadingMaxNumber, int failedTryAgain)
        {
            return EngineSystem<AssetSystem>.System.CreatePatchDownloader(packageName, tags, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建补丁下载器，用于下载更新当前资源版本所有的资源包文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public static PatchDownloaderOperation CreatePatchDownloader(string packageName, int downloadingMaxNumber, int failedTryAgain)
        {
            return EngineSystem<AssetSystem>.System.CreatePatchDownloader(packageName, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建补丁下载器，用于下载更新指定的资源列表依赖的资源包文件
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetInfos">资源信息列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public static PatchDownloaderOperation CreateBundleDownloader(string packageName, AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain)
        {
            return EngineSystem<AssetSystem>.System.CreateBundleDownloader(packageName, assetInfos, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建补丁解压器
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="tag">资源标签</param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        public static PatchUnpackerOperation CreatePatchUnpacker(string packageName, string tag, int unpackingMaxNumber, int failedTryAgain)
        {
            return EngineSystem<AssetSystem>.System.CreatePatchUnpacker(packageName, tag, unpackingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建补丁解压器
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="tags">资源标签列表</param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        public static PatchUnpackerOperation CreatePatchUnpacker(string packageName, string[] tags, int unpackingMaxNumber, int failedTryAgain)
        {
            return EngineSystem<AssetSystem>.System.CreatePatchUnpacker(packageName, tags, unpackingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建补丁解压器
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        public static PatchUnpackerOperation CreatePatchUnpacker(string packageName, int unpackingMaxNumber, int failedTryAgain)
        {
            return EngineSystem<AssetSystem>.System.CreatePatchUnpacker(packageName, unpackingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 获取资源包调试信息
        /// </summary>
        /// <returns></returns>
        public static DebugReport GetAssetDebugReport()
        {
            return EngineSystem<AssetSystem>.System.GetDebugReport();
        }

    #endregion

    #region 系统参数

        /// <summary>
        /// 设置下载系统参数，启用断点续传功能文件的最小字节数
        /// </summary>
        public static void SetDownloadSystemBreakpointResumeFileSize(int fileBytes)
        {
            AssetDownloadSystem.BreakpointResumeFileSize = fileBytes;
        }

        /// <summary>
        /// 设置下载系统参数，下载失败后清理文件的HTTP错误码
        /// </summary>
        public static void SetDownloadSystemClearFileResponseCode(List<long> codes)
        {
            AssetDownloadSystem.ClearFileResponseCodes = codes;
        }

        /// <summary>
        /// 设置下载系统参数，自定义的证书认证实例
        /// </summary>
        public static void SetDownloadSystemCertificateHandler(CertificateHandler instance)
        {
            AssetDownloadSystem.CertificateHandlerInstance = instance;
        }

        /// <summary>
        /// 设置下载系统参数，自定义下载请求
        /// </summary>
        public static void SetDownloadSystemUnityWebRequest(DownloadRequestDelegate requestDelegate)
        {
            AssetDownloadSystem.RequestDelegate = requestDelegate;
        }

        /// <summary>
        /// 设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）
        /// </summary>
        public static void SetOperationSystemComponentMaxTimeSlice(long milliseconds)
        {
            if (milliseconds < 30)
            {
                milliseconds = 30;
                Log.Warning("MaxTimeSlice minimum value is 30 milliseconds.");
            }

            AsyncOperationMaxSliceTimeMs = milliseconds;
        }

        /// <summary>
        /// 设置缓存系统参数，已经缓存文件的校验等级
        /// </summary>
        public static void SetCacheSystemCachedFileVerifyLevel(EVerifyLevel verifyLevel)
        {
            CacheSystem.InitVerifyLevel = verifyLevel;
        }

    #endregion

    #region 沙盒相关

        /// <summary>
        /// 获取内置文件夹名称
        /// </summary>
        public static string GetStreamingAssetBuildinFolderName()
        {
            return UniverseConstant.STREAMING_ASSETS_BUILDIN_FOLDER;
        }

        /// <summary>
        /// 获取沙盒的根路径
        /// </summary>
        public static string GetSandboxRoot()
        {
            return AssetPath.GetPersistentRootPath();
        }

        /// <summary>
        /// 清空沙盒目录
        /// </summary>
        public static void ClearSandbox()
        {
            PersistentHelper.DeleteSandbox();
        }

    #endregion

    #region LocalPrefs

        public static int GetInt(string key)
        {
            return string.IsNullOrEmpty(key) ? default : PlayerPrefs.GetInt(Key(key));
        }

        public static float GetFloat(string key)
        {
            return string.IsNullOrEmpty(key) ? default : PlayerPrefs.GetFloat(Key(key));
        }

        public static string GetString(string key)
        {
            return string.IsNullOrEmpty(key) ? default : PlayerPrefs.GetString(Key(key));
        }

        public static void Set(string key, int value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            PlayerPrefs.SetInt(Key(key), value);
        }

        public static void Set(string key, float value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            PlayerPrefs.SetFloat(Key(key), value);
        }

        public static void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            PlayerPrefs.SetString(Key(key), value);
        }

    #endregion

    #region 生命周期
        
        public static void Start()
        {
            Kernel.Register(EngineUnits.Units);
            Kernel.Init();
        }

        public static void Reset()
        {
            Kernel.Reset();
        }

        public static void Update(float dt)
        {
            Kernel.Update(dt);
        }

        public static void FixedUpdate(float dt)
        {
            Kernel.FixedUpdate(dt);
        }

        public static void LateUpdate(float dt)
        {
            Kernel.LateUpdate(dt);
        }

        public static void Destroy()
        {
            Kernel.Destroy();
        }

        public static void ApplicationFocus(bool hasFocus)
        {
            Kernel.ApplicationFocus(hasFocus);
        }

        public static void ApplicationPause(bool pauseStatus)
        {
            Kernel.ApplicationPause(pauseStatus);
        }

        public static void ApplicationQuit()
        {
            Kernel.ApplicationQuit();
        }

    #endregion

    #region Private

        /// <summary>
        /// 获取Entity
        /// </summary>
        /// <param name="entityID"></param>
        /// <returns></returns>
        static Entity GetEntity(this EntityID entityID)
        {
            if (entityID == EntityID.None)
            {
                Log.Error("Entity id is not valid");
                return default;
            }

            Entity entity = EngineSystem<EntitySystem>.System.Find(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity;
        }

        static string Key(string value)
        {
            return ENGINE_PREFIX.SafeFormat(string.IsNullOrEmpty(value) ? "Null" : value);
        }

        public static string FormatName(string target)
        {
            return $"[{target}]";
        }

        static bool GetNode(string gName, out GameObject result, bool createIfAbsent = true)
        {
            result = null;
            if (string.IsNullOrEmpty(gName))
            {
                return false;
            }

            if (!createIfAbsent)
            {
                return s_Globals.TryGetValue(FormatName(gName), out result);
            }

            if (!s_Globals.TryGetValue(FormatName(gName), out result))
            {
                result = new(FormatName(gName));
                result.transform.SetParent(Root.transform);
                s_Globals[gName] = result;
            }

            return true;
        }

        static class EngineSystem<T> where T : EngineSystem, new()
        {
            static T s_Instance;
            public static T System => s_Instance ??= Kernel.Get<T>();
        }

        static class GameSystem<T> where T : GameSystem, new()
        {
            static T s_Instance;
            public static T System => s_Instance ??= EngineSystem<GameplaySystem>.System.Get<T>();
        }

    #endregion

    }
}
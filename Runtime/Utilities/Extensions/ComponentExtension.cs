using System.Collections.Generic;
using UnityEngine;

namespace Universe
{
    public static class ComponentExtension
    {
        public static TComponent GetOrAddComponent<TComponent>(this GameObject gameObject, bool includeChildren = false) where TComponent : Component
        {
            TComponent component = null;
            if (includeChildren)
            {
                component = gameObject.GetComponentInChildren<TComponent>();
                if (component == null)
                {
                    component = gameObject.AddComponent<TComponent>();
                }

                return component;
            }

            if (gameObject.TryGetComponent(out component))
            {
                return component;
            }

            component = gameObject.AddComponent<TComponent>();
            return component;
        }

        public static TComponent GetOrAddComponent<TComponent>(this Component baseComponent) where TComponent : Component
        {
            GameObject go = baseComponent.gameObject;
            if (go.TryGetComponent(out TComponent component))
            {
                return component;
            }

            component = go.AddComponent<TComponent>();
            return component;
        }

        public static TValue GetOrRegisterValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, bool addIfNull = false)
            where TKey : Component where TValue : Component
        {
            if (key == null)
            {
                return null;
            }

            bool found = dictionary.TryGetValue(key, out TValue value);
            if (found)
            {
                return value;
            }

            value = addIfNull ? key.gameObject.GetOrAddComponent<TValue>() : key.GetComponent<TValue>();
            if (value != null)
            {
                dictionary.Add(key, value);
            }

            return value;
        }

        /// <summary>
        /// Gets a "target" component within a particular branch (inside the hierarchy). The branch is defined by the "branch root object", which is also defined by the chosen 
        /// "branch root component". The returned component must come from a child of the "branch root object".
        /// </summary>
        /// <param name="callerComponent"></param>
        /// <param name="includeInactive">Include inactive objects?</param>
        /// <typeparam name="T1">Branch root component type.</typeparam>
        /// <typeparam name="T2">Target component type.</typeparam>
        /// <returns>The target component.</returns>
        public static T2 GetComponentInBranch<T1, T2>(this Component callerComponent, bool includeInactive = true) where T1 : Component where T2 : Component
        {
            T1[] rootComponents = callerComponent.transform.root.GetComponentsInChildren<T1>(includeInactive);

            if (rootComponents.Length == 0)
            {
                Debug.LogWarning($"Root component: No objects found with {typeof(T1).Name} component");
                return null;
            }

            for (int i = 0; i < rootComponents.Length; i++)
            {
                T1 rootComponent = rootComponents[i];

                // Is the caller a child of this root?
                if (!callerComponent.transform.IsChildOf(rootComponent.transform) && !rootComponent.transform.IsChildOf(callerComponent.transform))
                    continue;

                T2 targetComponent = rootComponent.GetComponentInChildren<T2>(includeInactive);

                if (targetComponent == null)
                    continue;

                return targetComponent;
            }

            return null;
        }

        /// <summary>
        /// Gets a "target" component within a particular branch (inside the hierarchy). The branch is defined by the "branch root object", which is also defined by the chosen 
        /// "branch root component". The returned component must come from a child of the "branch root object".
        /// </summary>
        /// <param name="callerComponent"></param>
        /// <param name="includeInactive">Include inactive objects?</param>
        /// <typeparam name="T1">Target component type.</typeparam>	
        /// <returns>The target component.</returns>
        public static T1 GetComponentInBranch<T1>(this Component callerComponent, bool includeInactive = true) where T1 : Component
        {
            return callerComponent.GetComponentInBranch<T1, T1>(includeInactive);
        }
        
        #region Animator

        /// <summary>
        /// Gets the current clip effective length, that is, the original length divided by the playback speed. The length value is always positive, regardless of the speed sign. 
        /// It returns false if the clip is not valid.
        /// </summary>
        public static bool GetCurrentClipLength(this Animator animator, ref float length)
        {
            if (animator.runtimeAnimatorController == null)
                return false;

            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);

            if (clipInfo.Length == 0)
                return false;


            float clipLength = clipInfo[0].clip.length;
            float speed = animator.GetCurrentAnimatorStateInfo(0).speed;


            length = Mathf.Abs(clipLength / speed);

            return true;
        }

        public static bool MatchTarget(this Animator animator, Vector3 targetPosition, Quaternion targetRotation, AvatarTarget avatarTarget, float startNormalizedTime, float targetNormalizedTime)
        {
            if (animator.runtimeAnimatorController == null)
                return false;

            if (animator.isMatchingTarget)
                return false;

            if (animator.IsInTransition(0))
                return false;

            MatchTargetWeightMask weightMask = new(Vector3.one, 1f);

            animator.MatchTarget(
                targetPosition,
                targetRotation,
                avatarTarget,
                weightMask,
                startNormalizedTime,
                targetNormalizedTime
            );


            return true;
        }

        public static bool MatchTarget(this Animator animator, Vector3 targetPosition, AvatarTarget avatarTarget, float startNormalizedTime, float targetNormalizedTime)
        {
            if (animator.runtimeAnimatorController == null)
                return false;

            if (animator.isMatchingTarget)
                return false;

            if (animator.IsInTransition(0))
                return false;

            MatchTargetWeightMask weightMask = new(Vector3.one, 0f);

            animator.MatchTarget(
                targetPosition,
                Quaternion.identity,
                avatarTarget,
                weightMask,
                startNormalizedTime,
                targetNormalizedTime
            );

            return true;
        }

        public static bool MatchTarget(this Animator animator, Transform target, AvatarTarget avatarTarget, float startNormalizedTime, float targetNormalizedTime)
        {
            if (animator.runtimeAnimatorController == null)
                return false;

            if (animator.isMatchingTarget)
                return false;

            if (animator.IsInTransition(0))
                return false;

            MatchTargetWeightMask weightMask = new(Vector3.one, 1f);

            animator.MatchTarget(
                target.position,
                target.rotation,
                avatarTarget,
                weightMask,
                startNormalizedTime,
                targetNormalizedTime
            );


            return true;
        }

        public static bool MatchTarget(this Animator animator, Transform target, AvatarTarget avatarTarget, float startNormalizedTime, float targetNormalizedTime, MatchTargetWeightMask weightMask)
        {
            if (animator.runtimeAnimatorController == null)
                return false;

            if (animator.isMatchingTarget)
                return false;

            if (animator.IsInTransition(0))
                return false;

            animator.MatchTarget(
                target.position,
                target.rotation,
                AvatarTarget.Root,
                weightMask,
                startNormalizedTime,
                targetNormalizedTime
            );


            return true;
        }

        #endregion
    }
}
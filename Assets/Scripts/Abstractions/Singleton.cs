using UnityEngine;

namespace Abstractions
{
    /// <summary>
    /// Inheriting from this class will follow the singleton pattern.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : class
    {
        public static T Instance { get; private set; }

        /// <summary>
        /// Registers this component as the instance unless another one is already registered.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (Instance != null && !ReferenceEquals(Instance, this))
            {
                SingletonUtility.HandleDuplicate(this, typeof(T));
                return;
            }

            Instance = this as T;
        }

        /// <summary>
        /// If this is the registered instance, hands over to a waiting one in edit mode or clears it, so a duplicate going away keeps the real instance.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (ReferenceEquals(Instance, this))
            {
                Instance = SingletonUtility.FindEditModeSuccessor(this) as T;
            }
        }
    }

    /// <summary>
    /// Singleton whose instance is only accessible to the inheriting class.
    /// </summary>
    public abstract class PrivateSingleton<T> : MonoBehaviour where T : class
    {
        protected static T Instance;

        /// <summary>
        /// Registers this component as the instance unless another one is already registered.
        /// </summary>
        protected virtual void Awake()
        {
            if (Instance != null && !ReferenceEquals(Instance, this))
            {
                SingletonUtility.HandleDuplicate(this, typeof(T));
                return;
            }

            Instance = this as T;
        }

        /// <summary>
        /// Hands over to a waiting instance in edit mode, or clears it, on destroy rather than disable, because Awake will not run again to restore it.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (ReferenceEquals(Instance, this))
            {
                Instance = SingletonUtility.FindEditModeSuccessor(this) as T;
            }
        }
    }

    /// <summary>
    /// Shared helpers for the singleton base classes.
    /// </summary>
    internal static class SingletonUtility
    {
        /// <summary>
        /// Removes a duplicate in Play mode; in edit mode only warns, since a second open scene must never lose its components.
        /// </summary>
        public static void HandleDuplicate(Component duplicate, System.Type type)
        {
            if (Application.isPlaying)
            {
                Debug.LogError($"Duplicate singleton {type} found on {duplicate.name}. Removing the duplicate.", duplicate);
                Object.Destroy(duplicate);
            }
            else
            {
                Debug.LogWarning($"{type} on {duplicate.name} in scene '{duplicate.gameObject.scene.name}' waits while another one is registered, and takes over when that one goes away.", duplicate);
            }
        }

        /// <summary>
        /// In edit mode, finds another enabled component of the leaving one's type, outside preview scenes, to take over as the instance; in Play mode there is none.
        /// </summary>
        public static Component FindEditModeSuccessor(Behaviour leaving)
        {
            #if UNITY_EDITOR
            if (Application.isPlaying) return null;
            foreach (var candidate in Object.FindObjectsByType(leaving.GetType(), FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (candidate is Behaviour { isActiveAndEnabled: true } behaviour && behaviour != leaving
                    && !UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(behaviour.gameObject.scene))
                {
                    return behaviour;
                }
            }
            #endif
            return null;
        }
    }
}

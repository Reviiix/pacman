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
        /// Clears the instance only if this is the registered one, so a duplicate going away keeps the real instance.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
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
        /// Clears the instance on destroy rather than disable, because Awake will not run again to restore it.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
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
                Debug.LogWarning($"Another {type} is already registered; {duplicate.name} in scene '{duplicate.gameObject.scene.name}' is ignored while both are open.", duplicate);
            }
        }
    }
}

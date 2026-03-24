using UnityEngine;

namespace JournalSystem
{
    public abstract class SingletonInstance<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        var singletonObject = new GameObject(typeof(T).Name + " (Singleton)");
                        _instance = singletonObject.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                _instance?.gameObject.SetActive(false);
            }

            _instance = this as T;
            _instance.gameObject.SetActive(true);
        }

        // Changed to protected virtual so Journal can call base.OnEnable()
        protected virtual void OnEnable()
        {
            if (_instance != null && _instance != this)
            {
                _instance.gameObject.SetActive(false);
            }
            _instance = this as T;
            _instance.gameObject.SetActive(true);
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
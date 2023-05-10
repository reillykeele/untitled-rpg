using UnityEngine;

namespace Util.Singleton
{
    public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                        _instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            _instance = this as T;
        }

        protected virtual void OnApplicationQuit()
        {
            _instance = null;
            Destroy(gameObject);
        }
    }
}
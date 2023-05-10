using UnityEngine;

namespace Util.Singleton
{
    public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (transform.root != transform && transform.childCount <= 1)
                    Destroy(transform.root.gameObject);
                else
                    Destroy(gameObject);
                return;
            }

            base.Awake();
        }
    }
}

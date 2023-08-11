using UnityEngine;
using UntitledRPG.Actor;
using UntitledRPG.Actor.Interface;

namespace UntitledRPG.Environment
{
    [RequireComponent(typeof(Collider))]
    public class Water : MonoBehaviour
    {
        private Renderer _renderer;

        void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        public float GetWaterY(Vector3 pos)
        {
            return 0.0f;
        }

        void OnTriggerEnter(Collider collider)
        {
            collider.GetComponent<IFloatie>()?.SubscribeToWater(this);
        }

        void OnTriggerExit(Collider collider)
        {
            collider.GetComponent<IFloatie>()?.UnsubscribeToWater(this);
        }
    }
}

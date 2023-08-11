using UnityEngine;
using UntitledRPG.Actor.Interface;

namespace UntitledRPG.Actor.Interactable
{
    [RequireComponent(typeof(Collider))]
    public class RangedInteractable : MonoBehaviour, IInteractable
    {
        /// <inheritdoc />
        public int Priority { get; }

        private Collider _collider;

        void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        void OnValidate()
        {
            var collider = GetComponent<Collider>();
            if (collider.isTrigger == false)
            {
                collider.isTrigger = true;
                Debug.LogWarning($"[{gameObject.name}] Ranged Interactable collider must be a trigger. Setting isTrigger = true.");
            }
        }

        /// <inheritdoc />
        public virtual void Interact()
        {
            // ?
        }

        /// <inheritdoc />
        public void HighlightInteractable()
        {
            Debug.Log($"Current interactable is {gameObject.name}");
        }

        void OnTriggerEnter(Collider collider)
        {
            if (collider.TryGetComponent<IRangedInteracter>(out var interacter))
            {
                interacter.Subscribe(this);
            }
        }

        void OnTriggerExit(Collider collider)
        {
            if (collider.TryGetComponent<IRangedInteracter>(out var interacter))
            {
                interacter.Unsubscribe(this);
            }
        }
    }
}

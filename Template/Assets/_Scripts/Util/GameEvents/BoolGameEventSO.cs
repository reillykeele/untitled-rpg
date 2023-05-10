using UnityEngine;
using UnityEngine.Events;

namespace Util.GameEvents
{
    [CreateAssetMenu(fileName = "BoolGameEvent", menuName = "Game Event/Bool Game Event")]
    public class BoolGameEventSO : ScriptableObject
    {
        public UnityAction<bool> OnEventRaised;

        public void RaiseEvent(bool value) => OnEventRaised?.Invoke(value);
    }
}
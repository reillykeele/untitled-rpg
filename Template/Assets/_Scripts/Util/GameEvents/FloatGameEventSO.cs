using UnityEngine;
using UnityEngine.Events;

namespace Util.GameEvents
{
    [CreateAssetMenu(fileName = "FloatGameEvent", menuName = "Game Event/Float Game Event")]
    public class FloatGameEventSO : ScriptableObject
    {
        public UnityAction<float> OnEventRaised;

        public void RaiseEvent(float value) => OnEventRaised?.Invoke(value);
    }
}

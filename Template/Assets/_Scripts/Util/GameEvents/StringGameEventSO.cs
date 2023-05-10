using UnityEngine;
using UnityEngine.Events;

namespace Util.GameEvents
{
    [CreateAssetMenu(fileName = "StringGameEvent", menuName = "Game Event/String Game Event")]
    public class StringGameEventSO : ScriptableObject
    {
        public UnityAction<string> OnEventRaised;

        public void RaiseEvent(string value) => OnEventRaised?.Invoke(value);
    }
}
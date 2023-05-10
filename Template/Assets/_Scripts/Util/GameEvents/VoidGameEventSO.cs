using UnityEngine;
using UnityEngine.Events;

namespace Util.GameEvents
{
    [CreateAssetMenu(fileName = "VoidGameEvent", menuName = "Game Event/Void Game Event")]
    public class VoidGameEventSO : ScriptableObject
    {
        public UnityAction OnEventRaised;

        public void RaiseEvent() => OnEventRaised?.Invoke();
    }
}

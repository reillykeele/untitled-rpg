using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Util.FSM
{
    public class StateMachine : MonoBehaviour
    {
        [SerializeField] private bool _useUpdate = true;
        [SerializeField] private bool _useFixedUpdate = false;

        [SerializeField] private StateListSO _stateSOs;

        private List<State> _states;
        private State _currentState;

        public State GetCurrentState => _currentState;

        void Awake()
        {
            _states = _stateSOs.Initialize(this);                       
        }

        void Start()
        {
            var initialState =  _states.FirstOrDefault();
            SetCurrentState(initialState);
        }

        public void TransitionState<T>() where T : State
        {
            var state = GetState<T>();
            if (state != null)            
                SetCurrentState(state);     
            else
                Debug.LogWarning($"Requested state \"{typeof(T)}\" not included in state list.");
        }

        public void SetCurrentState(State nextState)
        {
            if (_currentState == nextState) return;

            _currentState?.Exit();

            _currentState = nextState;

            _currentState?.Enter();
        }

        public void Update()
        {
            if (_useUpdate)
                _currentState?.Update();
        }

        public void FixedUpdate()
        {
            if (_useFixedUpdate)
                _currentState?.FixedUpdate();
        }

        public State GetState<T>() where T : State
        {
            return _states.OfType<T>().FirstOrDefault();
        }

        // pass-through 
        public new T GetComponent<T>() where T : Component => base.GetComponent<T>();
    }
}

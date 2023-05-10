using System.Collections.Generic;
using UnityEngine;

namespace Util.FSM
{
    [CreateAssetMenu(fileName = "States", menuName = "State Machine/State List")]
    public class StateListSO : ScriptableObject
    {
        public List<StateSO> StateSOs;

        public List<State> Initialize(StateMachine stateMachine)
        {
            var states = new List<State>();
            
            foreach (var state in StateSOs)
            {
                states.Add(state.Initialize(stateMachine));
            }

            return states;
        }
    }
    
    public abstract class StateSO : ScriptableObject
    {
        public abstract State Initialize(StateMachine stateMachine);
    }

    public abstract class State
    {
        protected StateMachine _stateMachine;

        internal State() {}

        public State(StateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public virtual void Enter() {}
        public virtual void Update() {}
        public virtual void FixedUpdate() {}
        public virtual void Exit() {}
    }
}

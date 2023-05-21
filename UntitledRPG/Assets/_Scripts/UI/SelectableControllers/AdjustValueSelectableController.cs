using ReiBrary.UI.Controllers.Selectables;
using UnityEngine;
using UnityEngine.Events;
using UntitledRPG.Input;

namespace UntitledRPG.UI.SelectableControllers
{
    public class AdjustValueSelectableController : ASelectableController
    {
        [Header("Input")] [SerializeField] protected InputReader _inputReader;

        [SerializeField] protected float _incrementValue = 5f;

        protected float _value;

        public UnityEvent<string> OnValueChangedEvent;

        protected virtual void Start()
        {
            OnValueChangedEvent.Invoke("" + _value);
        }

        protected void OnNavigate(Vector2 nav)
        {
            if (nav == Vector2.left)
                Decrease();
            else if (nav == Vector2.right)
                Increase();
        }

        protected virtual void Increase() { }
        protected virtual void Decrease() { }
    }
}
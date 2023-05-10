using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Util.Input;

namespace Template.Input
{
	[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
    public class InputReader : ScriptableObject, GameInput.IGameplayActions, GameInput.IMenuActions, IInputReader
    {
        private GameInput _gameInput;

		#region Gameplay Events
		
        public event UnityAction<Vector2> MoveEvent = delegate { };
        public event UnityAction ExampleEvent = delegate { };
        public event UnityAction ExampleCancelledEvent = delegate { };

        #endregion

        #region Menu Events

        public event UnityAction<Vector2> MenuNavigateEvent = delegate { };
        public event UnityAction MenuNavigateCancelledEvent = delegate { };
        public event UnityAction MenuPauseEvent = delegate { };
        public event UnityAction MenuUnpauseEvent = delegate { };
        public event UnityAction MenuAcceptButtonEvent = delegate { };
        public event UnityAction MenuCancelButtonEvent = delegate { };
        public event UnityAction<int> ChangeTabEvent = delegate { };

        #endregion

        private void OnEnable()
		{
			if (_gameInput == null)
			{
				_gameInput = new GameInput();

				_gameInput.Gameplay.SetCallbacks(this);
				_gameInput.Menu.SetCallbacks(this);

				// Default
				// EnableGameplayInput();
                EnableMenuInput();
			}
        }

		private void OnDisable()
		{
			DisableAllInput();
		}

		public void DisableAllInput()
		{
			_gameInput.Gameplay.Disable();
			_gameInput.Menu.Disable();
		}

        #region Gameplay Actions

		public void EnableGameplayInput()
		{
			_gameInput.Menu.Disable();
			_gameInput.Gameplay.Enable();
		}

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveEvent.Invoke(context.ReadValue<Vector2>());
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                MenuPauseEvent.Invoke();
        }

        // TODO: Remove
        public void OnExample(InputAction.CallbackContext context)
        {
            switch (context.phase)
		    {
			    case InputActionPhase.Performed:
				    ExampleEvent.Invoke();
				    break;
			    case InputActionPhase.Canceled:
				    ExampleCancelledEvent.Invoke();
				    break;
		    }
        }

        #endregion

		#region Menu Actions

		public void EnableMenuInput()
		{
			_gameInput.Gameplay.Disable();
			_gameInput.Menu.Enable();
		}

        public void OnNavigate(InputAction.CallbackContext context)
        {
            // Handled by event system, but we still may want to access
            // ex. slider controls, enum pickers, etc.

            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    MenuNavigateEvent.Invoke(context.ReadValue<Vector2>());
                    break;
                case InputActionPhase.Canceled:
                    MenuNavigateCancelledEvent.Invoke();
                    break;
            }
        }

        public void OnAccept(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                MenuAcceptButtonEvent.Invoke();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                MenuCancelButtonEvent.Invoke();
        }

        public void OnUnpause(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                MenuUnpauseEvent.Invoke();
        }

        public void OnChangeTab(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                var val = context.ReadValue<float>();
                ChangeTabEvent.Invoke(Mathf.RoundToInt(val));
            }
        }

        #endregion

        void OnDeviceLost()
        {

        }

    }
}
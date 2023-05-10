using Template.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using Util.Audio;
using Util.Systems;
using Util.UI;
using Util.UI.Controllers;

namespace UI.UIControllers
{
    public class StartMenuUIController : UIController
    {
        [Header("Start Menu UI Controller")] 
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private UIPage _targetUiPageType;

        [Header("Audio")] [SerializeField] 
        private AudioSoundSO _startSound;

        protected override void Awake()
        {
            base.Awake();

            // GameManager.Instance.CurrentGameState = GameState.Menu;
        }

        void OnEnable()
        {
            // TODO Bind to start button event
        }

        void OnDisable()
        {
            // TODO Un-bind to start button event
        }

        void Update()
        {
            // TODO: Use Input System
            if (Keyboard.current?.anyKey.wasPressedThisFrame == true ||
                Gamepad.current?.startButton.wasPressedThisFrame == true ||
                Mouse.current?.leftButton.wasPressedThisFrame == true)
            {
                // _canvasAudioController.Play(CanvasAudioController.CanvasAudioSoundType.Start);
                if (_startSound != null)
                    AudioSystem.Instance.PlayAudioSound(_startSound);
                _canvasController.SwitchUI(_targetUiPageType, true);
            }
        }
    }
}

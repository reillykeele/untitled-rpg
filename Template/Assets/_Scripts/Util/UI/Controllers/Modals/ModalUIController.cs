using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Util.Helpers;
using Util.UI.Controllers;

namespace Util.UI.Modals
{
    public class ModalUIController : UIController
    {
        [Header("Modal Components")]
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private Button _yesButton;
        [SerializeField] private TextMeshProUGUI _yesButtonText;
        [SerializeField] private Button _noButton;
        [SerializeField] private TextMeshProUGUI _noButtonText;

        [SerializeField] private UnityEvent _defaultYesEvent = new UnityEvent();
        [SerializeField] private UnityEvent _defaultNoEvent = new UnityEvent();

        private Action _yesAction;
        private Action _noAction;

        void OnEnable()
        {
            _yesButton.onClick.AddListener(OnYes);
            _noButton.onClick.AddListener(OnNo);
        }

        void OnDisable()
        {
            _yesButton.onClick.RemoveListener(OnYes);
            _noButton.onClick.RemoveListener(OnNo);
        }

        public void DisplayModal(string title, string description, Action yesAction = null, Action noAction = null, string yesButtonText = "Yes", string noButtonText = "No")
        {
            if (title.IsNullOrEmpty() == false)
                _title.text = title;
            else
                _title.gameObject.Disable();

            if (description.IsNullOrEmpty() == false) 
                _description.text = description;
            else 
                _description.gameObject.Disable();

            // if (yesButtonText.IsNullOrEmpty() == false)
            //     _yesButtonText.text = yesButtonText;
            // else
            //     _yesButton.gameObject.Disable();
            //
            // if (noButtonText.IsNullOrEmpty() == false)
            //     _noButtonText.text = noButtonText;
            // else 
            //     _noButton.gameObject.Disable();

            _yesAction = yesAction ?? _defaultYesEvent.Invoke;
            // _noAction = noAction ?? _defaultNoEvent.Invoke;
            _noAction = _defaultNoEvent.Invoke;

            OpenModal();
        }

        public void OpenModal() => _canvasController.DisplayUI(Page);

        public void CloseModal() => _canvasController.HideUI(Page);

        // public override void ReturnToUI()
        // {
        //     if (IsFocused)
        //         OnNo();
        // }

        private void OnYes() => _yesAction?.Invoke();
        private void OnNo() => _noAction?.Invoke();

    }
}

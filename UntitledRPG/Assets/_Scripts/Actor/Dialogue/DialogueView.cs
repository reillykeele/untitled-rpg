using System;
using System.Collections;
using ReiBrary.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

namespace UntitledRPG.Actor.Dialogue
{
    public class DialogueView : DialogueViewBase
    {
        [SerializeField] private CanvasGroup _dialogueCanvasGroup;
        [SerializeField] private TextMeshProUGUI _lineText;
        [SerializeField] private Button _advanceButton;

        void Awake()
        {
            _dialogueCanvasGroup.alpha = 0;
            _dialogueCanvasGroup.interactable = false;
            _dialogueCanvasGroup.blocksRaycasts = false;
        }

        /// <inheritdoc />
        public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            // base.RunLine(dialogueLine, onDialogueLineFinished);

            _lineText.gameObject.Enable();
            _dialogueCanvasGroup.gameObject.Enable();

            _dialogueCanvasGroup.alpha = 1;
            _dialogueCanvasGroup.interactable = true;
            _dialogueCanvasGroup.blocksRaycasts = true;

            _lineText.text = dialogueLine.TextWithoutCharacterName.Text;

            // onDialogueLineFinished();
        }

        /// <inheritdoc />
        public override void InterruptLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            base.InterruptLine(dialogueLine, onDialogueLineFinished);
        }

        /// <inheritdoc />
        public override void DismissLine(Action onDismissalComplete)
        {
            // base.DismissLine(onDismissalComplete);

            _dialogueCanvasGroup.alpha = 0;
            _dialogueCanvasGroup.interactable = false;
            _dialogueCanvasGroup.blocksRaycasts = false;

            onDismissalComplete();
        }

        /// <inheritdoc/>
        public override void UserRequestedViewAdvancement()
        {
            // base.UserRequestedViewAdvancement();

            requestInterrupt?.Invoke();
        }

        /// <inheritdoc />
        public override void DialogueStarted() => base.DialogueStarted();

        /// <inheritdoc />
        public override void DialogueComplete()
        {
            base.DialogueComplete();
        }
    }
}

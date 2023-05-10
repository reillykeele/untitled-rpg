using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Util.Attributes;
using Util.Audio;
using Util.Helpers;
using Util.Systems;
using Util.UI.Controllers.Selectables;
using Util.UI.Tween;

namespace Util.UI.Controllers
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIController : MonoBehaviour
    {
        [Header("UI Controller")]
        public UIPage Page;
        public UIPage ReturnPage;
        [SerializeField] private bool _hideOnNullReturnPage = false;

        [SerializeField] public Selectable _initialSelected = null;
        [SerializeField, ReadOnly] protected Selectable _lastSelected = null;

        [Header("Audio")] 
        [SerializeField] protected AudioSoundSO _returnToPreviousAudio;

        // Parent
        protected CanvasController _canvasController;
        protected CanvasAudioController _canvasAudioController;

        // Components
        protected CanvasGroup _canvasGroup;
        protected List<ASelectableController> _selectableControllers;
        protected IEnumerable<BaseTween> _tweens;
        protected IEnumerable<Animator> _animators;

        [Header("State")]
        [SerializeField, ReadOnly] public bool IsTransitioning = false;
        [SerializeField] public bool IsFocused => _canvasController.ActivePage == Page;

        protected virtual void Awake()
        {
            _canvasController = GetComponentInParent<CanvasController>();
            _canvasAudioController = GetComponentInParent<CanvasAudioController>();

            _canvasGroup = GetComponent<CanvasGroup>();
            _selectableControllers = GetComponentsInChildren<ASelectableController>().ToList();
            _tweens = GetComponentsInChildren<BaseTween>();
            _animators = GetComponentsInChildren<Animator>();
        }

        public virtual void Reset()
        {
            _lastSelected = null;
        }

        public virtual void Enable(bool resetOnSwitch = false)
        {
            if (resetOnSwitch)
                Reset();

            SetActive();

            gameObject.Enable();

            IsTransitioning = false;
        }

        public virtual IEnumerator EnableCoroutine(bool resetOnSwitch = false, bool transition = true)
        {
            if (IsTransitioning)
                yield break;
            
            IsTransitioning = true;

            if (resetOnSwitch)
                Reset();

            SetActive();

            var transitionDuration = 0f;
            if (transition)
                foreach (var tween in _tweens)
                {
                    if (tween.ShouldTweenInOnEnable() == false && tween.ShouldTweenIn())
                    {
                        transitionDuration = Mathf.Max(transitionDuration, tween.GetDurationIn());
                        tween.TweenIn();
                    }
                    else
                        tween.Reset();
                }

            gameObject.Enable();

            yield return new WaitForSecondsRealtime(transitionDuration);

            IsTransitioning = false;
        }

        public virtual void Disable()
        {
            OnLoseFocus();
            gameObject.Disable();

            IsTransitioning = false;
        }

        public virtual IEnumerator DisableCoroutine()
        {
            if (IsTransitioning)
                yield break;
            
            IsTransitioning = true;

            OnLoseFocus();

            var transitionDuration = 0f;
            foreach (var tween in _tweens.Where(x => x.gameObject.activeInHierarchy && x.ShouldTweenOut()))
            {
                transitionDuration = Mathf.Max(transitionDuration, tween.GetDurationOut());
                tween.TweenOut();
            }

            yield return new WaitForSecondsRealtime(transitionDuration);

            gameObject.Disable();
            IsTransitioning = false;
        }

        public virtual void OnLoseFocus()
        {
            _lastSelected = EventSystem.current?.currentSelectedGameObject?.GetComponent<Selectable>();
        }

        public virtual void ReturnToUI()
        {
            if (IsFocused == false) return;

            if (ReturnPage != null)
            {
                if (_returnToPreviousAudio != null)
                    AudioSystem.Instance.PlayAudioSound(_returnToPreviousAudio);
                
                _canvasController.SwitchUI(ReturnPage, resetTargetOnSwitch: false, transition: true);
            }
            else if (_hideOnNullReturnPage)
            {
                if (_returnToPreviousAudio != null)
                    AudioSystem.Instance.PlayAudioSound(_returnToPreviousAudio);
                
                _canvasController.HideUI(Page);
            }
        }

        public void SetActive()
        {
            if (Gamepad.current != null)
                SetSelected();
        }

        public virtual void SetSelected()
        {
            if (_lastSelected != null)
                _lastSelected.Select();
            else if (_initialSelected != null)
                _initialSelected.Select();
            else
                _selectableControllers?.FirstOrDefault()?.Select();
        }
    }
}

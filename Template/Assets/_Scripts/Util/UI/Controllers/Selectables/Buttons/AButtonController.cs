using UnityEngine;
using UnityEngine.EventSystems;

namespace Util.UI.Controllers.Selectables.Buttons
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public abstract class AButtonController : ASelectableController
    {
        protected UnityEngine.UI.Button _button;

        protected override void Awake()
        {
            base.Awake();

            _button = GetComponent<UnityEngine.UI.Button>();

            _button.onClick.AddListener(OnClickHandler);
        }

        public override void Select() => _button.Select();

        public override void OnSelect(BaseEventData eventData) { }

        private void OnClickHandler()
        {
            if (_uiController.IsFocused) OnClick();
        }

        protected virtual void OnClick() { }
    }
}

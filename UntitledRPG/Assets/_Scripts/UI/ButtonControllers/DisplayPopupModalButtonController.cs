using UnityEngine;
using UnityEngine.Events;
using ReiBrary.UI;
using ReiBrary.UI.Controllers.Selectables.Buttons;
using ReiBrary.UI.Modals;

namespace Template.UI.ButtonControllers
{
    public class DisplayPopupModalButtonController : AButtonController
    {
        [SerializeField] private UIPage _modalPage;

        [SerializeField] private string _modalTitle = "";
        [SerializeField] private string _modalDescription = "";

        public UnityEvent OnModalYesEvent = new UnityEvent();
        public UnityEvent OnModalNoEvent = new UnityEvent();

        protected override void OnClick()
        {
            var modal = (ModalUIController) _canvasController.GetUI(_modalPage);
            if (modal == null) 
                return;

            modal.DisplayModal(_modalTitle, _modalDescription, OnModalYesEvent.Invoke, OnModalNoEvent.Invoke);
        }
    }
}

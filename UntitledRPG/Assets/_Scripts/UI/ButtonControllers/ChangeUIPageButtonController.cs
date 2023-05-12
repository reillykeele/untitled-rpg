using ReiBrary.UI;
using ReiBrary.UI.Controllers.Selectables.Buttons;

namespace Template.UI.ButtonControllers
{
    public class ChangeUIPageButtonController : AButtonController
    {
        public UIPage TargetUiPage;

        protected override void OnClick()
        {
            _canvasController.SwitchUI(TargetUiPage);
        }
    }
}
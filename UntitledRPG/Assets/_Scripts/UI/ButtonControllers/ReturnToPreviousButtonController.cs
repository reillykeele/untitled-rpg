using ReiBrary.UI.Controllers.Selectables.Buttons;

namespace Template.UI.ButtonControllers
{
    public class ReturnToPreviousButtonController : AButtonController
    {
        protected override void OnClick()
        {
            _canvasController.ReturnToPrevious();
        }
    }
}

using ReiBrary.Systems;
using ReiBrary.UI.Controllers.Selectables.Buttons;

namespace Template.UI.ButtonControllers
{
    public class ResumeGameButtonController : AButtonController
    {
        protected override void OnClick() => GameSystem.Instance.ResumeGame();
    }
}

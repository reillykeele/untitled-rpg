using ReiBrary.Enums;
using ReiBrary.Systems;
using ReiBrary.UI.Controllers.Selectables.Buttons;

namespace Template.UI.ButtonControllers
{
    public class SetGameStateButtonController : AButtonController
    {
        public GameState TargetGameState;

        protected override void OnClick()
        {
            GameSystem.Instance.ChangeGameState(TargetGameState);
        }
    }
}
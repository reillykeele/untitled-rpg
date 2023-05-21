using ReiBrary.Systems;
using ReiBrary.UI.Controllers.Selectables.Buttons;

namespace UntitledRPG.UI.ButtonControllers
{
    public class QuitButtonController : AButtonController
    {
        protected override void OnClick()
        {
            // _canvasAudioController?.FadeOutBackgroundMusic();
            LoadingSystem.Instance.QuitGame();
        }
    }
}
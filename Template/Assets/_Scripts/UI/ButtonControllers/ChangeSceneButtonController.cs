using Util.Coroutine;
using Util.Systems;
using Util.UI.Controllers.Selectables.Buttons;

namespace Template.UI.ButtonControllers
{
    public class ChangeSceneButtonController : AButtonController
    {
        public string TargetScene;
        public float Delay = 0f;

        protected override void OnClick()
        {
            // _canvasAudioController?.FadeOutBackgroundMusic();
            StartCoroutine(CoroutineUtil.WaitForExecute(() => LoadingSystem.Instance.LoadSceneCoroutine(TargetScene), Delay));
        }
    }
}

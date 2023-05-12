using UnityEngine.EventSystems;
using UnityEngine;
using ReiBrary.Audio;
using ReiBrary.Systems;
using ReiBrary.UI.Controllers.Selectables.Buttons;

namespace Template.UI.ButtonControllers
{
    public class PlaySoundOnClickButtonController : AButtonController
    {
        [SerializeField] private AudioSoundSO _audioSound;

        protected override void OnClick() => AudioSystem.Instance.PlayAudioSound(_audioSound);
    }
}

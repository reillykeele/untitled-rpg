using ReiBrary.Audio;
using ReiBrary.Systems;
using ReiBrary.UI.Controllers.Selectables;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UntitledRPG.UI.SelectableControllers
{
    public class PlaySoundOnSelectSelectableController : ASelectableController
    {
        [SerializeField] private AudioSoundSO _audioSound;

        public override void OnSelect(BaseEventData eventData) => AudioSystem.Instance.PlayAudioSound(_audioSound);
    }
}

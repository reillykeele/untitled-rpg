using UnityEngine;
using UnityEngine.EventSystems;
using ReiBrary.Audio;
using ReiBrary.Systems;
using ReiBrary.UI.Controllers.Selectables;

namespace Template.UI.SelectableControllers
{
    public class PlaySoundOnSelectSelectableController : ASelectableController
    {
        [SerializeField] private AudioSoundSO _audioSound;

        public override void OnSelect(BaseEventData eventData) => AudioSystem.Instance.PlayAudioSound(_audioSound);
    }
}

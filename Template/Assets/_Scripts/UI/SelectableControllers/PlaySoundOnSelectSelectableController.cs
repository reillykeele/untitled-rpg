using UnityEngine;
using UnityEngine.EventSystems;
using Util.Audio;
using Util.Systems;
using Util.UI.Controllers.Selectables;

namespace Template.UI.SelectableControllers
{
    public class PlaySoundOnSelectSelectableController : ASelectableController
    {
        [SerializeField] private AudioSoundSO _audioSound;

        public override void OnSelect(BaseEventData eventData) => AudioSystem.Instance.PlayAudioSound(_audioSound);
    }
}

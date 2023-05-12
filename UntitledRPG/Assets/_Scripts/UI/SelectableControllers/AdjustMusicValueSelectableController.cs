using UnityEngine;
using UnityEngine.EventSystems;
using ReiBrary.Enums;
using ReiBrary.Systems;

namespace Template.UI.SelectableControllers
{
    public class AdjustMusicValueSelectableController : AdjustValueSelectableController
    {
        [SerializeField] 
        private AudioMixerGroupVolumeType _mixerGroup;

        protected override void Start()
        {
            _value = GetVolume() + 80;

            OnValueChangedEvent.Invoke("" + _value);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            _inputReader.MenuNavigateEvent += OnNavigate;
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            _inputReader.MenuNavigateEvent -= OnNavigate;
        }

        protected override void Increase()
        {
            var newVol = Mathf.Min(20, GetVolume() + _incrementValue);
            SetVolume(newVol);

            OnValueChangedEvent.Invoke("" + _value);
        }

        protected override void Decrease()
        {
            var newVol = Mathf.Max(-80, GetVolume() - _incrementValue);
            SetVolume(newVol);

            OnValueChangedEvent.Invoke("" + _value);
        }

        private float GetVolume()
        {
            var volName = _mixerGroup.ToString();
            AudioSystem.Instance.Mixer.GetFloat(volName, out var vol);
            return vol;
        }

        private void SetVolume(float vol)
        {
            var volName = _mixerGroup.ToString();
            AudioSystem.Instance.Mixer.SetFloat(volName, vol);

            _value = vol + 80f;
        }
    }
}

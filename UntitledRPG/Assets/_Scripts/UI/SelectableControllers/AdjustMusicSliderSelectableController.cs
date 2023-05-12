using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReiBrary.Enums;
using ReiBrary.Systems;

namespace Template.UI.SelectableControllers
{
    public class AdjustMusicSliderSelectableController : AdjustValueSelectableController
    {
        [SerializeField] private AudioMixerGroupVolumeType _mixerGroup;

        [SerializeField] private Slider _slider;

        private float _direction;
        
        protected override void Start()
        {
            var vol = AudioSystem.Instance.GetMixerVolume(_mixerGroup.ToString());
            _slider.value = vol;

            _slider.onValueChanged.AddListener(SetVolumeFromSlider);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            StopNav();
            Unbind();
        }

        public override void OnSelect(BaseEventData eventData) => Bind();

        public override void OnDeselect(BaseEventData eventData)
        {
            StopNav();
            Unbind();
        }

        private void Bind()
        {
            _inputReader.MenuNavigateEvent += StartNav;
            _inputReader.MenuNavigateCancelledEvent += StopNav;
        }

        private void Unbind()
        {
            _inputReader.MenuNavigateEvent -= StartNav;
            _inputReader.MenuNavigateCancelledEvent -= StopNav;
        }

        void Update()
        {
            if (_direction != 0)
            {
                _slider.value += _direction * _incrementValue * Time.deltaTime;
            }
        }

        private void StartNav(Vector2 nav)
        {
            if (nav == Vector2.left || nav == Vector2.right)
                _direction = nav.x;
        }

        private void StopNav()
        {
            _direction = 0f;
        }

        private void SetVolumeFromSlider(float value)
        {
            AudioSystem.Instance.SetMixerVolume(_mixerGroup.ToString(), value);
        }
    }
}
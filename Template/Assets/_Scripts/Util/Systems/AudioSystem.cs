using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using Util.Attributes;
using Util.Enums;
using Util.Input;
using Util.Singleton;
using UnityEngine.Rendering;
using Util.Audio;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Util.Systems
{
    /// <summary>
    /// Controls the flow of the game's system and the application's state.
    /// </summary>
    public class AudioSystem : Singleton<AudioSystem>
    {
        [Header("Audio")]
        public AudioMixer Mixer;

        [SerializeField] private AudioStreamSO[] _audioStreams;
        private Hashtable _streamHashtable;

        protected override void Awake()
        {
            base.Awake();

            _streamHashtable = new Hashtable();
            RegisterAudioStreams();
        }

        void Start()
        {
            // Load mixer group volumes from preferences
            foreach (var mixerGroup in Mixer.FindMatchingGroups(""))
            {
                var mixerGroupProperty = $"{mixerGroup.name}Volume";
                var prefVolume = PlayerPrefs.GetFloat(mixerGroupProperty, 1.0f);

                Mixer.SetFloat(mixerGroupProperty, AudioHelper.PercentToMixerVolume(prefVolume));
            }
        }

        public void PlayAudioSound(AudioSoundSO sound)
        {
            var audioSource = GetStream(sound.AudioStream);
            audioSource.PlayOneShot(sound.AudioClip);
        }

        /// <summary>
        /// Returns the mixer group's volume.
        /// </summary>
        /// <param name="mixerGroup">The name of the mixer group.</param>
        /// <returns>The volume percentage as a float between 0 and 1.</returns>
        public float GetMixerVolume(string mixerGroup)
        {
            Mixer.GetFloat(mixerGroup, out var mixerVolume);

            var volume = AudioHelper.MixerVolumeToPercent(mixerVolume);

            return volume;
        }

        /// <summary>
        /// Sets the mixer group's volume and saves it to player preferences.
        /// </summary>
        /// <param name="mixerGroup">The name of the mixer group.</param>
        /// <param name="volume">The volume percentage as a float between 0 and 1.</param>
        public void SetMixerVolume(string mixerGroup, float volume)
        {
            var mixerVolume = AudioHelper.PercentToMixerVolume(volume);

            Mixer.SetFloat(mixerGroup, mixerVolume);

            PlayerPrefs.SetFloat($"{mixerGroup}", volume);
        }

        private AudioSource GetStream(AudioStreamSO stream) => (AudioSource) _streamHashtable[stream.id];

        private void RegisterAudioStreams()
        {
            foreach (var stream in _audioStreams)
            {
                // Create new audio source
                var audioSource = gameObject.AddComponent<AudioSource>();

                audioSource.outputAudioMixerGroup = stream.MixerGroup;

                _streamHashtable.Add(stream.id, audioSource);
            }
        }
    }
}
 
using UnityEngine;
using UnityEngine.Audio;
using Util.Attributes;

namespace Util.Audio
{
    [CreateAssetMenu(fileName = "Stream", menuName = "Audio/Stream")]
    public class AudioStreamSO : ScriptableObject
    {
        [UniqueIdentifier] public string id;
        public AudioMixerGroup MixerGroup;
    }
}

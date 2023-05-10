using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Util.AddressableUtil
{
    public class AssetReferenceAudioClip : AssetReferenceT<AudioClip>
    {
        public AssetReferenceAudioClip(string guid) : base(guid) { }
    }
}
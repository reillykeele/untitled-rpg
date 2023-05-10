using UnityEngine;
using Util.Attributes;

namespace Util.UI
{
    [CreateAssetMenu(fileName = "Page", menuName = "UI/UI Page")]
    public class UIPage : ScriptableObject
    {
        [UniqueIdentifier] public string id;
    }
}

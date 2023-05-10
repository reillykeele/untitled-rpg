using UnityEngine;
using UnityEngine.UI;

namespace Util.UI.MultiTarget
{
    public class MultiTargetGraphic : MonoBehaviour
    {
        [SerializeField] 
        private Graphic[] targetGraphics;

        public Graphic[] GetTargetGraphics => targetGraphics;
    }
}

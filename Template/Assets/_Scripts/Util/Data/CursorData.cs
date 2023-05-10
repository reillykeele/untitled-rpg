using System;
using UnityEngine;
using Util.Enums;

namespace Util.Data
{
    [CreateAssetMenu(fileName = "CursorData", menuName = "Configuration/Cursor Data")]
    public class CursorData : UnityEngine.ScriptableObject
    {
        public Texture2D CursorTexture2D;
        public CursorHotspotType HotspotType;
        public Vector2 HotspotLocation;

        public Vector2 GetHotspot()
        {
            return HotspotType switch
            {
                CursorHotspotType.TopLeft => Vector2.zero,
                CursorHotspotType.TopRight => new Vector2(CursorTexture2D.width, 0),
                CursorHotspotType.BottomLeft => new Vector2(0, CursorTexture2D.height),
                CursorHotspotType.BottomRight => new Vector2(CursorTexture2D.width, CursorTexture2D.height),
                CursorHotspotType.Center => new Vector2(CursorTexture2D.width / 2, CursorTexture2D.height / 2),
                CursorHotspotType.Custom => HotspotLocation,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
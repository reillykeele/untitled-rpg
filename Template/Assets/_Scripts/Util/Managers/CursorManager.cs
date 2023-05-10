using UnityEngine;
using Util.Data;
using Util.Enums;
using Util.Singleton;
using Util.Systems;

namespace Util.Managers
{
    public class CursorManager : Singleton<CursorManager>
    {
        [Header("Configuration")] 
        [SerializeField] private CursorMode CursorMode = CursorMode.ForceSoftware;
        [SerializeField] private CursorLockMode LockMode = CursorLockMode.None;
        [SerializeField] private bool DisplayCursorOnLoadScreen = false;

        [Header("Cursor Sprites")]
        [SerializeField] private CursorData _menuCursor;

        // private bool _isVisible = true;

        void Start()
        {
            Cursor.lockState = LockMode;

            if (DisplayCursorOnLoadScreen == false)
            {
                LoadingSystem.Instance.OnLoadStartEvent.AddListener(HideCursor);
                LoadingSystem.Instance.OnLoadEndEvent.AddListener(ShowCursor);
            }

            GameSystem.Instance.OnGameStateChangeEvent.AddListener(OnGameStateChanged);

            OnGameStateChanged(GameSystem.Instance.CurrentGameState);
        }

        public void ShowCursor()
        {
            Cursor.visible = true;
        }

        public void HideCursor()
        {
            Cursor.visible = false;
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state is GameState.Menu or GameState.Paused)
            {
                // Display menu cursor
                SetCursor(_menuCursor);
            }
        }

        private void SetCursor(CursorData cursor)
        {
            Cursor.SetCursor(cursor.CursorTexture2D, cursor.GetHotspot(), CursorMode);
        }
        
    }
}

using UnityEngine;
using UnityEngine.Events;

namespace Util.Input
{
    public interface IMenuInputReader
    {
        public event UnityAction<Vector2> MenuNavigateEvent;
        public event UnityAction MenuNavigateCancelledEvent;
        public event UnityAction MenuUnpauseEvent;
        public event UnityAction MenuAcceptButtonEvent;
        public event UnityAction MenuCancelButtonEvent;
        public event UnityAction<int> ChangeTabEvent;

        public void EnableMenuInput();
    }
}
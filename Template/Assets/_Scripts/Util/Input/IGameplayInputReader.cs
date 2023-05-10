using UnityEngine.Events;

namespace Util.Input
{
    public interface IGameplayInputReader
    {
        public event UnityAction MenuPauseEvent;

        public void EnableGameplayInput();
    }
}
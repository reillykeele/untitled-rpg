using UntitledRPG.Actor.Interactable;

namespace UntitledRPG.Actor.Interface
{
    public interface IRangedInteracter
    {
        void Subscribe(RangedInteractable interactable);
        void Unsubscribe(RangedInteractable interactable);
    }
}
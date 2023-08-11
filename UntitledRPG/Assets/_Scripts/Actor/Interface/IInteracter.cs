namespace UntitledRPG.Actor.Interface
{
    public interface IInteracter
    {
        void Subscribe(IInteractable interactable);
        void Unsubscribe(IInteractable interactable);
    }
}
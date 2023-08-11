namespace UntitledRPG.Actor.Interface
{
    public interface IInteractable
    {
        int Priority { get; }

        void Interact();
        void HighlightInteractable();
    }
}

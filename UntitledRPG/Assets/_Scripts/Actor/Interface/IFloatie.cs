using UntitledRPG.Environment;

namespace UntitledRPG.Actor.Interface
{
    public interface IFloatie
    {
        void SubscribeToWater(Water water);
        void UnsubscribeToWater(Water water);
    }
}

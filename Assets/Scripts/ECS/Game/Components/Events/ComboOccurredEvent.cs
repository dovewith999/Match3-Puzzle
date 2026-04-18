using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct ComboOccurredEvent : IComponentData
    {
        public int ComboCount;
    }

    public struct NoScoreElapsedEvent : IComponentData
    {
        public float ElapsedSeconds;
    }
}

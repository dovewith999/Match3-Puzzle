using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct ScoreChangedEvent : IComponentData
    {
        public int NewScore;
    }
}

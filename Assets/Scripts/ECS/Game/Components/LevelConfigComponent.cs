using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct LevelConfigComponent : IComponentData
    {
        public int TimeInSeconds;
        public int Score1Star;
        public int Score2Star;
        public int Score3Star;
    }
}


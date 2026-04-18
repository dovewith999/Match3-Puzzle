using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct ScoreComponent : IComponentData
    {
        public int CurrentScore;
        public int ComboCount;
        public float ComboMultiplier;
        public float TimeSinceLastScore;
    }
}


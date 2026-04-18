using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct TimerTickEvent : IComponentData
    {
        public float RemainingSeconds;
        public int DisplaySeconds;
    }

    public struct LowTimeWarningEvent : IComponentData
    {
        public float RemainingSeconds;
    }
}

using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct TimerComponent : IComponentData
    {
        public float RemainingSeconds;
        public bool IsRunning;
        public bool IsExpired;
    }
}

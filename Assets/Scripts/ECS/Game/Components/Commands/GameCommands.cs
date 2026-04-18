using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct GameStartCommand : IComponentData { }

    public struct GamePauseCommand : IComponentData
    {
        public bool IsPaused;
    }

    public struct GameRetryCommand : IComponentData { }
}

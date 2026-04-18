using Unity.Entities;

namespace Match3.ECS.UI
{
    public struct HudStateComponent : IComponentData
    {
        public int DisplayScore;
        public int DisplayRemaining;
        public int DisplayTarget;
        public int StarIndex;
    }
}

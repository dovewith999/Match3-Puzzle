using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct SwapRequestComponent : IComponentData
    {
        public int FromX;
        public int FromY;
        public int ToX;
        public int ToY;
        public byte IsRollback; // 1 = true (Burst blittable)
    }
}

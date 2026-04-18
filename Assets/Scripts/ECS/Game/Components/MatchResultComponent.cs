using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct MatchResultComponent : IComponentData
    {
        public int X;
        public int Y;
        public int HorizontalCount;
        public int VerticalCount;
        public ColorTypeECS Color;
    }
}

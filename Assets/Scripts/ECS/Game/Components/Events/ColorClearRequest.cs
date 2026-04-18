using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct ColorClearRequest : IComponentData
    {
        public ColorTypeECS TargetColor;
        public bool IsFromSpecialPiece;
    }
}

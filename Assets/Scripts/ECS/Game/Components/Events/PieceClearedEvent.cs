using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct PieceClearedEvent : IComponentData
    {
        public int X;
        public int Y;
        public PieceTypeECS PieceType;
    }
}

using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct SpecialPieceHoldComponent : IComponentData
    {
        public int PieceX;
        public int PieceY;
        public bool IsHolding;
    }

    public struct SpecialPieceDragDirection : IComponentData
    {
        public DragDirectionECS Direction;
    }

    public enum DragDirectionECS : byte
    {
        None,
        Up,
        Down,
        Left,
        Right,
    }
}

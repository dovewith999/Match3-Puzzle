using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct PieceComponent : IComponentData
    {
        public int X;
        public int Y;
        public PieceTypeECS PieceType;
        public ColorTypeECS ColorType;
    }

    public enum PieceTypeECS : byte
    {
        Empty,
        Normal,
        Special,
    }

    public enum ColorTypeECS : byte
    {
        Yellow,
        Purple,
        Red,
        Blue,
        Green,
        Pink,
        None,
    }
}

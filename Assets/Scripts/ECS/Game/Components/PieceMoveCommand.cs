using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct PieceMoveCommand : IComponentData
    {
        public Entity TargetEntity;
        public int ToX;
        public int ToY;
        public bool IsRollback;
    }
}

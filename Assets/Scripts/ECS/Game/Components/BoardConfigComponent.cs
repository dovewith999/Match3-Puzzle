using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct BoardConfigComponent : IComponentData
    {
        public int XDim;
        public int YDim;
        public float FillTime;
    }
}

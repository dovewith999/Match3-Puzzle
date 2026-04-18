using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct GameResultEvent : IComponentData
    {
        public bool IsWin;
        public int FinalScore;
        public int StarCount;
    }
}

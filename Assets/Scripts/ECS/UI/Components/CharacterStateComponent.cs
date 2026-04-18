using Unity.Entities;

namespace Match3.ECS.UI
{
    public struct CharacterStateComponent : IComponentData
    {
        public CharacterExpressionECS Expression;
        public float ExpressionTimer;
    }

    public enum CharacterExpressionECS : byte
    {
        Idle,
        Happy,
        Excited,
        Hyper,
        Bored,
        Nervous,
    }
}

using Unity.Entities;

namespace Match3.ECS.Game
{
    public struct ComboComponent : IComponentData
    {
        public int Count;
        public float Multiplier;
        public float TimeSinceLastMatch;

        public static float GetMultiplier(int comboCount)
        {
            if (comboCount >= 5)
            {
                return 3.0f;
            }

            if (comboCount >= 3)
            {
                return 2.0f;
            }

            if (comboCount == 2)
            {
                return 1.2f;
            }

            return 1.0f;
        }

        public static int GetBaseScore(int matchCount)
        {
            if (matchCount >= 5)
            {
                return 300;
            }

            if (matchCount == 4)
            {
                return 150;
            }

            return 60;
        }
    }
}

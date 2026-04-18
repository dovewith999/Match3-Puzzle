using Match3.ECS.Game;
using Unity.Entities;
using UnityEngine;

namespace Match3.ECS.Authoring
{
    public class GameConfigAuthoring : MonoBehaviour
    {
        [Header("Level Config")]
        public int timeInSeconds = 60;
        public int score1Star = 500;
        public int score2Star = 1000;
        public int score3Star = 1500;

        public class Baker : Baker<GameConfigAuthoring>
        {
            public override void Bake(GameConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new LevelConfigComponent
                {
                    TimeInSeconds = authoring.timeInSeconds,
                    Score1Star = authoring.score1Star,
                    Score2Star = authoring.score2Star,
                    Score3Star = authoring.score3Star,
                });

                AddComponent(entity, new TimerComponent
                {
                    RemainingSeconds = authoring.timeInSeconds,
                    IsRunning = false,
                    IsExpired = false,
                });

                AddComponent(entity, new ScoreComponent
                {
                    CurrentScore = 0,
                    ComboCount = 0,
                    ComboMultiplier = 1.0f,
                    TimeSinceLastScore = 0f,
                });

                AddComponent(entity, new ComboComponent
                {
                    Count = 0,
                    Multiplier = 1.0f,
                    TimeSinceLastMatch = 0f,
                });
            }
        }
    }
}

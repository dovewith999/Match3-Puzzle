using Unity.Burst;
using Unity.Entities;

namespace Match3.ECS.Game
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(MatchDetectionSystem))]
    public partial struct ScoreSystem : ISystem
    {
        private const float NoScoreThreshold = 5f;

        private bool _noScoreEventSent;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ScoreComponent>();
            state.RequireForUpdate<ComboComponent>();
            _noScoreEventSent = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var combo = SystemAPI.GetSingleton<ComboComponent>();
            var score = SystemAPI.GetSingleton<ScoreComponent>();

            combo.TimeSinceLastMatch += deltaTime;
            score.TimeSinceLastScore += deltaTime;

            bool hasAnyMatch = false;

            foreach (var match in SystemAPI.Query<RefRO<MatchResultComponent>>())
            {
                hasAnyMatch = true;

                int totalCount = match.ValueRO.HorizontalCount + match.ValueRO.VerticalCount;
                int matchCount = totalCount > 0 ? totalCount : 3;

                combo.Count++;
                combo.TimeSinceLastMatch = 0f;
                combo.Multiplier = ComboComponent.GetMultiplier(combo.Count);

                int baseScore = ComboComponent.GetBaseScore(matchCount);
                int earned = (int)(baseScore * combo.Multiplier);

                score.CurrentScore += earned;
                score.TimeSinceLastScore = 0f;
                _noScoreEventSent = false;

                var ecb = SystemAPI
                    .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged);

                var scoreEvent = ecb.CreateEntity();
                ecb.AddComponent(scoreEvent, new ScoreChangedEvent
                {
                    NewScore = score.CurrentScore,
                });

                var comboEvent = ecb.CreateEntity();
                ecb.AddComponent(comboEvent, new ComboOccurredEvent
                {
                    ComboCount = combo.Count,
                });
            }

            if (!hasAnyMatch)
            {
                combo.Count = 0;
                combo.Multiplier = 1.0f;

                if (!_noScoreEventSent && score.TimeSinceLastScore >= NoScoreThreshold)
                {
                    _noScoreEventSent = true;
                    PublishNoScoreEvent(ref state);
                }
            }

            SystemAPI.SetSingleton(combo);
            SystemAPI.SetSingleton(score);
        }

        [BurstCompile]
        private void PublishNoScoreEvent(ref SystemState state)
        {
            var ecb = SystemAPI
                .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new NoScoreElapsedEvent
            {
                ElapsedSeconds = NoScoreThreshold,
            });
        }
    }
}

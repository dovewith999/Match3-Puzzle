using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Match3.ECS.Game
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct LevelRuleSystem : ISystem
    {
        private const float LowTimeThreshold = 5f;

        private bool _lowTimeEventSent;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LevelConfigComponent>();
            state.RequireForUpdate<TimerComponent>();
            state.RequireForUpdate<ScoreComponent>();
            _lowTimeEventSent = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timer = SystemAPI.GetSingleton<TimerComponent>();

            if (!timer.IsRunning || timer.IsExpired)
            {
                return;
            }

            var remaining = timer.RemainingSeconds - SystemAPI.Time.DeltaTime;
            remaining = math.max(remaining, 0f);

            var ecb = SystemAPI
                .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            PublishTimerTick(ref ecb, remaining);

            if (!_lowTimeEventSent && remaining <= LowTimeThreshold)
            {
                _lowTimeEventSent = true;
                PublishLowTimeWarning(ref ecb, remaining);
            }

            if (remaining <= 0f)
            {
                PublishGameResult(ref state, ref ecb);
            }

            SystemAPI.SetSingleton(new TimerComponent
            {
                RemainingSeconds = remaining,
                IsRunning = remaining > 0f,
                IsExpired = remaining <= 0f,
            });
        }

        [BurstCompile]
        private static void PublishTimerTick(ref EntityCommandBuffer ecb, float remaining)
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new TimerTickEvent
            {
                RemainingSeconds = remaining,
                DisplaySeconds = (int)math.ceil(remaining),
            });
        }

        [BurstCompile]
        private static void PublishLowTimeWarning(ref EntityCommandBuffer ecb, float remaining)
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new LowTimeWarningEvent
            {
                RemainingSeconds = remaining,
            });
        }

        [BurstCompile]
        private void PublishGameResult(ref SystemState state, ref EntityCommandBuffer ecb)
        {
            var score = SystemAPI.GetSingleton<ScoreComponent>();
            var config = SystemAPI.GetSingleton<LevelConfigComponent>();

            int starCount = 0;

            if (score.CurrentScore >= config.Score3Star)
            {
                starCount = 3;
            }
            else if (score.CurrentScore >= config.Score2Star)
            {
                starCount = 2;
            }
            else if (score.CurrentScore >= config.Score1Star)
            {
                starCount = 1;
            }

            var resultEntity = ecb.CreateEntity();
            ecb.AddComponent(resultEntity, new GameResultEvent
            {
                IsWin = score.CurrentScore >= config.Score1Star,
                FinalScore = score.CurrentScore,
                StarCount = starCount,
            });
        }
    }
}

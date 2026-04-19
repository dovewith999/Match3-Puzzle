using Unity.Burst;
using Unity.Entities;

namespace Match3.ECS.Game
{
    // MatchDetectionSystem 이후에 실행되어 매칭 결과가 있는지 확인합니다.
    // 매칭이 없으면 LastSwapComponent에 기록된 스왑을 역방향으로 롤백합니다.
    // daltonbr/Match3의 GameGrid.SwapPieces()에서 match 없을 때 원위치하는 로직과 동일합니다.
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(MatchDetectionSystem))]
    [UpdateBefore(typeof(ScoreSystem))]
    public partial struct SwapValidationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LastSwapComponent>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var lastSwap = SystemAPI.GetSingleton<LastSwapComponent>();

            // 검증 대기 중인 스왑이 없으면 스킵
            if (lastSwap.HasPendingSwap == 0)
            {
                return;
            }

            var matchQuery = SystemAPI.QueryBuilder()
                .WithAll<MatchResultComponent>()
                .Build();

            // 매칭이 있으면 유효한 스왑 → 플래그만 해제하고 종료
            if (!matchQuery.IsEmpty)
            {
                SystemAPI.SetSingleton(new LastSwapComponent { HasPendingSwap = 0 });
                return;
            }

            // 매칭이 없으면 역방향 롤백 스왑 요청 발행
            var ecb = SystemAPI
                .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var rollback = ecb.CreateEntity();
            ecb.AddComponent(rollback, new SwapRequestComponent
            {
                FromX      = lastSwap.ToX,
                FromY      = lastSwap.ToY,
                ToX        = lastSwap.FromX,
                ToY        = lastSwap.FromY,
                IsRollback = 1,
            });

            SystemAPI.SetSingleton(new LastSwapComponent { HasPendingSwap = 0 });
        }
    }
}

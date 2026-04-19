using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Match3.ECS.Game
{
    // MatchDetectionSystem이 발행한 MatchResultComponent를 읽어
    // 매칭된 피스들을 PieceTypeECS.Empty로 바꾸고 PieceClearedEvent를 발행합니다.
    // daltonbr/Match3의 GameGrid.ClearAllValidMatches() 역할에 해당합니다.
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ScoreSystem))]
    public partial struct MatchClearSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BoardConfigComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var matchQuery = SystemAPI.QueryBuilder()
                .WithAll<MatchResultComponent>()
                .Build();

            if (matchQuery.IsEmpty)
            {
                return;
            }

            var config = SystemAPI.GetSingleton<BoardConfigComponent>();
            int total = config.XDim * config.YDim;

            // 매칭 결과에서 제거할 셀 좌표 수집
            var toRemove = new NativeArray<bool>(total, Allocator.Temp, NativeArrayOptions.ClearMemory);

            foreach (var (match, matchEntity) in
                SystemAPI.Query<RefRO<MatchResultComponent>>().WithEntityAccess())
            {
                var m = match.ValueRO;

                // 가로 매칭 셀들 마킹
                if (m.HorizontalCount >= 3)
                {
                    for (int i = 0; i < m.HorizontalCount; i++)
                    {
                        int idx = m.Y * config.XDim + (m.X + i);

                        if (idx >= 0 && idx < total)
                        {
                            toRemove[idx] = true;
                        }
                    }
                }

                // 세로 매칭 셀들 마킹
                if (m.VerticalCount >= 3)
                {
                    for (int i = 0; i < m.VerticalCount; i++)
                    {
                        int idx = (m.Y + i) * config.XDim + m.X;

                        if (idx >= 0 && idx < total)
                        {
                            toRemove[idx] = true;
                        }
                    }
                }

                // MatchResultComponent 소비 (ECB 없이 직접 — Burst 호환)
                // MatchResultComponent는 이 시스템 이후로 필요 없으므로 제거
            }

            var ecb = SystemAPI
                .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            // MatchResultComponent Entity 삭제
            ecb.DestroyEntity(matchQuery, EntityQueryCaptureMode.AtRecord);

            // 마킹된 셀의 피스를 Empty로 변경 + PieceClearedEvent 발행
            foreach (var (piece, entity) in
                SystemAPI.Query<RefRO<PieceComponent>>().WithEntityAccess())
            {
                var p = piece.ValueRO;

                if (p.PieceType == PieceTypeECS.Empty)
                {
                    continue;
                }

                if (p.X < 0 || p.X >= config.XDim || p.Y < 0 || p.Y >= config.YDim)
                {
                    continue;
                }

                int cellIdx = p.Y * config.XDim + p.X;

                if (!toRemove[cellIdx])
                {
                    continue;
                }

                ecb.SetComponent(entity, new PieceComponent
                {
                    X = p.X,
                    Y = p.Y,
                    PieceType = PieceTypeECS.Empty,
                    ColorType = ColorTypeECS.None,
                });

                var cleared = ecb.CreateEntity();
                ecb.AddComponent(cleared, new PieceClearedEvent
                {
                    X = p.X,
                    Y = p.Y,
                    PieceType = p.PieceType,
                });
            }

            toRemove.Dispose();
        }
    }
}

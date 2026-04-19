using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Match3.ECS.Game
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(MatchDetectionSystem))]
    public partial struct SwapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BoardConfigComponent>();
            state.RequireForUpdate<LastSwapComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var swapQuery = SystemAPI.QueryBuilder()
                .WithAll<SwapRequestComponent>()
                .Build();

            if (swapQuery.IsEmpty)
            {
                return;
            }

            var ecb = SystemAPI
                .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var config = SystemAPI.GetSingleton<BoardConfigComponent>();
            int total  = config.XDim * config.YDim;

            var pieces   = new NativeArray<PieceComponent>(total, Allocator.Temp);
            var entities = new NativeArray<Entity>(total, Allocator.Temp);

            foreach (var (piece, entity) in
                SystemAPI.Query<RefRO<PieceComponent>>().WithEntityAccess())
            {
                var p = piece.ValueRO;

                if (p.X < 0 || p.X >= config.XDim || p.Y < 0 || p.Y >= config.YDim)
                {
                    continue;
                }

                int idx    = p.Y * config.XDim + p.X;
                pieces[idx]   = p;
                entities[idx] = entity;
            }

            foreach (var (request, requestEntity) in
                SystemAPI.Query<RefRO<SwapRequestComponent>>().WithEntityAccess())
            {
                var req = request.ValueRO;

                // 롤백 스왑(IsRollback==1)이 아닐 때만 LastSwapComponent에 기록
                if (req.IsRollback == 0)
                {
                    SystemAPI.SetSingleton(new LastSwapComponent
                    {
                        FromX          = req.FromX,
                        FromY          = req.FromY,
                        ToX            = req.ToX,
                        ToY            = req.ToY,
                        HasPendingSwap = 1,
                    });
                }

                ExecuteSwap(ref ecb, pieces, entities, config, req);
                ecb.DestroyEntity(requestEntity);
            }

            pieces.Dispose();
            entities.Dispose();
        }

        [BurstCompile]
        private static void ExecuteSwap(
            ref EntityCommandBuffer ecb,
            in NativeArray<PieceComponent> pieces,
            in NativeArray<Entity> entities,
            in BoardConfigComponent config,
            in SwapRequestComponent req)
        {
            int fromIdx = req.FromY * config.XDim + req.FromX;
            int toIdx   = req.ToY   * config.XDim + req.ToX;

            if (fromIdx < 0 || fromIdx >= pieces.Length ||
                toIdx   < 0 || toIdx   >= pieces.Length)
            {
                return;
            }

            var fromPiece = pieces[fromIdx];
            var toPiece   = pieces[toIdx];

            if (fromPiece.PieceType == PieceTypeECS.Empty ||
                toPiece.PieceType   == PieceTypeECS.Empty)
            {
                return;
            }

            ecb.SetComponent(entities[fromIdx], new PieceComponent
            {
                X         = req.ToX,
                Y         = req.ToY,
                PieceType = fromPiece.PieceType,
                ColorType = fromPiece.ColorType,
            });

            ecb.SetComponent(entities[toIdx], new PieceComponent
            {
                X         = req.FromX,
                Y         = req.FromY,
                PieceType = toPiece.PieceType,
                ColorType = toPiece.ColorType,
            });
        }
    }
}

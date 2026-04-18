using Unity.Burst;
using Unity.Entities;

namespace Match3.ECS.Game
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(MatchDetectionSystem))]
    public partial struct ColorClearSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BoardConfigComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            bool hasRequest = false;
            ColorTypeECS targetColor = ColorTypeECS.None;

            foreach (var (request, requestEntity) in SystemAPI.Query<RefRO<ColorClearRequest>>().WithEntityAccess())
            {
                hasRequest = true;
                targetColor = request.ValueRO.TargetColor;

                var ecb = SystemAPI .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>() .CreateCommandBuffer(state.WorldUnmanaged);

                ecb.DestroyEntity(requestEntity);
                break;
            }

            if (!hasRequest)
            {
                return;
            }

            ClearAllMatchingColor(ref state, targetColor);
        }

        [BurstCompile]
        private void ClearAllMatchingColor(ref SystemState state, ColorTypeECS targetColor)
        {
            var ecb = SystemAPI .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>() .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (piece, entity) in SystemAPI.Query<RefRO<PieceComponent>>().WithEntityAccess())
            {
                var p = piece.ValueRO;

                if (p.PieceType != PieceTypeECS.Normal || p.ColorType != targetColor)
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

                var clearedEvent = ecb.CreateEntity();
                ecb.AddComponent(clearedEvent, new PieceClearedEvent
                {
                    X = p.X,
                    Y = p.Y,
                    PieceType = p.PieceType,
                });
            }
        }
    }
}

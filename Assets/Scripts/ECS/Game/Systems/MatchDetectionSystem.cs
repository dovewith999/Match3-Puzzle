using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Match3.ECS.Game
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct MatchDetectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BoardConfigComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<BoardConfigComponent>();
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>() .CreateCommandBuffer(state.WorldUnmanaged);

            var pieces = new NativeArray<PieceComponent>(
                config.XDim * config.YDim, Allocator.Temp);

            foreach (var piece in SystemAPI.Query<RefRO<PieceComponent>>())
            {
                var p = piece.ValueRO;
                if (p.X >= 0 && p.X < config.XDim && p.Y >= 0 && p.Y < config.YDim)
                {
                    pieces[p.Y * config.XDim + p.X] = p;
                }
            }

            ScanAndEmitMatches(ref ecb, pieces, config);

            pieces.Dispose();
        }

        [BurstCompile]
        private static void ScanAndEmitMatches(ref EntityCommandBuffer ecb, in NativeArray<PieceComponent> pieces, in BoardConfigComponent config)
        {
            for (int y = 0; y < config.YDim; y++)
            {
                for (int x = 0; x < config.XDim; x++)
                {
                    var piece = pieces[y * config.XDim + x];

                    if (piece.PieceType != PieceTypeECS.Normal)
                    {
                        continue;
                    }

                    var color = piece.ColorType;
                    int hCount = CountConsecutive(pieces, config, x, y, 1, 0, color);
                    int vCount = CountConsecutive(pieces, config, x, y, 0, 1, color);

                    bool hMatch = hCount >= 3;
                    bool vMatch = vCount >= 3;

                    if (!hMatch && !vMatch)
                    {
                        continue;
                    }

                    var matchEntity = ecb.CreateEntity();
                    ecb.AddComponent(matchEntity, new MatchResultComponent
                    {
                        X = x,
                        Y = y,
                        HorizontalCount = hMatch ? hCount : 0,
                        VerticalCount = vMatch ? vCount : 0,
                        Color = color,
                    });
                }
            }
        }

        [BurstCompile]
        private static int CountConsecutive( in NativeArray<PieceComponent> pieces, in BoardConfigComponent config, int startX, int startY, int dx, int dy, ColorTypeECS color)
        {
            int count = 1;
            int nx = startX + dx;
            int ny = startY + dy;

            while (nx < config.XDim && ny < config.YDim)
            {
                var neighbor = pieces[ny * config.XDim + nx];

                if (neighbor.PieceType != PieceTypeECS.Normal || neighbor.ColorType != color)
                {
                    break;
                }

                count++;
                nx += dx;
                ny += dy;
            }

            return count;
        }
    }
}

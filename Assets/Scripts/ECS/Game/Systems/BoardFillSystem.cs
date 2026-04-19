using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Match3.ECS.Game
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColorClearSystem))]
    public partial struct BoardFillSystem : ISystem
    {
        private uint _randomSeed;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BoardConfigComponent>();
            _randomSeed = 12345u;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<BoardConfigComponent>();
            int total = config.XDim * config.YDim;

            var pieces = new NativeArray<PieceComponent>(total, Allocator.Temp);
            var entities = new NativeArray<Entity>(total, Allocator.Temp);

            for (int i = 0; i < total; i++)
            {
                entities[i] = Entity.Null;
            }

            foreach (var (piece, entity) in
                SystemAPI.Query<RefRO<PieceComponent>>().WithEntityAccess())
            {
                var p = piece.ValueRO;

                if (p.X < 0 || p.X >= config.XDim || p.Y < 0 || p.Y >= config.YDim)
                {
                    continue;
                }

                int idx = p.Y * config.XDim + p.X;
                pieces[idx] = p;
                entities[idx] = entity;
            }

            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            var rng = new Random(_randomSeed);

            ApplyGravity(ref ecb, ref pieces, ref entities, in config);
            SpawnNewPieces(ref ecb, ref pieces, ref entities, in config, ref rng);

            _randomSeed = rng.state == 0u ? 1u : rng.state;

            pieces.Dispose();
            entities.Dispose();
        }

        [BurstCompile]
        private static void ApplyGravity(
            ref EntityCommandBuffer ecb,
            ref NativeArray<PieceComponent> pieces,
            ref NativeArray<Entity> entities,
            in BoardConfigComponent config)
        {
            for (int x = 0; x < config.XDim; x++)
            {
                for (int y = config.YDim - 2; y >= 0; y--)
                {
                    int idx = y * config.XDim + x;
                    var piece = pieces[idx];

                    if (piece.PieceType == PieceTypeECS.Empty)
                    {
                        continue;
                    }

                    int belowIdx = (y + 1) * config.XDim + x;
                    var below = pieces[belowIdx];

                    if (below.PieceType != PieceTypeECS.Empty)
                    {
                        continue;
                    }

                    if (entities[idx] == Entity.Null || entities[belowIdx] == Entity.Null)
                    {
                        continue;
                    }

                    var moved = new PieceComponent
                    {
                        X = x,
                        Y = y + 1,
                        PieceType = piece.PieceType,
                        ColorType = piece.ColorType,
                    };

                    ecb.SetComponent(entities[idx], new PieceComponent
                    {
                        X = x,
                        Y = y,
                        PieceType = PieceTypeECS.Empty,
                        ColorType = ColorTypeECS.None,
                    });

                    ecb.SetComponent(entities[belowIdx], moved);

                    pieces[belowIdx] = moved;
                    pieces[idx] = new PieceComponent
                    {
                        X = x,
                        Y = y,
                        PieceType = PieceTypeECS.Empty,
                        ColorType = ColorTypeECS.None,
                    };
                }
            }
        }

        [BurstCompile]
        private static void SpawnNewPieces(
            ref EntityCommandBuffer ecb,
            ref NativeArray<PieceComponent> pieces,
            ref NativeArray<Entity> entities,
            in BoardConfigComponent config,
            ref Random rng)
        {
            const int colorCount = 6;

            for (int x = 0; x < config.XDim; x++)
            {
                int topIdx = x;
                var top = pieces[topIdx];

                if (top.PieceType != PieceTypeECS.Empty)
                {
                    continue;
                }

                if (entities[topIdx] == Entity.Null)
                {
                    continue;
                }

                var color = (ColorTypeECS)rng.NextInt(0, colorCount);

                ecb.SetComponent(entities[topIdx], new PieceComponent
                {
                    X = x,
                    Y = 0,
                    PieceType = PieceTypeECS.Normal,
                    ColorType = color,
                });

                var spawnCmd = ecb.CreateEntity();
                ecb.AddComponent(spawnCmd, new PieceMoveCommand
                {
                    TargetEntity = entities[topIdx],
                    ToX = x,
                    ToY = 0,
                    IsRollback = false,
                });
            }
        }
    }
}

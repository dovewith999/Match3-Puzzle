using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Match3.ECS.Game
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct BoardInitSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoardConfigComponent>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<BoardConfigComponent>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            SpawnEmptySlots(ref ecb, in config);

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            state.Enabled = false;
        }

        public void OnStopRunning(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
        }

        [BurstCompile]
        private static void SpawnEmptySlots(ref EntityCommandBuffer ecb, in BoardConfigComponent config)
        {
            for (int y = 0; y < config.YDim; ++y)
            {
                for (int x = 0; x < config.XDim; ++x)
                {
                    var entity = ecb.CreateEntity();
                    ecb.AddComponent(entity, new PieceComponent
                    {
                        X = x,
                        Y = y,
                        PieceType = PieceTypeECS.Empty,
                        ColorType = ColorTypeECS.None,
                    });
                }
            }
        }
    }
}

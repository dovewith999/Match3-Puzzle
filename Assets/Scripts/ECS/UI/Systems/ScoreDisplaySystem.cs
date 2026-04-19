using Unity.Entities;
using UnityEngine;

namespace Match3.ECS.UI
{
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class ScoreDisplaySystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _ecbSystem;

        protected override void OnCreate()
        {
            _ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
            RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly(typeof(Game.ScoreChangedEvent))));
        }

        protected override void OnUpdate()
        {
            var ecb = _ecbSystem.CreateCommandBuffer();

            foreach (var (scoreEvent, entity) in
                SystemAPI.Query<RefRO<Game.ScoreChangedEvent>>().WithEntityAccess())
            {
                int newScore = scoreEvent.ValueRO.NewScore;
                ecb.DestroyEntity(entity);
                UpdateHud(newScore);
            }
        }

        private static void UpdateHud(int newScore)
        {
            var hud = Object.FindFirstObjectByType<HudBridge>();

            if (hud == null)
            {
                return;
            }

            hud.UpdateScore(newScore);
        }
    }
}

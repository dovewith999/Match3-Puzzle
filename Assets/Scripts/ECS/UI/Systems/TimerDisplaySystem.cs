using Unity.Entities;
using UnityEngine;

namespace Match3.ECS.UI
{
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class TimerDisplaySystem : SystemBase
    {
        private int _lastDisplayed = -1;
        private EndSimulationEntityCommandBufferSystem _ecbSystem;

        protected override void OnCreate()
        {
            _ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
            RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly(typeof(Game.TimerTickEvent))));
        }

        protected override void OnUpdate()
        {
            var ecb = _ecbSystem.CreateCommandBuffer();

            foreach (var (tickEvent, entity) in
                SystemAPI.Query<RefRO<Game.TimerTickEvent>>().WithEntityAccess())
            {
                int displaySeconds = tickEvent.ValueRO.DisplaySeconds;
                ecb.DestroyEntity(entity);

                if (displaySeconds == _lastDisplayed)
                {
                    continue;
                }

                _lastDisplayed = displaySeconds;
                UpdateHud(displaySeconds);
            }
        }

        private static void UpdateHud(int displaySeconds)
        {
            var hud = Object.FindFirstObjectByType<HudBridge>();

            if (hud == null)
            {
                return;
            }

            hud.UpdateTimer(displaySeconds);
        }
    }
}

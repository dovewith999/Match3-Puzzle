using Unity.Entities;
using UnityEngine;

namespace Match3.ECS.UI
{
    [WorldSystemFilter(WorldSystemFilterFlags.Presentation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class TimerDisplaySystem : SystemBase
    {
        private int _lastDisplayed = -1;

        protected override void OnCreate()
        {
            RequireForUpdate(GetEntityQuery(
                ComponentType.ReadOnly(typeof(Game.TimerTickEvent))));
        }

        protected override void OnUpdate()
        {
            foreach (var (tickEvent, entity) in
                SystemAPI.Query<RefRO<Game.TimerTickEvent>>().WithEntityAccess())
            {
                int displaySeconds = tickEvent.ValueRO.DisplaySeconds;
                EntityManager.DestroyEntity(entity);

                if (displaySeconds == _lastDisplayed)
                {
                    continue;
                }

                _lastDisplayed = displaySeconds;

                var hud = Object.FindFirstObjectByType<HudBridge>();

                if (hud == null)
                {
                    continue;
                }

                hud.UpdateTimer(displaySeconds);
            }
        }
    }
}

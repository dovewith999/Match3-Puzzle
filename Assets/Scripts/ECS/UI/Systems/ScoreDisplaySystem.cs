using Unity.Entities;
using UnityEngine;

namespace Match3.ECS.UI
{
    [WorldSystemFilter(WorldSystemFilterFlags.Presentation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class ScoreDisplaySystem : SystemBase
    {
        private Game.ScoreComponent _lastScore;

        protected override void OnCreate()
        {
            RequireForUpdate(GetEntityQuery(
                ComponentType.ReadOnly(typeof(Game.ScoreChangedEvent))));
        }

        protected override void OnUpdate()
        {
            foreach (var (scoreEvent, entity) in
                SystemAPI.Query<RefRO<Game.ScoreChangedEvent>>().WithEntityAccess())
            {
                int newScore = scoreEvent.ValueRO.NewScore;
                OnScoreChanged(newScore);
                EntityManager.DestroyEntity(entity);
            }
        }

        private static void OnScoreChanged(int newScore)
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

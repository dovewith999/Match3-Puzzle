using Unity.Entities;
using UnityEngine;

namespace Match3.ECS.UI
{
    [WorldSystemFilter(WorldSystemFilterFlags.Presentation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class GameOverSystem : SystemBase
    {
        private bool _handled;

        protected override void OnCreate()
        {
            _handled = false;
            RequireForUpdate(GetEntityQuery( ComponentType.ReadOnly(typeof(Game.GameResultEvent))));
        }

        protected override void OnUpdate()
        {
            if (_handled)
            {
                return;
            }

            foreach (var (result, entity) in SystemAPI.Query<RefRO<Game.GameResultEvent>>().WithEntityAccess())
            {
                _handled = true;
                ShowResult(result.ValueRO.FinalScore, result.ValueRO.StarCount);
                EntityManager.DestroyEntity(entity);
                break;
            }
        }

        private static void ShowResult(int finalScore, int starCount)
        {
            var bridge = Object.FindFirstObjectByType<GameOverBridge>();

            if (bridge == null)
            {
                return;
            }

            bridge.Show(finalScore, starCount);
        }
    }
}

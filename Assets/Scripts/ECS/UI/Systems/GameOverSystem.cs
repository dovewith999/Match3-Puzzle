using Unity.Entities;
using UnityEngine;

namespace Match3.ECS.UI
{
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class GameOverSystem : SystemBase
    {
        private bool _handled;
        private EndSimulationEntityCommandBufferSystem _ecbSystem;

        protected override void OnCreate()
        {
            _handled = false;
            _ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
            RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly(typeof(Game.GameResultEvent))));
        }

        protected override void OnUpdate()
        {
            if (_handled)
            {
                return;
            }

            var ecb = _ecbSystem.CreateCommandBuffer();

            foreach (var (result, entity) in
                SystemAPI.Query<RefRO<Game.GameResultEvent>>().WithEntityAccess())
            {
                _handled = true;
                ecb.DestroyEntity(entity);
                ShowResult(result.ValueRO.FinalScore, result.ValueRO.StarCount);
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

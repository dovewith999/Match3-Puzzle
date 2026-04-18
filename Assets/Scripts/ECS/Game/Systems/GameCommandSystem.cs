using Unity.Entities;
using UnityEngine.SceneManagement;

namespace Match3.ECS.Game
{
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class GameCommandSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<TimerComponent>();
        }

        protected override void OnUpdate()
        {
            HandleStartCommand();
            HandlePauseCommand();
            HandleRetryCommand();
        }

        private void HandleStartCommand()
        {
            foreach (var (_, entity) in SystemAPI.Query<RefRO<GameStartCommand>>().WithEntityAccess())
            {
                var timer = SystemAPI.GetSingleton<TimerComponent>();
                SystemAPI.SetSingleton(new TimerComponent
                {
                    RemainingSeconds = timer.RemainingSeconds,
                    IsRunning = true,
                    IsExpired = false,
                });
                EntityManager.DestroyEntity(entity);
            }
        }

        private void HandlePauseCommand()
        {
            foreach (var (cmd, entity) in SystemAPI.Query<RefRO<GamePauseCommand>>().WithEntityAccess())
            {
                var timer = SystemAPI.GetSingleton<TimerComponent>();
                SystemAPI.SetSingleton(new TimerComponent
                {
                    RemainingSeconds = timer.RemainingSeconds,
                    IsRunning = !cmd.ValueRO.IsPaused,
                    IsExpired = timer.IsExpired,
                });
                EntityManager.DestroyEntity(entity);
            }
        }

        private void HandleRetryCommand()
        {
            foreach (var (_, entity) in SystemAPI.Query<RefRO<GameRetryCommand>>().WithEntityAccess())
            {
                EntityManager.DestroyEntity(entity);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}

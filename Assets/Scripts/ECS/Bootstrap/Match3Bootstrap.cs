using System.Collections.Generic;
using Unity.Entities;

namespace Match3.ECS
{
    // Game World (LocalSimulation) / UI World (Presentation) 분리 Bootstrap
    // SubScene/Baker 없이 Bootstrap 단계에서 싱글턴 컴포넌트를 코드로 직접 생성합니다.
    // 이렇게 하면 SubScene 런타임 캐시 빌드 의존성이 사라집니다.
    public class Match3Bootstrap : ICustomBootstrap
    {
        // 기획서 기준값: 60초, 별 점수 500/1000/1500, 보드 8x8
        private const int TimeInSeconds = 60;
        private const int Score1Star    = 500;
        private const int Score2Star    = 1000;
        private const int Score3Star    = 1500;
        private const int BoardXDim     = 8;
        private const int BoardYDim     = 8;
        private const float FillTime    = 0.1f;

        public bool Initialize(string defaultWorldName)
        {
            var gameWorld = new World("Game World", WorldFlags.Live);
            var uiWorld   = new World("UI World",   WorldFlags.Live);

            // SubScene 대신 코드로 직접 싱글턴 생성
            World.DefaultGameObjectInjectionWorld = gameWorld;

            var gameSystems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.LocalSimulation);
            AddSystemsToWorld(gameWorld, gameSystems);

            var uiSystems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Presentation);
            AddSystemsToWorld(uiWorld, uiSystems);

            // 싱글턴 Entity 생성 (Authoring/Baker/SubScene 불필요)
            CreateSingletons(gameWorld);

            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(gameWorld);
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(uiWorld);

            return true;
        }

        private static void CreateSingletons(World gameWorld)
        {
            var em = gameWorld.EntityManager;

            // --- LevelConfig + Timer + Score + Combo 를 하나의 Entity에 묶어서 싱글턴으로 생성 ---
            var configEntity = em.CreateEntity(
                typeof(Game.LevelConfigComponent),
                typeof(Game.TimerComponent),
                typeof(Game.ScoreComponent),
                typeof(Game.ComboComponent));

            em.SetComponentData(configEntity, new Game.LevelConfigComponent
            {
                TimeInSeconds = TimeInSeconds,
                Score1Star    = Score1Star,
                Score2Star    = Score2Star,
                Score3Star    = Score3Star,
            });

            em.SetComponentData(configEntity, new Game.TimerComponent
            {
                RemainingSeconds = TimeInSeconds,
                IsRunning        = true,   // 게임 시작과 동시에 타이머 시작
                IsExpired        = false,
            });

            em.SetComponentData(configEntity, new Game.ScoreComponent
            {
                CurrentScore      = 0,
                ComboCount        = 0,
                ComboMultiplier   = 1.0f,
                TimeSinceLastScore = 0f,
            });

            em.SetComponentData(configEntity, new Game.ComboComponent
            {
                Count              = 0,
                Multiplier         = 1.0f,
                TimeSinceLastMatch = 0f,
            });

            // --- 스왑 검증용 싱글턴 ---
            var swapValidationEntity = em.CreateEntity(typeof(Game.LastSwapComponent));
            em.SetComponentData(swapValidationEntity, new Game.LastSwapComponent
            {
                HasPendingSwap = 0,
            });

            // --- 보드 설정 싱글턴 ---
            var boardEntity = em.CreateEntity(typeof(Game.BoardConfigComponent));

            em.SetComponentData(boardEntity, new Game.BoardConfigComponent
            {
                XDim     = BoardXDim,
                YDim     = BoardYDim,
                FillTime = FillTime,
            });
        }

        private static void AddSystemsToWorld(World world, IReadOnlyList<System.Type> systems)
        {
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);
        }
    }
}

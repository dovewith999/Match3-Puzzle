using System.Collections.Generic;
using Unity.Entities;

namespace Match3.ECS
{
    public class Match3Bootstrap : ICustomBootstrap
    {
        public bool Initialize(string defaultWorldName)
        {
            var gameWorld = new World("Game World", WorldFlags.Live);
            var uiWorld = new World("UI World", WorldFlags.Live);

            World.DefaultGameObjectInjectionWorld = gameWorld;

            var gameSystems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.LocalSimulation);
            AddSystemsToWorld(gameWorld, gameSystems);

            var uiSystems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Presentation);
            AddSystemsToWorld(uiWorld, uiSystems);

            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(gameWorld);
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(uiWorld);

            return true;
        }

        private static void AddSystemsToWorld(World world, IReadOnlyList<System.Type> systems)
        {
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);
        }
    }
}

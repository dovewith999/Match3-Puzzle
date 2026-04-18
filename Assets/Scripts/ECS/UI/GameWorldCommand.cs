using Unity.Entities;
using UnityEngine;

namespace Match3.ECS.UI
{
    public static class GameWorldCommand
    {
        public static void Send<T>(T command) where T : unmanaged, IComponentData
        {
            var gameWorld = World.DefaultGameObjectInjectionWorld;

            if (gameWorld == null || !gameWorld.IsCreated)
            {
                Debug.LogWarning($"[GameWorldCommand] Game World not found. Command {typeof(T).Name} dropped.");
                return;
            }

            var entity = gameWorld.EntityManager.CreateEntity();
            gameWorld.EntityManager.AddComponentData(entity, command);
        }
    }
}

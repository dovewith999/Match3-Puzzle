using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Match3.ECS.Game
{
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class InputSystem : SystemBase
    {
        private int2 _pressedCell;
        private bool _hasPressedCell;

        protected override void OnCreate()
        {
            RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly(typeof(BoardConfigComponent))));
        }

protected override void OnUpdate()
        {
            var mouse = UnityEngine.InputSystem.Mouse.current;

            if (mouse == null)
            {
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _pressedCell = ConvertMouseToCell(mouse.position.ReadValue());
                _hasPressedCell = true;
            }

            if (!mouse.leftButton.wasReleasedThisFrame || !_hasPressedCell)
            {
                return;
            }

            var released = ConvertMouseToCell(mouse.position.ReadValue());
            _hasPressedCell = false;

            if (!IsCellAdjacent(_pressedCell, released))
            {
                return;
            }

            EnqueueSwapRequest(_pressedCell, released);
        }

        private void EnqueueSwapRequest(int2 from, int2 to)
        {
            var ecbSystem = World.GetOrCreateSystemManaged(
                typeof(BeginSimulationEntityCommandBufferSystem))
                as BeginSimulationEntityCommandBufferSystem;

            var ecb = ecbSystem.CreateCommandBuffer();
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new SwapRequestComponent
            {
                FromX = from.x,
                FromY = from.y,
                ToX = to.x,
                ToY = to.y,
            });
        }

private int2 ConvertMouseToCell(Vector2 screenPos)
        {
            var configEntity = GetEntityQuery(
                ComponentType.ReadOnly(typeof(BoardConfigComponent))).GetSingletonEntity();
            var config = EntityManager.GetComponentData<BoardConfigComponent>(configEntity);
            var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            var x = Mathf.RoundToInt(worldPos.x + config.XDim / 2.0f);
            var y = Mathf.RoundToInt(config.YDim / 2.0f - worldPos.y);
            return new int2(x, y);
        }

        private static bool IsCellAdjacent(int2 a, int2 b)
        {
            return (a.x == b.x && math.abs(a.y - b.y) == 1) ||
                   (a.y == b.y && math.abs(a.x - b.x) == 1);
        }
    }
}

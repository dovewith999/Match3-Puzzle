using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Match3.ECS.Game
{
    // 마우스(에디터/PC)와 터치(모바일) 입력을 통합 처리합니다.
    // Press → Cell 기록, Release → 인접 Cell 스왑 요청
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

        protected override void OnStartRunning()
        {
            // 멀티터치 지원을 위해 EnhancedTouch 활성화
            EnhancedTouchSupport.Enable();
        }

        protected override void OnStopRunning()
        {
            EnhancedTouchSupport.Disable();
        }

        protected override void OnUpdate()
        {
            if (Camera.main == null)
            {
                return;
            }

            Vector2 pressPos = Vector2.zero;
            Vector2 releasePos = Vector2.zero;
            bool pressed = false;
            bool released = false;

            // --- 터치 입력 (모바일 우선) ---
            if (Touchscreen.current != null && Touch.activeTouches.Count > 0)
            {
                var touch = Touch.activeTouches[0];

                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    pressPos = touch.screenPosition;
                    pressed = true;
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                         touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    releasePos = touch.screenPosition;
                    released = true;
                }
            }
            // --- 마우스 입력 (에디터/PC 폴백) ---
            else if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    pressPos = Mouse.current.position.ReadValue();
                    pressed = true;
                }
                else if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    releasePos = Mouse.current.position.ReadValue();
                    released = true;
                }
            }

            if (pressed)
            {
                _pressedCell = ScreenToCell(pressPos);
                _hasPressedCell = true;
            }

            if (released && _hasPressedCell)
            {
                var releasedCell = ScreenToCell(releasePos);
                _hasPressedCell = false;

                if (IsCellAdjacent(_pressedCell, releasedCell))
                {
                    EnqueueSwap(_pressedCell, releasedCell);
                }
            }
        }

        private void EnqueueSwap(int2 from, int2 to)
        {
            if (World.GetOrCreateSystemManaged(typeof(BeginSimulationEntityCommandBufferSystem))
                is not BeginSimulationEntityCommandBufferSystem ecbSystem)
            {
                return;
            }

            var ecb = ecbSystem.CreateCommandBuffer();
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new SwapRequestComponent
            {
                FromX = from.x,
                FromY = from.y,
                ToX   = to.x,
                ToY   = to.y,
            });
        }

        private int2 ScreenToCell(Vector2 screenPos)
        {
            var config = SystemAPI.GetSingleton<BoardConfigComponent>();
            var worldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, 0f));
            var x = Mathf.RoundToInt(worldPos.x + config.XDim / 2.0f - 0.5f);
            var y = Mathf.RoundToInt(config.YDim / 2.0f - worldPos.y - 0.5f);
            return new int2(x, y);
        }

        private static bool IsCellAdjacent(int2 a, int2 b)
        {
            return (a.x == b.x && math.abs(a.y - b.y) == 1) ||
                   (a.y == b.y && math.abs(a.x - b.x) == 1);
        }
    }
}

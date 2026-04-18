using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Match3.ECS.Game
{
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class SpecialPieceInputSystem : SystemBase
    {
        private const float HoldThreshold = 0.4f;
        private const float DragThreshold = 30f;

        private bool _sp_touching;
        private float _sp_holdTimer;
        private bool _sp_holding;
        private Vector2 _sp_pressPos;
        private int2 _sp_pressCell;

        protected override void OnCreate()
        {
            RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly(typeof(BoardConfigComponent))));
        }

        protected override void OnUpdate()
        {
            var mouse = Mouse.current;
            if (mouse == null) { return; }

            // Press
            if (mouse.leftButton.wasPressedThisFrame)
            {
                var sp = mouse.position.ReadValue();
                var cfg = SystemAPI.GetSingleton<BoardConfigComponent>();
                var w = Camera.main.ScreenToWorldPoint(new Vector3(sp.x, sp.y, 0f));
                var cell = new int2(Mathf.RoundToInt(w.x + cfg.XDim / 2.0f), Mathf.RoundToInt(cfg.YDim / 2.0f - w.y));

                if (cell.x >= 0 && cell.x < cfg.XDim && cell.y >= 0 && cell.y < cfg.YDim)
                {
                    var arr = GetEntityQuery(ComponentType.ReadOnly(typeof(PieceComponent))).ToEntityArray(Unity.Collections.Allocator.Temp);
                    bool isSpecial = false;

                    foreach (var e in arr)
                    {
                        var p = EntityManager.GetComponentData<PieceComponent>(e);

                        if (p.X == cell.x && p.Y == cell.y && p.PieceType == PieceTypeECS.Special)
                        {
                            isSpecial = true;
                            break;
                        }
                    }

                    arr.Dispose();

                    if (isSpecial)
                    {
                        _sp_touching = true;
                        _sp_holdTimer = 0f;
                        _sp_pressPos = sp;
                        _sp_pressCell = cell;
                    }
                }
            }

            // Hold tick
            if (_sp_touching && !_sp_holding)
            {
                _sp_holdTimer += SystemAPI.Time.DeltaTime;

                if (_sp_holdTimer >= HoldThreshold)
                {
                    _sp_holding = true;
                    var holdEcb = (World.GetOrCreateSystemManaged(typeof(BeginSimulationEntityCommandBufferSystem)) as BeginSimulationEntityCommandBufferSystem).CreateCommandBuffer();
                    var holdEntity = holdEcb.CreateEntity();
                    holdEcb.AddComponent(holdEntity, new SpecialPieceHoldComponent { PieceX = _sp_pressCell.x, PieceY = _sp_pressCell.y, IsHolding = true });
                }
            }

            // Release
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                if (_sp_holding)
                {
                    var relPos = mouse.position.ReadValue();
                    var delta = relPos - _sp_pressPos;

                    if (delta.magnitude >= DragThreshold)
                    {
                        DragDirectionECS dir;
                        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                        {
                            dir = delta.x > 0 ? DragDirectionECS.Right : DragDirectionECS.Left;
                        }
                        else
                        {
                            dir = delta.y > 0 ? DragDirectionECS.Up : DragDirectionECS.Down;
                        }

                        if (dir != DragDirectionECS.None)
                        {
                            int2 step;
                            switch (dir)
                            {
                                case DragDirectionECS.Up:    step = new int2(0, -1); break;
                                case DragDirectionECS.Down:  step = new int2(0, 1);  break;
                                case DragDirectionECS.Left:  step = new int2(-1, 0); break;
                                default:                     step = new int2(1, 0);  break;
                            }

                            var scanCfg = SystemAPI.GetSingleton<BoardConfigComponent>();
                            int nx = _sp_pressCell.x + step.x;
                            int ny = _sp_pressCell.y + step.y;
                            var scanArr = GetEntityQuery(ComponentType.ReadOnly(typeof(PieceComponent))).ToEntityArray(Unity.Collections.Allocator.Temp);
                            var foundColor = ColorTypeECS.None;

                            while (nx >= 0 && nx < scanCfg.XDim && ny >= 0 && ny < scanCfg.YDim)
                            {
                                foreach (var se in scanArr)
                                {
                                    var sp2 = EntityManager.GetComponentData<PieceComponent>(se);

                                    if (sp2.X == nx && sp2.Y == ny && sp2.PieceType == PieceTypeECS.Normal)
                                    {
                                        foundColor = sp2.ColorType;
                                        break;
                                    }
                                }

                                if (foundColor != ColorTypeECS.None) { break; }

                                nx += step.x;
                                ny += step.y;
                            }

                            scanArr.Dispose();

                            if (foundColor != ColorTypeECS.None)
                            {
                                var clearEcb = (World.GetOrCreateSystemManaged(typeof(BeginSimulationEntityCommandBufferSystem)) as BeginSimulationEntityCommandBufferSystem).CreateCommandBuffer();
                                var clearEntity = clearEcb.CreateEntity();
                                clearEcb.AddComponent(clearEntity, new ColorClearRequest { TargetColor = foundColor, IsFromSpecialPiece = true });
                            }
                        }
                    }
                }

                _sp_touching = false;
                _sp_holdTimer = 0f;
                _sp_holding = false;
                _sp_pressPos = Vector2.zero;
                _sp_pressCell = int2.zero;
            }
        }
    }
}

using Unity.Entities;
using UnityEngine;

namespace Match3.ECS.UI
{
    [WorldSystemFilter(WorldSystemFilterFlags.Presentation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class CharacterSystem : SystemBase
    {
        private const float ExpressionHoldTime = 2f;

        private CharacterExpressionECS _current;
        private float _holdTimer;

        protected override void OnCreate()
        {
            _current = CharacterExpressionECS.Idle;
            _holdTimer = 0f;
        }

        protected override void OnUpdate()
        {
            _holdTimer -= SystemAPI.Time.DeltaTime;

            var next = ConsumeEvents();

            if (next == CharacterExpressionECS.Idle && _holdTimer > 0f)
            {
                return;
            }

            ApplyExpression(next);
        }

        private CharacterExpressionECS ConsumeEvents()
        {
            var highest = CharacterExpressionECS.Idle;

            foreach (var (ev, entity) in
                SystemAPI.Query<RefRO<Game.ComboOccurredEvent>>().WithEntityAccess())
            {
                int count = ev.ValueRO.ComboCount;
                EntityManager.DestroyEntity(entity);

                CharacterExpressionECS expr;

                if (count >= 5)
                {
                    expr = CharacterExpressionECS.Hyper;
                }
                else if (count >= 3)
                {
                    expr = CharacterExpressionECS.Excited;
                }
                else
                {
                    expr = CharacterExpressionECS.Happy;
                }

                if (expr > highest)
                {
                    highest = expr;
                }
            }

            if (highest > CharacterExpressionECS.Idle)
            {
                return highest;
            }

            foreach (var (_, entity) in
                SystemAPI.Query<RefRO<Game.LowTimeWarningEvent>>().WithEntityAccess())
            {
                EntityManager.DestroyEntity(entity);
                return CharacterExpressionECS.Nervous;
            }

            foreach (var (_, entity) in
                SystemAPI.Query<RefRO<Game.NoScoreElapsedEvent>>().WithEntityAccess())
            {
                EntityManager.DestroyEntity(entity);
                return CharacterExpressionECS.Bored;
            }

            return CharacterExpressionECS.Idle;
        }

        private void ApplyExpression(CharacterExpressionECS next)
        {
            _current = next;
            _holdTimer = ExpressionHoldTime;

            var portrait = Object.FindFirstObjectByType<CharacterPortraitBridge>();

            if (portrait == null)
            {
                return;
            }

            portrait.SetExpression(_current);
        }
    }
}

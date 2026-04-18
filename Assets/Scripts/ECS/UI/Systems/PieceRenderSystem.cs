using Match3.ECS.Game;
using Unity.Entities;
using UnityEngine;

namespace Match3.ECS.UI
{
    [WorldSystemFilter(WorldSystemFilterFlags.Presentation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class PieceRenderSystem : SystemBase
    {
        private static Sprite s_squareSprite;
        private PieceColorConfig m_colorConfig;
        private Transform m_boardRoot;

        protected override void OnCreate()
        {
            RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<PieceComponent>()));
        }

        protected override void OnStartRunning()
        {
            m_colorConfig = Resources.Load<PieceColorConfig>("PieceColorConfig");
            var rootGO = new GameObject("BoardRoot");
            m_boardRoot = rootGO.transform;
        }

        protected override void OnStopRunning()
        {
            if (m_boardRoot != null)
            {
                Object.Destroy(m_boardRoot.gameObject);
            }
        }

        protected override void OnUpdate()
        {
            foreach (var (piece, entity) in SystemAPI.Query<RefRO<PieceComponent>>() .WithNone<PieceVisualComponent>() .WithEntityAccess())
            {
                if (piece.ValueRO.PieceType == PieceTypeECS.Empty)
                {
                    continue;
                }

                var go = new GameObject("Piece");
                go.transform.SetParent(m_boardRoot);
                go.transform.position = CellToWorld(piece.ValueRO.X, piece.ValueRO.Y);
                go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
                go.AddComponent<SpriteRenderer>();
                ApplyColorToRenderer(go, piece.ValueRO.ColorType);

                EntityManager.AddComponentData(entity, new PieceVisualComponent
                {
                    VisualObject = go,
                    LastX = piece.ValueRO.X,
                    LastY = piece.ValueRO.Y,
                    LastColor = piece.ValueRO.ColorType,
                    LastPieceType = piece.ValueRO.PieceType,
                });
            }

            foreach (var (piece, visual, entity) in SystemAPI.Query<RefRO<PieceComponent>, RefRW<PieceVisualComponent>>() .WithEntityAccess())
            {
                var p = piece.ValueRO;

                if (p.PieceType == PieceTypeECS.Empty)
                {
                    if (visual.ValueRO.VisualObject.IsValid())
                    {
                        Object.Destroy(visual.ValueRO.VisualObject.Value);
                    }

                    EntityManager.RemoveComponent<PieceVisualComponent>(entity);
                    continue;
                }

                var go = visual.ValueRO.VisualObject.Value;

                if (go == null)
                {
                    continue;
                }

                if (p.X != visual.ValueRO.LastX || p.Y != visual.ValueRO.LastY)
                {
                    go.transform.position = CellToWorld(p.X, p.Y);
                    visual.ValueRW.LastX = p.X;
                    visual.ValueRW.LastY = p.Y;
                }

                if (p.ColorType != visual.ValueRO.LastColor)
                {
                    ApplyColorToRenderer(go, p.ColorType);
                    visual.ValueRW.LastColor = p.ColorType;
                }
            }
        }

        // daltonbr/Match3 ColorPiece.SetColor 대응
        // PieceColorConfig 없을 때 단색 사각형으로 폴백 → 스프라이트 없이도 테스트 가능
        private void ApplyColorToRenderer(GameObject go, ColorTypeECS colorType)
        {
            var sr = go.GetComponent<SpriteRenderer>();

            if (sr == null)
            {
                return;
            }

            if (m_colorConfig != null)
            {
                var sprite = m_colorConfig.GetSprite(colorType);

                if (sprite != null)
                {
                    sr.sprite = sprite;
                    sr.color = Color.white;
                    return;
                }
            }

            if (s_squareSprite == null)
            {
                var tex = new Texture2D(32, 32);
                var fill = new Color[32 * 32];
                for (int i = 0; i < fill.Length; i++)
                {
                    fill[i] = Color.white;
                }

                tex.SetPixels(fill);
                tex.Apply();
                s_squareSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
            }

            sr.sprite = s_squareSprite;
            sr.color = colorType switch
            {
                ColorTypeECS.Yellow => new Color(1f, 0.9f, 0.2f),
                ColorTypeECS.Purple => new Color(0.7f, 0.3f, 1f),
                ColorTypeECS.Red => new Color(1f, 0.3f, 0.3f),
                ColorTypeECS.Blue => new Color(0.3f, 0.6f, 1f),
                ColorTypeECS.Green => new Color(0.3f, 0.9f, 0.4f),
                ColorTypeECS.Pink => new Color(1f, 0.5f, 0.8f),
                _ => Color.gray,
            };
        }

        // daltonbr/Match3 GameGrid.GetWorldPosition 동일 공식 (보드 중앙 = 월드 원점)
        private static Vector3 CellToWorld(int x, int y)
        {
            const int xDim = 8;
            const int yDim = 8;
            return new Vector3(-xDim / 2.0f + x + 0.5f, yDim / 2.0f - y - 0.5f, 0f);
        }
    }
}

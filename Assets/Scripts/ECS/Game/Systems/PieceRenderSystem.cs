using Match3.ECS.Game;
using Unity.Entities;
using UnityEngine;

namespace Match3.ECS.Game
{
    // Game World(LocalSimulation)에서 PieceComponent를 읽어 SpriteRenderer GameObject를 생성/갱신/삭제합니다.
    // daltonbr/Match3 GameGrid.SpawnNewPiece + ColorPiece.SetColor 역할을 ECS 방식으로 구현합니다.
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class PieceRenderSystem : SystemBase
    {
        private static Sprite s_squareSprite;
        private Match3.ECS.PieceColorConfig m_colorConfig;
        private Transform m_boardRoot;

        // ECB Singleton — structural change(AddComponent/RemoveComponent)를 지연 적용하기 위해 사용
        private EndSimulationEntityCommandBufferSystem m_ecbSystem;

        protected override void OnCreate()
        {
            RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<PieceComponent>()));
            m_ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnStartRunning()
        {
            m_colorConfig = Resources.Load<Match3.ECS.PieceColorConfig>("PieceColorConfig");
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
            var ecb = m_ecbSystem.CreateCommandBuffer();

            // 1) PieceVisualComponent가 없는 새 피스 → SpriteRenderer GameObject 생성 후 컴포넌트 추가
            foreach (var (piece, entity) in
                SystemAPI.Query<RefRO<PieceComponent>>()
                    .WithNone<PieceVisualComponent>()
                    .WithEntityAccess())
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
                PaintRenderer(go, piece.ValueRO.ColorType);

                // ECB로 지연 추가 — 현재 프레임 foreach 반복 중 structural change 방지
                ecb.AddComponent(entity, new PieceVisualComponent
                {
                    VisualObject  = go,
                    LastX         = piece.ValueRO.X,
                    LastY         = piece.ValueRO.Y,
                    LastColor     = piece.ValueRO.ColorType,
                    LastPieceType = piece.ValueRO.PieceType,
                });
            }

            // 2) Empty로 바뀐 피스 → GameObject 파괴 + 컴포넌트 제거 (ECB로 지연)
            foreach (var (piece, visual, entity) in
                SystemAPI.Query<RefRO<PieceComponent>, RefRO<PieceVisualComponent>>()
                    .WithEntityAccess())
            {
                if (piece.ValueRO.PieceType != PieceTypeECS.Empty)
                {
                    continue;
                }

                if (visual.ValueRO.VisualObject.IsValid())
                {
                    Object.Destroy(visual.ValueRO.VisualObject.Value);
                }

                ecb.RemoveComponent<PieceVisualComponent>(entity);
            }

            // 3) 위치 또는 색상이 변한 피스 → Transform/SpriteRenderer 즉시 갱신
            // (PieceVisualComponent 값 변경은 RefRW로 처리 — structural change 없음)
            foreach (var (piece, visual) in
                SystemAPI.Query<RefRO<PieceComponent>, RefRW<PieceVisualComponent>>())
            {
                var p  = piece.ValueRO;
                var go = visual.ValueRO.VisualObject.Value;

                if (go == null)
                {
                    continue;
                }

                if (p.X != visual.ValueRO.LastX || p.Y != visual.ValueRO.LastY)
                {
                    go.transform.position = CellToWorld(p.X, p.Y);
                    visual.ValueRW.LastX  = p.X;
                    visual.ValueRW.LastY  = p.Y;
                }

                if (p.ColorType != visual.ValueRO.LastColor)
                {
                    PaintRenderer(go, p.ColorType);
                    visual.ValueRW.LastColor = p.ColorType;
                }
            }
        }

        // daltonbr/Match3 ColorPiece.SetColor 대응
        // PieceColorConfig SO 스프라이트 우선, 없으면 단색 사각형 폴백
        private void PaintRenderer(GameObject go, ColorTypeECS colorType)
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
                    sr.color  = Color.white;
                    return;
                }
            }

            if (s_squareSprite == null)
            {
                var tex  = new Texture2D(32, 32);
                var fill = new Color[32 * 32];

                for (int i = 0; i < fill.Length; i++)
                {
                    fill[i] = Color.white;
                }

                tex.SetPixels(fill);
                tex.Apply();
                s_squareSprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, 32, 32),
                    new Vector2(0.5f, 0.5f),
                    32f);
            }

            sr.sprite = s_squareSprite;
            sr.color  = colorType switch
            {
                ColorTypeECS.Yellow => new Color(1f,   0.9f, 0.2f),
                ColorTypeECS.Purple => new Color(0.7f, 0.3f, 1f),
                ColorTypeECS.Red    => new Color(1f,   0.3f, 0.3f),
                ColorTypeECS.Blue   => new Color(0.3f, 0.6f, 1f),
                ColorTypeECS.Green  => new Color(0.3f, 0.9f, 0.4f),
                ColorTypeECS.Pink   => new Color(1f,   0.5f, 0.8f),
                _                   => Color.gray,
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

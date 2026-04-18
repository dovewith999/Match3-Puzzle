using UnityEngine;

namespace Match3.ECS
{
    // 색상 타입별 스프라이트를 에디터에서 설정하는 ScriptableObject
    // daltonbr/Match3의 ColorPiece.ColorSprite 구조를 ECS 친화적으로 변환한 것입니다.
    [CreateAssetMenu(fileName = "PieceColorConfig", menuName = "Match3/Piece Color Config")]
    public class PieceColorConfig : ScriptableObject
    {
        [System.Serializable]
        public struct ColorEntry
        {
            public Game.ColorTypeECS colorType;
            public Sprite sprite;
            public Color fallbackColor;
        }

        public ColorEntry[] entries;

        public Sprite GetSprite(Game.ColorTypeECS color)
        {
            foreach (var entry in entries)
            {
                if (entry.colorType == color)
                {
                    return entry.sprite;
                }
            }

            return null;
        }

        public Color GetColor(Game.ColorTypeECS color)
        {
            foreach (var entry in entries)
            {
                if (entry.colorType == color)
                {
                    return entry.fallbackColor;
                }
            }

            return Color.white;
        }
    }
}

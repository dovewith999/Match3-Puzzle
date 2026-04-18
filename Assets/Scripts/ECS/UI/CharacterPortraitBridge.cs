using UnityEngine;

namespace Match3.ECS.UI
{
    public class CharacterPortraitBridge : MonoBehaviour
    {
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite happySprite;
        [SerializeField] private Sprite excitedSprite;
        [SerializeField] private Sprite hyperSprite;
        [SerializeField] private Sprite boredSprite;
        [SerializeField] private Sprite nervousSprite;

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetExpression(CharacterExpressionECS expression)
        {
            var sprite = ResolveSprite(expression);

            if (_spriteRenderer != null && sprite != null)
            {
                _spriteRenderer.sprite = sprite;
            }
        }

        private Sprite ResolveSprite(CharacterExpressionECS expression)
        {
            switch (expression)
            {
                case CharacterExpressionECS.Happy: return happySprite;
                case CharacterExpressionECS.Excited: return excitedSprite;
                case CharacterExpressionECS.Hyper: return hyperSprite;
                case CharacterExpressionECS.Bored: return boredSprite;
                case CharacterExpressionECS.Nervous: return nervousSprite;
                default: return idleSprite;
            }
        }
    }
}

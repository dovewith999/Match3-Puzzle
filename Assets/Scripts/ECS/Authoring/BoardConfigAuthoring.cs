using Match3.ECS.Game;
using Unity.Entities;
using UnityEngine;

namespace Match3.ECS.Authoring
{
    public class BoardConfigAuthoring : MonoBehaviour
    {
        [Header("Board Config")]
        public int xDim = 8;
        public int yDim = 8;
        public float fillTime = 0.1f;

        public class Baker : Baker<BoardConfigAuthoring>
        {
            public override void Bake(BoardConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new BoardConfigComponent
                {
                    XDim = authoring.xDim,
                    YDim = authoring.yDim,
                    FillTime = authoring.fillTime,
                });
            }
        }
    }
}

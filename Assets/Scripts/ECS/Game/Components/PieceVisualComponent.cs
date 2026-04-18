using Unity.Entities;

namespace Match3.ECS.Game
{
    // 각 피스 Entity에 붙어서 화면 GameObject의 instanceID를 추적하는 컴포넌트
    // UnityObjectRef<GameObject>로 참조를 ECS 안전하게 저장합니다.
    public struct PieceVisualComponent : IComponentData
    {
        public UnityObjectRef<UnityEngine.GameObject> VisualObject;
        public int LastX;
        public int LastY;
        public ColorTypeECS LastColor;
        public PieceTypeECS LastPieceType;
    }
}

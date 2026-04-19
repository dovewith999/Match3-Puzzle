using Unity.Entities;

namespace Match3.ECS.Game
{
    // 가장 최근 스왑의 두 셀 좌표를 저장하는 싱글턴 컴포넌트
    // SwapValidationSystem이 매칭 실패 시 롤백에 사용합니다.
    public struct LastSwapComponent : IComponentData
    {
        public int FromX;
        public int FromY;
        public int ToX;
        public int ToY;
        public byte HasPendingSwap; // 1 = true (Burst blittable)
    }
}

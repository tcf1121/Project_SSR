using UnityEngine;

/// <summary>
/// 간단한 사다리 정의용 컴포넌트.
/// • <c>bottom</c> : 사다리 아래쪽 빈 Transform (Collider 위치)
/// • <c>top</c>    : 사다리 위쪽 빈 Transform
/// MonoBehaviour 한 장짜리라 프리팹에 붙여 두면 LadderClimber 가 참조합니다.
/// </summary>
public class LadderBounds : MonoBehaviour
{
    public Transform bottom;   // 사다리 아래쪽
    public Transform top;      // 사다리 위쪽
}
using UnityEngine;

/// <summary>
/// 사다리 기능이 없는 몬스터를 위한 '아무것도 하지 않는' 클래스입니다.
/// NullReferenceException을 방지하는 역할을 합니다.
/// </summary>
public class NullClimber : IMonsterClimber
{
    public bool IsClimbing => false; // 등반 중인지 물어보면 항상 '아니오'라고 답합니다.

    public void Init(MonsterBrain brain) { } // 초기화해도 아무것도 하지 않습니다.

    public bool TryFindAndClimb(int dir, Vector2 playerPos) => false; // 등반을 시도해도 항상 '실패'라고 답합니다.
}
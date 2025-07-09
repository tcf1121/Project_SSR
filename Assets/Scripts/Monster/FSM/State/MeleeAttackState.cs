using UnityEngine;


/// <summary>
/// 근접 공격 상태 – 플레이어가 사정거리 안에 있는 동안 공격 애니메이션을 무한 반복
/// </summary>
public class MeleeAttackState : AttackState
{
    /* ───── refs ───── */
    private Monster _monster;
    private readonly MonsterStatEntry _statData;
    private BoxCollider2D _attackBox;

    /* ───── runtime ───── */
    private bool isAttacking;
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    public MeleeAttackState(Monster monster)
    {
        _monster = monster;
        _statData = monster.Brain.StatData;
        _attackBox = monster.AttackBoxCol;
    }

    public void Enter()
    {
        _attackBox.enabled = false;
        PlayAttack();                                // 첫 타격
    }

    public void Tick()
    {
        /* ───── Attack 클립 완료 대기 ───── */
        if (isAttacking)
        {
            var info = _monster.Animator.GetCurrentAnimatorStateInfo(0);
            if (!_monster.Animator.IsInTransition(0) && info.shortNameHash == AttackHash && info.normalizedTime >= 1f)
                isAttacking = false;                 // 클립 1루프 종료
            else
                return;                              // 진행 중이면 대기
        }
        else
        {
            if (_monster.PlayerInRange(_statData.attackRange))
            {
                PlayAttack();                            // ★ 범위 안이면 즉시 다시 공격
            }
            else if (!_monster.PlayerInRange(_statData.chaseRange))
            {
                _monster.ChangeState(StateID.Patrol);
                return;
            }
            else
            {
                if(_monster.Brain.StatData.isFlying)
                {
                    _monster.ChangeState(StateID.FloatChase); // 비행 유닛은 FloatChase로 전환
                    return;
                }
                _monster.ChangeState(StateID.Chase);        // 범위 밖 → 추격
            }





        }



    }

    public void Exit()
    {
        _monster.Rigid.velocity = Vector2.zero;
        _attackBox.enabled = false;
        isAttacking = false;
    }

    /* ───── helpers ───── */
    private void PlayAttack()
    {
        _monster.Rigid.velocity = Vector2.zero;

        // 스프라이트 방향 고정 (Y·Z 비율 유지)
        _monster.FlipMonster();

        _monster.Animator.Play("Attack", 0, 0f);                 // ★ 클립을 0초로 강제 재시작
        isAttacking = true;
        // 타격 판정은 애니메이션 이벤트(ActivateHitBox / DeactivateHitBox)로 처리
    }

    // Animation Event
    public void Attack() => _attackBox.enabled = true;
    public void FinishAttack() => _attackBox.enabled = false;

    public void CancelAttack()
    {
        isAttacking = false;
        _attackBox.enabled = false;    // 어떤 프레임이 오더라도 타격 박스 OFF
    }
}
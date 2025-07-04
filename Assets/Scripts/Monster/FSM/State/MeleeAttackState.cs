using UnityEngine;


/// <summary>
/// 근접 공격 상태 – 플레이어가 사정거리 안에 있는 동안 공격 애니메이션을 무한 반복
/// </summary>
public class MeleeAttackState : IState
{
    /* ───── refs ───── */
    private readonly MonsterBrain brain;
    private readonly Rigidbody2D rb;
    private readonly Transform tf;
    private readonly Collider2D hitBox;
    private readonly MonsterStatEntry statData;
    private Animator anim;

    /* ───── runtime ───── */
    private Transform player;
    private bool isAttacking;
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    public MeleeAttackState(MonsterBrain brain, Collider2D hitBox)
    {
        this.brain = brain;
        this.hitBox = hitBox;
        this.rb = brain.GetComponent<Rigidbody2D>();
        this.tf = brain.transform;
        this.statData = brain.StatData;
    }

    public void Enter()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        anim = brain.GetComponent<Animator>();
        rb.velocity = Vector2.zero;
        hitBox.enabled = false;

        PlayAttack();                                // 첫 타격
    }

    public void Tick()
    {
        if (player == null)
        {
            brain.ChangeState(StateID.Patrol);
            return;
        }

        /* ───── Attack 클립 완료 대기 ───── */
        if (isAttacking)
        {
            var info = anim.GetCurrentAnimatorStateInfo(0);
            if (!anim.IsInTransition(0) && info.shortNameHash == AttackHash && info.normalizedTime >= 1f)
                isAttacking = false;                 // 클립 1루프 종료
            else
                return;                              // 진행 중이면 대기
        }

        /* ───── 거리 재판단 & 재공격 ───── */
        float dist = Vector2.Distance(tf.position, player.position);

        if (dist <= statData.attackRange)
        {
            PlayAttack();                            // ★ 범위 안이면 즉시 다시 공격
        }
        else
        {
            brain.ChangeState(StateID.Chase);        // 범위 밖 → 추격
        }
    }

    public void Exit()
    {
        rb.velocity = Vector2.zero;
        hitBox.enabled = false;
        isAttacking = false;
    }

    /* ───── helpers ───── */
    private void PlayAttack()
    {
        rb.velocity = Vector2.zero;

        // 스프라이트 방향 고정 (Y·Z 비율 유지)
        int dir = player.position.x > tf.position.x ? 1 : -1;
        tf.localScale = new Vector3(Mathf.Abs(tf.localScale.x) * dir, tf.localScale.y, tf.localScale.z);

        anim.Play("Attack", 0, 0f);                 // ★ 클립을 0초로 강제 재시작
        isAttacking = true;
        // 타격 판정은 애니메이션 이벤트(ActivateHitBox / DeactivateHitBox)로 처리
    }

    // Animation Event
    public void ActivateHitBox() => hitBox.enabled = true;
    public void DeactivateHitBox() => hitBox.enabled = false;
}
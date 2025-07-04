using UnityEngine;


/// <summary>
/// 몬스터가 피격되었을 때 실행되는 FSM 상태입니다.
/// 일정 시간 동안 경직/넉백 및 UI 출력 후 이전 상태로 복귀합니다.
/// </summary>
public class TakeDamageState : IState
{
    private readonly MonsterBrain brain;
    private readonly Rigidbody2D rb;
    private readonly HitInfo hit;
    private Animator anim;
    private float timer;
    private bool stagger;

    public TakeDamageState(MonsterBrain brain, HitInfo hit)
    {
        this.brain = brain;
        rb = brain.Monster.Rigid;
        this.hit = hit;
        Animator anim = brain.GetComponent<Animator>();
    }

    public void Enter()
    {
        var stats = brain.Monster.MonsterStats;
        Debug.Log($"현재 hp :{stats.CurrentHP}");

        // 체력 감소
        int newHP = stats.CurrentHP - hit.damage;
        stats.SetHP(newHP);

        //Debug.Log($"[TakeDamageState] CurrentHP: {stats.CurrentHP}, MaxHP: {stats.MaxHP}");

        // 체력바 갱신
        if (brain.Monster.HpBarFill != null)
        {
            brain.Monster.HpBar.gameObject.SetActive(true);
            brain.Monster.HpBarFill.fillAmount = (float)stats.CurrentHP / stats.MaxHP;
        }

        // 데미지 텍스트 출력
        //  brain.ShowDamageText(hit.damage);

        // 사망 처리
        stats.KillIfDead();
        if (stats.CurrentHP <= 0)
            return;

        // 경직 조건 판단
        stagger = hit.causesStagger || hit.damage >= brain.StatData.staggerThreshold;

        if (stagger)
        {
            float knockback = brain.StatData.knockbackForce; // ← SO에서 읽음
            brain.PlayAnim(AnimNames.Stagger); // 피격 애니메이션 재생
            brain.ApplyKnockback(hit.origin, knockback);
            timer = 0.35f;
        }
        else
        {
            timer = 0.1f;
        }
    }

    public void Tick()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            brain.Sm.ChangeState(StateID.Idle);
        }
    }

    public void Exit()
    {
        rb.velocity = Vector2.zero;
    }
}

/// <summary>
/// 피격 정보를 담는 구조체. 데미지, 넉백 방향, 경직 여부 등 포함
/// </summary>
public struct HitInfo
{
    public int damage;
    public Vector2 origin;
    public bool causesStagger;

    public HitInfo(int dmg, Vector2 origin, bool stagger = false)
    {
        this.damage = dmg;
        this.origin = origin;
        this.causesStagger = stagger;
    }
}
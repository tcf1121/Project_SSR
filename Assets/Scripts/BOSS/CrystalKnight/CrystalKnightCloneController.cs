using UnityEngine;
using System.Collections;

// 크리스탈 나이트 '분신'을 위한 전용 컨트롤러 스크립트
// 원본에서 페이즈 변환, 분신 소환 등 복잡한 로직을 모두 제거한 가벼운 버전입니다.
public class CrystalKnightCloneController : MonoBehaviour
{
    public enum State { Idle, Chase, MeleeAttack, RangedAttack, Wait, Die }
    public State currentState = State.Idle;

    [Header("분신 스탯")]
    public float maxHP = 500f; // 원본보다 훨씬 낮은 체력
    public float currentHP;
    public float attackPower = 20f; // 원본보다 낮은 공격력
    public float moveSpeed = 2.5f; // 원본보다 약간 빠르게 설정하여 압박감 추가
    public float detectRange = 15f; // 원본보다 넓은 감지 범위
    public float attackRange = 1.5f;

    private Transform player;
    private Animator animator;
    private float lastAttackTime;

    [Header("공격 설정")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float rangedAttackDamageMultiplier = 1.0f;
    public float attackCooldown = 3f;
    public float rangedAttackMinRange = 3f;
    public float rangedAttackMaxRange = 7f;
    public float prepareAttackDuration = 0.7f;
    public float waitAfterAttackDuration = 1.5f;

    void Awake()
    {
        currentHP = maxHP;
        player = GameObject.FindWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        ChangeState(State.Idle);
        // 분신은 스스로 소멸 타이머를 가집니다.
        Destroy(gameObject, 30f); // 30초 후 자동 소멸
    }

    void ChangeState(State newState)
    {
        if (currentState == State.Die) return;
        StopAllCoroutines();
        currentState = newState;

        switch (currentState)
        {
            case State.Idle: StartCoroutine(IdleState()); break;
            case State.Chase: StartCoroutine(ChaseState()); break;
            case State.MeleeAttack: StartCoroutine(MeleeAttackState()); break;
            case State.RangedAttack: StartCoroutine(RangedAttackState()); break;
            case State.Wait: StartCoroutine(WaitState()); break;
            case State.Die: StartCoroutine(DieState()); break;
        }
    }

    // ... (상태 코루틴들은 원본과 거의 동일하지만, 변신/소환 로직이 없음) ...

    IEnumerator ChaseState()
    {
        while (currentState == State.Chase)
        {
            if (player == null) { ChangeState(State.Idle); yield break; }

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                if (distanceToPlayer <= rangedAttackMaxRange)
                {
                    // 분신은 준비 동작 없이 바로 공격할 수 있음 (선택적)
                    if (distanceToPlayer >= rangedAttackMinRange)
                        ChangeState(State.RangedAttack);
                    else
                        ChangeState(State.MeleeAttack);
                    yield break;
                }
            }
            MoveTowardsPlayer();
            if (distanceToPlayer > detectRange) ChangeState(State.Idle);
            yield return null;
        }
    }
    
    // ... (나머지 상태 및 함수들) ...
    IEnumerator IdleState() { while (true) { if (player != null && Vector2.Distance(transform.position, player.position) <= detectRange) ChangeState(State.Chase); yield return null; } }
    IEnumerator MeleeAttackState() { animator.SetTrigger("Attack"); lastAttackTime = Time.time; yield return null; float animLen = animator.GetCurrentAnimatorStateInfo(0).length; yield return new WaitForSeconds(animLen); ChangeState(State.Wait); }
    IEnumerator RangedAttackState() { animator.SetTrigger("Attack2"); lastAttackTime = Time.time; if (projectilePrefab != null && firePoint != null && player != null) { Vector2 dir = (player.position - firePoint.position).normalized; GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity); CKSpell spell = proj.GetComponent<CKSpell>(); if (spell != null) { spell.SetDirection(dir); spell.damage = attackPower * rangedAttackDamageMultiplier; } } yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); ChangeState(State.Wait); }
    IEnumerator WaitState() { yield return new WaitForSeconds(waitAfterAttackDuration); ChangeState(State.Chase); }
    IEnumerator DieState() { animator.SetTrigger("Die"); Destroy(gameObject, 2f); yield return null; }
    void MoveTowardsPlayer() { if (player == null) return; Vector2 dir = (player.position - transform.position).normalized; transform.position += (Vector3)dir * moveSpeed * Time.deltaTime; if (dir.x > 0) transform.localScale = new Vector3(1, 1, 1); else if (dir.x < 0) transform.localScale = new Vector3(-1, 1, 1); }
    public void OnMeleeAttackHit() { if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange) player.GetComponent<PlayerStats>()?.TakeDamage(attackPower); }
    public void TakeDamage(float damage) { currentHP -= damage; if (currentHP <= 0) ChangeState(State.Die); }
}

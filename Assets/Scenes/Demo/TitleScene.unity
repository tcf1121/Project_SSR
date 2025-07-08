using System.Collections;
using UnityEngine;

public class BODController : MonoBehaviour
{
    [Header("상태")]
    [SerializeField] private BossState currentState = BossState.Idle;
    public enum BossState { Idle, Chasing, Attacking, CastingSkill, Dead }

    [Header("스탯")]
    public float maxHP = 3000;
    private float currentHP;
    public float attackPower = 60;
    public float moveSpeed = 7f;

    [Header("감지 및 공격 범위")]
    public float detectionRange = 15f;
    public float normalAttackRange = 3f;
    public float skillMinRange = 4f;
    public float skillMaxRange = 7f;


    [Header("공격 설정")]
    public float attackCooldown = 2f;
    private float lastAttackTime;

    [Header("흑뢰 스킬")]
    public GameObject blackLightningPrefab;
    public float singleCastTime = 1f; // 시전시간

    [Header("다중 흑뢰 스킬")]
    public float multiCastTime = 1f; // 시전시간
    public int multiLightningCount = 3; // 다중 흑뢰 개수
    public float multiLightningInterval = 0.5f; // 다중 흑뢰 간격
    public float multiLightningArea = 2f; // 다중 흑뢰 범위

    [Header("공격 판정")]
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1, 1);
    [SerializeField] private Vector2 attackBoxOffset = new Vector2(0.5f, 0.5f);
    [SerializeField] private LayerMask playerLayer;


    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        player = GameObject.FindGameObjectWithTag("Player").transform; // 태그로 플레이어 찾기
    }

    void Start()
    {
        currentHP = maxHP;
        lastAttackTime = -attackCooldown; // 시작하자마자 공격 가능하도록
    }

    void Update()
    {
        if (player == null || currentState == BossState.Dead)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        switch (currentState)
        {
            case BossState.Idle:
                UpdateIdleState();
                break;
            case BossState.Chasing:
                UpdateChasingState();
                break;
            case BossState.Attacking:
                UpdateAttackingState();
                break;
            case BossState.CastingSkill:
                break;
        }
    }

    void UpdateIdleState()
    {
        // 플레이어가 감지 범위 안에 들어오면 추적 시작
        if (Vector2.Distance(transform.position, player.position) < detectionRange)
        {
            ChangeState(BossState.Chasing);
        }
    }

    void UpdateChasingState() // 추적 상태 업데이트
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        MoveTowardsPlayer();

        // 플레이어가 멀면 원거리공격, 가까우면 근접공격
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // 원거리 스킬 범위
            if (distanceToPlayer >= skillMinRange && distanceToPlayer <= skillMaxRange)
            {
                ChangeState(BossState.CastingSkill);
                StartCoroutine(CastSkill());
            }
            // 일반 공격 범위
            else if (distanceToPlayer <= normalAttackRange)
            {
                ChangeState(BossState.Attacking);
                StartCoroutine(PerformNormalAttack());
            }
        }
    }

    void UpdateAttackingState()
    {
        rb.velocity = Vector2.zero;
    }


    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

        if (direction.x != 0)
        {
            float sign = Mathf.Sign(direction.x);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * sign, transform.localScale.y, transform.localScale.z);
        }

        animator.SetBool("isWalking", true);
    }

    IEnumerator PerformNormalAttack()
    {
        animator.SetTrigger("attack");
        lastAttackTime = Time.time;

        yield return new WaitForSeconds(0.5f);

        Vector2 boxCastOrigin = (Vector2)transform.position + new Vector2(attackBoxOffset.x * transform.localScale.x, attackBoxOffset.y);
        RaycastHit2D hit = Physics2D.BoxCast(boxCastOrigin, attackBoxSize, 0f, Vector2.zero, 0f, playerLayer);

        if (hit.collider != null)
        {
            hit.collider.GetComponent<PlayerStats>().TakeDamage(attackPower);
        }

        yield return new WaitForSeconds(0.5f);

        ChangeState(BossState.Chasing);
    }

    IEnumerator CastSkill()
    {
        animator.SetTrigger("castMagic");
        lastAttackTime = Time.time;
        rb.velocity = Vector2.zero; // 캐스팅 중 이동 정지
        animator.SetBool("isWalking", false);

        // 체력이 50% 미만이면 다중 흑뢰, 아니면 일반 흑뢰
        if (currentHP / maxHP < 0.5f)
        {
            yield return StartCoroutine(CastMultiBlackLightning());
        }
        else
        {
            yield return StartCoroutine(CastSingleBlackLightning());
        }

        ChangeState(BossState.Chasing);
    }

    IEnumerator CastSingleBlackLightning()
    {
        yield return new WaitForSeconds(singleCastTime);

        if (blackLightningPrefab != null && player != null)
        {
            // 플레이어 위치에 번개 생성
            GameObject spell = Instantiate(blackLightningPrefab, player.position, Quaternion.identity);
            BODSpell bodSpell = spell.GetComponent<BODSpell>();
            if (bodSpell != null)
            {
                bodSpell.damage = attackPower * 2.0f;
            }
        }
    }

    IEnumerator CastMultiBlackLightning()
    {
        yield return new WaitForSeconds(multiCastTime);

        for (int i = 0; i < multiLightningCount; i++)
        {
            if (blackLightningPrefab != null && player != null)
            {
                Vector2 randomPos = (Vector2)player.position + Random.insideUnitCircle * (multiLightningArea / 2);
                GameObject spell = Instantiate(blackLightningPrefab, randomPos, Quaternion.identity);
                BODSpell bodSpell = spell.GetComponent<BODSpell>();
                if (bodSpell != null)
                {
                    bodSpell.damage = attackPower * 1.6f;
                }
            }
            yield return new WaitForSeconds(multiLightningInterval);
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentState == BossState.Dead) return;

        currentHP -= damage;
        animator.SetTrigger("hurt");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        ChangeState(BossState.Dead);
        animator.SetTrigger("death");
        rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false; // 보스가 죽으면 콜라이더 끄기
        Destroy(gameObject, 3f);
    }

    void ChangeState(BossState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (currentState)
        {
            case BossState.Idle:
                animator.SetBool("isWalking", false);
                rb.velocity = Vector2.zero;
                break;
            case BossState.Chasing:
                animator.SetBool("isWalking", true);
                break;
            case BossState.Attacking:
            case BossState.CastingSkill:
                animator.SetBool("isWalking", false);
                rb.velocity = Vector2.zero;
                break;
            case BossState.Dead:
                break;
        }
    }
}

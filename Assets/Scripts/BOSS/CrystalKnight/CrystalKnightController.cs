using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CrystalKnightController : MonoBehaviour
{
    public enum State { Idle, Chase, PrepareAttack, MeleeAttack, RangedAttack, Wait, Transform, Summon, Die }
    public State currentState = State.Idle;

    [Header("1페이즈 스탯")]
    public float maxHP = 3625f;
    public float currentHP;
    public float attackPower = 48f;
    public float moveSpeed = 2f;

    [Header("2페이즈 스탯 강화")]
    public float phase2AttackPowerMultiplier = 1.5f;
    public float phase2MoveSpeedMultiplier = 1.2f;
    public float phase2AttackCooldownMultiplier = 0.7f;

    [Header("감지 및 공격 범위")]
    public float detectRange = 10f;
    public float attackRange = 1.5f;
    public float rangedAttackMinRange = 3f;
    public float rangedAttackMaxRange = 7f;
    public float idealHoldingDistance = 4f;

    private Transform player;
    private Animator animator;
    private float lastAttackTime;
    private float lastSummonTime;
    private bool isPhase2 = false;

    [Header("원거리 공격 설정")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float rangedAttackDamageMultiplier = 1.2f;

    [Header("분신 소환 설정 (2페이즈)")]
    public GameObject clonePrefab;
    public Transform[] summonPositions;
    public float summonCooldown = 15f;

    [Header("공격 쿨타임")]
    public float attackCooldown = 2f;

    [Header("패턴 설정")]
    public float prepareAttackDuration = 0.5f;
    public float waitAfterAttackDuration = 1.0f;
    [SerializeField] private GameObject hpUI;
    [SerializeField] private Image fillHP;

    void Awake()
    {
        currentHP = maxHP;
        player = GameObject.FindWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        ChangeState(State.Idle);
        lastSummonTime = -summonCooldown;
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
            case State.PrepareAttack: StartCoroutine(PrepareAttackState()); break;
            case State.MeleeAttack: StartCoroutine(MeleeAttackState()); break;
            case State.RangedAttack: StartCoroutine(RangedAttackState()); break;
            case State.Wait: StartCoroutine(WaitState()); break;
            case State.Transform: StartCoroutine(TransformState()); break;
            case State.Summon: StartCoroutine(SummonState()); break;
            case State.Die: StartCoroutine(DieState()); break;
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentState == State.Die || currentState == State.Transform) return;
        currentHP -= damage;
        fillHP.fillAmount = currentHP / maxHP;
        if (!isPhase2 && currentHP / maxHP <= 0.4f)
        {
            ChangeState(State.Transform);
        }
        else if (currentHP <= 0)
        {
            ChangeState(State.Die);
        }
    }


    IEnumerator IdleState()
    {
        animator.SetBool("Move", false);
        while (true)
        {
            if (player != null && Vector2.Distance(transform.position, player.position) <= detectRange)
            {
                ChangeState(State.Chase);
            }
            yield return null;
        }
    }

    IEnumerator ChaseState()
    {
        while (true)
        {
            if (player == null) { ChangeState(State.Idle); yield break; }

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer > idealHoldingDistance)
            {
                MoveTowardsPlayer();
                animator.SetBool("Move", true);
            }
            else
            {
                animator.SetBool("Move", false);
            }

            if (isPhase2 && Time.time >= lastSummonTime + summonCooldown)
            {
                ChangeState(State.Summon);
                yield break;
            }

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                if (distanceToPlayer <= rangedAttackMaxRange)
                {
                    ChangeState(State.PrepareAttack);
                    yield break;
                }
            }

            if (distanceToPlayer > detectRange)
            {
                ChangeState(State.Idle);
            }
            yield return null;
        }
    }

    IEnumerator PrepareAttackState()
    {
        animator.SetBool("Move", false);
        yield return new WaitForSeconds(prepareAttackDuration);

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist >= rangedAttackMinRange && dist <= rangedAttackMaxRange)
        {
            ChangeState(State.RangedAttack);
        }
        else if (dist <= attackRange)
        {
            ChangeState(State.MeleeAttack);
        }
        else
        {
            ChangeState(State.Chase);
        }
    }

    IEnumerator MeleeAttackState()
    {
        animator.SetTrigger("Attack");
        lastAttackTime = Time.time;
        yield return null;
        float animLen = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLen);
        ChangeState(State.Wait);
    }

    IEnumerator RangedAttackState()
    {
        animator.SetTrigger("Attack2");
        lastAttackTime = Time.time;
        if (projectilePrefab != null && firePoint != null && player != null)
        {
            Vector2 dir = (player.position - firePoint.position).normalized;
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            CKSpell spell = proj.GetComponent<CKSpell>();
            if (spell != null)
            {
                spell.SetDirection(dir);
                spell.damage = attackPower * rangedAttackDamageMultiplier;
            }
        }
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        ChangeState(State.Wait);
    }

    IEnumerator WaitState()
    {
        animator.SetBool("Move", false);
        yield return new WaitForSeconds(waitAfterAttackDuration);
        ChangeState(State.Chase);
    }

    IEnumerator TransformState()
    {
        animator.SetBool("Move", false);
        animator.SetTrigger("Transform");
        yield return null;
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength);
        isPhase2 = true;
        attackPower *= phase2AttackPowerMultiplier;
        moveSpeed *= phase2MoveSpeedMultiplier;
        attackCooldown *= phase2AttackCooldownMultiplier;
        lastSummonTime = Time.time;
        ChangeState(State.Chase);
    }

    IEnumerator SummonState()
    {
        animator.SetBool("Move", false);
        lastSummonTime = Time.time;
        animator.SetTrigger("Summon");
        yield return new WaitForSeconds(1.0f);
        foreach (Transform pos in summonPositions)
        {
            if (clonePrefab != null) Instantiate(clonePrefab, pos.position, pos.rotation);
        }
        yield return new WaitForSeconds(0.5f);
        ChangeState(State.Wait);
    }

    IEnumerator DieState()
    {
        hpUI.SetActive(false);
        animator.SetBool("Move", false);
        animator.SetTrigger("Die");
        Destroy(gameObject, 2f);
        yield return null;
    }


    void MoveTowardsPlayer()
    {
        if (player == null) return;
        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)dir * moveSpeed * Time.deltaTime;
        if (dir.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (dir.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    public void OnMeleeAttackHit()
    {
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            player.GetComponent<PlayerStats>()?.TakeDamage(attackPower);
        }
    }
}

using UnityEngine;
using System.Collections;

public class CrystalKnightController : MonoBehaviour
{
    [Header("스탯")]
    public float maxHP = 3625f;
    public float currentHP;
    public float attackPower = 48f;
    public float moveSpeed = 2f;
    public float detectRange = 10f;

    private Transform player;
    private Animator animator;
    private bool isAttacking = false;

    [Header("공격")]
    public GameObject attackPrefab;
    public Transform firePoint;
    public float attackCooldown = 2f;
    public float rangedAttackMinRange = 3f;
    public float rangedAttackMaxRange = 5f;
    public float projectileSpeed = 10f;

    [Header("근거리 공격 (콜라이더 기반)")]
    public float meleeDamageInterval = 1f; // 근접 공격 데미지 간격
    private Coroutine currentMeleeDamageCoroutine;

    void Awake()
    {
        currentHP = maxHP;
        player = GameObject.FindWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 플레이어 감지 및 추적
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectRange)
        {
            MoveTowardsPlayer();

            if (!isAttacking)
            {
                StartCoroutine(AttackRoutine(distanceToPlayer));
            }
        }
    }

    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    IEnumerator AttackRoutine(float distance)
    {
        isAttacking = true;

        // 근접 공격은 OnTriggerEnter2D에서 처리되므로 여기서는 제외
        if (distance >= rangedAttackMinRange && distance <= rangedAttackMaxRange)
        {
            yield return StartCoroutine(DoRangedAttack());
        }
        // 만약 어떤 공격도 해당되지 않으면, 쿨타임만 적용하고 다음 공격 기회 대기
        else
        {
            isAttacking = false; // 공격을 하지 않았으므로 isAttacking을 바로 해제
            yield break; // 코루틴 종료
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    IEnumerator DoMeleeAttack()
    {
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    // 콜라이더 진입 시 근접 공격 시작
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetTrigger("Attack");

            if (currentMeleeDamageCoroutine != null)
            {
                StopCoroutine(currentMeleeDamageCoroutine);
            }

            currentMeleeDamageCoroutine = StartCoroutine(ApplyMeleeDamageOverTime(other.gameObject));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentMeleeDamageCoroutine != null)
            {
                StopCoroutine(currentMeleeDamageCoroutine);
                currentMeleeDamageCoroutine = null;
            }
        }
    }

    // 시간 간격으로 데미지 적용
    IEnumerator ApplyMeleeDamageOverTime(GameObject playerObject)
    {
        PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats component not found on player object!");
            yield break;
        }

        while (true)
        {
            playerStats.TakeDamage(attackPower * 0.7f);
            yield return new WaitForSeconds(meleeDamageInterval);
        }
    }

    IEnumerator DoRangedAttack()
    {
        if (animator == null) yield break;
        animator.SetTrigger("Attack2");
        // 투사체 생성 로직
        if (attackPrefab != null && firePoint != null)
        {
            Vector2 direction = (player.position - firePoint.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject projectile = Instantiate(attackPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * projectileSpeed;
            }
        }
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
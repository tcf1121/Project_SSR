using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemController : MonoBehaviour
{
    [Header("스탯")]
    public float maxHP = 5000;
    public float currentHP;
    public float attackPower = 40;
    public float moveSpeed = 4f;
    public float detectRange = 10f; // 감지 범위

    [Header("공격")]
    public GameObject attackPrefab; // 팔 프리팹 넣으면 될듯
    public Transform firePoint;
    public float attackCooldown = 3f;

    [Header("레이저")]
    public GameObject laserPrefab;
    public float laserChargeTime = 2f; // 충전 시간
    public float laserDuration = 3f; // 레이저 지속 시간

    [Header("단단해지기")]
    public float hardenDuration = 3f;
    private bool isHarden = false;

    private Transform player;
    private Animator animator;
    private bool isAttacking = false;

    void Start()
    {
        currentHP = maxHP;
        player = GameObject.FindWithTag("Player").transform;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float Distance = Vector2.Distance(transform.position, player.position);

        if (!isAttacking)
        {
            MoveTowardsPlayer();
        }

        if (Distance <= detectRange && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    void MoveTowardsPlayer() // 플레이어가 걸어서 못가는 곳가면 수정필요
    {
        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // 랜덤 패턴 선택
        int random = Random.Range(0, 3);

        switch (random)
        {
            case 0:
                yield return StartCoroutine(DoNormalAttack());
                break;
            case 1:
                yield return StartCoroutine(DoHarden());
                break;
            case 2:
                yield return StartCoroutine(DoLaserAttack());
                break;
        }

        yield return new WaitForSeconds(1f); // 패턴 간 대기
        isAttacking = false;
    }

    IEnumerator DoNormalAttack()
    {
        animator.SetTrigger("Attack");

        Vector2 dir = (player.position - firePoint.position).normalized;
        GameObject bullet = Instantiate(attackPrefab, firePoint.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().velocity = dir * 10f; // 속도 조절

        yield return new WaitForSeconds(attackCooldown);
    }

    IEnumerator DoHarden()
    {
        isHarden = true;
        animator.SetTrigger("Harden");
        yield return new WaitForSeconds(hardenDuration);
        isHarden = false;
    }

    IEnumerator DoLaserAttack()
    {
        animator.SetTrigger("ChargeLaser");

        yield return new WaitForSeconds(laserChargeTime);

        GameObject laser = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);
        laser.transform.right = (player.position - firePoint.position).normalized;

        yield return new WaitForSeconds(laserDuration);
        Destroy(laser);
    }

    public void TakeDamage(float damage)
    {
        if (isHarden)
            damage *= 0.1f;

        currentHP -= damage;
        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        animator.SetTrigger("Die");
        // 죽는 연출 후 Destroy 가능
        Destroy(gameObject, 2f); // TODO : 시체 남겨야하나?
    }
}


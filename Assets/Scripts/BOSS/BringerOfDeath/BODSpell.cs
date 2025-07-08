using System.Collections;
using UnityEngine;

public class BODSpell : MonoBehaviour
{
    public float damage;
    private float activationDelay = 1f; // 공격 딜레이

    private Collider2D spellCollider;
    private Animator animator;

    void Start()
    {
        spellCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        if (spellCollider != null)
        {
            spellCollider.enabled = false;
        }

        // 1초 뒤 콜라이더를 활성화하는 코루틴 시작
        StartCoroutine(ActivateSpell());

        // 애니메이션 길이를 가져와서, 그 시간이 지나면 오브젝트를 파괴
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        Destroy(gameObject, animationLength);

    }

    IEnumerator ActivateSpell()
    {
        yield return new WaitForSeconds(activationDelay);

        if (spellCollider != null)
        {
            spellCollider.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
                Debug.Log($"BODSpell: 플레이어에게 {damage} 데미지를 입혔습니다.");
            }

            if (spellCollider != null) spellCollider.enabled = false; // 중복 데미지 방지를 위해 공격 후에 콜라이더 비활성화
        }
    }
}
using UnityEngine;

public class MeleeAttackBehavior : IAttackBehavior
{
    private Collider2D hitBox;
    private LayerMask playerLayer;
    private MonsterBrain brainRef;            // 마지막 Execute에서 전달된 Brain 저장

    public MeleeAttackBehavior(Collider2D hitBox, LayerMask playerLayer)
    {
        this.hitBox = hitBox;
        this.playerLayer = playerLayer;
    }

    public void Execute(MonsterBrain brain)
    {
        if (hitBox == null) return;

        brainRef = brain;

        ResizeHitBox();
        Debug.Log($"Execute – meleeRadius={brainRef.StatData.meleeRadius}");
    }

    // 애니메이션 이벤트에서 호출될 메서드
    public void ActivateHitBoxExternally(bool activate)
    {
        if (hitBox == null) return;

        if (activate)
        {
            ResizeHitBox();        // 한 번 더
            PerformMeleeAttack(brainRef);
        }

        hitBox.enabled = activate;
    }

    /*──────────────────────────────────────────────*/
    /* helper : StatData.meleeRadius → HitBox 크기  */
    /*──────────────────────────────────────────────*/
    private void ResizeHitBox()
    {
        if (brainRef == null || hitBox == null) return;

        float r = Mathf.Max(0.01f, brainRef.StatData.meleeRadius);
        Debug.Log($"[ResizeHitBox] type={hitBox.GetType().Name}, newR={r}");

        switch (hitBox)
        {
            case CircleCollider2D cc:
                cc.radius = r;
                break;

            case BoxCollider2D bc:
                bc.size = new Vector2(r * 0.25f, r * 0.25f);
                break;

            case CapsuleCollider2D cap:
                cap.size = new Vector2(r * 1f, r * 1f);
                break;
        }
    }

    // PerformMeleeAttack 메서드는 내부 사용만
    private void PerformMeleeAttack(MonsterBrain brainToUse)
    {
        if (hitBox == null || brainToUse == null) return;

        Vector2 hitBoxCenter = hitBox.bounds.center;
        Vector2 hitBoxSize = hitBox.bounds.size;   // ResizeHitBox로 이미 조정
        float hitBoxAngle = hitBox.transform.rotation.eulerAngles.z;

        foreach (var col in Physics2D.OverlapBoxAll(hitBoxCenter, hitBoxSize, hitBoxAngle, playerLayer))
        {
            if (col.TryGetComponent(out PlayerStats playerStats))
            {
                float damage = brainToUse.StatData.damage;
                playerStats.TakeDamage(damage);
                Debug.Log($"[{brainToUse.gameObject.name}] {col.name} (플레이어)에게 {damage} 데미지 적용!");
            }
        }
    }
}
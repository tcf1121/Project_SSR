using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

public class PlayerProjectile : Projectile
{
    private int _damage;
    [SerializeField] private Rigidbody2D rigid;
    [SerializeField] private Animator animator;
    [SerializeField] private Vector2 veloc;
    private bool isAttack;

    public void Launch(int damage, Vector2 dir)
    {
        _damage = damage;
        rigid.velocity = (dir + veloc) * speed;
        Vector3 scale = transform.localScale;
        scale.x = dir == Vector2.right ? scale.x : scale.x * -1f;
        transform.localScale = scale;
        alive = 0;
        isAttack = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            rigid.velocity = Vector2.zero;
            if (animator != null)
            {
                if (!isAttack)
                {
                    animator.SetTrigger("Attack");
                    isAttack = true;
                }

                else
                    collision.gameObject.GetComponent<Monster>().GetDamage(_damage);
            }
            else
            {
                collision.gameObject.GetComponent<Monster>().GetDamage(_damage);
                ProjectilePool.Instance.ReleasePlayer(this);
            }

        }
    }

    private void Update()
    {
        alive += Time.deltaTime;
        if (alive >= lifeTime)
        {
            rigid.velocity = Vector2.zero;
            ProjectilePool.Instance.ReleasePlayer(this);
        }
    }

    public void EndAni()
    {
        ProjectilePool.Instance.ReleasePlayer(this);
    }
}

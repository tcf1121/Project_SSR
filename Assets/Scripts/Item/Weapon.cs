using System.Collections.Generic;
using UnityEngine;
using Utill;


public class Weapon : MonoBehaviour
{
    public Player Player { get => _player; }
    [SerializeField] private Player _player;
    public ItemPart ItemPart { get => itemPart; }
    public float AttackCycle { get => attackCycle; }
    [SerializeField] private float attackCycle;
    public float DamageRatio { get => damageRatio; }
    [SerializeField] private float damageRatio;
    [SerializeField] private float _strengthening;
    [SerializeField] private ItemPart itemPart;
    [SerializeField] PlayerProjectile projectilePrefab;
    [SerializeField] Transform muzzlePoint;
    [SerializeField] List<Animator> _animator;

    public void SetPlayer(Player player)
    {
        _player = player;
    }

    public void Enhancement()
    {
        damageRatio += _strengthening;
    }
    void OnEnable()
    {

    }
    public void Attack()
    {
        if (projectilePrefab != null)
        {
            Vector2 dir = _player.PlayerController.FacingRight ? Vector2.right : Vector2.left;
            ProjectilePool pool = ProjectilePool.Instance;
            PlayerProjectile p = pool.GetPlayer(projectilePrefab, muzzlePoint.position);
            p.Launch((int)(_player.PlayerStats.FinalStats.Atk * damageRatio), dir);
        }


        else
        {
            foreach (Animator animator in _animator)
                animator.SetTrigger("Attack");
        }

    }
}


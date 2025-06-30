using System.Collections;
using System.Collections.Generic;
using SCR;
using UnityEngine;

namespace SCR
{
    public class Weapon : MonoBehaviour
    {
        public Player Player { get => _player; }
        private Player _player;
        public ItemPart ItemPart { get => itemPart; }
        public float AttackCycle { get => attackCycle; }
        [SerializeField] private float attackCycle;
        public float DamageRatio { get => damageRatio; }
        [SerializeField] private float damageRatio;
        [SerializeField] private float _strengthening;
        [SerializeField] private ItemPart itemPart;

        [SerializeField] List<Animator> _animator;

        public void SetPlayer(Player player)
        {
            _player = player;
        }

        public void Enhancement()
        {
            damageRatio += _strengthening;
        }

        public void Attack()
        {
            foreach (Animator animator in _animator)
                animator.SetTrigger("Attack");
        }
    }
}


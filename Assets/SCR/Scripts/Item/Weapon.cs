using System.Collections;
using System.Collections.Generic;
using SCR;
using UnityEngine;

namespace SCR
{
    public class Weapon : MonoBehaviour
    {
        public ItemPart ItemPart { get => itemPart; }
        public float AttackCycle { get => attackCycle; }
        [SerializeField] private float attackCycle;
        public float DamageRatio { get => damageRatio; }
        [SerializeField] private float damageRatio;
        [SerializeField] private ItemPart itemPart;
        [SerializeField] private List<BoxCollider2D> _AttackCollider;
        [SerializeField] Animator _animator;


        public void Attack()
        {
            _animator.SetTrigger("Attack");
        }

        public void AttackOnAll()
        {
            for (int i = 0; i < _AttackCollider.Count; i++)
                AttackOn(i);
        }

        public void AttackOffAll()
        {
            for (int i = 0; i < _AttackCollider.Count; i++)
                AttackOff(i);
        }


        public void AttackOn(int index)
        {
            _AttackCollider[index].enabled = true;
        }

        public void AttackOff(int index)
        {
            _AttackCollider[index].enabled = false;
        }
    }
}


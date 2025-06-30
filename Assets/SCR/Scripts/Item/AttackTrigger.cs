using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class AttackTrigger : MonoBehaviour
    {
        [SerializeField] private Weapon weapon;
        [SerializeField] private BoxCollider2D _attackCollider;

        void OnTriggerEnter2D(Collider2D collision)
        {

            if (collision.gameObject.CompareTag("Monster"))
            {
                Debug.Log($"데미지 {weapon.Player.PlayerStats.FinalStats.Atk * weapon.DamageRatio}");
                return;
            }
        }

        public void AttackOn()
        {
            _attackCollider.enabled = true;
        }

        public void AttackOff()
        {
            _attackCollider.enabled = false;
        }
    }
}

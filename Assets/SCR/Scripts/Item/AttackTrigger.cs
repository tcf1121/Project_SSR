using System.Collections;
using System.Collections.Generic;
using PHG;
using UnityEngine;

namespace SCR
{
    public class AttackTrigger : MonoBehaviour
    {
        [SerializeField] private Weapon weapon;

        void OnTriggerEnter2D(Collider2D collision)
        {
            Vector2 direction = collision.transform.position - GameManager.Player.transform.position;
            direction.Normalize();
            direction.x *= -1;
            HitInfo hitInfo = new HitInfo(
                (int)(weapon.Player.PlayerStats.FinalStats.Atk * weapon.DamageRatio),
                direction, true);


            if (collision.gameObject.CompareTag("Monster"))
            {
                collision.gameObject.GetComponent<MonsterBrain>().EnterDamageState(hitInfo);
                return;
            }
        }
    }
}

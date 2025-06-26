using System.Collections;
using System.Collections.Generic;
using LHE;
using UnityEngine;

namespace HCW
{
    public class Bullet : MonoBehaviour
    {
        public float damage = 40f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                var hp = other.GetComponent<PlayerStats>();

                if (hp != null)
                    hp.TakeDamage(damage);

                Destroy(gameObject);
            }
            else if (other.CompareTag("Ground"))
            {
                Destroy(gameObject);
            }

        }

        void Start()
        {
            Destroy(gameObject, 3f); // 기본적으로 일정 시간 후에 파괴
        }
    }
}

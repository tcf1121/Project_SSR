using System.Collections;
using System.Collections.Generic;
using LHE;
using UnityEngine;

namespace HCW
{
    public class LaserDamage : MonoBehaviour
    {
        public float damageMultiplier = 1.8f; // 데미지 배율
        public float tickInterval = 0.5f; // 틱 간격
        private float damage = 0f;

        // 이미 데미지 준 오브젝트 체크
        private HashSet<GameObject> damagedThisTick = new HashSet<GameObject>();

        public void Init(float bossAttackPower)
        {
            damage = bossAttackPower * damageMultiplier;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (!damagedThisTick.Contains(other.gameObject))
                {
                    var hp = other.GetComponent<PlayerStats>();
                    if (hp != null)
                        hp.TakeDamage(damage);

                    damagedThisTick.Add(other.gameObject);
                    StartCoroutine(ResetTick(other.gameObject));
                }
            }
        }

        IEnumerator ResetTick(GameObject obj)
        {
            yield return new WaitForSeconds(tickInterval);
            damagedThisTick.Remove(obj);
        }
    }
}
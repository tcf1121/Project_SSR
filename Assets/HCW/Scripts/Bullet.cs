using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HCW
{
    public class Bullet : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // 데미지 처리?
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

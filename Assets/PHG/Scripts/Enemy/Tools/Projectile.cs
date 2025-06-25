using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PHG
/// <summary>
/// 몬스터 발사체 ? 충돌·수명 종료 시 풀로 복귀
/// </summary>
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        //멤버변수
        [SerializeField] private float speed = 10f; //이동속도
        [SerializeField] private float lifeTime = 5f; //최대 생존 시간
        private Rigidbody2D rb;

        // 런타임
        private float alive;

        private void Awake() => rb = GetComponent<Rigidbody2D>();

        public void Launch(Vector2 dir)
        {
            alive = 0f;
            rb.velocity = dir.normalized * speed;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            alive += Time.deltaTime;
            if (alive >= lifeTime) ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if(col.CompareTag("Player"))
            {
                //TODO : 데미지
            }
            ReturnToPool();

        }

        private void ReturnToPool()
        {
            rb.velocity = Vector2.zero;
            ProjectilePool.Instance.Return(this);
        }



    }
}

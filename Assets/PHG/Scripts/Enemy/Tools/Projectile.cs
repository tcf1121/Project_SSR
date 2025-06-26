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
        public int PoolKey { get; set; }
        // 런타임
        private float alive;
        //디버그용
        private Vector2 lastDir;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
     
        }
        public void Launch(Vector2 dir)
        {
            alive = 0f;
            lastDir = dir.normalized;
            rb.velocity = dir.normalized * speed;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        }

        private void Update()
        {
            
            //실시간 방향 추적
            Debug.DrawRay(transform.position, lastDir * 0.5f, Color.cyan, 0.1f);
            alive += Time.deltaTime;
            if (alive >= lifeTime) ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
           
            if (col.CompareTag("Player"))
            {
                //TODO : 데미지
            ReturnToPool();
            }

        }

        private void ReturnToPool()
        {
            rb.velocity = Vector2.zero;
            ProjectilePool.Instance.Release(this);
        }



    }
}

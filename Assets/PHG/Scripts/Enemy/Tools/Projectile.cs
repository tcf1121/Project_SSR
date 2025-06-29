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
        private MonsterBrain brain;
        
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
        private void OnEnable()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
                Debug.Log("Awake단계에서 rb받아오기 실패");
            }
        }
        public void Launch(Vector2 dir, float newSpeed)
        {
            if(rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
                Debug.Log("Awake, OnEnable단계에서 rb받아오기 실패");
                if (rb == null) // 재시도 후에도 null이면 더 이상 진행할 수 없으므로 종료
                {
                    Debug.LogError($"[Projectile {gameObject.name}] Launch() 재시도 후에도 Rigidbody2D가 여전히 NULL입니다. 발사 중단.", this);
                    return;
                }
            }
            alive = 0f;
            lastDir = dir.normalized;
            this.speed = newSpeed;
            rb.velocity = dir.normalized * this.speed;

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
            // 풀로 반환하기 전에 Rigidbody2D의 velocity를 0으로 설정하여 잔류 움직임을 방지
            // 이 시점에도 rb가 null일 수 있으므로 null 체크
            if (rb == null) Debug.LogWarning($"[Projectile {gameObject.name}] ReturnToPool() 중 Rigidbody2D가 NULL입니다. Velocity를 0으로 설정할 수 없습니다.", this);
            else rb.velocity = Vector2.zero;

            ProjectilePool.Instance.Release(this); // 풀에 투사체 반환
        }



    }
}

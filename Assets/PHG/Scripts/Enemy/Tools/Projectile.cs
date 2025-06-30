using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PHG
/// <summary>
/// ���� �߻�ü ? �浹������ ���� �� Ǯ�� ����
/// </summary>
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        //�������
        [SerializeField] private float speed = 10f; //�̵��ӵ�
        [SerializeField] private float lifeTime = 5f; //�ִ� ���� �ð�
        private MonsterBrain brain;
        
        private Rigidbody2D rb;
        public int PoolKey { get; set; }
        // ��Ÿ��
        private float alive;
        //����׿�
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
                Debug.Log("Awake�ܰ迡�� rb�޾ƿ��� ����");
            }
        }
        public void Launch(Vector2 dir, float newSpeed)
        {
            if(rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
                Debug.Log("Awake, OnEnable�ܰ迡�� rb�޾ƿ��� ����");
                if (rb == null) // ��õ� �Ŀ��� null�̸� �� �̻� ������ �� �����Ƿ� ����
                {
                    Debug.LogError($"[Projectile {gameObject.name}] Launch() ��õ� �Ŀ��� Rigidbody2D�� ������ NULL�Դϴ�. �߻� �ߴ�.", this);
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
            
            //�ǽð� ���� ����
            Debug.DrawRay(transform.position, lastDir * 0.5f, Color.cyan, 0.1f);
            alive += Time.deltaTime;
            if (alive >= lifeTime) ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
           
            if (col.CompareTag("Player"))
            {
                //TODO : ������
            ReturnToPool();
            }

        }

        private void ReturnToPool()
        {
            // Ǯ�� ��ȯ�ϱ� ���� Rigidbody2D�� velocity�� 0���� �����Ͽ� �ܷ� �������� ����
            // �� �������� rb�� null�� �� �����Ƿ� null üũ
            if (rb == null) Debug.LogWarning($"[Projectile {gameObject.name}] ReturnToPool() �� Rigidbody2D�� NULL�Դϴ�. Velocity�� 0���� ������ �� �����ϴ�.", this);
            else rb.velocity = Vector2.zero;

            ProjectilePool.Instance.Release(this); // Ǯ�� ����ü ��ȯ
        }



    }
}

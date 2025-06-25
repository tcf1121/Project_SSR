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
        private Rigidbody2D rb;

        // ��Ÿ��
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
                //TODO : ������
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

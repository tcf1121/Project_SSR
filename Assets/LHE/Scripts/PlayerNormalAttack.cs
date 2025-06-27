using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace LHE
{

    public class PlayerNormalAttack : MonoBehaviour
    {
        [Header("���� ����")]
        [SerializeField] private float attackDuration = 0.2f; // ���� ���� �ð� �ִϸ��̼ǿ� ���� ����
        [SerializeField] private float attackCooldown = 1f; // ���� ��Ÿ�� (���� ���⿡���� ����ǵ��� ����)
        [SerializeField] private LayerMask enemyLayerMask = 1 << 12; // ������ �ν��� ���̾�

        [Header("���� ����")]
        [SerializeField] private Collider2D attackCollider; // ���� ���� �ݶ��̴�
        [SerializeField] private Transform attackPoint; // ���� ��ġ

        [Header("�����")]
        [SerializeField] private bool showAttackRange = true;

        // ===== ������Ʈ ���� =====
        private PlayerController playerController;
        private PlayerStats playerStats;

        // ===== ���� ���� =====
        private bool isAttacking = false;
        private bool canAttack = true;
        private float attackCooldownTimer = 0f;

        // ==== �ߺ� ������ ���� ��ü� ====
        private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>(); 

        #region ����Ƽ �ֱ�
        void Awake()
        {
            playerController = GetComponent<PlayerController>();
            playerStats = GetComponent<PlayerStats>();

            if (attackCollider != null) // �ʱ� ��Ȱ��ȭ
                attackCollider.enabled = false;
        }

        private void Update()
        {
            HandleAttackCooldown();
        }
        #endregion

        #region �Է� ó��
        public void OnNormalAttack()
        {
            if (CanPerformAttack()) // ���� ���ɿ��� üũ
            {
                StartCoroutine(AttackSequence());
            }
        }
        #endregion

        #region ���� ó��
        /// <summary>
        /// ���� ���� ���� ���� �Ǻ�
        /// </summary>
        /// <returns>���� ��ȯ</returns>
        private bool CanPerformAttack()
        {
            if (canAttack && !isAttacking && !playerController.isWallSliding &&
                !playerController.isClimbing && !playerController.isDashing && !playerStats.isDead)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// �Ϲ� ���� ������ �ڷ�ƾ
        /// </summary>
        private IEnumerator AttackSequence()
        {
            StartAttack();  // ���� ����

            yield return new WaitForSeconds(attackDuration); // ���

            EndAttack(); // ���� ����
        }

        /// <summary>
        /// �Ϲ� ���� ����
        /// </summary>
        private void StartAttack()
        {
            isAttacking = true;

            // �Ϲ� ���� �Ҹ� �� ��Ÿ�� ����
            canAttack = false;
            attackCooldownTimer = attackCooldown;

            hitEnemies.Clear(); // ����Ʈ �ʱ�ȭ

            // ���� �ݶ��̴� Ȱ��ȭ
            if (attackCollider != null)
                attackCollider.enabled = true;

            // ���� ���� ���ϸ��̼� �߰�
            // animator.SetTrigger("Attack");
        }

        /// <summary>
        /// �Ϲ� ���� ����
        /// </summary>
        private void EndAttack()
        {
            isAttacking = false;

            // ���� �ݶ��̴� ��Ȱ��ȭ
            if (attackCollider != null)
                attackCollider.enabled = false;
        }    

        /// <summary>
        /// ���� ��Ÿ�� ó��
        /// </summary>
        private void HandleAttackCooldown()
        {
            if (!canAttack)
            {
                attackCooldownTimer -= Time.deltaTime;
                if (attackCooldownTimer <= 0f)
                {
                    canAttack = true ;
                }
            }
        }

        #endregion 

        #region �浹 ó��
        /// <summary>
        /// ���� �ݶ��̴� Ʈ����
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isAttacking) return; // �������� �ƴϸ� ����

            if (hitEnemies.Contains(other)) return; // �ߺ� ����

            if (!IsEnemy(other)) return; // �����̾ �ƴϸ� ����

            DealDamageToEnemy(other);
        }

        /// <summary>
        /// �� ���̾ ������ �ִ��� Ȯ��
        /// </summary>
        /// <param name="collider">Ȯ���� ������Ʈ�� �ݶ��̴�</param>
        /// <returns>�� ���̾� ����</returns>
        private bool IsEnemy(Collider2D collider)
        {
            // �ε��� �ݶ��̴��� ������Ʈ ���̾� �����ͼ� ���̾� ��ȣ�� ���������� ��
            return ((1 << collider.gameObject.layer) & enemyLayerMask) != 0;
        }

        /// <summary>
        /// ������ ������ ó�� (���� ��ũ��Ʈ Ȯ�� �ʿ�
        /// </summary>
        /// <param name="enemy"></param>
        private void DealDamageToEnemy(Collider2D enemy)
        {
            hitEnemies.Add(enemy); // �ߺ� ���� ����Ʈ�� �߰�

            // �������� ������ ��� ó��
            Debug.Log("���� ������ ó���ʿ�!");
            // ��� ���� ��ũ��Ʈ �ľ� �ʿ�
        }
        #endregion

        #region ����׿� �����
        void OnDrawGizmosSelected()
        {
            if (!showAttackRange || attackCollider == null) return;

            // ���� ���� �ð�ȭ
            Gizmos.color = isAttacking ? Color.red : Color.yellow;
            BoxCollider2D boxCol = (BoxCollider2D)attackCollider;
            Gizmos.DrawWireCube(
                (Vector2)attackCollider.transform.position + boxCol.offset,
                Vector2.Scale(boxCol.size, attackCollider.transform.lossyScale));
        }
        #endregion

        // ����
        // ���� ���ӽð�
        // ���� ��Ÿ��
        // ���� ���̾�

        // ���� ������ ���� �ݶ��̴�
        // �� �ݶ��̴��� ��ġ

        // �����

        // ������Ʈ ��������

        // ������Ʈ���� ��Ÿ�� ó��

        // xŰ �Է�ó��

        // ���� ����
        // ���� ���� ����
        // ���� ����

        // ���� �ڷ�ƾ (���ݽ��࿡ ��)
        // �� ���� ���� , ��� , ���� ����

        // ���� ����
        // ������ ����, ��Ÿ�ӽ���, ���� �ݶ��̴� Ȱ��ȭ, �ִϸ��̼�

        // ��������
        // ������ �ݴ�

        // �浹 ó��
        // �������� �ƴϸ� ����
        // �����̾����� Ȯ��
        // �ߺ� ���� ���� �ʿ�
        // ������ ó��
        // 
        // ������ ó���� �ٸ����� § ���� ��ũ��Ʈ Ȯ���Ͽ� �����Ұ�

    }
}

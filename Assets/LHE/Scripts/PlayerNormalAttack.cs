using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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

        private enum Weapon
        {
            dagger = 1
            // ����� ������ ���⿡ ���� ���ݼӵ� (�������x)
        }

        // ===== ������Ʈ ���� =====
        private PlayerController playerController;
        private PlayerStats playerStats;
        private Weapon weapon;

        // ===== ���� ���� =====
        private bool isAttacking = false;
        private bool canAttack = true;
        private float attackCooldownTimer = 0f;

        // �ߺ����� �������� ����ϰ� �װ����� �ٽ� ���� ������ ���� �ϰ� ���� ���ӽð� �ٵǸ� ����

        #region ����Ƽ �ֱ�
        void Awake()
        {
            playerController = GetComponent<PlayerController>();
            playerStats = GetComponent<PlayerStats>();

            // ���� �ݶ��̴� ����
        }

        private void Update()
        {
            HandleAttackCooldown();
        }
        #endregion

        #region �Է� ó��

        private void OnNormalAttack(InputValue inputValue)
        {
            if (CanPerformAttack()) // ���� ���ɿ��� üũ
            {

            }
        }

        #endregion

        #region ���� ó��
        private bool CanPerformAttack()
        {
            // ���� ��Ÿ���� �Ǿ����, ���������� �ʾƾ���,
            // Ŭ�����x, ��Ÿ����x (�̰� ��ȹ�е鿡�� Ȯ�� �ޱ�)
            return 
        }

        private void PerformAttack()
        {
            // ���� �ڷ�ƾ ����
        }

        private IEnumerator AttackSequence()
        {
            StartAttack();  // ���� ����

            yield return new WaitForSeconds(attackDuration); // ���� �� ���

            EndAttack(); // ���� ����
        }

        private void StartAttack()
        {

        }

        private void EndAttack()
        {

            canAttack = false; // ������ ������ ��Ÿ�� ���� (�ƴϸ� �������� �̵�, ��ȹ�� ȸ��)
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
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isAttacking) return; // �������� �ƴϸ� ����

            // �ߺ� ���� ��� �ʿ�

            if (!IsEnemy(other)) return; // �����̾ �ƴϸ� ����

        }

        private bool IsEnemy(Collider2D collider)
        {
            // �ε��� �ݷ������� ������Ʈ ���̾� �����ͼ� ���̾� ��ȣ�� ���������� ��
            return ((1 << collider.gameObject.layer) & enemyLayerMask) != 0;
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

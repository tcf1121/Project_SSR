using System;
using UnityEngine;

namespace LHE
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("�ɸ��� �⺻ ����")] // �Ŀ� �����̺����� ����
        [SerializeField] public int level = 1;
        [SerializeField] private float currentHp;
        [SerializeField] private float currentExp = 0;
        [SerializeField] private float money = 0;

        [Header("�⺻ ����")]
        [SerializeField] public float maximumHp = 100f;
        [SerializeField] private float reqExp;
        [SerializeField] public float atk = 10f;
        [SerializeField] public float hpRegen = 1f;
        [SerializeField] public float speed = 7f;
        [SerializeField] public float jump = 1f;

        [Header("���� ����")]
        [SerializeField] private bool isDead = false;

        // ü�� ��� Ÿ�̸�
        private float regenTimer = 0f;
        private float regenInterval = 0.2f;

        // ���� �б� ���� ����
        public int Level => level;
        public float CurrentExp
        {
            get => currentExp;
            set
            {
                currentExp = value;
                LevelUpCheck();
            }
        }
        public float Money { get; set; }
        public float MaximumHp => maximumHp;
        public float Atk => atk;
        public float HPRegen => hpRegen;
        public float Speed => speed;
        public float Jump => jump;
        public float CurrentHp => currentHp;


        void Awake()
        {

        }
        void Start()
        {

        }

        private void FixedUpdate()
        {
            // ü�� ��� ����
        }

        #region ����ġ �� ���� ����
        public void LevelUpCheck()
        {
            while (currentExp >= reqExp)
            {
                currentExp -= reqExp;
                LevelUp();
            }
        }

        public void LevelUp ()
        {
            level++;
            RequiredExp(); // �ʿ����ġ ����
            LevelUpRecalculateStats();
            currentHp = maximumHp;
            // ���� ���� �˸� OnLevelUp?.Invoke(level);
        }

        /// <summary>
        /// �ʿ� ����ġ ��� �Ҽ� ù�ڸ����� �ݿø�
        /// </summary>
        public void RequiredExp()
        {
            float requiredExp = 30f; // 1���� �⺻ ����ġ

            for (int i = 2; i <= level; i++)
             {
                    requiredExp *= 1.6f;
            }

            reqExp = Mathf.Round(requiredExp * 10) * 0.1f;
        }
        #endregion

        #region ���� ��� ���� ���� 
        public void LevelUpRecalculateStats ()
        {
            LevelUpRecalculateHpStats();
            LevelUpRecalculateAtkStats();
            LevelUpRecalculateHPRegenStats();
        }

        public void LevelUpRecalculateHpStats()
        {
            float RecalculateHP = 100f; // 1���� �⺻ ü��

            for (int i = 2; i <= level; i++)
            {
                RecalculateHP += 33f;
            }

            maximumHp = Mathf.Round(RecalculateHP * 10) * 0.1f;
        }

        public void LevelUpRecalculateAtkStats()
        {
            float RecalculateAtk = 10f; // 1���� �⺻ ���ݷ�

            for (int i = 2; i <= level; i++)
            {
                RecalculateAtk += 2.5f;
            }

            atk = Mathf.Round(RecalculateAtk * 10) * 0.1f;
        }

        public void LevelUpRecalculateHPRegenStats()
        {
            float RecalculateHPRegen = 1f; // 1���� �⺻ ü��

            for (int i = 2; i <= level; i++)
            {
                RecalculateHPRegen += 0.2f;
            }

            hpRegen = Mathf.Round(RecalculateHPRegen * 10) * 0.1f;
        }
        #endregion

        #region ���� ����
        public void Die()
        {
            isDead = true;
        }

        public void Live()
        {
            isDead = false;
        }

        // ����
        #endregion

        #region HP ����
        /// <summary>
        /// ü�� ȸ��
        /// </summary>
        public void Heal(float healAmount)
        {
            float oldHp = currentHp;
            currentHp = Mathf.Min(currentHp + healAmount, MaximumHp);

            if (currentHp != oldHp)
            {
                // OnHpChanged?.Invoke(currentHp); ���� ü�� ���� �˸�
            }
        }

        /// <summary>
        /// ������ �ޱ�
        /// </summary>
        public void TakeDamage(float damage)
        {
            float oldHp = currentHp;
            currentHp = Mathf.Max(currentHp - damage, 0f);

            if (currentHp != oldHp)
            {
                // OnHpChanged?.Invoke(currentHp); ���� ü�� ���� �˸�
            }

            if (currentHp <= 0)
            {
                Die();
                // ��� ó�� ����
            }
        }

        public void PlayerHpRegen()
        {
            regenTimer += Time.deltaTime;////////////////////

        }

        /// <summary>
        /// ü�� �ִ� ȸ��
        /// </summary>
        public void FullHPRecovery()
        {
            currentHp = maximumHp;
        }

        /// <summary>
        /// HP ���� ��ȯ (0~1) _��� �����ۿ� ���� ü���� �����ϸ� ������ �����ϸ� �����ϵ��� �� _�ݴ뵵 ��������
        /// </summary>
        public float GetHpRatio() => currentHp / MaximumHp;
        #endregion

        //#region �� ����
        //public void GainMoney(float amount)
        //{
        //    money += amount;
        //}

        //public bool SpendMoney(float amount)
        //{
        //    if (money >= amount)
        //    {
        //        money -= amount;
        //        return true;
        //    }
        //    return false;
        //}
        //#endregion
    }
}
using System;
using UnityEngine;

namespace LHE
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("�ɸ��� �⺻ ����")]
        [SerializeField] public int level = 1;
        [SerializeField] public float currenExp = 0;
        [SerializeField] public float money = 0;

        [Header("�⺻ ����")]
        [SerializeField] public float maximumHp = 100f;
        [SerializeField] public float atk = 10f;
        [SerializeField] public float hpRegen = 1f;
        [SerializeField] public float speed = 7f;
        [SerializeField] public float jump = 1f;

        [Header("���� ����")]
        [SerializeField] private float currentHp;

        // ���� ���� ���� (�ܺο��� �б� ����)
        public float MaximumHp { get; private set; }
        public float CurrentHp => currentHp;
        public float Atk { get; private set; }
        public float HpRegen { get; private set; }
        public float Speed { get; private set; }
        public float Jump { get; private set; }

        public int Level => level;
        public float CurrenExp => currenExp;
        public float Money => money;

        // �������� �ʿ��� ����ġ ���
        public float RequiredExp => CalculateRequiredExp(level);

        // �̺�Ʈ �ý���
        public static event Action<int> OnLevelUp;
        public static event Action<float> OnHpChanged;
        public static event Action<float> OnExpGained;

        #region ����Ƽ �ֱ�
        void Awake()
        {
            RecalculateAllStats();
        }

        void Start()
        {
            currentHp = MaximumHp;
            OnHpChanged?.Invoke(currentHp);
        }
        #endregion

        #region ���� ���
        /// <summary>
        /// ��� ���� ����
        /// </summary>
        public void RecalculateAllStats()
        {
            MaximumHp = CalculateMaxHp();
            Atk = CalculateAtk();
            HpRegen = CalculateHpRegen();
            Speed = CalculateSpeed();
            Jump = CalculateJump();

            // HP�� �ִ�ġ�� �ʰ����� �ʵ���
            currentHp = Mathf.Min(currentHp, MaximumHp);
        }

        private float CalculateMaxHp() => maximumHp + (level - 1) * 33f;
        private float CalculateAtk() => atk + (level - 1) * 2.5f;
        private float CalculateHpRegen() => hpRegen + (level - 1) * 0.2f;
        private float CalculateSpeed() => speed;
        private float CalculateJump() => jump;

        private float CalculateRequiredExp(int currentLevel)
        {
            float requiredExp = 30f; // 1���� �⺻ ����ġ

            for (int i = 2; i <= currentLevel; i++)
            {
                requiredExp *= 1.6f;
            }

            return requiredExp;
        }
        #endregion

        #region ����ġ �� ���� ����
        /// <summary>
        /// ����ġ ȹ��
        /// </summary>
        public void GainExp(float expAmount)
        {
            currenExp += expAmount;
            OnExpGained?.Invoke(expAmount);

            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            while (currenExp >= RequiredExp && level < 99) // �ִ� ���� 99
            {
                currenExp -= RequiredExp;
                level++;

                RecalculateAllStats();
                currentHp = MaximumHp; // ������ �� ü�� ���� ȸ��

                OnLevelUp?.Invoke(level);
                OnHpChanged?.Invoke(currentHp);

                Debug.Log($"������! ���� ����: {level}");
            }
        }
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
                OnHpChanged?.Invoke(currentHp);
        }

        /// <summary>
        /// ������ �ޱ�
        /// </summary>
        public void TakeDamage(float damage)
        {
            currentHp = Mathf.Max(currentHp - damage, 0f);
            OnHpChanged?.Invoke(currentHp);

            if (currentHp <= 0)
            {
                Debug.Log("�÷��̾� ���!");
                // ��� ó�� ����
            }
        }

        /// <summary>
        /// HP ���� ��ȯ (0~1)
        /// </summary>
        public float GetHpRatio() => currentHp / MaximumHp;
        #endregion

        #region �� ����
        public void GainMoney(float amount)
        {
            money += amount;
        }

        public bool SpendMoney(float amount)
        {
            if (money >= amount)
            {
                money -= amount;
                return true;
            }
            return false;
        }
        #endregion
    }
}
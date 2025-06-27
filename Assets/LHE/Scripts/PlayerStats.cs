using UnityEngine;

namespace LHE
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("�ɸ��� �⺻ ����")] // �Ŀ� �����̺����� ����
        [SerializeField] private int level = 1;
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

        [Header("���ʽ� ���� (���/����)")]
        [SerializeField] private float bonusMaximumHp = 0f;
        [SerializeField] private float bonusAtk = 0f;
        [SerializeField] private float bonusHpRegen = 0f;
        [SerializeField] private float bonusSpeed = 0f;
        [SerializeField] private float bonusJump = 0f;

        [Header("���� ����")]
        [SerializeField] public bool isDead = false;

        // ü�� ��� Ÿ�̸�
        private float regenTimer = 0f;

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
            FullHPRecovery();
        }

        private void Update()
        {
            PlayerHpRegen();
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

        public void LevelUp()
        {
            level++;
            RequiredExp(); // �ʿ����ġ ����
            LevelUpRecalculateStats();
            currentHp = FinalMaximumHp;
            // ���� ���� �˸� OnLevelUp?.Invoke(level);
        }

        /// <summary>
        /// �ʿ� ����ġ ��� (�Ҽ� ù�ڸ����� �ݿø�)
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
        public void LevelUpRecalculateStats()
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
            // OnDie?.Invoke(isDead); ���� ���� ���� �˸� (��Ʈ�� ���߱�)
        }

        public void Live()
        {
            isDead = false;
            // OnDie?.Invoke(isDead); ���� ���� ���� �˸� (��Ʈ�� ����)
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
            currentHp = Mathf.Min(currentHp + healAmount, FinalMaximumHp);

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

        /// <summary>
        /// ü�� ���
        /// </summary>
        public void PlayerHpRegen()
        {
            if (isDead == true) { return; }
            if (currentHp == FinalMaximumHp) { return; }

            regenTimer += Time.deltaTime;

            if (regenTimer > 0.2)
            {
                regenTimer -= 0.2f;
                Heal(FinalHpRegen * 0.2f);
            }
        }

        /// <summary>
        /// ü�� �ִ����� ȸ��
        /// </summary>
        public void FullHPRecovery()
        {
            currentHp = FinalMaximumHp;
        }

        /// <summary>
        /// HP ���� ��ȯ (0~1) _��� �����ۿ� ���� ü���� �����ϸ� ������ �����ϸ� �����ϵ��� �� _�ݴ뵵 ��������
        /// </summary>
        public float GetHpRatio()
        {
            if (FinalMaximumHp <= 0)
                return 0f;
            return currentHp / FinalMaximumHp;
        }
        #endregion

        #region ��� �Ǵ� ���� �ܺ� ��ҿ� ���� ���� ����
        // ���ʽ� ���Ȱ������� ���� (�������ΰ� �ƴ϶�� ������� �������)
        public float BonusMaximumHp
        {
            get => bonusMaximumHp;
            set
            {
                if (bonusMaximumHp != value) // ���� ������ ����� ����
                {
                    float hpRatio = GetHpRatio();
                    bonusMaximumHp = value; 
                    currentHp = FinalMaximumHp * hpRatio;  

                    // OnHpChanged?.Invoke(currentHp); HP ���� �˸�
                }
            } 
        }
        public float BonusAtk
        {
            get => bonusAtk;
            set => bonusAtk = value;
        }

        public float BonusHpRegen
        {
            get => bonusHpRegen;
            set => bonusHpRegen = value;
        }

        public float BonusSpeed
        {
            get => bonusSpeed;
            set => bonusSpeed = value;
        }

        public float BonusJump
        {
            get => bonusJump;
            set => bonusJump = value;
        }
        #endregion

        #region ���� ���� ���� ����
        public float FinalMaximumHp { get => Mathf.Max(1, maximumHp + BonusMaximumHp); }
        public float FinalAtk { get => atk + BonusAtk; }
        public float FinalHpRegen { get => hpRegen + BonusHpRegen; }
        public float FinalSpeed { get => speed + BonusSpeed; }
        public float FinalJump { get => jump + BonusJump; }
        #endregion

        #region �� ����
        /// <summary>
        /// �� �Ҹ� �� ���
        /// </summary>
        /// <param name="amount">���</param>
        /// <returns>���� ���� ����</returns>
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
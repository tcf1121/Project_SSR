using System;
using UnityEngine;

namespace LHE
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("케릭터 기본 정보")]
        [SerializeField] public int level = 1;
        [SerializeField] public float currenExp = 0;
        [SerializeField] public float money = 0;

        [Header("기본 스탯")]
        [SerializeField] public float maximumHp = 100f;
        [SerializeField] public float atk = 10f;
        [SerializeField] public float hpRegen = 1f;
        [SerializeField] public float speed = 7f;
        [SerializeField] public float jump = 1f;

        [Header("현재 상태")]
        [SerializeField] private float currentHp;

        // 계산된 최종 스탯 (외부에서 읽기 전용)
        public float MaximumHp { get; private set; }
        public float CurrentHp => currentHp;
        public float Atk { get; private set; }
        public float HpRegen { get; private set; }
        public float Speed { get; private set; }
        public float Jump { get; private set; }

        public int Level => level;
        public float CurrenExp => currenExp;
        public float Money => money;

        // 레벨업에 필요한 경험치 계산
        public float RequiredExp => CalculateRequiredExp(level);

        // 이벤트 시스템
        public static event Action<int> OnLevelUp;
        public static event Action<float> OnHpChanged;
        public static event Action<float> OnExpGained;

        #region 유니티 주기
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

        #region 스탯 계산
        /// <summary>
        /// 모든 스탯 재계산
        /// </summary>
        public void RecalculateAllStats()
        {
            MaximumHp = CalculateMaxHp();
            Atk = CalculateAtk();
            HpRegen = CalculateHpRegen();
            Speed = CalculateSpeed();
            Jump = CalculateJump();

            // HP가 최대치를 초과하지 않도록
            currentHp = Mathf.Min(currentHp, MaximumHp);
        }

        private float CalculateMaxHp() => maximumHp + (level - 1) * 33f;
        private float CalculateAtk() => atk + (level - 1) * 2.5f;
        private float CalculateHpRegen() => hpRegen + (level - 1) * 0.2f;
        private float CalculateSpeed() => speed;
        private float CalculateJump() => jump;

        private float CalculateRequiredExp(int currentLevel)
        {
            float requiredExp = 30f; // 1레벨 기본 경험치

            for (int i = 2; i <= currentLevel; i++)
            {
                requiredExp *= 1.6f;
            }

            return requiredExp;
        }
        #endregion

        #region 경험치 및 레벨 관리
        /// <summary>
        /// 경험치 획득
        /// </summary>
        public void GainExp(float expAmount)
        {
            currenExp += expAmount;
            OnExpGained?.Invoke(expAmount);

            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            while (currenExp >= RequiredExp && level < 99) // 최대 레벨 99
            {
                currenExp -= RequiredExp;
                level++;

                RecalculateAllStats();
                currentHp = MaximumHp; // 레벨업 시 체력 완전 회복

                OnLevelUp?.Invoke(level);
                OnHpChanged?.Invoke(currentHp);

                Debug.Log($"레벨업! 현재 레벨: {level}");
            }
        }
        #endregion

        #region HP 관리
        /// <summary>
        /// 체력 회복
        /// </summary>
        public void Heal(float healAmount)
        {
            float oldHp = currentHp;
            currentHp = Mathf.Min(currentHp + healAmount, MaximumHp);

            if (currentHp != oldHp)
                OnHpChanged?.Invoke(currentHp);
        }

        /// <summary>
        /// 데미지 받기
        /// </summary>
        public void TakeDamage(float damage)
        {
            currentHp = Mathf.Max(currentHp - damage, 0f);
            OnHpChanged?.Invoke(currentHp);

            if (currentHp <= 0)
            {
                Debug.Log("플레이어 사망!");
                // 사망 처리 로직
            }
        }

        /// <summary>
        /// HP 비율 반환 (0~1)
        /// </summary>
        public float GetHpRatio() => currentHp / MaximumHp;
        #endregion

        #region 돈 관리
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
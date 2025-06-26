using System;
using UnityEngine;

namespace LHE
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("케릭터 기본 정보")] // 후에 프라이빗으로 변경
        [SerializeField] public int level = 1;
        [SerializeField] private float currentHp;
        [SerializeField] private float currentExp = 0;
        [SerializeField] private float money = 0;

        [Header("기본 스탯")]
        [SerializeField] public float maximumHp = 100f;
        [SerializeField] private float reqExp;
        [SerializeField] public float atk = 10f;
        [SerializeField] public float hpRegen = 1f;
        [SerializeField] public float speed = 7f;
        [SerializeField] public float jump = 1f;

        [Header("현재 상태")]
        [SerializeField] private bool isDead = false;

        // 체력 재생 타이머
        private float regenTimer = 0f;
        private float regenInterval = 0.2f;

        // 변수 읽기 쓰기 관리
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
            // 체력 재생 구현
        }

        #region 경험치 및 레벨 관리
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
            RequiredExp(); // 필요경험치 재계산
            LevelUpRecalculateStats();
            currentHp = maximumHp;
            // 레벨 변경 알림 OnLevelUp?.Invoke(level);
        }

        /// <summary>
        /// 필요 경험치 계산 소수 첫자리까지 반올림
        /// </summary>
        public void RequiredExp()
        {
            float requiredExp = 30f; // 1레벨 기본 경험치

            for (int i = 2; i <= level; i++)
             {
                    requiredExp *= 1.6f;
            }

            reqExp = Mathf.Round(requiredExp * 10) * 0.1f;
        }
        #endregion

        #region 레벨 기반 스탯 재계산 
        public void LevelUpRecalculateStats ()
        {
            LevelUpRecalculateHpStats();
            LevelUpRecalculateAtkStats();
            LevelUpRecalculateHPRegenStats();
        }

        public void LevelUpRecalculateHpStats()
        {
            float RecalculateHP = 100f; // 1레벨 기본 체력

            for (int i = 2; i <= level; i++)
            {
                RecalculateHP += 33f;
            }

            maximumHp = Mathf.Round(RecalculateHP * 10) * 0.1f;
        }

        public void LevelUpRecalculateAtkStats()
        {
            float RecalculateAtk = 10f; // 1레벨 기본 공격력

            for (int i = 2; i <= level; i++)
            {
                RecalculateAtk += 2.5f;
            }

            atk = Mathf.Round(RecalculateAtk * 10) * 0.1f;
        }

        public void LevelUpRecalculateHPRegenStats()
        {
            float RecalculateHPRegen = 1f; // 1레벨 기본 체력

            for (int i = 2; i <= level; i++)
            {
                RecalculateHPRegen += 0.2f;
            }

            hpRegen = Mathf.Round(RecalculateHPRegen * 10) * 0.1f;
        }
        #endregion

        #region 생명 관리
        public void Die()
        {
            isDead = true;
        }

        public void Live()
        {
            isDead = false;
        }

        // 리셋
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
            {
                // OnHpChanged?.Invoke(currentHp); 현재 체력 변경 알림
            }
        }

        /// <summary>
        /// 데미지 받기
        /// </summary>
        public void TakeDamage(float damage)
        {
            float oldHp = currentHp;
            currentHp = Mathf.Max(currentHp - damage, 0f);

            if (currentHp != oldHp)
            {
                // OnHpChanged?.Invoke(currentHp); 현재 체력 변경 알림
            }

            if (currentHp <= 0)
            {
                Die();
                // 사망 처리 로직
            }
        }

        public void PlayerHpRegen()
        {
            regenTimer += Time.deltaTime;////////////////////

        }

        /// <summary>
        /// 체력 최대 회복
        /// </summary>
        public void FullHPRecovery()
        {
            currentHp = maximumHp;
        }

        /// <summary>
        /// HP 비율 반환 (0~1) _장비 아이템에 의해 체력이 증가하면 비율을 유지하며 증가하도록 함 _반대도 마찬가지
        /// </summary>
        public float GetHpRatio() => currentHp / MaximumHp;
        #endregion

        //#region 돈 관리
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
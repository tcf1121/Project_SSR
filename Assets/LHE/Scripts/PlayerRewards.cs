using System;
using UnityEngine;


namespace LHE
{
    /// <summary>
    /// 플레이어에게 보상관련 클래스
    /// </summary>
    public class PlayerRewards : MonoBehaviour
    {
        private PlayerStats playerStats;

        void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
        }

        /// <summary>
        /// 보상 획득 (돈, 경험치) // 아이템 추후 추가 필요
        /// </summary>
        /// <param name="getMoney">돈 획득량</param>
        /// <param name="getExp">경험치 획득량</param>
        private void GetCompensation(float getMoney, float getExp)
        {
            GetMoney(getMoney);
            GetExp(getExp);
        }

        /// <summary>
        /// 돈 획득
        /// </summary>
        /// <param name="getMoney">획득량</param>
        private void GetMoney(float getMoney)
        {
            playerStats.money += getMoney;
        }

        /// <summary>
        /// 경험치 획득
        /// </summary>
        /// <param name="getExp">획득량</param>
        private void GetExp(float getExp)
        {
            playerStats.currenExp += getExp;
            LevelUpCheck();
        }

        /// <summary>
        /// 레벨업 체크
        /// </summary>
        private void LevelUpCheck()
        {
            if (playerStats.currenExp > playerStats.reqExp)
            {
                LevelUp();
                playerStats.currenExp -= playerStats.reqExp;
            }
        }

        /// <summary>
        /// 레벨업
        /// </summary>
        private void LevelUp()
        {
            playerStats.level++;
            RequiredLevelUp(playerStats.level);
        }

        /// <summary>
        /// 레벨업에 필요한 경험치 계산 레벨업 필요 경험치 (수정중)
        /// </summary>
        /// <param name="level">기준 레벨</param>
        private void RequiredLevelUp(int level)
        {
            // 대략적인 근사:  값 수정중 
            playerStats.reqExp = MathF.Round(0.571f * MathF.Pow(level, 3) * 100) / 100f; // 둘쨋자리에서 반올림
        }
    }
}

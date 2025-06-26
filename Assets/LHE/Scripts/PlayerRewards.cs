using System;
using UnityEngine;


namespace LHE
{
    /// <summary>
    /// 플레이어에게 보상관련 클래스 (돈, 겸험치, 아이템, 버프 등등)
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
            playerStats.Money += getMoney;
        }

        /// <summary>
        /// 경험치 획득
        /// </summary>
        /// <param name="getExp">획득량</param>
        private void GetExp(float getExp)
        {
            playerStats.CurrentExp += getExp;
            // LevelUpCheck();
        }

        // 아이템 획득
    }
}

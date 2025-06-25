using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LHE
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("케릭터 상태")]
        public int level;
        public float currentHp;
        

        [Header("케릭터 스탯")]
        public float maximumHp;
        public float atk;
        public float hpRegen;
        public float speed;
        public float jump;

        private float reqExp;
        private float stackExp;

        // 경험치함수
        int GetRequiredExp(int level)
        {
            // 대략적인 근사: regExp ≈ 5.7 * level ^ 1.85
            double exp = 5.7 * Math.Pow(level, 1.85);
            return (int)Math.Round(exp);
        }

        int GetStackExp(int level)
        {
            int sum = 0;
            for (int i = 1; i <= level; i++)
            {
                sum += GetRequiredExp(i);
            }
            return sum;
        }
    }
}

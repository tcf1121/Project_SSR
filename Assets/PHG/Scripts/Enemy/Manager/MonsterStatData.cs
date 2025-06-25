using UnityEngine;

namespace PHG
{
    [CreateAssetMenu(fileName = "MonsterStatData", menuName = "Samsara/Monster Stat Data", order = 1)]
    public class MonsterStatData : ScriptableObject
    {
        [Header("기본 스탯")]
        [Tooltip("체력")]
        public int maxHP = 100;
        [Tooltip("공격력")]
        public int damage = 10;
        [Tooltip("이동속도")]
        public float moveSpeed = 2f;

        [Header("AI 인식 범위")]
        [Tooltip("감지 범위")]
        public float patrolRange = 3f;
        [Tooltip("추격 범위")]
        public float chaseRange = 6f;
        [Tooltip("공격 범위")]
        public float attackRange = 1f;
        [Tooltip("돌진 범위")]
        public float chargeRange = 2.5f; 
    }
}
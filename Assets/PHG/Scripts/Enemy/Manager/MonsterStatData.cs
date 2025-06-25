using UnityEngine;

namespace PHG
{
    [CreateAssetMenu(fileName = "MonsterStatData", menuName = "Samsara/Monster Stat Data", order = 1)]
    public class MonsterStatData : ScriptableObject
    {
        [Header("�⺻ ����")]
        [Tooltip("ü��")]
        public int maxHP = 100;
        [Tooltip("���ݷ�")]
        public int damage = 10;
        [Tooltip("�̵��ӵ�")]
        public float moveSpeed = 2f;

        [Header("AI �ν� ����")]
        [Tooltip("���� ����")]
        public float patrolRange = 3f;
        [Tooltip("�߰� ����")]
        public float chaseRange = 6f;
        [Tooltip("���� ����")]
        public float attackRange = 1f;
        [Tooltip("���� ����")]
        public float chargeRange = 2.5f; 
    }
}
using UnityEngine;

namespace PHG
{
    public class MonsterStats : MonoBehaviour
    {
        [SerializeField] private MonsterStatData statData;

        private int currentHP;

        public int MaxHP => statData.maxHP;
        public int CurrentHP => currentHP;
        public int Damage => statData.damage;
        public float MoveSpeed => statData.moveSpeed;
        public float PatrolRange => statData.patrolRange;
        public float ChaseRange => statData.chaseRange;
        public float AttackRange => statData.attackRange;
        public float ChargeRange => statData.chargeRange;
        public bool UsePatrol => statData.usePatrol; //  SO 플래그 사용

        private void Awake()
        {
            currentHP = statData.maxHP;
        }

        public void TakeDamage(int amount)
        {
            currentHP -= amount;
            if (currentHP <= 0)
            {
                currentHP = 0;
                // TODO: FSM 사망 상태 전환 등
            }
        }
    }
}
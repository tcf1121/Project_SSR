using UnityEngine;


namespace PHG
{

    public class MonsterStats : MonoBehaviour
    {
        [SerializeField] private MonsterStatData statData;
        [SerializeField] private bool usePatrol = true;

        [SerializeField] private float chargeRange = 2.5f;

        private int currentHP;

        public int MaxHP => statData.maxHP;
        public int CurrentHP => currentHP;
        public int Damage => statData.damage;
        public float MoveSpeed => statData != null ? statData.moveSpeed : 0f;
        public float PatrolRange => statData.patrolRange;
        public float ChaseRange => statData.chaseRange;
        public float AttackRange => statData.attackRange;

        public float ChargeRange => chargeRange;
        public bool UsePatrol => usePatrol;

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
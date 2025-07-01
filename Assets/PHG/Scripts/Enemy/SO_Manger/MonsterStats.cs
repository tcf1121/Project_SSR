using UnityEngine;

namespace PHG
{
    public class MonsterStats : MonoBehaviour
    {
        [SerializeField] private MonsterStatEntry statData;

        private int currentHP;
        private MonsterBrain brain;


        public int MaxHP => Mathf.RoundToInt(statData.maxHP * brain.Coeff);
        public int CurrentHP => currentHP;
        public int Damage => Mathf.RoundToInt(statData.damage * brain.Coeff);
        public float MoveSpeed => statData.moveSpeed;
        public float PatrolRange => statData.patrolRange;
        public float ChaseRange => statData.chaseRange;
        public float AttackRange => statData.attackRange;
        public float ChargeRange => statData.chargeRange;
        public bool UsePatrol => statData.usePatrol; //  SO 플래그 사용



        private void Awake()
        {
            currentHP = statData.maxHP;
            brain = GetComponent<MonsterBrain>();
        }
    

        public void TakeDamage(int amount)
        {
            currentHP -= amount;
            if (currentHP <= 0)
            {
                currentHP = 0;

                if (brain != null)
                    brain.ChangeState(StateID.Dead);

            }
        }
    }
}
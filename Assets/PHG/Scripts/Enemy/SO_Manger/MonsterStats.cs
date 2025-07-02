using UnityEngine;

namespace PHG
{
    public class MonsterStats : MonoBehaviour
    {
        [SerializeField]
        private MonsterStatEntry localRuntimeStats;

        private MonsterBrain brain;

        // ��Ÿ�� ���� ��� ĳ��
        private int currentHP;
        private int maxHP;
        private int damage;
        private float moveSpeed;
        private float patrolRange;
        private float chaseRange;
        private float attackRange;
        private float chargeRange;
        private bool usePatrol;

        public int MaxHP => Mathf.RoundToInt(maxHP * brain.Coeff);
        public int CurrentHP => currentHP;
        public int Damage => Mathf.RoundToInt(damage * brain.Coeff);
        public float MoveSpeed => moveSpeed;
        public float PatrolRange => patrolRange;
        public float ChaseRange => chaseRange;
        public float AttackRange => attackRange;
        public float ChargeRange => chargeRange;
        public bool UsePatrol => usePatrol;

        public MonsterStatEntry RuntimeData => localRuntimeStats;

        private void Awake()
        {
            brain = GetComponent<MonsterBrain>();
            if (brain != null)
            {
                brain.Stats = this;
                localRuntimeStats = brain.statData; // �� �������� ���� MonsterBrain���� ���� �� �״�� ��
            }

            if (localRuntimeStats == null)
            {
                Debug.LogError("[MonsterStats] RuntimeStatEntry ����!");
                enabled = false;
                return;
            }

            InitializeRuntimeStats();
        }

        private void InitializeRuntimeStats()
        {
            maxHP = localRuntimeStats.maxHP;
            damage = localRuntimeStats.damage;
            moveSpeed = localRuntimeStats.moveSpeed;
            patrolRange = localRuntimeStats.patrolRange;
            chaseRange = localRuntimeStats.chaseRange;
            attackRange = localRuntimeStats.attackRange;
            chargeRange = localRuntimeStats.chargeRange;
            usePatrol = localRuntimeStats.usePatrol;

            currentHP = maxHP;
        }

        public void SetHP(int newHP)
        {
            currentHP = Mathf.Clamp(newHP, 0, MaxHP);
        }

        public void KillIfDead()
        {
            if (currentHP <= 0 && brain != null)
                brain.ChangeState(StateID.Dead);
        }


    }
}

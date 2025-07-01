using SCR;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace PHG
{
    public class MonsterStats : MonoBehaviour
    {
        [SerializeField] private MonsterStatEntry statData;
        public MonsterStatEntry StatData => statData;

        private int currentHP;
        private MonsterBrain brain;

        public int MaxHP => Mathf.RoundToInt(statData.maxHP * brain.Coeff);              // ��� ����
        public int CurrentHP => currentHP;                                               // ��� ����
        public int Damage => Mathf.RoundToInt(statData.damage * brain.Coeff);            // ��� ����
        public float MoveSpeed => statData.moveSpeed;
        public float PatrolRange => statData.patrolRange;
        public float ChaseRange => statData.chaseRange;
        public float AttackRange => statData.attackRange;
        public float ChargeRange => statData.chargeRange;
        public bool UsePatrol => statData.usePatrol;

        private void Awake()
        {
            brain = GetComponent<MonsterBrain>();
            currentHP = statData.maxHP;
        }

        /// <summary>
        /// TakeDamageState���� ���� HP ���� �� ���
        /// </summary>
        public void SetHP(int newHP)
        {
            currentHP = Mathf.Clamp(newHP, 0, MaxHP);
        }

        /// <summary>
        /// ���� HP�� 0 ������ ��� ���� ����
        /// </summary>
        public void KillIfDead()
        {
            if (currentHP <= 0 && brain != null)
                brain.ChangeState(StateID.Dead);
        }

#if UNITY_EDITOR
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                int dmg = 10;
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    Vector2 origin = player.transform.position;
                    var hit = new HitInfo(dmg, origin, true);
                    brain.EnterDamageState(hit);
                }
            }
        }
#endif
    }
}
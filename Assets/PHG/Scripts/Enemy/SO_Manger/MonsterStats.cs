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

        public int MaxHP => Mathf.RoundToInt(statData.maxHP * brain.Coeff);              // 멤버 변수
        public int CurrentHP => currentHP;                                               // 멤버 변수
        public int Damage => Mathf.RoundToInt(statData.damage * brain.Coeff);            // 멤버 변수
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
        /// TakeDamageState에서 직접 HP 수정 시 사용
        /// </summary>
        public void SetHP(int newHP)
        {
            currentHP = Mathf.Clamp(newHP, 0, MaxHP);
        }

        /// <summary>
        /// 현재 HP가 0 이하일 경우 죽음 전이
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
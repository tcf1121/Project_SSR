using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace PHG
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MonsterBrain : MonoBehaviour
    {

        [Header("Sensor/Mask")]
        public Transform sensor; //하위 빈 gameObject
        public LayerMask groundMask; // groundLayer 지정
        public Transform targetLadder; // 사다리용
        public Transform targetLadderTop;
        [SerializeField] private bool canClimbLadders = true;
        public bool CanClimbLadders => canClimbLadders;

        [Header("References")]
        [SerializeField] private MonsterStats stats;
        [SerializeField] private Collider2D hitBox; // 근접 공격용 히트박스
        public MonsterStats Stats => stats;

        private StateMachine sm;
        public StateMachine Sm => sm;

        private IdleState idle;
        private FloatChaseState floatChase;
        private PatrolState patrol;
        private ChaseState chase;
        private RangeAttackState rangeAttack;
        private DeadState dead;
        private MeleeAttackState meleeAttack;

        private IState attack;

        private void Awake()
        {
            idle = new IdleState(this);
            //이동 상태 분기
            patrol = new PatrolState(this);
            chase = new ChaseState(this);
            dead = new DeadState(this);
            floatChase = new FloatChaseState(this);

            //공격 상태 분기
            if (HasRangedTag())
            {
                rangeAttack = new RangeAttackState(this);                  // 분기
                attack = rangeAttack;
            }
            else
            {
                meleeAttack = new MeleeAttackState(this, hitBox);       // 근접
                attack = meleeAttack;
            }

            sm = new StateMachine();
            sm.Register(StateID.Idle, idle);
            sm.Register(StateID.Chase, IsFlying() ? floatChase : chase);
            sm.Register(StateID.Attack, attack);
            sm.Register(StateID.Dead, dead);
            sm.Register(StateID.Patrol, patrol);
            
            
            
            sm.ChangeState(StateID.Idle);

            
        }



        private void FixedUpdate() => sm.Tick();
        public void ChangeState(StateID id)
        {
            //UsePatrol 확인
            if (id == StateID.Patrol && !Stats.UsePatrol)
                return;
            sm.ChangeState(id);
            Debug.Log($"ChangeState from {sm.CurrentStateID} → {id}");
        }

        private bool IsFlying()
        {
            return GetComponent<FlyingTag>() != null;
        }

        private bool HasRangedTag()
        {
            return GetComponent<RangedTag>() != null;
        }


    }
}

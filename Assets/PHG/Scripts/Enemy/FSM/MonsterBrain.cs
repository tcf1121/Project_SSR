using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PHG;

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
        public MonsterStats Stats => stats;


        private StateMachine sm;

        private PatrolState patrol;
        private ChaseState chase;
        private AttackState attack;
        private DeadState dead;

        private void Awake()
        {
            patrol = new PatrolState(this);
            chase = new ChaseState(this);
            attack = new AttackState(this);
            dead = new DeadState(this);

            sm = new StateMachine();
            sm.Register(StateID.Idle, patrol);
            sm.Register(StateID.Chase, chase);
            sm.Register(StateID.Attack, attack);
            sm.Register(StateID.Dead, dead);
            sm.Register(StateID.Patrol, patrol);
            sm.ChangeState(StateID.Idle);
        }



        private void FixedUpdate() => sm.Tick();
        public void ChangeState(StateID id) => sm.ChangeState(id);





    }
}
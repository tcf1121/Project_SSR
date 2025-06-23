using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PHG;


[RequireComponent(typeof(Rigidbody2D))]
public class MonsterBrain : MonoBehaviour
{

    [Header("Sensor/Mask")]
    public Transform sensor; //ÇÏÀ§ ºó gameObject
    public LayerMask groundMask; // groundLayer ÁöÁ¤

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

        sm.ChangeState(StateID.Idle);
    }
    private void FixedUpdate() => sm.Tick();

    [Header("Params")]
    public float moveSpeed = 2f;
    public float patrolRange = 3f;
    public float chaseRange = 6f;
    public float attackRange = 1f;

    public void ChangeState(StateID id) => sm.ChangeState(id);





}

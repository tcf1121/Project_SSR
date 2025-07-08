<<<<<<< HEAD
﻿
using System.Collections;
=======
﻿using System.Collections;
>>>>>>> parent of 953c5e6 ([Chor] Develop_MonsterAI_and_Animation)
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Rigidbody2D))]
public partial class MonsterBrain : MonoBehaviour, IMonsterJumper
{
    [SerializeField] private Monster _monster;
    public Monster Monster { get => _monster; }

    public Vector2 LastGroundCheckPos => jumper.LastGroundCheckPos;
    public float GroundCheckRadius => jumper.GroundCheckRadius;
    public MonsterStatEntry StatData { get; private set; }

    private StateMachine stateMachine;
    public StateMachine StateMachine => stateMachine;

    private IState idle, patrol, chase, dead, aimReady;
    private AttackState attack;
    private TakeDamageState takeDamage;


    // 점프 시스템
    #region Jump
    private JumpMove jumper;
    public bool IsGrounded() => jumper.IsGrounded();
    public bool IsMidJump => jumper.IsMidJump;
    public bool IsClimbing => climber.IsClimbing;
    public bool ReadyToJump() => jumper.ReadyToJump();
    public bool PerformJump(int dir, float dy, float jumpForce, float horizontalFactor, float lockDuration) =>
        jumper.PerformJump(dir, dy, jumpForce, horizontalFactor, lockDuration);
    public void UpdateTimer(float deltaTime) => jumper.UpdateTimer(deltaTime);
    #endregion

    // 사다리 시스템 (인터페이스 기반)
    #region Ladder
    private IMonsterClimber climber;
    public IMonsterClimber Climber => climber;
    #endregion

    // 낙하 데미지 관련 변수
    private float _fallStartPosY;
    private bool _isFalling = false;
    [SerializeField] private float _minFallDistanceForDamage = 3.0f; // 낙하 데미지 발생 최소 거리
    [SerializeField] private int _baseFallDamage = 10; // 기본 낙하 데미지
    [SerializeField] private float _damageMultiplierPerMeter = 5.0f; // 1미터당 추가 데미지


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_monster.AllMonsterStatData != null)
            StatData = _monster.AllMonsterStatData.GetStatEntry(_monster.MonsterSpecies);
    }
#endif
    public void SetBrain()
    {

        StatData = _monster.AllMonsterStatData.GetStatEntry(_monster.MonsterSpecies);


        // 원거리 몬스터
        if (StatData.isRanged) attack = new RangeAttackState(_monster);
        // 근접 몬스터
        else attack = new MeleeAttackState(_monster);


        // 점프 시스템 초기화
        jumper = new JumpMove();
        jumper.Init(_monster, StatData);

        // 사다리 시스템 인터페이스 연결
        if (StatData.enableLadderClimb)
        {
            // 사다리 기능이 있으면, 실제 LadderClimber를 생성
            climber = new LadderClimber();
<<<<<<< HEAD
=======
            climber.Init(_monster);
>>>>>>> parent of 953c5e6 ([Chor] Develop_MonsterAI_and_Animation)
        }
        else
        {
            // ⭐ 사다리 기능이 없으면, 기능 없는 NullClimber를 생성
            climber = new NullClimber();
        }
        climber.Init(this);
        // 상태 등록
        idle = new IdleState(_monster);
        patrol = new PatrolState(_monster);
        chase = StatData.isFlying ? new FloatChaseState(_monster) : new ChaseState(_monster);
        dead = new DeadState(_monster);
        aimReady = new AimReadyState(_monster);
        takeDamage = new TakeDamageState(_monster, 0);

        IState normalChase = new ChaseState(_monster);
        IState floatChaseState = new FloatChaseState(_monster);


        stateMachine = new StateMachine();
        stateMachine.Register(StateID.Idle, idle);
        stateMachine.Register(StateID.Patrol, patrol);
        stateMachine.Register(StateID.Chase, normalChase);
        stateMachine.Register(StateID.FloatChase, floatChaseState);

        stateMachine.Register(StateID.Attack, attack);
        stateMachine.Register(StateID.Dead, dead);
        stateMachine.ChangeState(StateID.Idle);
        stateMachine.Register(StateID.TakeDamage, takeDamage);
        stateMachine.Register(StateID.AimReady, aimReady);
        _monster.MonsterStats.EnableStats();


    }

    private void FixedUpdate()
    {
        jumper?.UpdateTimer(Time.fixedDeltaTime);
        stateMachine?.Tick();
<<<<<<< HEAD

=======
        climber?.UpdateClimbTimer(Time.fixedDeltaTime);
>>>>>>> parent of 953c5e6 ([Chor] Develop_MonsterAI_and_Animation)
        // --- 낙하 데미지 감지 및 적용 ---
        HandleFallDamage();
    }

    public void ChangeState(StateID id)
    {
        if (stateMachine == null) return;

        bool usePatrolFlag = StatData != null ? StatData.usePatrol
                                             : (_monster.MonsterStats != null && _monster.MonsterStats.UsePatrol);

        //if (id == StateID.Patrol && !usePatrolFlag)
        //    return;

        stateMachine.ChangeState(id);
    }

    public void EnterDamageState(int damage)
    {
        takeDamage.SetDamage(damage);
        stateMachine.Register(StateID.TakeDamage, takeDamage);
        stateMachine.ChangeState(StateID.TakeDamage);
    }

    //Conflict 예상 -----------------------------------------------
    public void ApplyKnockback(Vector2 origin, float force)
    {
        Vector2 dir = ((Vector2)transform.position - origin).normalized;

        if (StatData.hasIdleAnim)
            _monster.PlayAnim(AnimNames.Stagger);

        // "퍽!" 느낌용 넉백 속도 세팅
        _monster.Rigid.velocity = dir * force;

        // 아주 짧게 밀린 뒤 즉시 멈춤
        StartCoroutine(StopVelocityAfter(StatData.knockbackDuration));
    }

    private void HandleFallDamage()
    {
        bool currentlyGrounded = jumper.IsGrounded(); // jumper를 통해 현재 지면 상태 확인

        if (!currentlyGrounded && !_isFalling)
        {
            // 지상에서 떨어지기 시작 (낙하 시작)
            _isFalling = true;
            _fallStartPosY = _monster.Transfrom.position.y;
            // Debug.Log($"[{_monster.name}] 낙하 시작! 시작 높이: {_fallStartPosY}");
        }
        else if (currentlyGrounded && _isFalling)
        {
            // 착지 (낙하 종료)
            _isFalling = false;
            float fallDistance = _fallStartPosY - _monster.Transfrom.position.y;

            if (fallDistance > _minFallDistanceForDamage)
            {
                // 낙하 데미지 적용
                int damageToApply = _baseFallDamage + Mathf.FloorToInt((fallDistance - _minFallDistanceForDamage) * _damageMultiplierPerMeter);
                _monster.GetDamage(damageToApply); // Monster의 GetDamage 메서드 호출
            }
            // else { Debug.Log($"[{_monster.name}] 착지 (낙하 거리: {fallDistance:F2}m, 데미지 없음)"); }
        }
    }

    private IEnumerator StopVelocityAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        _monster.Rigid.velocity = Vector2.zero;
    }
    //--------------------------------------------------------------

    public void Attack()
    {
        attack.Attack();
    }

    public void FinishAttack()
    {
        attack.FinishAttack();
    }

}
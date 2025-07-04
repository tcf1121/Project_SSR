using System.Collections;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Rigidbody2D))]
public partial class MonsterBrain : MonoBehaviour, IMonsterJumper
{
    [SerializeField] private Monster _monster;
    public Monster Monster { get => _monster; }
    [SerializeField] private float staggerThreshold = 15f;


    public Vector2 LastGroundCheckPos => jumper.LastGroundCheckPos;
    public float GroundCheckRadius => jumper.GroundCheckRadius;
    public MonsterStatEntry StatData { get; private set; }

    public bool IsFlying { get; private set; }
    public bool IsRanged { get; private set; }
    public bool IsCharging { get; private set; }
    public bool CanJump { get; private set; }
    public bool CanClimbLadders { get; private set; }

    public Transform Target { get; set; } //애니메이션용 위치

    public LayerMask groundMask;
    private LayerMask ladderMask;
    public IAttackBehavior attackBehavior;
    private Transform muzzle;
    public Transform Muzzle => muzzle;


    private StateMachine sm;
    public StateMachine Sm => sm;

    private IState idle, patrol, chase, attack, takeDamage, dead, aimReady;

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_monster.AllMonsterStatData != null)
            StatData = _monster.AllMonsterStatData.GetStatEntry(_monster.MonsterSpecies);

    }
#endif
    private void OnEnable()
    {

        StatData = _monster.AllMonsterStatData.GetStatEntry(_monster.MonsterSpecies);

        IsFlying = StatData.isFlying;
        IsRanged = StatData.isRanged;
        IsCharging = StatData.isCharging;
        CanJump = StatData.enableJump;
        CanClimbLadders = StatData.enableLadderClimb;
        groundMask = StatData.groundMask;
        ladderMask = StatData.ladderMask;

        if (IsRanged)
        {
            muzzle = _monster.MuzzlePoint;
            attackBehavior = new RangedAttackBehavior();
        }
        else // 근접 몬스터
        {
            attackBehavior = new MeleeAttackBehavior(_monster.AttackBox, StatData.playerLayer); // StatData.playerLayer 추가 필요
            //Debug.Log($"[MonsterBrain] MeleeAttackBehavior 초기화됨. 할당된 HitBox: {_monster.AttackBox?.name}, PlayerLayer: {StatData.playerLayer.value}");
        }


        // 점프 시스템 초기화
        jumper = new JumpMove();
        jumper.Init(_monster.Rigid, _monster.Transfrom, StatData, groundMask);

        // 사다리 시스템 인터페이스 연결
        if (StatData.enableLadderClimb)
        {
            climber = new LadderClimber();
            climber.Init(this);
        }

        // 상태 등록
        idle = new IdleState(this);
        patrol = new PatrolState(this);
        chase = IsFlying ? new FloatChaseState(this) : new ChaseState(this);
        dead = new DeadState(this);
        attack = (IsRanged || _monster.AttackBox == null)
                    ? new RangeAttackState(this)
                    : new MeleeAttackState(this, _monster.AttackBox);
        aimReady = new AimReadyState(this);
        if (StatData.idleMode == MonsterStatEntry.IdleMode.GreedInteract)
        {
            var interact = GetComponent<Interactable>() ?? gameObject.AddComponent<Interactable>();
            idle = new GreedIdleState(this, interact);
        }
        takeDamage = new TakeDamageState(this, new HitInfo(0, Vector2.zero));




        sm = new StateMachine();
        sm.Register(StateID.Idle, idle);
        sm.Register(StateID.Patrol, patrol);
        sm.Register(StateID.Chase, chase);
        sm.Register(StateID.Attack, attack);
        sm.Register(StateID.Dead, dead);
        sm.ChangeState(StateID.Idle);
        sm.Register(StateID.TakeDamage, takeDamage);
        sm.Register(StateID.AimReady, aimReady);
        _monster.MonsterStats.EnableStats();


    }

    private void FixedUpdate()
    {
        jumper?.UpdateTimer(Time.fixedDeltaTime);
        sm?.Tick();
        climber?.UpdateClimbTimer(Time.fixedDeltaTime);
    }

    public void ChangeState(StateID id)
    {
        if (sm == null) return;

        bool usePatrolFlag = StatData != null ? StatData.usePatrol
                                             : (_monster.MonsterStats != null && _monster.MonsterStats.UsePatrol);

        if (id == StateID.Patrol && !usePatrolFlag)
            return;

        sm.ChangeState(id);
    }

    public void EnterDamageState(HitInfo hit)
    {
        takeDamage = new TakeDamageState(this, hit);
        sm.Register(StateID.TakeDamage, takeDamage);
        sm.ChangeState(StateID.TakeDamage);
    }

    //Conflict 예상 -----------------------------------------------
    public void ApplyKnockback(Vector2 origin, float force)
    {
        Vector2 dir = ((Vector2)transform.position - origin).normalized;

        if (StatData.hasIdleAnim)
            PlayAnim(AnimNames.Stagger);

        // "퍽!" 느낌용 넉백 속도 세팅
        _monster.Rigid.velocity = dir * force;

        // 아주 짧게 밀린 뒤 즉시 멈춤
        StartCoroutine(StopVelocityAfter(StatData.knockbackDuration));
    }

    private IEnumerator StopVelocityAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        _monster.Rigid.velocity = Vector2.zero;
    }
    //--------------------------------------------------------------

    public void Clone(MonsterBrain monsterBrain)
    {
        staggerThreshold = monsterBrain.staggerThreshold;
        IsFlying = monsterBrain.IsFlying;
        IsRanged = monsterBrain.IsRanged;
        IsCharging = monsterBrain.IsCharging;
        CanJump = monsterBrain.CanJump;
        CanClimbLadders = monsterBrain.CanClimbLadders;
    }

    public void PlayAnim(string animName)
    {
        _monster.Animator.Play(animName);
    }

    public void FireProjectile()
    {
        if (attackBehavior != null)
        {
            //Debug.Log($"[{gameObject.name}] Animation Event: FireProjectile() 호출됨!");
            if (attackBehavior is MeleeAttackBehavior meleeBehavior)
            {
                meleeBehavior.ActivateHitBoxExternally(true); // 공격 시작 시 히트박스 활성화
                Debug.Log($"[{gameObject.name}] MeleeAttackBehavior 히트박스 활성화 요청됨.");
            }
            attackBehavior.Execute(this); // 근접이든 원거리든 이 메서드가 호출되어 Behavior 실행
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] attackBehavior가 할당되지 않았습니다.");
        }
    }
    public void DeactivateMeleeHitBox()
    {
        if (attackBehavior is MeleeAttackBehavior meleeBehavior) // MeleeAttackBehavior 인스턴스인지 확인
        {
            meleeBehavior.ActivateHitBoxExternally(false); // 외부 비활성화 메서드 호출
            Debug.Log($"[{gameObject.name}] Animation Event: Melee HitBox Deactivated");
        }
    }
}
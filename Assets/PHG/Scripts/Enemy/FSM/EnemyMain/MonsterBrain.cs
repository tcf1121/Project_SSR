using LHE;
using SCR;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PHG
{
    [RequireComponent(typeof(Rigidbody2D))]
    public partial class MonsterBrain : MonoBehaviour, IMonsterJumper
    {
        [Header("Sensor / Mask")]
        public Transform sensor;
        public Transform wallSensor;

        [Header("References")]
        [SerializeField] private BoxCollider2D hitBox;
        [SerializeField] private AllMonsterStatData allMonsterStatData;
        [SerializeField] private MonsterType thisMonsterType;
        [SerializeField] private GameObject damageTextPrefab;
        [SerializeField] private float staggerThreshold = 15f;
        [SerializeField] private RectTransform hpBar;
        [SerializeField] private Image hpBarFill;
        [SerializeField] private MonsterStats monsterStats;
        [SerializeField] public Animator animator;

        public RectTransform HpBar { get => hpBar; }
        public Image HpBarFill { get => hpBarFill; }

        public MonsterStats MonsterStats { get => monsterStats; }

        public Vector2 LastGroundCheckPos => jumper.LastGroundCheckPos;
        public float GroundCheckRadius => jumper.GroundCheckRadius;
        public MonsterStatEntry StatData { get; private set; }

        public bool IsFlying { get; private set; }
        public bool IsRanged { get; private set; }
        public bool IsCharging { get; private set; }
        public bool CanJump { get; private set; }
        public bool CanClimbLadders { get; private set; }

        public Rigidbody2D rb { get; private set; }
        public Transform tf { get; private set; }

        public float Coeff { get; private set; } = 1f;
        public int SpawnStage { get; set; }
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
        public bool ReadyToJump() => jumper.ReadyToJump();
        public bool PerformJump(int dir, float dy, float jumpForce, float horizontalFactor, float lockDuration) =>
            jumper.PerformJump(dir, dy, jumpForce, horizontalFactor, lockDuration);
        public void UpdateTimer(float deltaTime) => jumper.UpdateTimer(deltaTime);
        #endregion

        // 사다리 시스템 (인터페이스 기반)
        #region Ladder
        private IMonsterClimber climber;
        public IMonsterClimber Climber => climber;

        public AllMonsterStatData AllStatData => allMonsterStatData;
        public MonsterType MonsterType => thisMonsterType;
        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (allMonsterStatData != null)
                StatData = allMonsterStatData.GetStatEntry(thisMonsterType);

        }
#endif

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            tf = transform;
        }

        private void OnEnable()
        {
        
            StatData = allMonsterStatData.GetStatEntry(thisMonsterType);
            if (allMonsterStatData == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[MonsterBrain] AllMonsterStatData가 할당되지 않음 – 비활성화", this);
#endif
                enabled = false;
                return;
            }
            else
            {
                Debug.Log($"[MonsterBrain] Loaded StatData: {StatData.monsterType}, Prefab: {StatData.projectileprefab}");
            }

            if (StatData == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[MonsterBrain] '{thisMonsterType}' 스탯 데이터가 없음 – Default 대체 시도", this);
#endif
                StatData = allMonsterStatData.GetStatEntry(MonsterType.Default);
                if (StatData == null)
                {
                    enabled = false;
                    return;
                }
            }
            IsFlying = StatData.isFlying;
            IsRanged = StatData.isRanged;
            IsCharging = StatData.isCharging;
            CanJump = StatData.enableJump;
            CanClimbLadders = StatData.enableLadderClimb;

            groundMask = StatData.groundMask;
            ladderMask = StatData.ladderMask;

            if (IsRanged)
                attackBehavior = new RangedAttackBehavior();
            // 점프 시스템 초기화
            jumper = new JumpMove();
            jumper.Init(rb, tf, StatData, groundMask);

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
            attack = (IsRanged || hitBox == null)
                        ? new RangeAttackState(this)
                        : new MeleeAttackState(this, hitBox);
            aimReady = new AimReadyState(this);
            if (StatData.idleMode == MonsterStatEntry.IdleMode.GreedInteract)
            {
                var interact = GetComponent<Interactable>() ?? gameObject.AddComponent<Interactable>();
                idle = new GreedIdleState(this, interact);
            }
            takeDamage = new TakeDamageState(this, new HitInfo(0, Vector2.zero));


            muzzle = tf.Find("MuzzlePoint");
            // if (muzzle == null)
            // {
            //     Debug.LogError($"[MonsterBrain.OnEnable] MuzzlePoint를 찾지 못했습니다. tf.name: {tf.name}");
            //
            //     // 여기에 추가하여 MuzzlePoint GameObject의 활성화 상태와 tf 자체의 활성화 상태를 확인합니다.
            //     GameObject muzzleObjectInScene = GameObject.Find("MuzzlePoint"); // 씬 전체에서 MuzzlePoint를 찾아봅니다.
            //     if (muzzleObjectInScene != null)
            //     {
            //         Debug.LogError($"하지만 씬에서 'MuzzlePoint'를 찾았습니다. 활성화 상태: {muzzleObjectInScene.activeInHierarchy} (Hierarchy), {muzzleObjectInScene.activeSelf} (Self)");
            //         if (!muzzleObjectInScene.activeInHierarchy)
            //         {
            //             Debug.LogError("MuzzlePoint가 현재 비활성화 상태이므로 Find로 찾을 수 없습니다.");
            //         }
            //     }
            //     else
            //     {
            //         Debug.LogError("씬 전체에서도 'MuzzlePoint' 게임 오브젝트를 찾을 수 없습니다. 이름 철자나 존재 여부를 확인하세요.");
            //     }
            // }
            // else
            // {
            //     Debug.Log($"[MonsterBrain.OnEnable] MuzzlePoint 찾음: {muzzle.name}");
            // }

            sm = new StateMachine();
            sm.Register(StateID.Idle, idle);
            sm.Register(StateID.Patrol, patrol);
            sm.Register(StateID.Chase, chase);
            sm.Register(StateID.Attack, attack);
            sm.Register(StateID.Dead, dead);
            sm.ChangeState(StateID.Idle);
            sm.Register(StateID.TakeDamage, takeDamage);
            sm.Register(StateID.AimReady, aimReady);
            InitializeStats(GameManager.Stage);
            monsterStats.EnableStats();


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
                                                 : (monsterStats != null && monsterStats.UsePatrol);

            if (id == StateID.Patrol && !usePatrolFlag)
                return;

            sm.ChangeState(id);
        }

        public void InitializeStats(int stage)
        {
            SpawnStage = stage - 1;
            float T = GameManager.StageManager.DangerIndexManager.GetDangerIndex();
            float S = SpawnStage;
            Coeff = (1.0f + 0.1012f * T) * Mathf.Pow(1.15f, S);
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
            rb.velocity = dir * force;

            // 아주 짧게 밀린 뒤 즉시 멈춤
            StartCoroutine(StopVelocityAfter(StatData.knockbackDuration));
        }

        private IEnumerator StopVelocityAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            rb.velocity = Vector2.zero;
        }
        //--------------------------------------------------------------

        public void Clone(MonsterBrain monsterBrain)
        {
            sensor.transform.position = monsterBrain.sensor.transform.position;
            wallSensor.transform.position = monsterBrain.wallSensor.transform.position;

            if (monsterBrain.hitBox != null)
            {
                hitBox.size = monsterBrain.hitBox.size;
                hitBox.offset = monsterBrain.hitBox.offset;
            }

            //monsterStats = monsterBrain.monsterStats;
            thisMonsterType = monsterBrain.thisMonsterType;
            staggerThreshold = monsterBrain.staggerThreshold;

            //StatData = monsterBrain.StatData;
            IsFlying = monsterBrain.IsFlying;
            IsRanged = monsterBrain.IsRanged;
            IsCharging = monsterBrain.IsCharging;
            CanJump = monsterBrain.CanJump;
            CanClimbLadders = monsterBrain.CanClimbLadders;

            gameObject.transform.position = monsterBrain.gameObject.transform.position;
            gameObject.transform.localScale = monsterBrain.gameObject.transform.localScale;
            hpBar.sizeDelta = monsterBrain.hpBar.sizeDelta;
            hpBar.position = monsterBrain.hpBar.position;

            // public float Coeff { get; private set; } = 1f;
            // public int SpawnStage { get; set; }
        }

        public void PlayAnim(string animName)
        {
            if (animator == null) animator = GetComponent<Animator>();
            animator.Play(animName);
        }

        public void FireProjectile()
        {
           // Debug.Log("애니메이션 이벤트: FireProjectile() 호출됨");
            if (attackBehavior != null)
            {
                attackBehavior.Execute(this);
            }
            else
            {
               // Debug.LogWarning(" attackBehavior is null");
            }
        }
    }
}
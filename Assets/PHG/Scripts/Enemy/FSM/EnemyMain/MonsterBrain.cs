using Unity.VisualScripting; 
using UnityEngine;
// using System.Collections.Generic; // 이 네임스페이스가 MonsterBrain 내부에서 직접 사용되지 않는다면 제거 고려

namespace PHG
{
    /// <summary>
    /// Core brain that owns the per‑enemy finite‑state‑machine and exposes shared data to individual states.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MonsterBrain : MonoBehaviour
    {
        /* ───────── Inspector ───────── */
        [Header("Sensor / Mask")]
        public Transform sensor;                         // 하위 빈 gameObject – 바닥 감지용 (ground checker)
        public Transform wallSensor;                     // 하위 빈 gameObject – 벽 감지 전용 (wall sensor)
        public LayerMask groundMask;                     // 발 밑과 전방 체크에 사용할 Ground 레이어 마스크

        [SerializeField] private bool canClimbLadders = true;
        public bool CanClimbLadders => canClimbLadders;

        [Header("References")]
        [SerializeField] private Collider2D hitBox;  // 근접 공격시 사용 – 없으면 원거리 몬스터로 간주
        [SerializeField] private MonsterStats _runtimeStats;       // 런타임 변동 스탯 (HP 등)


        // ★★★ 변경: 모든 몬스터 스탯을 담고 있는 단일 ScriptableObject 참조 ★★★
        [SerializeField, Tooltip("모든 몬스터 스탯을 담고 있는 단일 ScriptableObject")]
        private AllMonsterStatData allMonsterStatData;

        // ★★★ 추가: 이 몬스터 인스턴스의 타입을 지정 (인스펙터에서 설정) ★★★
        [SerializeField, Tooltip("이 몬스터 인스턴스의 타입")]
        private MonsterType thisMonsterType;

        /* ───────── Public helpers ───────── */
        // 런타임 변동 스탯 (기존 'Stats' 이름이 이제 MonsterStatEntry와 겹치므로 'RuntimeStats'로 변경)
        public MonsterStats RuntimeStats => _runtimeStats;

        // ★★★ 몬스터의 고정 스탯 (AllMonsterStatData에서 로드됨). 기존 'statData' 변수명을 유지하여 기존 코드와의 호환성을 높임. ★★★
        public MonsterStatEntry statData { get; private set; } // 이 필드가 기존 statData의 역할을 대체합니다.

        /// <summary>다른 스크립트(특히 State)에서 이동속도가 필요할 때 편하게 가져오도록 Helper 프로퍼티 제공.</summary>
        public float MoveSpeed => statData != null ? statData.moveSpeed : 0f;

        // ★★★ 추가: Rigidbody2D와 Transform 캐싱 필드 (상태 스크립트에서 접근하기 위해) ★★★
        public Rigidbody2D rb { get; private set; }
        public Transform tf { get; private set; }

        /* ───────── FSM ───────── */
        private StateMachine sm;
        public StateMachine Sm => sm;
        public AllMonsterStatData AllStatData => allMonsterStatData;
        public MonsterType MonsterType => thisMonsterType;
        // 미리 생성해서 캐싱할 State 인스턴스
        private IState idle;
        private IState patrol;
        private IState chase;
        private IState attack;
        private IState dead;

        /* ====================================================================== */
        private void Awake()
        {
            // ★★★ Rigidbody2D와 Transform 캐싱 (기존 코드에서 누락되어 있었을 수 있음) ★★★
            rb = GetComponent<Rigidbody2D>();
            tf = transform;

            // 1. AllMonsterStatData ScriptableObject가 할당되었는지 확인
            if (allMonsterStatData == null)
            {
#if UNITY_EDITOR // 에디터에서만 경고 메시지를 표시하여 빌드에는 포함되지 않도록 함
                Debug.LogWarning($"[MonsterBrain] <color=red>AllMonsterStatData가 할당되지 않았습니다</color> on {name}. FSM 초기화 불가. 스크립트 비활성화.", this);
#endif
                enabled = false; // 컴포넌트를 비활성화하여 더 이상 실행되지 않도록 함
                return;
            }

            // 2. 이 몬스터의 specific한 스탯 데이터를 AllMonsterStatData에서 찾아 할당
            statData = allMonsterStatData.GetStatEntry(thisMonsterType);

            // 3. 해당 몬스터 타입의 스탯 데이터가 없는 경우 처리
            if (statData == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[MonsterBrain] <color=red>'{thisMonsterType}' 타입에 해당하는 스탯 데이터를 'AllMonsterStatData'에서 찾을 수 없습니다!</color> 'Default' 스탯으로 대체 시도.", this);
#endif
                // Fallback: MonsterType.Default 스탯을 찾아서 대체합니다. (필수: Default 타입이 AllMonsterStatData에 정의되어 있어야 함)
                statData = allMonsterStatData.GetStatEntry(MonsterType.Default);
                if (statData == null) // Default 스탯도 없는 경우
                {
#if UNITY_EDITOR
                    Debug.LogError($"[MonsterBrain] <color=red>'Default' 스탯도 'AllMonsterStatData'에서 찾을 수 없습니다! 몬스터 스탯 설정이 잘못되었습니다. 스크립트 비활성화.</color>", this);
#endif
                    enabled = false;
                    return;
                }

            }

            // ───── State 인스턴스 준비 ─────
            idle = new IdleState(this);
            patrol = new PatrolState(this);
            chase = GetComponent<FlyingTag>() != null ? new FloatChaseState(this) : new ChaseState(this);
            dead = new DeadState(this);

            attack = (GetComponent<RangedTag>() != null || hitBox == null)
                                     ? new RangeAttackState(this)
                                     : new MeleeAttackState(this, hitBox);

            // ───── FSM 초기화 ─────
            sm = new StateMachine();

            //----------Idle 루트-----------
            // ★★★ statData.idleMode 접근 시 MonsterStatEntry.IdleMode로 변경 ★★★
            if (statData.idleMode == MonsterStatEntry.IdleMode.GreedInteract)
            {
                // Interactable 컴포넌트 보장: GreedIdleState가 필요로 하는 경우 추가
                var interact = GetComponent<Interactable>() ?? gameObject.AddComponent<Interactable>();
                idle = new GreedIdleState(this, interact);
            }
            else
            {
                idle = new IdleState(this);
            }
            sm.Register(StateID.Idle, idle);
            //--------------------------------


            sm.Register(StateID.Patrol, patrol);
            sm.Register(StateID.Chase, chase);
            sm.Register(StateID.Attack, attack);
            sm.Register(StateID.Dead, dead);

            sm.ChangeState(StateID.Idle); // 초기 상태 설정
        }

        /* ───────── Unity Loop ───────── */
        private void FixedUpdate()
        {
            if (sm == null) return; // Awake 단계에서 초기화 실패 시 안전 장치
            sm.Tick(); // 현재 상태의 Tick 메서드 호출
        }

        /* ───────── External API ───────── */
        /// <summary>
        /// Called by States to transition. Contains guard‑logic so external callers don’t have to replicate it.
        /// </summary>
        public void ChangeState(StateID id)
        {
            if (sm == null) return; // FSM이 초기화되지 않았다면 아무것도 하지 않음 (스탯 로드 실패 등)

            // 순찰 사용 여부: ScriptableObject의 statData.usePatrol 값이 우선 적용되며,
            // statData가 null이거나 usePatrol 설정이 없다면 런타임 스탯(_runtimeStats)의 UsePatrol 값을 참고합니다.
            bool usePatrolFlag = statData != null ? statData.usePatrol : (RuntimeStats != null && RuntimeStats.UsePatrol);

            // 요청된 상태가 Patrol인데, 순찰 사용 플래그가 비활성화되어 있으면 상태 전환을 막습니다.
            if (id == StateID.Patrol && !usePatrolFlag)
                return;

            sm.ChangeState(id); // 상태 전환
        }
    }
}
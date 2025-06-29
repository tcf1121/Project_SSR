using Unity.VisualScripting;
using UnityEngine;

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
        public Transform sensor;                     // 하위 빈 gameObject – 바닥 감지용 (ground checker)
        public Transform wallSensor;                 // 하위 빈 gameObject – 벽 감지 전용 (wall sensor)
        public LayerMask groundMask;                 // 발 밑과 전방 체크에 사용할 Ground 레이어 마스크

        [SerializeField] private bool canClimbLadders = true;
        public bool CanClimbLadders => canClimbLadders;

        [Header("References")]
        [SerializeField] private Collider2D hitBox;  // 근접 공격시 사용 – 없으면 원거리 몬스터로 간주
        [SerializeField] private MonsterStats stats;      // 런타임 변동 스탯 (HP 등)
        [SerializeField] private MonsterStatData statData; // 설계용 ScriptableObject (이동속도 · 공격범위 등)

        /* ───────── Public helpers ───────── */
        public MonsterStats Stats => stats;
        public MonsterStatData StatData => statData;

        /// <summary>다른 스크립트(특히 State)에서 이동속도가 필요할 때 편하게 가져오도록 Helper 프로퍼티 제공.</summary>
        public float MoveSpeed => statData != null ? statData.moveSpeed : 0f;

        /* ───────── FSM ───────── */
        private StateMachine sm;
        public StateMachine Sm => sm;

        // 미리 생성해서 캐싱할 State 인스턴스
        private IState idle;
        private IState patrol;
        private IState chase;
        private IState attack;
        private IState dead;

        /* ====================================================================== */
        private void Awake()
        {
            // 필수 SO 누락 시 더 진행하지 않고 컴포넌트를 비활성화(NullReference 예방)
            if (statData == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[MonsterBrain] <color=orange>StatData not assigned</color> on {name}. FSM will not initialize.", this);
#endif
                enabled = false; // 기타 컴포넌트의 Update 호출 방지
                return;
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
            if (statData.idleMode == MonsterStatData.IdleMode.GreedInteract)
            {
                // Interactable컴포넌트 보장
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

            sm.ChangeState(StateID.Idle);
        }

        /* ───────── Unity Loop ───────── */
        private void FixedUpdate()
        {
            if (sm == null) return; // Awake 단계에서 초기화 실패 시 안전
            sm.Tick();
        }

        /* ───────── External API ───────── */
        /// <summary>
        /// Called by States to transition. Contains guard‑logic so external callers don’t have to replicate it.
        /// </summary>
        public void ChangeState(StateID id)
        {
            if (sm == null) return; // StatData 누락 → FSM 미초기화 → 아무 동작 안함

            // 순찰 사용 여부: SO 우선, 아니면 런타임 Stats 참고 – 기본값 true
            bool usePatrolFlag = statData != null ? statData.usePatrol : (stats != null && stats.UsePatrol);

            if (id == StateID.Patrol && !usePatrolFlag)
                return;

            sm.ChangeState(id);
        }
    }
}
using System;
using UnityEngine;

namespace PHG
{
    /// <summary>
    /// ScriptableObject holding balance-tunable parameters for each monster type.
    /// Jump 관련 필드는 <see cref="JumpMove"/> 컴포넌트(또는 CanJump 플래그)가 true일 때만 사용한다.
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterStatData", menuName = "Samsara/Monster Stat Data", order = 1)]
    [Serializable]
    public class MonsterStatEntry
    {
        /* ───────── Basic Info ───────── */
        [Tooltip("몬스터 이름 (디버그용)")]
        public MonsterType monsterType = MonsterType.None;
        [Header("Sensor Masks")]
        [Tooltip("감지할 바닥 레이어")]
        public LayerMask groundMask;
        [Tooltip("감지할 사다리 레이어")]
        public LayerMask ladderMask;
        [Tooltip("플레이어 레이어")] // 
        public LayerMask playerLayer;

        /* ───────── 기본 스탯 ───────── */
        [Header("Base Stats")]
        [Tooltip("체력")] public int maxHP = 100;    // 멤버 변수
        [Tooltip("공격력")] public int damage = 10;     // 멤버 변수
        [Tooltip("이동속도")] public float moveSpeed = 1.5f;       // 멤버 변수
        [Tooltip("돌진 가속도 배수")] public float rushMultiplier = 1.5f;// 멤버 변수
        [Tooltip("점프판단 허용 높이")] public float maxClimbableHeight = 2f; // 멤버 변수

        [Header("Damage Response")]
        [Tooltip("한 번에 이 값 이상의 데미지를 입으면 강제 경직")]
        public float staggerThreshold = 15f;
        [Tooltip("넉백 세기")]
        public float knockbackForce = 3f; // ← 추가
        [Tooltip("넉백 지속 시간(초)")]
        [Range(0f, 0.5f)]
        public float knockbackDuration = 0.07f;

        /* ───────── AI 감지 범위 ───────── */
        [Header("AI Detection Ranges")]
        [Tooltip("순찰 감지 범위")] public float patrolRange = 3f;    // 멤버 변수
        [Tooltip("추격 종료 범위")] public float chaseRange = 6f;    // 멤버 변수
        [Tooltip("공격 범위")] public float attackRange = 1f;    // 멤버 변수
        [Tooltip("돌진(가속) 발동 범위")] public float chargeRange = 2.5f;  // 멤버 변수

        /* ───────── 점프 설정 ───────── */
        [Header("Jump Settings")]
        [Tooltip("점프 기능 활성 여부")] public bool enableJump = false;       // 멤버 변수
        [Tooltip("수평 임펄스 계수 (0~1)")][Range(0f, 1f)] public float jumpHorizontalFactor = 0.6f; // 멤버 변수
        [Tooltip("기본 수직 점프 힘")] public float jumpForce = 4f;           // 멤버 변수
        [Tooltip("연속 점프 최소 간격(초)")] public float jumpCooldown = 0.45f;     // 멤버 변수
        [Tooltip("점프 시 y 위치 차이에 따른 점프력 보정 최대치")]
        public float maxJumpYDiffForAdjustment = 4f;
        [Tooltip("바닥 감지 오프셋 (Pivot 기준)")]
        public Vector2 groundCheckOffset = new Vector2(0f, -0.05f);
        [Tooltip("바닥 감지 반경")]
        public float groundCheckRadius = 0.12f;

        /* ───────── 동작 모드 ───────── */
        [Header("Behaviour Flags")]
        [Tooltip("순찰 AI 사용 여부")] public bool usePatrol = true;      // 멤버 변수
        public enum IdleMode { Default, GreedInteract }
        [Tooltip("Idle 동작 타입")] public IdleMode idleMode = IdleMode.Default; // 멤버 변수

        /* ───────── 원거리 전용 ───────── */
        [Header("Ranged Attack")]
        [Tooltip("원거리 공격 쿨타임")] public float rangedCooldown = 2f;   // 멤버 변수
        [Tooltip("투사체 프리팹")] public Projectile projectileprefab;  // 멤버 변수
        [Tooltip("사격 대기 범위")] public float readyRange = 10f;      // 멤버 변수
        [Tooltip("탄속")] public float projectileSpeed = 10f;   // 멤버 변수
        [Tooltip("투사체 수명(초)")] public float projectileLife = 3f;     // 멤버 변수
        public enum FirePattern { Single, Spread }
        [Tooltip("발사 패턴")] public FirePattern firePattern = FirePattern.Single; // 멤버 변수
        [Tooltip("Spread 탄알 수")] public int pelletCount = 5;          // 멤버 변수
        [Tooltip("Spread 각도(°)")][Range(30f, 360f)] public float spreadAngle = 60f; // 멤버 변수

        /* ───────── 근접 전용 ───────── */
        [Header("Melee Attack")]
        [Tooltip("공격 주기")] public float meleeCooldown = 0.6f;    // 멤버 변수
        public float attackAnimationTotalDuration = 0.6f;
        [Tooltip("근접 공격 범위")] public float meleeRadius = 0.25f;     // 멤버 변수

        /* ───────── Ability Flags (NEW) ───────── */
        [Header("Ability Flags")]
        [Tooltip("비행 유닛 여부")] public bool isFlying = false; // 멤버 변수
        [Tooltip("원거리 유닛 여부")] public bool isRanged = false; // 멤버 변수
        [Tooltip("추가 이동속도(Charge) 발동 유닛")] public bool isCharging = false; // 멤버 변수
        [Tooltip("사다리 등반 가능 여부")] public bool enableLadderClimb = false; // 멤버 변수


        [Header("Ladder Climb Settings")]
        [Tooltip("사다리 등반 기준이 되는 Y 거리 차이 (Threshold)")]
        public float climbYThreshold = 0.7f;
        [Tooltip("앞으로 탐색할 거리 (사다리 감지용)")]
        public Vector2 ladderForwardOffset = new Vector2(0.15f, 0f);

        [Tooltip("OverlapCircle 탐지 반경 (사다리 감지용)")]
        public float ladderDetectRadius = 0.35f;

        [Tooltip("사다리 오르내리는 속도")]
        public float climbSpeed = 3f;

        /*보상 정보 필드*/
        [Header("보상 정보")]
        [Tooltip("몬스터 처치 시 플레이어에게 주는 경험치")] public float expReward = 10f;// 멤버 변수
        [Tooltip("몬스터 처치 시 플레이어에게 주는 골드")] public float goldReward = 5f; // 멤버 변수
        [Header("보상 계수")]
        public float rewardCoefficient = 1.0f;

        [Header("Animation Flags")]
        [Tooltip("대기 애니메이션")] public bool hasIdleAnim = true;
        [Tooltip("이동 애니메이션")] public bool hasWalkAnim = true;
        [Tooltip("공격 애니메이션")] public bool hasAttackAnim = true;
        [Tooltip("경직 애니메이션")] public bool hasStaggerAnim = true;
        [Tooltip("사망 애니메이션")] public bool hasDeadAnim = true;

        /* ───────── Convenience Accessors ───────── */
        public float JumpForce => enableJump ? jumpForce : 0f;
        public float JumpCooldown => enableJump ? jumpCooldown : 0f;
    }
}
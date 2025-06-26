using UnityEngine;

namespace PHG
{
    /// <summary>
    /// ScriptableObject holding balance‑tunable parameters for each monster type.
    /// Jump 관련 필드는 <see cref="JumpMove"/> 컴포넌트가 존재할 때만 사용한다.
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterStatData", menuName = "Samsara/Monster Stat Data", order = 1)]
    public class MonsterStatData : ScriptableObject
    {

        [Header("기본 스탯")]
        [Tooltip("체력")]
        public int maxHP = 100;
        [Tooltip("공격력")]
        public int damage = 10;
        [Tooltip("이동속도")]
        public float moveSpeed = 2f;

        

        [Header("AI 인식 범위")]
        [Tooltip("감지 범위 (순찰 감지)")]
        public float patrolRange = 3f;
        [Tooltip("추격 범위")]
        public float chaseRange = 6f;
        [Tooltip("공격 범위")]
        public float attackRange = 1f;
        [Tooltip("돌진 범위 (근접/비행 가속)")]
        public float chargeRange = 2.5f;

        

        [Header("점프 설정")]
        [Tooltip("점프 기능 활성 여부 (JumpMove 컴포넌트가 있을 때만 적용)")]
        public bool enableJump = false;
        [Tooltip("수평 임펄스 계수 (0~1)")]
        [Range(0f, 1f)] public float jumpHorizontalFactor = 0.6f;
        [Tooltip("기본 수직 점프 힘")]
        public float jumpForce = 4f;
        [Tooltip("연속 점프 최소 간격 (초)")]
        public float jumpCooldown = 0.45f;

        [Header("동작 플래그")]
        [Tooltip("순찰 AI 사용 여부")]
        public bool usePatrol = true;

        [Header("원거리 전용")]
        [SerializeField, Tooltip("원거리 공격 쿨타임")]
        public float rangedCooldown = 2f;
        [SerializeField, Tooltip("투사체 프리팹")]
        public Projectile projectileprefab;
        [SerializeField, Tooltip("사격 대기 범위")]
        public float readyRange = 10f;

        // ───────── Convenience Accessors ─────────
        public float JumpForce => enableJump ? jumpForce : 0f;
        public float JumpCooldown => enableJump ? jumpCooldown : 0f;
    }
}

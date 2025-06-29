using System;
using UnityEngine;

namespace PHG
{
    /// <summary>
    /// ScriptableObject holding balance‑tunable parameters for each monster type.
    /// Jump 관련 필드는 <see cref="JumpMove"/> 컴포넌트가 존재할 때만 사용한다.
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterStatData", menuName = "Samsara/Monster Stat Data", order = 1)]
    [Serializable]
    public class MonsterStatEntry 
    {
        [Tooltip("몬스터 이름 (디버그용)")]
        public MonsterType monsterType = MonsterType.None;

        [Header("기본 스탯")]
        [Tooltip("체력")]
        public int maxHP = 100;
        [Tooltip("공격력")]
        public int damage = 10;
        [Tooltip("이동속도")]
        public float moveSpeed = 2f;
        [Tooltip("돌진 가속도")]
        public float rushMultiplier = 1.5f;
        [Tooltip("몬스터 점프판단 높이")]
        public float maxClimbableHeight = 2.0f;


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
        public enum IdleMode { Default, GreedInteract }
        [Tooltip("Idle 상태동작 타입")]
        public IdleMode idleMode = IdleMode.Default;

        [Header("원거리 전용")]
        [SerializeField, Tooltip("원거리 공격 쿨타임")]
        public float rangedCooldown = 2f;
        [SerializeField, Tooltip("투사체 프리팹")]
        public Projectile projectileprefab;
        [SerializeField, Tooltip("사격 대기 범위")]
        public float readyRange = 10f;
        [SerializeField, Tooltip("탄속")]
        public float projectileSpeed = 10f;
        [SerializeField, Tooltip("투사체 최대 생존 시간 (초)")]
        public float projectileLife = 3f;

        public enum FirePattern { Single, Spread }
        [Tooltip("발사 패턴")]
        public FirePattern firePattern = FirePattern.Single;

        [Tooltip("Spread 모드시 탄알 수")]
        public int pelletCount = 5;
        [Tooltip("Spread 모드시 발사 각도 (도 단위)")]
        [Range(30, 360f)] public float spreadAngle = 60f;



        [Header("근거리 전용")]
        [Tooltip("공격주기")]
        public float meleeCooldown = 0.6f;
        [Tooltip("근접 공격 범위")]
        public float meleeRadius = 1f;

        

        // ───────── Convenience Accessors ─────────
        public float JumpForce => enableJump ? jumpForce : 0f;
        public float JumpCooldown => enableJump ? jumpCooldown : 0f;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace LHE
{

    public class PlayerNormalAttack : MonoBehaviour
    {
        [Header("공격 설정")]
        [SerializeField] private float attackDuration = 0.2f; // 공격 지속 시간 애니메이션에 따라서 조절
        [SerializeField] private float attackCooldown = 1f; // 공격 쿨타임 (추후 무기에따라 변경되도록 설정)
        [SerializeField] private LayerMask enemyLayerMask = 1 << 12; // 적으로 인식할 레이어

        [Header("공격 설정")]
        [SerializeField] private Collider2D attackCollider; // 공격 범위 콜라이더
        [SerializeField] private Transform attackPoint; // 공격 위치

        [Header("디버그")]
        [SerializeField] private bool showAttackRange = true;

        // ===== 컴포넌트 참조 =====
        private PlayerController playerController;
        private PlayerStats playerStats;

        // ===== 상태 변수 =====
        private bool isAttacking = false;
        private bool canAttack = true;
        private float attackCooldownTimer = 0f;

        // ==== 중복 방지를 위한 헤시셋 ====
        private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>(); 

        #region 유니티 주기
        void Awake()
        {
            playerController = GetComponent<PlayerController>();
            playerStats = GetComponent<PlayerStats>();

            if (attackCollider != null) // 초기 비활성화
                attackCollider.enabled = false;
        }

        private void Update()
        {
            HandleAttackCooldown();
        }
        #endregion

        #region 입력 처리
        public void OnNormalAttack()
        {
            if (CanPerformAttack()) // 공격 가능여부 체크
            {
                StartCoroutine(AttackSequence());
            }
        }
        #endregion

        #region 공격 처리
        /// <summary>
        /// 공격 가능 조건 여부 판별
        /// </summary>
        /// <returns>여부 반환</returns>
        private bool CanPerformAttack()
        {
            if (canAttack && !isAttacking && !playerController.isWallSliding &&
                !playerController.isClimbing && !playerController.isDashing && !playerStats.isDead)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 일반 공격 시퀀스 코루틴
        /// </summary>
        private IEnumerator AttackSequence()
        {
            StartAttack();  // 공격 시작

            yield return new WaitForSeconds(attackDuration); // 대기

            EndAttack(); // 공격 종료
        }

        /// <summary>
        /// 일반 공격 시작
        /// </summary>
        private void StartAttack()
        {
            isAttacking = true;

            // 일반 공격 소모 및 쿨타임 시작
            canAttack = false;
            attackCooldownTimer = attackCooldown;

            hitEnemies.Clear(); // 리스트 초기화

            // 공격 콜라이더 활성화
            if (attackCollider != null)
                attackCollider.enabled = true;

            // 추후 공격 에니메이션 추가
            // animator.SetTrigger("Attack");
        }

        /// <summary>
        /// 일반 공격 종료
        /// </summary>
        private void EndAttack()
        {
            isAttacking = false;

            // 공격 콜라이더 비활성화
            if (attackCollider != null)
                attackCollider.enabled = false;
        }    

        /// <summary>
        /// 공격 쿨타임 처리
        /// </summary>
        private void HandleAttackCooldown()
        {
            if (!canAttack)
            {
                attackCooldownTimer -= Time.deltaTime;
                if (attackCooldownTimer <= 0f)
                {
                    canAttack = true ;
                }
            }
        }

        #endregion 

        #region 충돌 처리
        /// <summary>
        /// 공격 콜라이더 트리거
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isAttacking) return; // 공격중이 아니면 무시

            if (hitEnemies.Contains(other)) return; // 중복 방지

            if (!IsEnemy(other)) return; // 적레이어가 아니면 무시

            DealDamageToEnemy(other);
        }

        /// <summary>
        /// 적 레이어를 가지고 있는지 확인
        /// </summary>
        /// <param name="collider">확인할 오브젝트의 콜라이더</param>
        /// <returns>적 레이어 여부</returns>
        private bool IsEnemy(Collider2D collider)
        {
            // 부딪힌 콜라이더의 오브젝트 레이어 가져와서 레이어 번호를 이진법으로 비교
            return ((1 << collider.gameObject.layer) & enemyLayerMask) != 0;
        }

        /// <summary>
        /// 적에게 데미지 처리 (몬스터 스크립트 확인 필요
        /// </summary>
        /// <param name="enemy"></param>
        private void DealDamageToEnemy(Collider2D enemy)
        {
            hitEnemies.Add(enemy); // 중복 방지 리스트에 추가

            // 실질적인 데미지 계산 처리
            Debug.Log("공격 데미지 처리필요!");
            // 상대 몬스터 스크립트 파악 필요
        }
        #endregion

        #region 디버그용 기즈모
        void OnDrawGizmosSelected()
        {
            if (!showAttackRange || attackCollider == null) return;

            // 공격 범위 시각화
            Gizmos.color = isAttacking ? Color.red : Color.yellow;
            BoxCollider2D boxCol = (BoxCollider2D)attackCollider;
            Gizmos.DrawWireCube(
                (Vector2)attackCollider.transform.position + boxCol.offset,
                Vector2.Scale(boxCol.size, attackCollider.transform.lossyScale));
        }
        #endregion

        // 변수
        // 공격 지속시간
        // 공격 쿨타임
        // 적의 레이어

        // 공격 범위를 가질 콜라이더
        // 그 콜라이더의 위치

        // 디버그

        // 컴포넌트 가져오기

        // 업데이트에서 쿨타임 처리

        // x키 입력처리

        // 공격 로직
        // 공격 가능 여부
        // 공격 실행

        // 공격 코루틴 (공격실행에 들어감)
        // ㄴ 공격 시작 , 대기 , 공격 종료

        // 공격 시작
        // 공격중 상태, 쿨타임실행, 공격 콜라이더 활성화, 애니메이션

        // 공격종려
        // 위사항 반대

        // 충돌 처리
        // 공격중이 아니면 무시
        // 적레이어인지 확인
        // 중복 방지 구현 필요
        // 데미지 처리
        // 
        // 데미지 처리는 다른분이 짠 몬스터 스크립트 확인하여 연결할것

    }
}

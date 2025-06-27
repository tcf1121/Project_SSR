using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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

        private enum Weapon
        {
            dagger = 1
            // 무기들 종류와 무기에 따른 공격속도 (데모버전x)
        }

        // ===== 컴포넌트 참조 =====
        private PlayerController playerController;
        private PlayerStats playerStats;
        private Weapon weapon;

        // ===== 상태 변수 =====
        private bool isAttacking = false;
        private bool canAttack = true;
        private float attackCooldownTimer = 0f;

        // 중복방지 때린적을 기억하고 그공격이 다시 적을 때릴수 없게 하고 공격 지속시간 다되면 증발

        #region 유니티 주기
        void Awake()
        {
            playerController = GetComponent<PlayerController>();
            playerStats = GetComponent<PlayerStats>();

            // 공격 콜라이더 설정
        }

        private void Update()
        {
            HandleAttackCooldown();
        }
        #endregion

        #region 입력 처리

        private void OnNormalAttack(InputValue inputValue)
        {
            if (CanPerformAttack()) // 공격 가능여부 체크
            {

            }
        }

        #endregion

        #region 공격 처리
        private bool CanPerformAttack()
        {
            // 공격 쿨타임이 되어야함, 공격중이지 않아야함,
            // 클라밍중x, 벽타기중x (이건 기획분들에게 확인 받기)
            return 
        }

        private void PerformAttack()
        {
            // 공격 코루틴 실행
        }

        private IEnumerator AttackSequence()
        {
            StartAttack();  // 공격 시작

            yield return new WaitForSeconds(attackDuration); // 유지 및 대기

            EndAttack(); // 공격 종료
        }

        private void StartAttack()
        {

        }

        private void EndAttack()
        {

            canAttack = false; // 공격이 끝나면 쿨타임 시작 (아니면 시작으로 이동, 기획과 회의)
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
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isAttacking) return; // 공격중이 아니면 무시

            // 중복 방지 기능 필요

            if (!IsEnemy(other)) return; // 적레이어가 아니면 무시

        }

        private bool IsEnemy(Collider2D collider)
        {
            // 부딪힌 콜러이터의 오브젝트 레이어 가져와서 레이어 번호를 이진법으로 비교
            return ((1 << collider.gameObject.layer) & enemyLayerMask) != 0;
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

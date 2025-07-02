using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Interactions;

namespace SCR
{

    public enum PlayerState
    {
        Idle,
        Move,
        Dash,
        Sit,
        Climb
    }
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        // ===== 링크 스크립트 =====
        private Player player;

        // ===== 입력 상태 =====
        public Vector2 InputDirection { get { return _inputDirection; } }
        private Vector2 _inputDirection;

        // ===== 이동 상태 =====
        private float currentSpeed;
        public bool FacingRight { get => facingRight; }
        private bool facingRight = true;

        // ===== 상태 플래그들 =====
        private bool pressNormalAttack;
        private bool isGrounded;
        private bool isTouchingWall;
        private bool isWallSliding;
        private bool canClimb;
        private bool canJump;
        private PlayerState playerState;

        // ===== 타이머들 =====
        private float jumpBufferCounter;
        private float wallTouchTimer;
        private float ladderActionTimer;
        private float wallJumpInputBlockTimer = 0f; // 벽점프 타이머

        // ===== 기타 상태 변수 =====
        private Vector2 dashDirection;
        private Collider2D currentLadder;
        private Coroutine pickCor;

        // ===== 쿨타임 =====
        private bool[] IsCool = { false, false, false, false, false };
        private float[] CoolTimes = { 0, 0, 3f, 5f, 10f };

        #region 유니티 주기
        void Awake()
        {
            player = GetComponent<Player>();
            playerState = PlayerState.Idle;
            canClimb = true;
            canJump = true;
            pressNormalAttack = false;
        }

        void FixedUpdate()
        {
            if (playerState == PlayerState.Climb)
            {
                HandleClimbing();
            }
            else if (playerState != PlayerState.Dash)
            {
                {
                    HandleMove();
                    HandleWallSlide();
                }
            }

        }

        void Update()
        {
            CheckEnvironment();
            HandleLadder();  // 사다리 처리
            HandleCrouch();  // 앉기 상태 처리
            UpdateTimers();
        }
        #endregion

        #region 입력
        private void OnMove(InputValue value)
        {
            _inputDirection = value.Get<Vector2>().normalized;

            if (playerState != PlayerState.Climb)
            {
                if (InputDirection == Vector2.zero) playerState = PlayerState.Idle;
                else if (InputDirection.y == -1) playerState = PlayerState.Sit;
                else playerState = PlayerState.Move;
            }
        }

        private void OnJump()
        {
            HandleJump();
        }

        private void OnDash()
        {
            if (playerState != PlayerState.Climb)
            {
                HandleDash();
            }
        }

        private void OnInteraction()
        {
            if (pickCor == null)
            {
                pickCor = StartCoroutine(pickup());
            }
        }

        private void OnEquippedUI()
        {
            player.ConditionalUI.EquipUI.OnOffEquipUI();
        }

        private void OnNormalAttack()
        {
            pressNormalAttack = !pressNormalAttack;
            player.AlwaysOnUI.SetAttack(pressNormalAttack);
            player.PlayerWeapon.UseNomalAttack(pressNormalAttack);
            if (pressNormalAttack)
                player.Animator.SetTrigger("Attack");

        }

        private void OnSkill()
        {
            if (!IsCool[3])
            {
                player.PlayerWeapon.UseSkill();
                StartCoroutine(CoolTime(3));
            }

        }

        private void OnUltimate()
        {
            if (!IsCool[4])
            {
                player.PlayerWeapon.UseUltimateSkill();
                StartCoroutine(CoolTime(4));
            }

        }
        // 장비창 켜기


        // 상호작용 사용
        #endregion

        #region 환경 감지
        /// <summary>
        /// 땅과 벽, 사다리 감지
        /// </summary>
        void CheckEnvironment()
        {
            // 땅 감지
            CheckGrounded();

            // 벽 감지
            CheckWall();

            // 사다리 감지
            CheckLadder();
        }

        /// <summary>
        /// 땅에 있는지 체크
        /// </summary>
        void CheckGrounded()
        {
            isGrounded = Physics2D.OverlapBox(player.PlayerPhysical.GroundCheck.position, player.PlayerPhysical.GroundCheckBoxSize, 0f, player.PlayerPhysical.AllGroundLayers);
        }

        /// <summary>
        /// 벽 감지
        /// </summary>
        void CheckWall()
        {
            // 현재 바라보는 방향으로 레이캐스트
            Vector2 wallCheckDirection = facingRight ? Vector2.right : Vector2.left;
            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, wallCheckDirection, player.PlayerPhysical.WallCheckDistance, player.PlayerPhysical.WallLayers);

            bool currentlyTouchingWall = wallHit.collider != null;

            if (currentlyTouchingWall)
            {
                isTouchingWall = true;
                wallTouchTimer += Time.deltaTime;  // 벽에 닿은 시간 누적
            }
            else
            {
                isTouchingWall = false;
                wallTouchTimer = 0f;  // 벽에서 떨어지면 타이머 리셋
            }
        }

        /// <summary>
        /// 사다리 감지
        /// </summary>
        void CheckLadder()
        {
            if (playerState != PlayerState.Climb && canClimb)
            {
                Collider2D ladder = Physics2D.OverlapBox(player.Collider.bounds.center, player.Collider.bounds.size, 0f, player.PlayerPhysical.LadderLayer);
                currentLadder = ladder;
            }
        }

        #endregion

        #region 상호작용 처리
        /// <summary>
        /// 사다리 상호작용 처리
        /// </summary>
        void HandleLadder()
        {
            // 사다리 탈출 딜레이 중이면 진입 불가
            if (playerState == PlayerState.Climb) return;

            // 사다리 근처에서 상하 방향키를 누르면 클라이밍 시작
            if (currentLadder != null)
                if (Mathf.Abs(InputDirection.y) > 0.1f)
                {
                    EnterLadder();
                }
        }

        /// <summary>
        /// 앉기 상태 처리
        /// </summary>
        private void HandleCrouch()
        {
            if (playerState == PlayerState.Sit)
            {
                SetScale(0.5f);
            }
            else SetScale(1f);

        }

        private IEnumerator pickup()
        {
            player.PlayerPhysical.PickTrigger.SetActive(true);
            yield return new WaitForSeconds(1.0f);
            player.PlayerPhysical.PickTrigger.SetActive(false);
            StopCoroutine(pickCor);
            pickCor = null;
        }
        #endregion

        #region 이동
        /// <summary>
        /// 이동을 위한 메서드
        /// </summary>
        private void HandleMove()
        {
            if (player.PlayerPhysical.IsWallJumpInputBlocked) return;

            float targetSpeed = CalculateTargetSpeed();
            ApplyMovement(targetSpeed);
            UpdateFacing();
        }

        /// <summary>
        /// 이동속도 측정
        /// </summary>
        /// <returns>속도 반환</returns>
        private float CalculateTargetSpeed()
        {
            float speed = InputDirection.x * player.PlayerPhysical.FinalSpeed; ;//

            // 공중에서 이동 속도 감소
            if (!isGrounded)
                speed *= player.PlayerPhysical.AirMoveSpeedMultiplier;

            // 바닥에서만 앉기 상태일 때 속도 감소
            if (playerState == PlayerState.Sit && isGrounded)
                speed *= player.PlayerPhysical.CrouchSpeedMultiplier;

            // 벽 충돌 시 이동 제한
            if (isTouchingWall && IsTryingToMoveIntoWall())
                speed = 0f;

            return speed;
        }

        /// <summary>
        /// 벽잡기 이동 제한 판별
        /// </summary>
        /// <returns>이동 가능 여부</returns>
        private bool IsTryingToMoveIntoWall()
        {
            return (facingRight && InputDirection.x > 0) || (!facingRight && InputDirection.x < 0);
        }

        /// <summary>
        /// 가속도, 감속도를 계산하여 물리력을 가함
        /// </summary>
        /// <param name="targetSpeed">최종 이동</param>
        private void ApplyMovement(float targetSpeed)
        {
            float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? player.PlayerPhysical.Acceleration : player.PlayerPhysical.Deceleration;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);

            // 벽점프 중일 때는 수평 속도를 건드리지 않음
            if (player.PlayerPhysical.IsWallJumpInputBlocked)
            {
                // Y축 속도만 유지하고 X축은 그대로 둠
                player.Rigid.velocity = new Vector2(player.Rigid.velocity.x, player.Rigid.velocity.y);
            }
            else
            {
                player.Rigid.velocity = new Vector2(currentSpeed, player.Rigid.velocity.y);
            }
            player.Animator.SetBool("Move", Mathf.Abs(currentSpeed) > 0.1f);
        }

        /// <summary>
        /// 케릭터 보는 방향 업데이트
        /// </summary>
        private void UpdateFacing()
        {
            if (InputDirection.x > 0 && !facingRight)
                Flip();
            else if (InputDirection.x < 0 && facingRight)
                Flip();
        }

        /// <summary>
        /// 케릭터 보는 방향 뒤집기
        /// </summary>
        void Flip()
        {
            Vector3 scale = transform.localScale;
            facingRight = !facingRight;
            scale.x = facingRight == true ? 1 : -1;
            transform.localScale = scale;
        }
        #endregion

        #region 점프
        /// <summary>
        /// 점프 가능한 조건 확인
        /// </summary>
        void HandleJump()
        {
            if (playerState == PlayerState.Climb)
                LadderJump();
            else if (isWallSliding)
                WallSlideJump();
            else
            {
                if (isGrounded)
                    HandleGroundJump();
            }

        }

        /// <summary>
        /// 아래 점프를 할 경우 (매달린 경우 점프, 하단점프 판별) 
        /// </summary>
        private void HandleGroundJump()
        {
            if (playerState == PlayerState.Sit)
                TryDropThroughPlatform();
            else
                ExecuteJump();
        }

        /// <summary>
        /// 점프를 위한 물리력 가함
        /// </summary>
        private void ExecuteJump()
        {
            player.Rigid.velocity = new Vector2(player.Rigid.velocity.x, player.PlayerPhysical.FinalJump);
        }

        private void HalfExecuteJump()
        {
            player.Rigid.velocity = new Vector2(player.Rigid.velocity.x, player.PlayerPhysical.FinalJump * 0.6f);
        }

        /// <summary>
        /// 하단 점프 시도
        /// </summary>
        void TryDropThroughPlatform()
        {
            // 발 밑에 관통 가능한 플랫폼이 있는지 확인
            Collider2D platform = Physics2D.OverlapBox(player.PlayerPhysical.GroundCheck.position, player.PlayerPhysical.GroundCheckBoxSize, 0f, player.PlayerPhysical.PlatformLayer);

            if (platform != null)
            {
                StartCoroutine(DropThroughPlatform(platform));
            }
            else
            {
                ExecuteJump();
            }
        }

        /// <summary>
        /// 플랫폼 관통 코루틴
        /// </summary>
        IEnumerator DropThroughPlatform(Collider2D platform)
        {
            // 플레이어와 플랫폼 간의 충돌 무시
            Physics2D.IgnoreCollision(player.Collider, platform, true);

            // 관통 시간 대기
            yield return new WaitForSeconds(player.PlayerPhysical.DropThroughTime);

            // 충돌 복구
            Physics2D.IgnoreCollision(player.Collider, platform, false);
        }

        #endregion

        #region 사다리/밧줄 매달리기

        /// <summary>
        /// 사다리 시스템 메서드
        /// </summary>
        private void EnterLadder()
        {
            SetClimbingState(true);
            currentSpeed = 0f;
            PositionPlayerOnLadder();
            StartLadderDelay();
        }

        /// <summary>
        /// 등반 상태 설정 (등반 여부, 중력 설정)
        /// </summary>
        /// <param name="climbing">등반 여부</param>
        private void SetClimbingState(bool climbing)
        {
            playerState = climbing ? PlayerState.Climb : PlayerState.Idle;
            player.Rigid.gravityScale = climbing ? 0f : player.OriginalGravityScale;
        }


        /// <summary>
        /// 사다리에 플레이어 위치 배치
        /// </summary>
        private void PositionPlayerOnLadder()
        {
            if (InputDirection.y == -1)
            {
                Collider2D platform = Physics2D.OverlapBox(player.PlayerPhysical.GroundCheck.position, player.PlayerPhysical.GroundCheckBoxSize, 0f, player.PlayerPhysical.PlatformLayer);

                if (platform != null)
                {
                    StartCoroutine(DropThroughPlatform(platform));
                }
            }

            Vector3 playerPos = transform.position;
            playerPos.x = currentLadder.bounds.center.x;

            float ladderTop = currentLadder.bounds.max.y;
            float ladderBottom = currentLadder.bounds.min.y;

            if (playerPos.y < ladderBottom)
                playerPos.y = ladderBottom;
            else if (playerPos.y > ladderTop)
                playerPos.y = ladderTop;

            transform.position = playerPos;
        }

        /// <summary>
        /// 사다리 작동 딜레이 (연속작동 방지)
        /// </summary>
        private void StartLadderDelay()
        {
            ladderActionTimer = player.PlayerPhysical.LadderActionDelay;
        }

        /// <summary>
        /// 사다리 타기 중 이동 처리
        /// </summary>
        void HandleClimbing()
        {
            if (playerState != PlayerState.Climb) return;

            float climbDirection = CalculateClimbDirection();
            player.Rigid.velocity = new Vector2(0f, climbDirection * player.PlayerPhysical.FinalSpeed);
        }


        /// <summary>
        /// 계산 방향 등반 및 등반 제한
        /// </summary>
        /// <returns>등반 방향 반환</returns>
        private float CalculateClimbDirection()
        {
            float direction = InputDirection.y;

            // 끝에서 이동 제한
            if (CheckLadderBounds())
                return direction = 0f;
            return direction;
        }

        /// <summary>
        /// 사다리 경계 체크
        /// </summary>
        private bool CheckLadderBounds()
        {
            bool Check = false;
            float playerTop = player.Collider.bounds.max.y;
            float playerBottom = player.Collider.bounds.min.y;
            float ladderTop = currentLadder.bounds.max.y;
            float ladderBottom = currentLadder.bounds.min.y;
            float ladderMid = ladderBottom + (ladderTop - ladderBottom / 2);
            if ((InputDirection.y > 0 && playerBottom >= ladderTop) ||
                (InputDirection.y < 0 &&
                    (playerTop <= ladderBottom || (playerTop <= ladderMid && isGrounded))))
            {
                Check = true;
                ExitLadder();
            }
            return Check;
        }

        /// <summary>
        /// 사다리에서 내리기
        /// </summary>
        void ExitLadder()
        {
            StartCoroutine(EndClimb());
        }

        private IEnumerator EndClimb()
        {
            SetClimbingState(false);
            float delay = 0.5f;
            currentLadder = null;
            playerState = PlayerState.Idle;
            canClimb = false;
            while (delay > 0.0f)
            {
                delay -= Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            canClimb = true;
        }

        /// <summary>
        /// 사다리에서 점프
        /// </summary>
        void LadderJump()
        {
            ExitLadder();  // 먼저 사다리에서 내리기
            HalfExecuteJump(); // 약점프 실행
        }

        /// <summary>
        /// 벽슬라이드 중 점프
        /// </summary>
        void WallSlideJump()
        {
            // 벽 슬라이드 상태가 아니면 리턴
            if (!isWallSliding) return;

            // 점프 방향 계산 (현재 바라보는 방향의 반대로 10도 각도)
            Vector2 jumpDirection;

            if (facingRight)
            {
                // 오른쪽을 보고 있다면 왼쪽 위로 점프 (170도)
                float angle = 150f * Mathf.Deg2Rad;
                jumpDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Flip();
            }
            else
            {
                // 왼쪽을 보고 있다면 오른쪽 위로 점프 (10도)
                float angle = 30f * Mathf.Deg2Rad;
                jumpDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Flip();
            }

            // 기존 속도 초기화 후 벽점프 적용
            Vector2 wallJumpVelocity = jumpDirection * player.PlayerPhysical.FinalJump;
            player.Rigid.velocity = wallJumpVelocity;

            // 벽 슬라이드 상태 해제
            isWallSliding = false;
            isTouchingWall = false;
            wallTouchTimer = 0f;

            // 입력 차단 시작
            player.PlayerPhysical.IsWallJumpInputBlocked = true;
            wallJumpInputBlockTimer = player.PlayerPhysical.WallJumpInputBlockTime;
        }
        #endregion

        #region 앉기

        /// <summary>
        /// 임시 앉기 애니메이션
        /// </summary>
        /// <param name="yScale">스케일 변경</param>
        private void SetScale(float yScale)
        {
            Vector3 scale = transform.localScale;
            scale.y = yScale;
            transform.localScale = scale;
        }
        #endregion

        #region 벽 슬라이드 (벽잡기)
        /// <summary>
        /// 벽 슬라이드 처리
        /// </summary>
        void HandleWallSlide()
        {
            // 벽에 닿아있고, 공중에 있고, 떨어지고 있을 때, 충분히 접촉
            bool shouldWallSlide = isTouchingWall && !isGrounded && player.Rigid.velocity.y < 0 && wallTouchTimer >= player.PlayerPhysical.WallSlideDelayTime;

            isWallSliding = shouldWallSlide;

            if (shouldWallSlide)
            {
                Vector2 velocity = player.Rigid.velocity;
                velocity.y = Mathf.Max(velocity.y, -player.PlayerPhysical.WallSlideSpeed);
                player.Rigid.velocity = velocity;
            }
        }
        #endregion

        #region 대쉬
        private void HandleDash()
        {
            if (!IsCool[2] && playerState != PlayerState.Dash)
            {
                StartCoroutine(StartDash(0.3f));
                StartCoroutine(CoolTime(2));
            }
        }

        /// <summary>
        /// 대쉬 사용
        /// </summary>
        /// <param name="delay">대쉬 딜레이</param>
        /// <returns></returns>
        private IEnumerator StartDash(float delay)
        {
            playerState = PlayerState.Dash;
            dashDirection = new Vector2(facingRight ? 1 : -1, 0);
            player.Rigid.AddForce(dashDirection * 5f, ForceMode2D.Impulse);
            while (delay > 0.0f)
            {
                delay -= Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            playerState = PlayerState.Idle;
        }

        #endregion

        #region 타이머 관리
        /// <summary>
        /// 타이머가 필요한 변수들 넣는 메서드
        /// </summary>
        private void UpdateTimers()
        {
            UpdateTimer(ref jumpBufferCounter);
            UpdateTimer(ref ladderActionTimer);
            UpdateWallJumpInputBlock();
        }

        /// <summary>
        /// 변수에 타임을 업데이트
        /// </summary>
        /// <param name="timer">타이머 적용이 필요한 변수</param>
        private void UpdateTimer(ref float timer)
        {
            if (timer > 0)
                timer -= Time.deltaTime;
        }

        /// <summary>
        /// 벽점프 입력 차단 해제 처리
        /// </summary>
        private void UpdateWallJumpInputBlock()
        {
            if (player.PlayerPhysical.IsWallJumpInputBlocked)
            {
                wallJumpInputBlockTimer -= Time.deltaTime;

                if (wallJumpInputBlockTimer <= 0f)
                {
                    player.PlayerPhysical.IsWallJumpInputBlocked = false;
                    wallJumpInputBlockTimer = 0f;
                }
            }
        }
        #endregion

        #region 쿨타임 관련
        private IEnumerator CoolTime(int index)
        {
            IsCool[index] = true;
            float currentCoolTime = CoolTimes[index];
            player.AlwaysOnUI.CoolTime(index, true);
            while (currentCoolTime > 0.0f)
            {
                currentCoolTime -= Time.deltaTime;
                player.AlwaysOnUI.SetCool(index, CoolTimes[index], currentCoolTime);
                yield return new WaitForFixedUpdate();
            }
            player.AlwaysOnUI.CoolTime(index, false);
            IsCool[index] = false;
        }
        #endregion
    }
}
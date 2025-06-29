using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using LHE;

namespace PHG
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController1 : MonoBehaviour
    {
        [Header("이동 설정")]
        [SerializeField] public float acceleration = 50f;
        [SerializeField] public float deceleration = 30f;
        [SerializeField] public float airMoveSpeedMultiplier = 0.75f; // 공중 이동 속도 배율

        [Header("점프 설정")]
        [SerializeField] private float jumpBufferTime = 0.2f;

        [Header("하단 점프 설정")]
        [SerializeField] private float dropThroughTime = 0.3f;  // 하단점프 관통 지속 시간

        [Header("대쉬 설정")]
        [SerializeField] private float dashForce = 40f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1.5f;
        [SerializeField] private float dashEndSpeedRatio = 0.2f;

        [Header("앉기 설정")]
        [SerializeField] private float crouchSpeedMultiplier = 0.5f;  // 앉기 시 속도 배율

        [Header("벽 붙잡기 설정")]
        [SerializeField] private float wallSlideSpeed = 1.2f; // 벽 슬라이드 속도
        [SerializeField] private float wallCheckDistance = 0.6f; // 벽 감지 거리
        [SerializeField] private float wallSlideDelayTime = 0.1f; // 벽잡기 활성화 지연 시간
        [SerializeField] private float wallJumpInputBlockTime = 0.4f; // 벽점프 후 입력 차단 시간
        [SerializeField] private bool isWallJumpInputBlocked = false; // 입력 차단 상태

        [Header("사다리/밧줄 설정")]
        [SerializeField] private float climbSpeed = 3f; // 사다리 오르는 속도
        [SerializeField] private float ladderActionDelay = 0.3f; // 사다리 재진입 딜레이

        [Header("환경 감지")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckBoxSize = new Vector2(0.8f, 0.05f);
        [SerializeField] private LayerMask groundLayerMask = 1 << 9; // 관통 불가능 (돌, 벽돌 등)
        [SerializeField] private LayerMask platformLayer = 1 << 10; // 관통 가능 (나무 판자, 구름 등)
        [SerializeField] private LayerMask allGroundLayers = (1 << 9) | (1 << 10); // 모든 바닥
        [SerializeField] private LayerMask ladderLayer = 1 << 11;  // 사다리/밧줄 레이어

        // ===== 컴포넌트 =====
        private Rigidbody2D rb;
        private Collider2D col;
        private PlayerStats playerStats;

        // ===== 입력 상태 =====
        private float horizontalInput; // 좌우 입력 (-1 ~ 1)
        private float verticalInput; // 상하 입력 (-1 ~ 1)
        private bool jumpInputDown;
        private bool dashInputDown;
        private bool crouchInput;  // 앉기 입력 상태

        // ===== 이동 상태 =====
        private float currentSpeed;
        private bool facingRight = true;

        // ===== 상태 플래그들 =====
        private bool isGrounded;
        public bool isTouchingWall;
        public bool isWallSliding;
        private bool isCrouching;
        public bool isOnLadder;
        public bool isClimbing;
        public bool isDashing;

        // ===== 타이머들 =====
        private float jumpBufferCounter;
        private float dashTimeLeft;
        private float dashCooldownLeft;
        private float wallTouchTimer;
        private float ladderActionTimer;
        private float wallJumpInputBlockTimer = 0f; // 벽점프 타이머

        // ===== 기타 상태 변수 =====
        private float dashProgress;
        private Vector2 dashDirection;
        private Collider2D currentLadder;
        private float originalGravityScale;
        private float originalPlayerHeight;

        #region 유니티 주기
        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            playerStats = GetComponent<PlayerStats>();
            originalGravityScale = rb.gravityScale;
            originalPlayerHeight = col.bounds.size.y;
        }

        void Start()
        {
            // 발밑에 바닥 체크 자동으로 생성
            if (groundCheck == null)
            {
                GameObject child = new GameObject("GroundCheckPos");
                child.transform.parent = this.transform;

                Collider2D col = GetComponent<Collider2D>();
                if (col != null)
                {
                    float bottomY = col.bounds.min.y;
                    Vector3 localBottom = transform.InverseTransformPoint(new Vector3(transform.position.x, bottomY, transform.position.z));
                    child.transform.localPosition = new Vector3(0, localBottom.y, 0);
                }
                else
                {
                    child.transform.localPosition = Vector3.zero;
                }
            }
            groundCheck = transform.Find("GroundCheckPos");
        }

        void FixedUpdate()
        {
            if (isClimbing)
            {
                HandleClimbing();
            }
            else
            {
                HandleMove();
                HandleWallSlide();
                HandleDash();
            }
            HandleJump();
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
        public void OnHorizon(InputValue inputValue)
        {
            horizontalInput = inputValue.Get<float>();
        }

        public void OnVertical(InputValue inputValue) // 화살표 위아래 키를 가져옴
        {
            verticalInput = inputValue.Get<float>();
            crouchInput = verticalInput < -0.1f;  // -0.1f 이하면 아래키로 인식
        }

        public void OnJump()
        {
            jumpInputDown = true;
            jumpBufferCounter = jumpBufferTime;
        }

        public void OnDash()
        {
            if (dashCooldownLeft <= 0f && !isClimbing)
            {
                dashInputDown = true;
            }
        }
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
            isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckBoxSize, 0f, allGroundLayers);
        }

        /// <summary>
        /// 벽 감지
        /// </summary>
        void CheckWall()
        {
            // 현재 바라보는 방향으로 레이캐스트
            Vector2 wallCheckDirection = facingRight ? Vector2.right : Vector2.left;
            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, wallCheckDirection, wallCheckDistance, allGroundLayers);

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
            if (!isClimbing)
            {
                Collider2D ladder = Physics2D.OverlapBox(col.bounds.center, col.bounds.size, 0f, ladderLayer);
                isOnLadder = ladder != null;
                currentLadder = ladder;
            }
            else if (currentLadder == null)
            {
                // 사다리 오브젝트가 삭제된 경우에만 강제 탈출
                Debug.Log("사다리 오브젝트 삭제로 인한 강제 탈출");
                ExitLadder();
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
            if (ladderActionTimer > 0f || !isOnLadder || isClimbing) return;

            // 사다리 근처에서 상하 방향키를 누르면 클라이밍 시작
            if (Mathf.Abs(verticalInput) > 0.1f)
            {
                EnterLadder();
            }
        }

        /// <summary>
        /// 앉기 상태 처리
        /// </summary>
        private void HandleCrouch()
        {
            if (isClimbing) return;

            if (crouchInput && !isCrouching)
                StartCrouch();
            else if (!crouchInput && isCrouching)
                EndCrouch();
        }
        #endregion

        #region 이동
        /// <summary>
        /// 이동을 위한 메서드
        /// </summary>
        private void HandleMove()
        {
            if (isWallJumpInputBlocked) return;

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
            float speed = horizontalInput * playerStats.FinalSpeed;

            // 공중에서 이동 속도 감소
            if (!isGrounded)
                speed *= airMoveSpeedMultiplier;

            // 바닥에서만 앉기 상태일 때 속도 감소
            if (isCrouching && isGrounded)
                speed *= crouchSpeedMultiplier;

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
            return (facingRight && horizontalInput > 0) || (!facingRight && horizontalInput < 0);
        }

        /// <summary>
        /// 가속도, 감속도를 계산하여 물리력을 가함
        /// </summary>
        /// <param name="targetSpeed">최종 이동</param>
        private void ApplyMovement(float targetSpeed)
        {
            float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);

            // 벽점프 중일 때는 수평 속도를 건드리지 않음
            if (isWallJumpInputBlocked)
            {
                // Y축 속도만 유지하고 X축은 그대로 둠
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
            }
        }

        /// <summary>
        /// 케릭터 보는 방향 업데이트
        /// </summary>
        private void UpdateFacing()
        {
            if (horizontalInput > 0 && !facingRight)
                Flip();
            else if (horizontalInput < 0 && facingRight)
                Flip();
        }

        /// <summary>
        /// 케릭터 보는 방향 뒤집기
        /// </summary>
        void Flip()
        {
            facingRight = !facingRight;
            transform.Rotate(0, 180, 0);
        }
        #endregion

        #region 점프
        /// <summary>
        /// 점프 가능한 조건 확인
        /// </summary>
        void HandleJump()
        {
            if (!jumpInputDown || jumpBufferCounter <= 0f) return;

            if (isClimbing)
                LadderJump();
            else if (isWallSliding)
                WallSlideJump();
            else if (isGrounded)
                HandleGroundJump();
        }

        /// <summary>
        /// 아래 점프를 할 경우 (매달린 경우 점프, 하단점프 판별) 
        /// </summary>
        private void HandleGroundJump()
        {
            if (isCrouching)
                TryDropThroughPlatform();
            else
                ExecuteJump();
        }

        /// <summary>
        /// 점프를 위한 물리력 가함
        /// </summary>
        private void ExecuteJump()
        {
            rb.velocity = new Vector2(rb.velocity.x, playerStats.FinalJump + 5);
            ConsumeJumpInput();
        }

        /// <summary>
        /// 사다리 탈출용 대각 약점프
        /// </summary>
        private void HalfExecuteJump()
        {
            rb.velocity = new Vector2(rb.velocity.x, (playerStats.FinalJump + 5) * 0.6f);
            ConsumeJumpInput();
        }

        /// <summary>
        /// 점프 상태 업데이트(점프소모)
        /// </summary>
        private void ConsumeJumpInput()
        {
            jumpInputDown = false;
            jumpBufferCounter = 0f;
        }

        /// <summary>
        /// 하단 점프 시도
        /// </summary>
        void TryDropThroughPlatform()
        {
            // 발 밑에 관통 가능한 플랫폼이 있는지 확인
            Collider2D platform = Physics2D.OverlapBox(groundCheck.position, groundCheckBoxSize, 0f, platformLayer);

            if (platform != null)
            {
                StartCoroutine(DropThroughPlatform(platform));
                ConsumeJumpInput();
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
            Physics2D.IgnoreCollision(col, platform, true);

            // 관통 시간 대기
            yield return new WaitForSeconds(dropThroughTime);

            // 충돌 복구
            Physics2D.IgnoreCollision(col, platform, false);
        }

        #endregion

        #region 사다리/밧줄 매달리기
        /// <summary>
        /// 사다리 시스템 메서드
        /// </summary>
        private void EnterLadder()
        {
            SetClimbingState(true);
            ResetMovementForLadder();
            PositionPlayerOnLadder();
            StartLadderDelay();
        }

        /// <summary>
        /// 등반 상태 설정 (등반 여부, 중력 설정)
        /// </summary>
        /// <param name="climbing">등반 여부</param>
        private void SetClimbingState(bool climbing)
        {
            isClimbing = climbing;
            rb.gravityScale = climbing ? 0f : originalGravityScale;
        }

        /// <summary>
        /// 사다리 매달리기전 움직임 리셋
        /// </summary>
        private void ResetMovementForLadder()
        {
            // 모든 진행 중인 동작 중단
            if (isDashing) StopDash();
            if (isCrouching) EndCrouch();

            currentSpeed = 0f;
        }

        /// <summary>
        /// 대쉬 초기화, 정지
        /// </summary>
        private void StopDash()
        {
            isDashing = false;
            dashTimeLeft = 0f;
            dashProgress = 0f;
        }

        /// <summary>
        /// 사다리에 플레이어 위치 배치
        /// </summary>
        private void PositionPlayerOnLadder()
        {
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
            ladderActionTimer = ladderActionDelay;
        }

        /// <summary>
        /// 사다리 타기 중 이동 처리
        /// </summary>
        void HandleClimbing()
        {
            if (currentLadder == null)
            {
                ExitLadder();
                return;
            }

            float climbDirection = CalculateClimbDirection();
            rb.velocity = new Vector2(0f, climbDirection * climbSpeed);

            UpdateFacingWhileClimbing();
            CheckLadderBounds();
        }

        /// <summary>
        /// 사다리 타는 중 캐릭터 보는 방향 업데이트
        /// </summary>
        private void UpdateFacingWhileClimbing()
        {
            if (horizontalInput > 0.1f && !facingRight)
                Flip();
            else if (horizontalInput < -0.1f && facingRight)
                Flip();
        }

        /// <summary>
        /// 계산 방향 등반 및 등반 제한
        /// </summary>
        /// <returns>등반 방향 반환</returns>
        private float CalculateClimbDirection()
        {
            float direction = verticalInput;
            float playerY = transform.position.y;
            float playerHalfHeight = originalPlayerHeight * 0.5f;

            float ladderTop = currentLadder.bounds.max.y;
            float ladderBottom = currentLadder.bounds.min.y;

            // 끝에서 이동 제한
            if (direction > 0 && playerY >= ladderTop + playerHalfHeight)
                return 0f;
            if (direction < 0 && playerY <= ladderBottom - playerHalfHeight)
                return 0f;

            return direction;
        }

        /// <summary>
        /// 사다리 경계 체크
        /// </summary>
        void CheckLadderBounds()
        {
            if (ladderActionTimer > 0f) return;

            float playerY = transform.position.y;
            float playerHalfHeight = originalPlayerHeight * 0.5f;
            float ladderTop = currentLadder.bounds.max.y;
            float ladderBottom = currentLadder.bounds.min.y;

            if (playerY >= ladderTop + playerHalfHeight ||
                (playerY <= ladderBottom - playerHalfHeight && isGrounded))
            {
                ExitLadder();
                HalfExecuteJump();
            }
        }

        /// <summary>
        /// 사다리에서 내리기
        /// </summary>
        void ExitLadder()
        {
            if (!isClimbing) return;

            SetClimbingState(false);
            ClearLadderState();
            StartLadderDelay();
        }

        /// <summary>
        /// 사다리 붙잡기 상태 초기화
        /// </summary>
        private void ClearLadderState()
        {
            isOnLadder = false;
            currentLadder = null;
        }

        /// <summary>
        /// 사다리에서 점프
        /// </summary>
        void LadderJump()
        {
            ExitLadder();  // 먼저 사다리에서 내리기
            HalfExecuteJump(); // 약점프 실행

            ConsumeJumpInput();
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
                // 오른쪽을 보고 있다면 왼쪽 위로 점프 (150도)
                float angle = 150f * Mathf.Deg2Rad;
                jumpDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Flip();
            }
            else
            {
                // 왼쪽을 보고 있다면 오른쪽 위로 점프 (30도)
                float angle = 30f * Mathf.Deg2Rad;
                jumpDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Flip();
            }

            // 벽점프 적용
            rb.velocity = jumpDirection * (playerStats.FinalJump + 5);

            // 벽 슬라이드 상태 해제
            isWallSliding = false;
            wallTouchTimer = 0f;

            // 입력 차단 시작
            isWallJumpInputBlocked = true;
            wallJumpInputBlockTimer = wallJumpInputBlockTime;

            ConsumeJumpInput();
        }
        #endregion

        #region 앉기
        /// <summary>
        /// 앉기 시작
        /// </summary>
        private void StartCrouch()
        {
            isCrouching = true;
            SetScale(0.5f);
        }

        /// <summary>
        /// 앉기 종료
        /// </summary>
        private void EndCrouch()
        {
            isCrouching = false;
            SetScale(1f);
        }

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
            bool shouldWallSlide = isTouchingWall && !isGrounded && rb.velocity.y < 0 && wallTouchTimer >= wallSlideDelayTime;

            isWallSliding = shouldWallSlide;

            if (shouldWallSlide)
            {
                Vector2 velocity = rb.velocity;
                velocity.y = Mathf.Max(velocity.y, -wallSlideSpeed);
                rb.velocity = velocity;
            }
        }
        #endregion

        #region 대쉬
        private void HandleDash()
        {
            if (dashInputDown && CanStartDash() && !isDashing)
            {
                StartDash();
                dashInputDown = false;
            }

            if (isDashing)
                UpdateDash();
        }

        /// <summary>
        /// 대쉬 쿨타임 체크
        /// </summary>
        /// <returns>가능 여부</returns>
        private bool CanStartDash()
        {
            return dashCooldownLeft <= 0f && !isClimbing;
        }

        /// <summary>
        /// 대쉬 시작 (무적시간 관련 메서드 추가 필요)
        /// </summary>
        void StartDash()
        {
            isDashing = true;
            dashTimeLeft = dashDuration;
            dashProgress = 0f;

            // 대쉬 방향 결정 (바라보는 방향)
            dashDirection = new Vector2(facingRight ? 1 : -1, 0);

            // 무적 시작 하는 메서드 추가 또는 코루틴으로 작동
        }

        /// <summary>
        /// 대쉬 진행 업데이트
        /// </summary>
        void UpdateDash()
        {
            dashTimeLeft -= Time.fixedDeltaTime;
            dashProgress = 1f - (dashTimeLeft / dashDuration);

            if (dashTimeLeft <= 0f)
                EndDash();
            else
                ApplyDashVelocity();
        }

        /// <summary>
        /// 대쉬 감속도 조절
        /// </summary>
        private void ApplyDashVelocity()
        {
            float speedRatio = Mathf.Lerp(1f, dashEndSpeedRatio, dashProgress);
            rb.velocity = dashDirection * dashForce * speedRatio;
        }

        /// <summary>
        /// 대쉬 종료
        /// </summary>
        void EndDash()
        {
            isDashing = false;
            dashCooldownLeft = dashCooldown;
            // 대쉬 종료 시 속도 조절 (급정거 방지)
            rb.velocity *= 0.3f;
        }
        #endregion

        #region 타이머 관리
        /// <summary>
        /// 타이머가 필요한 변수들 넣는 메서드
        /// </summary>
        private void UpdateTimers()
        {
            UpdateTimer(ref jumpBufferCounter);
            UpdateTimer(ref dashCooldownLeft);
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
            if (isWallJumpInputBlocked)
            {
                wallJumpInputBlockTimer -= Time.deltaTime;

                if (wallJumpInputBlockTimer <= 0f)
                {
                    isWallJumpInputBlocked = false;
                    wallJumpInputBlockTimer = 0f;
                }
            }
        }
        #endregion

        #region 디버그용 기즈모
        void OnDrawGizmosSelected()
        {
            // 바닥 감지
            if (groundCheck == null) return;

            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckBoxSize);

            // 벽 감지 레이

            if (!Application.isPlaying) return;

            Vector3 direction = facingRight ? Vector3.right : Vector3.left;
            Gizmos.color = isTouchingWall ? Color.blue : Color.gray;
            Gizmos.DrawRay(transform.position, direction * wallCheckDistance);


            // 벽 슬라이드 상태 표시
            if (isWallSliding)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.6f);
            }
        }
        #endregion
    }
}
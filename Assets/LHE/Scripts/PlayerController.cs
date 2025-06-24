using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LHE
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("이동 설정")]
        [SerializeField] public float moveSpeed = 5f;
        [SerializeField] public float acceleration = 50f;
        [SerializeField] public float deceleration = 30f;

        [Header("점프 설정")]
        [SerializeField] private float jumpForce = 6f;
        [SerializeField] private float jumpBufferTime = 0.2f;

        [Header("하단 점프 설정")]
        [SerializeField] private float dropThroughTime = 0.3f;  // 하단점프 관통 지속 시간

        [Header("대쉬 설정")]
        [SerializeField] private float dashForce = 40f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 2f;
        [SerializeField] private float dashEndSpeedRatio = 0.2f;

        [Header("앉기 설정")]
        [SerializeField] private float crouchSpeedMultiplier = 0.5f;  // 앉기 시 속도 배율

        [Header("벽 붙잡기 설정")]
        [SerializeField] private float wallSlideSpeed = 1.5f; // 벽 슬라이드 속도
        [SerializeField] private float wallCheckDistance = 0.6f; // 벽 감지 거리
        [SerializeField] private float wallSlideDelayTime = 0.1f; // 벽잡기 활성화 지연 시간

        [Header("사다리/밧줄 설정")]
        [SerializeField] private float climbSpeed = 3f; // 사다리 오르는 속도
        [SerializeField] private float ladderActionDelay = 0.3f; // 사다리 재진입 딜레이

        [Header("환경 감지")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckBoxSize = new Vector2(0.8f, 0.05f);
        [SerializeField] private LayerMask groundLayerMask = 1 << 9; // 관통 불가능 (돌, 벽돌 등)
        [SerializeField] private LayerMask platformLayer = 1 << 10; // 관통 가능 (나무 판자, 구름 등)
        [SerializeField] private LayerMask allGroundLayers = (1 << 9) | (1 << 10); // 모든 바닥
        [SerializeField] private LayerMask ladderLayer = 1 << 8;  // 사다리/밧줄 레이어

        // ===== 컴포넌트 =====
        private Rigidbody2D rb;
        private Collider2D col;

        // ===== 입력 상태 =====
        private float horizontalInput; // 좌우 입력 (-1 ~ 1)
        private float verticalInput; // 상하 입력 (-1 ~ 1)
        private bool jumpInputDown;
        private bool dashInputDown;
        private bool crouchInput;  // 앉기 입력 상태

        // ===== 이동 상태 =====
        private float currentSpeed;
        private bool facingRight = true;

        // ===== 점프 상태 =====
        private bool isGrounded;
        private float jumpBufferCounter;

        // ===== 대쉬 상태 =====
        private bool isDashing;
        private float dashTimeLeft;
        private float dashCooldownLeft;
        private float dashProgress;
        private Vector2 dashDirection;

        // ===== 벽 붙잡기 상태 =====
        private bool isTouchingWall;
        private bool isWallSliding;
        private float wallTouchTimer;

        // ===== 앉기 상태 =====
        private bool isCrouching;

        // ===== 사다리/밧줄 상태 =====
        private bool isOnLadder;
        private bool isClimbing;
        private Collider2D currentLadder;
        private float originalGravityScale;
        private float ladderActionTimer;
        private float originalPlayerHeight;

        #region 유니티 주기
        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            originalGravityScale = rb.gravityScale;
            originalPlayerHeight = col.bounds.size.y;
        }

        void Update()
        {
            CheckEnvironment();
            HandleLadder();  // 사다리 처리
            HandleCrouch();  // 앉기 상태 처리
            UpdateTimers();
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

        public void OnJump(InputValue inputValue)
        {
            if (inputValue.isPressed)
            {
                jumpInputDown = true;
                jumpBufferCounter = jumpBufferTime;
            }
        }

        public void OnDash(InputValue inputValue)
        {
            if (inputValue.isPressed && dashCooldownLeft <= 0f && !isClimbing)
            {
                dashInputDown = true;
            }
        }

        #endregion

        #region 체크 및 타이머
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
            // 클라이밍 중이 아닐 때만 사다리 감지
            if (!isClimbing)
            {
                // 새로운 사다리 찾기
                Collider2D newLadder = Physics2D.OverlapBox(
                    col.bounds.center,
                    col.bounds.size,
                    0f,
                    ladderLayer
                );

                if (newLadder != null)
                {
                    isOnLadder = true;
                    currentLadder = newLadder;
                }
                else
                {
                    isOnLadder = false;
                    currentLadder = null;
                }
            }
            else
            {
                if (currentLadder == null)
                {
                    // 사다리 오브젝트가 삭제된 경우에만 강제 탈출
                    Debug.Log("사다리 오브젝트 삭제로 인한 강제 탈출");
                    ExitLadder();
                }
            }
        }

        /// <summary>
        /// 조작키 중복을 방지하기 위한 타이머 및 쿨타임
        /// </summary>
        void UpdateTimers()
        {
            if (jumpBufferCounter > 0)
            {
                jumpBufferCounter -= Time.deltaTime;
            }

            if (dashCooldownLeft > 0)
            {
                dashCooldownLeft -= Time.deltaTime;
            }
            if (ladderActionTimer > 0)
            {
                ladderActionTimer -= Time.deltaTime;
            }
        }
        #endregion

        #region 이동
        /// <summary>
        /// 이동을 위한 물리력 가함
        /// </summary>
        void HandleMove()
        {
            float targetSpeed = horizontalInput * moveSpeed;

            // 앉기 상태일 때 속도 감소
            if (isCrouching && isGrounded)
            {
                targetSpeed *= crouchSpeedMultiplier;
            }

            // 벽에 닿았을 때 이동 제한
            if (isTouchingWall)
            {
                // 벽 쪽으로 이동하려고 할 때만 막기
                bool tryingToMoveIntoWall = (facingRight && horizontalInput > 0) || (!facingRight && horizontalInput < 0);

                if (tryingToMoveIntoWall)
                {
                    targetSpeed = 0f;  // 벽 쪽으로는 이동 불가
                }
            }

            // 가속도 감속도 조절
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);

            rb.velocity = new Vector2(currentSpeed, rb.velocity.y);

            // 방향키 방향에 따라 바라보기 
            if (horizontalInput > 0 && !facingRight)
            {
                Flip();
            }
            else if (horizontalInput < 0 && facingRight)
            {
                Flip();
            }
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

        #region 사다리/밧줄
        /// <summary>
        /// 사다리 상호작용 처리
        /// </summary>
        void HandleLadder()
        {
            // 사다리 탈출 딜레이 중이면 진입 불가
            if (ladderActionTimer > 0f) return;

            // 사다리 근처에서 상하 방향키를 누르면 클라이밍 시작
            if (isOnLadder && !isClimbing && Mathf.Abs(verticalInput) > 0.1f)
            {
                EnterLadder();
            }
        }

        /// <summary>
        /// 사다리 타기 시작
        /// </summary>
        void EnterLadder()
        {
            isClimbing = true;
            rb.gravityScale = 0f;  // 중력 제거

            // 대쉬 중이었다면 강제 종료
            if (isDashing)
            {
                isDashing = false;
                dashTimeLeft = 0f;
                dashProgress = 0f;
            }

            // 앉기 상태였다면 일어서기
            if (isCrouching)
            {
                EndCrouch();
            }

            // 사다리 경계값 계산
            float ladderTop = currentLadder.bounds.max.y;
            float ladderBottom = currentLadder.bounds.min.y;
            float ladderCenterX = currentLadder.bounds.center.x;
            float playerY = transform.position.y;

            // 플레이어 위치 보정
            Vector3 playerPos = transform.position;

            // X축: 사다리 중심으로 맞추기
            playerPos.x = ladderCenterX;

            // Y축: 플레이어 위치에 따라 보정
            if (playerY < ladderBottom)
            {
                // 사다리보다 아래에서 잡으면 → 사다리 가장 아래로 이동
                playerPos.y = ladderBottom;
            }
            else if (playerY > ladderTop)
            {
                // 사다리보다 위에서 잡으면 → 사다리 가장 위로 이동
                playerPos.y = ladderTop;
            }

            // 사다리 범위 내에 있으면 Y축은 그대로 유지
            transform.position = playerPos;

            // 속도 초기화
            rb.velocity = Vector2.zero;
            currentSpeed = 0f;

            // 충돌 무시
            col.enabled = false;

            // 사다리 진입 딜레이 시작 (진입 직후 탈출 방지)
            ladderActionTimer = ladderActionDelay;
        }

        /// <summary>
        /// 사다리 타기 중 이동 처리
        /// </summary>
        void HandleClimbing()
        {
            if (currentLadder == null)
            {
                Debug.Log("currentLadder가 null이어서 강제 탈출");
                ExitLadder();
                return;
            }

            float ladderTop = currentLadder.bounds.max.y;
            float ladderBottom = currentLadder.bounds.min.y;
            float playerY = transform.position.y;
            float playerHalfHeight = originalPlayerHeight * 0.5f;

            // 상하 이동 입력
            float climbDirection = verticalInput;

            // 사다리 끝에서 이동 제한 (더 여유있게)
            if (climbDirection > 0 && playerY >= ladderTop + playerHalfHeight)
            {
                // 위쪽 끝 근처에서 더 올라가려고 하면 이동 제한
                climbDirection = 0f;
            }
            else if (climbDirection < 0 && playerY <= ladderBottom - playerHalfHeight)
            {
                // 아래쪽 끝 근처에서 더 내려가려고 하면 이동 제한
                climbDirection = 0f;
            }

            rb.velocity = new Vector2(0f, climbDirection * climbSpeed);

            // 경계 체크
            CheckLadderBounds();
        }

        /// <summary>
        /// 사다리 경계 체크
        /// </summary>
        void CheckLadderBounds()
        {
            if (currentLadder == null) return;

            if (ladderActionTimer > 0f) return;

            float ladderTop = currentLadder.bounds.max.y;
            float ladderBottom = currentLadder.bounds.min.y;
            float playerY = transform.position.y;

            // 플레이어 높이의 절반
            float playerHalfHeight = originalPlayerHeight * 0.5f;

            // 위쪽 경계 체크
            if (playerY >= ladderTop + playerHalfHeight)
            {
                ExitLadder();
                return;
            }

            // 아래쪽 경계 체크  
            if (playerY <= ladderBottom - playerHalfHeight && isGrounded)
            {
                ExitLadder();
                return;
            }
        }

        /// <summary>
        /// 사다리에서 내리기
        /// </summary>
        void ExitLadder()
        {
            if (!isClimbing) return;

            isClimbing = false;
            rb.gravityScale = originalGravityScale;

            // 사다리 탈출 시 속도 초기화
            rb.velocity = Vector2.zero;
            currentSpeed = 0f;

            // 충돌 복구
            col.enabled = true;

            // 사다리 탈출 딜레이 시작
            ladderActionTimer = ladderActionDelay;

            // 상태 정리
            isOnLadder = false;
            currentLadder = null;
        }

        /// <summary>
        /// 사다리에서 점프
        /// </summary>
        void LadderJump()
        {
            ExitLadder();  // 먼저 사다리에서 내리기

            // 대쉬 초기화
            isDashing = false;
            dashTimeLeft = 0f;

            // 바라보는 방향으로 점프
            Vector2 jumpDirection = new Vector2(facingRight ? 1f : -1f, 1f).normalized;
            rb.velocity = jumpDirection * jumpForce;

            jumpInputDown = false;
            jumpBufferCounter = 0f;
        }
        #endregion

        #region 점프
        /// <summary>
        /// 점프 가능한 조건 확인
        /// </summary>
        void HandleJump()
        {
            if (jumpInputDown && jumpBufferCounter > 0f)
            {
                // 사다리 타는 중이면 사다리 점프
                if (isClimbing)
                {
                    LadderJump();
                }
                // 땅에 있으면 일반 점프 또는 하단 점프
                else if (isGrounded)
                {
                    if (isCrouching)
                    {
                        TryDropThroughPlatform();
                    }
                    else
                    {
                        Jump();
                    }
                }
            }
        }

        /// <summary>
        /// 점프를 위한 물리력 가함
        /// </summary>
        void Jump()
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

            // 점프 상태 업데이트
            jumpInputDown = false; // 점프 입력 소모
            jumpBufferCounter = 0f; // 점프 버퍼 소모
        }

        /// <summary>
        /// 하단 점프 시도
        /// </summary>
        void TryDropThroughPlatform()
        {
            // 발 밑에 관통 가능한 플랫폼이 있는지 확인
            Collider2D platformBelow = Physics2D.OverlapBox(groundCheck.position, groundCheckBoxSize, 0f, platformLayer);

            if (platformBelow != null)
            {
                StartCoroutine(DropThroughPlatform(platformBelow));

                jumpInputDown = false;
                jumpBufferCounter = 0f;
            }
            else
            {
                // 관통 가능한 플랫폼이 없으면 일반 점프
                Jump();
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

        #region 앉기
        /// <summary>
        /// 앉기 상태 처리
        /// </summary>
        void HandleCrouch()
        {
            // 사다리 타는 중이면 앉기 처리 안함
            if (isClimbing) return;

            if (crouchInput && !isCrouching)
            {
                StartCrouch();
            }
            else if (!crouchInput && isCrouching)
            {
                EndCrouch();
            }
        }

        /// <summary>
        /// 앉기 시작
        /// </summary>
        void StartCrouch()
        {
            isCrouching = true;

            // 스프라이트 스케일로 앉기 표현 (높이 50% 감소)
            Vector3 scale = transform.localScale;
            scale.y = 0.5f;
            transform.localScale = scale;
        }

        /// <summary>
        /// 앉기 종료
        /// </summary>
        void EndCrouch()
        {
            isCrouching = false;

            // 원래 스케일로 복원
            Vector3 scale = transform.localScale;
            scale.y = 1f;
            transform.localScale = scale;
        }
        #endregion

        #region 벽잡기
        /// <summary>
        /// 벽 슬라이드 처리
        /// </summary>
        void HandleWallSlide()
        {
            // 벽에 닿아있고, 공중에 있고, 떨어지고 있을 때, 충분히 접촉
            if (isTouchingWall && !isGrounded && rb.velocity.y < 0 && wallTouchTimer >= wallSlideDelayTime)
            {
                isWallSliding = true;

                // 낙하 속도를 제한
                Vector2 velocity = rb.velocity;
                velocity.y = Mathf.Max(velocity.y, -wallSlideSpeed);
                rb.velocity = velocity;
            }
            else
            {
                isWallSliding = false;
            }
        }
        #endregion

        #region 대쉬
        void HandleDash()
        {
            if (isClimbing) return;

            if (dashInputDown && dashCooldownLeft <= 0f && !isDashing)
            {
                StartDash();
                dashInputDown = false;
            }

            if (isDashing)
            {
                UpdateDash();
            }
        }

        /// <summary>
        /// 대쉬 시작
        /// </summary>
        void StartDash()
        {
            isDashing = true;
            dashTimeLeft = dashDuration;
            dashCooldownLeft = dashCooldown;
            dashProgress = 0f;

            // 대쉬 방향 결정 (바라보는 방향)
            dashDirection = new Vector2(facingRight ? 1 : -1, 0);

            // 무적 시작 하는 메서드 추가
        }

        /// <summary>
        /// 대쉬 진행 업데이트
        /// </summary>
        void UpdateDash()
        {
            dashTimeLeft -= Time.fixedDeltaTime;
            dashProgress = 1f - (dashTimeLeft / dashDuration);

            if (dashTimeLeft <= 0f)
            {
                EndDash();
            }
            else
            {
                // 시작속도에서 끝속도로 부드럽게 감소
                float speedRatio = Mathf.Lerp(1f, dashEndSpeedRatio, dashProgress);
                Vector2 dashVelocity = dashDirection * dashForce * speedRatio;

                rb.velocity = dashVelocity;
            }
        }

        /// <summary>
        /// 대쉬 종료
        /// </summary>
        void EndDash()
        {
            isDashing = false;
            // 대쉬 종료 시 속도 조절 (급정거 방지)
            rb.velocity *= 0.3f;
        }
        #endregion

        #region 디버그용 기즈모
        void OnDrawGizmosSelected()
        {
            // 바닥 감지
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireCube(groundCheck.position, groundCheckBoxSize);
            }

            // 벽 감지 레이
            if (Application.isPlaying)
            {
                Vector3 rayDirection = facingRight ? Vector3.right : Vector3.left;
                Gizmos.color = isTouchingWall ? Color.blue : Color.gray;
                Gizmos.DrawRay(transform.position, rayDirection * wallCheckDistance);
            }

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


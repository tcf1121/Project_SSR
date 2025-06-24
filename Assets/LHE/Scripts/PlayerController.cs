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
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float jumpBufferTime = 0.2f;

        [Header("대쉬 설정")]
        [SerializeField] private float dashForce = 50f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 2f;
        [SerializeField] private float dashEndSpeedRatio = 0.2f;

        [Header("앉기 설정")]
        [SerializeField] private float crouchSpeedMultiplier = 0.5f;  // 앉기 시 속도 배율

        [Header("벽 붙잡기 설정")]
        [SerializeField] private float wallSlideSpeed = 2f; // 벽 슬라이드 속도
        [SerializeField] private float wallCheckDistance = 0.6f; // 벽 감지 거리

        [Header("환경 감지")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckBoxSize = new Vector2(0.8f, 0.05f);
        [SerializeField] private LayerMask groundLayerMask = 1 << 9;
        [SerializeField] private LayerMask wallLayerMask = 1 << 9;

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

        // ===== 앉기 상태 =====
        private bool isCrouching;

        #region 유니티 주기
        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
        }

        void Update()
        {
            CheckEnvironment();
            HandleCrouch();  // 앉기 상태 처리
            UpdateTimers();
        }

        void FixedUpdate()
        {
            HandleMove();
            HandleWallSlide();
            HandleDash();

                
                
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

            // 아래 화살표 입력 체크 (음수 값이면 아래키)
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
            if (inputValue.isPressed && dashCooldownLeft <= 0f)
            {
                dashInputDown = true;
            }
        }

        #endregion

        #region 체크 및 타이머
        /// <summary>
        /// 땅과 벽 감지
        /// </summary>
        void CheckEnvironment()
        {
            // 땅 감지
            CheckGrounded();

            // 벽 감지
            CheckWall();
        }

        /// <summary>
        /// 땅에 있는지 체크
        /// </summary>
        void CheckGrounded()
        {
            isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckBoxSize, 0f, groundLayerMask);
        }

        /// <summary>
        /// 벽 감지
        /// </summary>
        void CheckWall()
        {
            // 현재 바라보는 방향으로 레이캐스트
            Vector2 wallCheckDirection = facingRight ? Vector2.right : Vector2.left;
            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, wallCheckDirection, wallCheckDistance, wallLayerMask);

            isTouchingWall = wallHit.collider != null;
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
        }
        #endregion

        #region 이동
        /// <summary>
        /// 이동을 위한 물리력 가함
        /// </summary>
        void HandleMove()
        {
            float targetSpeed = horizontalInput * moveSpeed;

            // 사다리 오르는 중일 경우 타겟 스피드 속도 조절하여 위 아래로만 적용

            // 앉기 상태일 때 속도 감소
            if (isCrouching)
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

        #region 점프
        /// <summary>
        /// 점프 가능한 조건 확인
        /// </summary>
        void HandleJump()
        {
            if (jumpInputDown && jumpBufferCounter > 0f && isGrounded)
            {
                Jump();
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

            Debug.Log("점프 실행!");
        }

        #endregion

        #region 앉기
        /// <summary>
        /// 앉기 상태 처리
        /// </summary>
        void HandleCrouch()
        {
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

            Debug.Log("앉기 시작");
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

            Debug.Log("앉기 종료");
        }
        #endregion

        #region 벽잡기
        /// <summary>
        /// 벽 슬라이드 처리
        /// </summary>
        void HandleWallSlide()
        {
            // 벽에 닿아있고, 공중에 있고, 떨어지고 있을 때
            if (isTouchingWall && !isGrounded && rb.velocity.y < 0)
            {
                // 낙하 속도를 제한
                Vector2 velocity = rb.velocity;
                velocity.y = Mathf.Max(velocity.y, -wallSlideSpeed);
                rb.velocity = velocity;
            }
        }
        #endregion

        #region 대쉬
        void HandleDash()
        {
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

            Debug.Log("대쉬 시작!");
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

            Debug.Log("대쉬 종료!");
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
            if (isTouchingWall && !isGrounded)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }

            // 앉기 상태 표시
            if (isCrouching)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(transform.position, new Vector3(1f, 0.5f, 1f));
            }
        }
        #endregion
    }
}


using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEditor.Timeline.TimelinePlaybackControls;

namespace LHE
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("이동")]
        [SerializeField] public float moveSpeed = 5f;
        [SerializeField] public float acceleration = 50f;
        [SerializeField] public float deceleration = 30f;

        private float currentSpeed;
        private bool facingRight = true;

        [Header("점프")]
        [SerializeField] public float jumpForce = 5f;
        public float jumpPressure;

        [Header("대쉬")]
        public float dashForce = 20f;
        public float dashDuration = 0.2f;
        public float dashCooldown = 1f;


        [Header("그라운드 체크")]
        public Transform groundCheck;
        public Vector2 groundCheckBoxSize = new Vector2(0.5f, 0.1f);
        public LayerMask groundLayerMask = 1 << 6;

        // 컨트롤 (카운터 등)
        private float jumpBufferCounter;

        // 인풋
        private Vector2 moveInput;
        private bool jumpInput;
        private bool jumpInputDown;
        private bool dashInput;
        private Vector2 dashDirection;

        // 중복 방지
        private float jumpBufferTime = 0.2f;

        // 컴포넌트
        private Rigidbody2D rb;
        private Collider2D col;

        // 점프용
        private bool isGrounded;
        private bool wasGrounded;

        // 대쉬용
        private bool isDashing;
        private float dashCooldownLeft;
        private float dashTimeLeft;

        #region 유니티 주기
        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
        }

        void Update()
        {
            CheckGrounded();

            // 벽체크
            // 사다리
            // 바닥(플랫폼, 블록 체크)

            // 핸들러 (타이머 +, 인풋, )
            // 타이머
            HandleTimers();
            // 매프레임 키 초기화
            HandleInput();
        }

        void FixedUpdate()
        {
            // 핸들러 및 체크로 조건 확인 후에 실제 작동
            if (isDashing)
            {
                HandleDash();
            }

            Movement();

            if (isGrounded)
            {
                HandleJump();
            }
            
            
        }

        #endregion

        #region 인풋
        public void OnMove(InputValue inputValue)
        {
            moveInput = inputValue.Get<Vector2>();
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
            if (inputValue.isPressed)
            {
                dashInput = true;
            }
        }
        #endregion

        #region 체크 및 타이머
        /// <summary>
        /// 땅에 있는지 체크
        /// </summary>
        void CheckGrounded()
        {
            isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckBoxSize, 0f, groundLayerMask);
        }

        /// <summary>
        /// 조작키 중복을 방지하기 위한 타이머들
        /// </summary>
        void HandleTimers()
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

        void HandleInput()
        {
            if (jumpInputDown)
            {
                jumpInputDown = false;
            }

            if (dashInput)
            {
                dashInput = false;
            }
        }

        #region 이동
        /// <summary>
        /// 이동을 위한 물리력 가함
        /// </summary>
        void Movement()
        {
            float targetSpeed = moveInput.x * moveSpeed;

            // 사다리 오르는 중일 경우 타겟 스피드 속도 조절하여 위 아래로만 적용

            // 가속도 감속도 조절
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);

            rb.velocity = new Vector2(currentSpeed, rb.velocity.y);

            // 방향키 방향에 따라 바라보기 
            if (moveInput.x > 0 && !facingRight)
            {
                Flip();
            }
            else if (moveInput.x < 0 && facingRight)
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
            // 뒤집기
            transform.Rotate(0, 180, 0);
        }
        #endregion

        #region 점프
        /// <summary>
        /// 점프 가능한 조건 확인
        /// </summary>
        void HandleJump()
        {
            if (jumpBufferCounter > 0f) // 코요테 카운터 추가 &&
            {
                Jump();
            }

            jumpBufferCounter = 0f;
        }

        /// <summary>
        /// 점프를 위한 물리력 가함
        /// </summary>
        void Jump()
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        #endregion

        #region 대쉬
        void HandleDash()
        {
            if (dashInput && dashCooldownLeft <= 0f && !isDashing)
            {
                StartDash();
            }

            if (isDashing)
            {
                dashTimeLeft -= Time.fixedDeltaTime;

                if (dashTimeLeft <= 0f)
                {
                    EndDash();
                }
                else
                {
                    rb.velocity = dashDirection * dashForce;
                }
            }
        }

        void StartDash()
        {
            isDashing = true;
            dashTimeLeft = dashDuration;
            dashCooldownLeft = dashCooldown;

            if (moveInput != Vector2.zero)
            {
                dashDirection = moveInput.normalized;
            }
            else
            {
                dashDirection = new Vector2(facingRight ? 1 : -1, 0);
            }

            rb.gravityScale = 0f;
        }

        void EndDash()
        {
            isDashing = false;
            rb.gravityScale = 1f;

            rb.velocity *= 0.5f;
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
        }
        #endregion
    }
}


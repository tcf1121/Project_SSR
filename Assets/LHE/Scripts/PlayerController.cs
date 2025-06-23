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
        [Header("�̵�")]
        [SerializeField] public float moveSpeed = 5f;
        [SerializeField] public float acceleration = 50f;
        [SerializeField] public float deceleration = 30f;

        private float currentSpeed;
        private bool facingRight = true;

        [Header("����")]
        [SerializeField] public float jumpForce = 5f;
        public float jumpPressure;

        [Header("�뽬")]
        public float dashForce = 20f;
        public float dashDuration = 0.2f;
        public float dashCooldown = 1f;


        [Header("�׶��� üũ")]
        public Transform groundCheck;
        public Vector2 groundCheckBoxSize = new Vector2(0.5f, 0.1f);
        public LayerMask groundLayerMask = 1 << 6;

        // ��Ʈ�� (ī���� ��)
        private float jumpBufferCounter;

        // ��ǲ
        private Vector2 moveInput;
        private bool jumpInput;
        private bool jumpInputDown;
        private bool dashInput;
        private Vector2 dashDirection;

        // �ߺ� ����
        private float jumpBufferTime = 0.2f;

        // ������Ʈ
        private Rigidbody2D rb;
        private Collider2D col;

        // ������
        private bool isGrounded;
        private bool wasGrounded;

        // �뽬��
        private bool isDashing;
        private float dashCooldownLeft;
        private float dashTimeLeft;

        #region ����Ƽ �ֱ�
        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
        }

        void Update()
        {
            CheckGrounded();

            // ��üũ
            // ��ٸ�
            // �ٴ�(�÷���, ��� üũ)

            // �ڵ鷯 (Ÿ�̸� +, ��ǲ, )
            // Ÿ�̸�
            HandleTimers();
            // �������� Ű �ʱ�ȭ
            HandleInput();
        }

        void FixedUpdate()
        {
            // �ڵ鷯 �� üũ�� ���� Ȯ�� �Ŀ� ���� �۵�
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

        #region ��ǲ
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

        #region üũ �� Ÿ�̸�
        /// <summary>
        /// ���� �ִ��� üũ
        /// </summary>
        void CheckGrounded()
        {
            isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckBoxSize, 0f, groundLayerMask);
        }

        /// <summary>
        /// ����Ű �ߺ��� �����ϱ� ���� Ÿ�̸ӵ�
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

        #region �̵�
        /// <summary>
        /// �̵��� ���� ������ ����
        /// </summary>
        void Movement()
        {
            float targetSpeed = moveInput.x * moveSpeed;

            // ��ٸ� ������ ���� ��� Ÿ�� ���ǵ� �ӵ� �����Ͽ� �� �Ʒ��θ� ����

            // ���ӵ� ���ӵ� ����
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);

            rb.velocity = new Vector2(currentSpeed, rb.velocity.y);

            // ����Ű ���⿡ ���� �ٶ󺸱� 
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
        /// �ɸ��� ���� ���� ������
        /// </summary>
        void Flip()
        {
            facingRight = !facingRight;
            // ������
            transform.Rotate(0, 180, 0);
        }
        #endregion

        #region ����
        /// <summary>
        /// ���� ������ ���� Ȯ��
        /// </summary>
        void HandleJump()
        {
            if (jumpBufferCounter > 0f) // �ڿ��� ī���� �߰� &&
            {
                Jump();
            }

            jumpBufferCounter = 0f;
        }

        /// <summary>
        /// ������ ���� ������ ����
        /// </summary>
        void Jump()
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        #endregion

        #region �뽬
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

        #region ����׿� �����
        void OnDrawGizmosSelected()
        {
            // �ٴ� ����
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireCube(groundCheck.position, groundCheckBoxSize);
            }
        }
        #endregion
    }
}


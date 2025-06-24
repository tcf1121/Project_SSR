using UnityEngine;
using UnityEngine.InputSystem;

namespace LHE
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("�̵� ����")]
        [SerializeField] public float moveSpeed = 5f;
        [SerializeField] public float acceleration = 50f;
        [SerializeField] public float deceleration = 30f;

        [Header("���� ����")]
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float jumpBufferTime = 0.2f;

        [Header("�뽬 ����")]
        [SerializeField] private float dashForce = 50f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1f;
        [SerializeField] private float dashEndSpeedRatio = 0.2f;

        [Header("�� ����� ����")]
        [SerializeField] private float wallSlideSpeed = 2f; // �� �����̵� �ӵ�
        [SerializeField] private float wallCheckDistance = 0.6f; // �� ���� �Ÿ�

        [Header("ȯ�� ����")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckBoxSize = new Vector2(0.8f, 0.05f);
        [SerializeField] private LayerMask groundLayerMask = 1 << 9;
        [SerializeField] private LayerMask wallLayerMask = 1 << 9;

        // ===== ������Ʈ =====
        private Rigidbody2D rb;
        private Collider2D col;

        // ===== �Է� ���� =====
        private Vector2 moveInput;
        private bool jumpInputDown;
        private bool dashInputDown;

        // ===== �̵� ���� =====
        private float currentSpeed;
        private bool facingRight = true;

        // ===== ���� ���� =====
        private bool isGrounded;
        private float jumpBufferCounter;

        // ===== �뽬 ���� =====
        private bool isDashing;
        private float dashTimeLeft;
        private float dashCooldownLeft;
        private float dashProgress;
        private Vector2 dashDirection;

        // ===== �� ����� ���� =====
        private bool isTouchingWall;

        #region ����Ƽ �ֱ�
        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
        }

        void Update()
        {
            CheckEnvironment();
            UpdateTimers();
        }

        void FixedUpdate()
        {

            if (isDashing)
            {
                HandleDash();
            }
            else
            {
                HandleMove();
                HandleWallSlide();
            }

            HandleJump();

        }
        #endregion

        #region �Է�
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
                dashInputDown = true;
            }
        }
        #endregion

        #region üũ �� Ÿ�̸�
        /// <summary>
        /// ���� �� ����
        /// </summary>
        void CheckEnvironment()
        {
            // �� ����
            CheckGrounded();

            // �� ����
            CheckWall();
        }

        /// <summary>
        /// ���� �ִ��� üũ
        /// </summary>
        void CheckGrounded()
        {
            isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckBoxSize, 0f, groundLayerMask);
        }

        /// <summary>
        /// �� ����
        /// </summary>
        void CheckWall()
        {
            // ���� �ٶ󺸴� �������� ����ĳ��Ʈ
            Vector2 wallCheckDirection = facingRight ? Vector2.right : Vector2.left;
            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, wallCheckDirection, wallCheckDistance, wallLayerMask);

            isTouchingWall = wallHit.collider != null;
        }

        /// <summary>
        /// ����Ű �ߺ��� �����ϱ� ���� Ÿ�̸� �� ��Ÿ��
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

        #region �̵�
        /// <summary>
        /// �̵��� ���� ������ ����
        /// </summary>
        void HandleMove()
        {
            float targetSpeed = moveInput.x * moveSpeed;

            // ��ٸ� ������ ���� ��� Ÿ�� ���ǵ� �ӵ� �����Ͽ� �� �Ʒ��θ� ����

            // ���� ����� �� �̵� ����
            if (isTouchingWall)
            {
                // �� ������ �̵��Ϸ��� �� ���� ����
                bool tryingToMoveIntoWall = (facingRight && moveInput.x > 0) || (!facingRight && moveInput.x < 0);

                if (tryingToMoveIntoWall)
                {
                    targetSpeed = 0f;  // �� �����δ� �̵� �Ұ�
                }
            }

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
            transform.Rotate(0, 180, 0);
        }


        #endregion

        #region ����
        /// <summary>
        /// ���� ������ ���� Ȯ��
        /// </summary>
        void HandleJump()
        {
            if (jumpInputDown && jumpBufferCounter > 0f && isGrounded)
            {
                Jump();
            }
        }

        /// <summary>
        /// ������ ���� ������ ����
        /// </summary>
        void Jump()
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

            // ���� ���� ������Ʈ
            jumpInputDown = false; // ���� �Է� �Ҹ�
            jumpBufferCounter = 0f; // ���� ���� �Ҹ�

            Debug.Log("���� ����!");
        }

        #endregion

        #region �����
        /// <summary>
        /// �� �����̵� ó��
        /// </summary>
        void HandleWallSlide()
        {
            // ���� ����ְ�, ���߿� �ְ�, �������� ���� ��
            if (isTouchingWall && !isGrounded && rb.velocity.y < 0)
            {
                // ���� �ӵ��� ����
                Vector2 velocity = rb.velocity;
                velocity.y = Mathf.Max(velocity.y, -wallSlideSpeed);
                rb.velocity = velocity;
            }
        }
        #endregion

        #region �뽬
        void HandleDash()
        {

            if (dashInputDown && dashCooldownLeft <= 0f && !isDashing)
            {
                StartDash();
            }

            if (isDashing)
            {
                UpdateDash();
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

        /// <summary>
        /// �뽬 ����
        /// </summary>
        void StartDash()
        {
            isDashing = true;
            dashTimeLeft = dashDuration;
            dashCooldownLeft = dashCooldown;
            dashProgress = 0f;

            // �뽬 ���� ���� (�ٶ󺸴� ����)
            dashDirection = new Vector2(facingRight ? 1 : -1, 0);

            // ���� ���� �ϴ� �޼��� �߰�
        }

        /// <summary>
        /// �뽬 ���� ������Ʈ
        /// </summary>
        void UpdateDash()
        {
            dashTimeLeft -= Time.fixedDeltaTime;

            // �뽬 ���൵ ��� (0~1)
            dashProgress = 1f - (dashTimeLeft / dashDuration);

            if (dashTimeLeft <= 0f)
            {
                EndDash();
            }
            else
            {
                // ���ۼӵ����� ���ӵ��� �ε巴�� ����
                float speedRatio = Mathf.Lerp(1f, dashEndSpeedRatio, dashProgress);
                Vector2 dashVelocity = dashDirection * dashForce * speedRatio;

                rb.velocity = dashVelocity;
            }
        }

        /// <summary>
        /// �뽬 ����
        /// </summary>
        void EndDash()
        {
            isDashing = false;
            dashInputDown = false;
            // �뽬 ���� �� �ӵ� ���� (������ ����)
            rb.velocity *= 0.3f;

            Debug.Log("�뽬 ����!");
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

            // �� ���� ����
            if (Application.isPlaying)
            {
                Vector3 rayDirection = facingRight ? Vector3.right : Vector3.left;
                Gizmos.color = isTouchingWall ? Color.blue : Color.gray;
                Gizmos.DrawRay(transform.position, rayDirection * wallCheckDistance);
            }

            // �� �����̵� ���� ǥ��
            if (isTouchingWall && !isGrounded)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
        }
        #endregion
    }
}


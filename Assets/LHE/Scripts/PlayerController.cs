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
        [SerializeField] private float dashCooldown = 2f;
        [SerializeField] private float dashEndSpeedRatio = 0.2f;

        [Header("�ɱ� ����")]
        [SerializeField] private float crouchSpeedMultiplier = 0.5f;  // �ɱ� �� �ӵ� ����

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
        private float horizontalInput; // �¿� �Է� (-1 ~ 1)
        private float verticalInput; // ���� �Է� (-1 ~ 1)

        private bool jumpInputDown;
        private bool dashInputDown;
        private bool crouchInput;  // �ɱ� �Է� ����

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

        // ===== �ɱ� ���� =====
        private bool isCrouching;

        #region ����Ƽ �ֱ�
        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
        }

        void Update()
        {
            CheckEnvironment();
            HandleCrouch();  // �ɱ� ���� ó��
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

        #region �Է�
        public void OnHorizon(InputValue inputValue)
        {
            horizontalInput = inputValue.Get<float>();
        }

        public void OnVertical(InputValue inputValue) // ȭ��ǥ ���Ʒ� Ű�� ������
        {
            verticalInput = inputValue.Get<float>();

            // �Ʒ� ȭ��ǥ �Է� üũ (���� ���̸� �Ʒ�Ű)
            crouchInput = verticalInput < -0.1f;  // -0.1f ���ϸ� �Ʒ�Ű�� �ν�
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
            float targetSpeed = horizontalInput * moveSpeed;

            // ��ٸ� ������ ���� ��� Ÿ�� ���ǵ� �ӵ� �����Ͽ� �� �Ʒ��θ� ����

            // �ɱ� ������ �� �ӵ� ����
            if (isCrouching)
            {
                targetSpeed *= crouchSpeedMultiplier;
            }

            // ���� ����� �� �̵� ����
            if (isTouchingWall)
            {
                // �� ������ �̵��Ϸ��� �� ���� ����
                bool tryingToMoveIntoWall = (facingRight && horizontalInput > 0) || (!facingRight && horizontalInput < 0);

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

        #region �ɱ�
        /// <summary>
        /// �ɱ� ���� ó��
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
        /// �ɱ� ����
        /// </summary>
        void StartCrouch()
        {
            isCrouching = true;

            // ��������Ʈ �����Ϸ� �ɱ� ǥ�� (���� 50% ����)
            Vector3 scale = transform.localScale;
            scale.y = 0.5f;
            transform.localScale = scale;

            Debug.Log("�ɱ� ����");
        }

        /// <summary>
        /// �ɱ� ����
        /// </summary>
        void EndCrouch()
        {
            isCrouching = false;

            // ���� �����Ϸ� ����
            Vector3 scale = transform.localScale;
            scale.y = 1f;
            transform.localScale = scale;

            Debug.Log("�ɱ� ����");
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
                dashInputDown = false;
            }

            if (isDashing)
            {
                UpdateDash();
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

            Debug.Log("�뽬 ����!");
        }

        /// <summary>
        /// �뽬 ���� ������Ʈ
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

            // �ɱ� ���� ǥ��
            if (isCrouching)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(transform.position, new Vector3(1f, 0.5f, 1f));
            }
        }
        #endregion
    }
}


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

        [Header("�뽬")]
        [SerializeField] public float dashForce = 50f;           // �뽬 ���� �ӵ� (�ְ�ӵ�)
        [SerializeField] public float dashDuration = 0.2f;       // �뽬 ���� �ð�
        [SerializeField] public float dashCooldown = 1f;         // �뽬 ��ٿ�
        [SerializeField] public float dashEndSpeedRatio = 0.2f;  // �뽬 ���� �� �ӵ� ���� (0~1)


        [Header("�׶��� üũ")]
        public Transform groundCheck;
        public Vector2 groundCheckBoxSize = new Vector2(0.5f, 0.1f);
        public LayerMask groundLayerMask = 1 << 9;

        // ��Ʈ�� (ī���� ��)
        private float jumpBufferCounter;

        // ��ǲ
        private Vector2 moveInput;
        private bool jumpInput;
        private bool jumpInputDown;

        private bool dashInputDown;
        private Vector2 dashDirection;

        // �ߺ� ����
        private float jumpBufferTime = 0.2f; // ���� ����

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
        private float dashProgress;


        #region ����Ƽ �ֱ�
        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
        }

        void Update()
        {
            CheckGrounded();
            HandleTimers();
        }

        void FixedUpdate()
        {
            
            Movement();
            HandleJump();
            HandleDash();
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
                dashInputDown = true;
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
        /// ����Ű �ߺ��� �����ϱ� ���� Ÿ�̸� �� ��Ÿ��
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
            rb.velocity = rb.velocity * 0.3f;

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
        }
        #endregion
    }
}


using System.Collections;
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
        [SerializeField] private float jumpForce = 6f;
        [SerializeField] private float jumpBufferTime = 0.2f;

        [Header("�ϴ� ���� ����")]
        [SerializeField] private float dropThroughTime = 0.3f;  // �ϴ����� ���� ���� �ð�

        [Header("�뽬 ����")]
        [SerializeField] private float dashForce = 40f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 2f;
        [SerializeField] private float dashEndSpeedRatio = 0.2f;

        [Header("�ɱ� ����")]
        [SerializeField] private float crouchSpeedMultiplier = 0.5f;  // �ɱ� �� �ӵ� ����

        [Header("�� ����� ����")]
        [SerializeField] private float wallSlideSpeed = 1.5f; // �� �����̵� �ӵ�
        [SerializeField] private float wallCheckDistance = 0.6f; // �� ���� �Ÿ�
        [SerializeField] private float wallSlideDelayTime = 0.1f; // ����� Ȱ��ȭ ���� �ð�

        [Header("��ٸ�/���� ����")]
        [SerializeField] private float climbSpeed = 3f; // ��ٸ� ������ �ӵ�
        [SerializeField] private float ladderActionDelay = 0.3f; // ��ٸ� ������ ������

        [Header("ȯ�� ����")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckBoxSize = new Vector2(0.8f, 0.05f);
        [SerializeField] private LayerMask groundLayerMask = 1 << 9; // ���� �Ұ��� (��, ���� ��)
        [SerializeField] private LayerMask platformLayer = 1 << 10; // ���� ���� (���� ����, ���� ��)
        [SerializeField] private LayerMask allGroundLayers = (1 << 9) | (1 << 10); // ��� �ٴ�
        [SerializeField] private LayerMask ladderLayer = 1 << 8;  // ��ٸ�/���� ���̾�

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
        private bool isWallSliding;
        private float wallTouchTimer;

        // ===== �ɱ� ���� =====
        private bool isCrouching;

        // ===== ��ٸ�/���� ���� =====
        private bool isOnLadder;
        private bool isClimbing;
        private Collider2D currentLadder;
        private float originalGravityScale;
        private float ladderActionTimer;
        private float originalPlayerHeight;

        #region ����Ƽ �ֱ�
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
            HandleLadder();  // ��ٸ� ó��
            HandleCrouch();  // �ɱ� ���� ó��
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

        #region �Է�
        public void OnHorizon(InputValue inputValue)
        {
            horizontalInput = inputValue.Get<float>();
        }

        public void OnVertical(InputValue inputValue) // ȭ��ǥ ���Ʒ� Ű�� ������
        {
            verticalInput = inputValue.Get<float>();
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
            if (inputValue.isPressed && dashCooldownLeft <= 0f && !isClimbing)
            {
                dashInputDown = true;
            }
        }

        #endregion

        #region üũ �� Ÿ�̸�
        /// <summary>
        /// ���� ��, ��ٸ� ����
        /// </summary>
        void CheckEnvironment()
        {
            // �� ����
            CheckGrounded();

            // �� ����
            CheckWall();

            // ��ٸ� ����
            CheckLadder();
        }

        /// <summary>
        /// ���� �ִ��� üũ
        /// </summary>
        void CheckGrounded()
        {
            isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckBoxSize, 0f, allGroundLayers);
        }

        /// <summary>
        /// �� ����
        /// </summary>
        void CheckWall()
        {
            // ���� �ٶ󺸴� �������� ����ĳ��Ʈ
            Vector2 wallCheckDirection = facingRight ? Vector2.right : Vector2.left;
            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, wallCheckDirection, wallCheckDistance, allGroundLayers);

            bool currentlyTouchingWall = wallHit.collider != null;

            if (currentlyTouchingWall)
            {
                isTouchingWall = true;
                wallTouchTimer += Time.deltaTime;  // ���� ���� �ð� ����
            }
            else
            {
                isTouchingWall = false;
                wallTouchTimer = 0f;  // ������ �������� Ÿ�̸� ����
            }
        }

        /// <summary>
        /// ��ٸ� ����
        /// </summary>
        void CheckLadder()
        {
            // Ŭ���̹� ���� �ƴ� ���� ��ٸ� ����
            if (!isClimbing)
            {
                // ���ο� ��ٸ� ã��
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
                    // ��ٸ� ������Ʈ�� ������ ��쿡�� ���� Ż��
                    Debug.Log("��ٸ� ������Ʈ ������ ���� ���� Ż��");
                    ExitLadder();
                }
            }
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
            if (ladderActionTimer > 0)
            {
                ladderActionTimer -= Time.deltaTime;
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

            // �ɱ� ������ �� �ӵ� ����
            if (isCrouching && isGrounded)
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

        #region ��ٸ�/����
        /// <summary>
        /// ��ٸ� ��ȣ�ۿ� ó��
        /// </summary>
        void HandleLadder()
        {
            // ��ٸ� Ż�� ������ ���̸� ���� �Ұ�
            if (ladderActionTimer > 0f) return;

            // ��ٸ� ��ó���� ���� ����Ű�� ������ Ŭ���̹� ����
            if (isOnLadder && !isClimbing && Mathf.Abs(verticalInput) > 0.1f)
            {
                EnterLadder();
            }
        }

        /// <summary>
        /// ��ٸ� Ÿ�� ����
        /// </summary>
        void EnterLadder()
        {
            isClimbing = true;
            rb.gravityScale = 0f;  // �߷� ����

            // �뽬 ���̾��ٸ� ���� ����
            if (isDashing)
            {
                isDashing = false;
                dashTimeLeft = 0f;
                dashProgress = 0f;
            }

            // �ɱ� ���¿��ٸ� �Ͼ��
            if (isCrouching)
            {
                EndCrouch();
            }

            // ��ٸ� ��谪 ���
            float ladderTop = currentLadder.bounds.max.y;
            float ladderBottom = currentLadder.bounds.min.y;
            float ladderCenterX = currentLadder.bounds.center.x;
            float playerY = transform.position.y;

            // �÷��̾� ��ġ ����
            Vector3 playerPos = transform.position;

            // X��: ��ٸ� �߽����� ���߱�
            playerPos.x = ladderCenterX;

            // Y��: �÷��̾� ��ġ�� ���� ����
            if (playerY < ladderBottom)
            {
                // ��ٸ����� �Ʒ����� ������ �� ��ٸ� ���� �Ʒ��� �̵�
                playerPos.y = ladderBottom;
            }
            else if (playerY > ladderTop)
            {
                // ��ٸ����� ������ ������ �� ��ٸ� ���� ���� �̵�
                playerPos.y = ladderTop;
            }

            // ��ٸ� ���� ���� ������ Y���� �״�� ����
            transform.position = playerPos;

            // �ӵ� �ʱ�ȭ
            rb.velocity = Vector2.zero;
            currentSpeed = 0f;

            // �浹 ����
            col.enabled = false;

            // ��ٸ� ���� ������ ���� (���� ���� Ż�� ����)
            ladderActionTimer = ladderActionDelay;
        }

        /// <summary>
        /// ��ٸ� Ÿ�� �� �̵� ó��
        /// </summary>
        void HandleClimbing()
        {
            if (currentLadder == null)
            {
                Debug.Log("currentLadder�� null�̾ ���� Ż��");
                ExitLadder();
                return;
            }

            float ladderTop = currentLadder.bounds.max.y;
            float ladderBottom = currentLadder.bounds.min.y;
            float playerY = transform.position.y;
            float playerHalfHeight = originalPlayerHeight * 0.5f;

            // ���� �̵� �Է�
            float climbDirection = verticalInput;

            // ��ٸ� ������ �̵� ���� (�� �����ְ�)
            if (climbDirection > 0 && playerY >= ladderTop + playerHalfHeight)
            {
                // ���� �� ��ó���� �� �ö󰡷��� �ϸ� �̵� ����
                climbDirection = 0f;
            }
            else if (climbDirection < 0 && playerY <= ladderBottom - playerHalfHeight)
            {
                // �Ʒ��� �� ��ó���� �� ���������� �ϸ� �̵� ����
                climbDirection = 0f;
            }

            rb.velocity = new Vector2(0f, climbDirection * climbSpeed);

            // ��� üũ
            CheckLadderBounds();
        }

        /// <summary>
        /// ��ٸ� ��� üũ
        /// </summary>
        void CheckLadderBounds()
        {
            if (currentLadder == null) return;

            if (ladderActionTimer > 0f) return;

            float ladderTop = currentLadder.bounds.max.y;
            float ladderBottom = currentLadder.bounds.min.y;
            float playerY = transform.position.y;

            // �÷��̾� ������ ����
            float playerHalfHeight = originalPlayerHeight * 0.5f;

            // ���� ��� üũ
            if (playerY >= ladderTop + playerHalfHeight)
            {
                ExitLadder();
                return;
            }

            // �Ʒ��� ��� üũ  
            if (playerY <= ladderBottom - playerHalfHeight && isGrounded)
            {
                ExitLadder();
                return;
            }
        }

        /// <summary>
        /// ��ٸ����� ������
        /// </summary>
        void ExitLadder()
        {
            if (!isClimbing) return;

            isClimbing = false;
            rb.gravityScale = originalGravityScale;

            // ��ٸ� Ż�� �� �ӵ� �ʱ�ȭ
            rb.velocity = Vector2.zero;
            currentSpeed = 0f;

            // �浹 ����
            col.enabled = true;

            // ��ٸ� Ż�� ������ ����
            ladderActionTimer = ladderActionDelay;

            // ���� ����
            isOnLadder = false;
            currentLadder = null;
        }

        /// <summary>
        /// ��ٸ����� ����
        /// </summary>
        void LadderJump()
        {
            ExitLadder();  // ���� ��ٸ����� ������

            // �뽬 �ʱ�ȭ
            isDashing = false;
            dashTimeLeft = 0f;

            // �ٶ󺸴� �������� ����
            Vector2 jumpDirection = new Vector2(facingRight ? 1f : -1f, 1f).normalized;
            rb.velocity = jumpDirection * jumpForce;

            jumpInputDown = false;
            jumpBufferCounter = 0f;
        }
        #endregion

        #region ����
        /// <summary>
        /// ���� ������ ���� Ȯ��
        /// </summary>
        void HandleJump()
        {
            if (jumpInputDown && jumpBufferCounter > 0f)
            {
                // ��ٸ� Ÿ�� ���̸� ��ٸ� ����
                if (isClimbing)
                {
                    LadderJump();
                }
                // ���� ������ �Ϲ� ���� �Ǵ� �ϴ� ����
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
        /// ������ ���� ������ ����
        /// </summary>
        void Jump()
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

            // ���� ���� ������Ʈ
            jumpInputDown = false; // ���� �Է� �Ҹ�
            jumpBufferCounter = 0f; // ���� ���� �Ҹ�
        }

        /// <summary>
        /// �ϴ� ���� �õ�
        /// </summary>
        void TryDropThroughPlatform()
        {
            // �� �ؿ� ���� ������ �÷����� �ִ��� Ȯ��
            Collider2D platformBelow = Physics2D.OverlapBox(groundCheck.position, groundCheckBoxSize, 0f, platformLayer);

            if (platformBelow != null)
            {
                StartCoroutine(DropThroughPlatform(platformBelow));

                jumpInputDown = false;
                jumpBufferCounter = 0f;
            }
            else
            {
                // ���� ������ �÷����� ������ �Ϲ� ����
                Jump();
            }
        }

        /// <summary>
        /// �÷��� ���� �ڷ�ƾ
        /// </summary>
        IEnumerator DropThroughPlatform(Collider2D platform)
        {
            // �÷��̾�� �÷��� ���� �浹 ����
            Physics2D.IgnoreCollision(col, platform, true);

            // ���� �ð� ���
            yield return new WaitForSeconds(dropThroughTime);

            // �浹 ����
            Physics2D.IgnoreCollision(col, platform, false);
        }

        #endregion

        #region �ɱ�
        /// <summary>
        /// �ɱ� ���� ó��
        /// </summary>
        void HandleCrouch()
        {
            // ��ٸ� Ÿ�� ���̸� �ɱ� ó�� ����
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
        /// �ɱ� ����
        /// </summary>
        void StartCrouch()
        {
            isCrouching = true;

            // ��������Ʈ �����Ϸ� �ɱ� ǥ�� (���� 50% ����)
            Vector3 scale = transform.localScale;
            scale.y = 0.5f;
            transform.localScale = scale;
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
        }
        #endregion

        #region �����
        /// <summary>
        /// �� �����̵� ó��
        /// </summary>
        void HandleWallSlide()
        {
            // ���� ����ְ�, ���߿� �ְ�, �������� ���� ��, ����� ����
            if (isTouchingWall && !isGrounded && rb.velocity.y < 0 && wallTouchTimer >= wallSlideDelayTime)
            {
                isWallSliding = true;

                // ���� �ӵ��� ����
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

        #region �뽬
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
            if (isWallSliding)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.6f);
            }
        }
        #endregion
    }
}


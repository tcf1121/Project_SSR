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
        [Header("�̵� ����")]
        [SerializeField] public float acceleration = 50f;
        [SerializeField] public float deceleration = 30f;
        [SerializeField] public float airMoveSpeedMultiplier = 0.75f; // ���� �̵� �ӵ� ����

        [Header("���� ����")]
        [SerializeField] private float jumpBufferTime = 0.2f;

        [Header("�ϴ� ���� ����")]
        [SerializeField] private float dropThroughTime = 0.3f;  // �ϴ����� ���� ���� �ð�

        [Header("�뽬 ����")]
        [SerializeField] private float dashForce = 40f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1.5f;
        [SerializeField] private float dashEndSpeedRatio = 0.2f;

        [Header("�ɱ� ����")]
        [SerializeField] private float crouchSpeedMultiplier = 0.5f;  // �ɱ� �� �ӵ� ����

        [Header("�� ����� ����")]
        [SerializeField] private float wallSlideSpeed = 1.2f; // �� �����̵� �ӵ�
        [SerializeField] private float wallCheckDistance = 0.6f; // �� ���� �Ÿ�
        [SerializeField] private float wallSlideDelayTime = 0.1f; // ����� Ȱ��ȭ ���� �ð�
        [SerializeField] private float wallJumpInputBlockTime = 0.4f; // ������ �� �Է� ���� �ð�
        [SerializeField] private bool isWallJumpInputBlocked = false; // �Է� ���� ����

        [Header("��ٸ�/���� ����")]
        [SerializeField] private float climbSpeed = 3f; // ��ٸ� ������ �ӵ�
        [SerializeField] private float ladderActionDelay = 0.3f; // ��ٸ� ������ ������

        [Header("ȯ�� ����")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckBoxSize = new Vector2(0.8f, 0.05f);
        [SerializeField] private LayerMask groundLayerMask = 1 << 9; // ���� �Ұ��� (��, ���� ��)
        [SerializeField] private LayerMask platformLayer = 1 << 10; // ���� ���� (���� ����, ���� ��)
        [SerializeField] private LayerMask allGroundLayers = (1 << 9) | (1 << 10); // ��� �ٴ�
        [SerializeField] private LayerMask ladderLayer = 1 << 11;  // ��ٸ�/���� ���̾�

        // ===== ������Ʈ =====
        private Rigidbody2D rb;
        private Collider2D col;
        private PlayerStats playerStats;

        // ===== �Է� ���� =====
        private float horizontalInput; // �¿� �Է� (-1 ~ 1)
        private float verticalInput; // ���� �Է� (-1 ~ 1)
        private bool jumpInputDown;
        private bool dashInputDown;
        private bool crouchInput;  // �ɱ� �Է� ����

        // ===== �̵� ���� =====
        private float currentSpeed;
        private bool facingRight = true;

        // ===== ���� �÷��׵� =====
        private bool isGrounded;
        public bool isTouchingWall;
        public bool isWallSliding;
        private bool isCrouching;
        public bool isOnLadder;
        public bool isClimbing;
        public bool isDashing;

        // ===== Ÿ�̸ӵ� =====
        private float jumpBufferCounter;
        private float dashTimeLeft;
        private float dashCooldownLeft;
        private float wallTouchTimer;
        private float ladderActionTimer;
        private float wallJumpInputBlockTimer = 0f; // ������ Ÿ�̸�

        // ===== ��Ÿ ���� ���� =====
        private float dashProgress;
        private Vector2 dashDirection;
        private Collider2D currentLadder;
        private float originalGravityScale;
        private float originalPlayerHeight;

        #region ����Ƽ �ֱ�
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
            // �߹ؿ� �ٴ� üũ �ڵ����� ����
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
            HandleLadder();  // ��ٸ� ó��
            HandleCrouch();  // �ɱ� ���� ó��
            UpdateTimers();
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

        #region ȯ�� ����
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
            if (!isClimbing)
            {
                Collider2D ladder = Physics2D.OverlapBox(col.bounds.center, col.bounds.size, 0f, ladderLayer);
                isOnLadder = ladder != null;
                currentLadder = ladder;
            }
            else if (currentLadder == null)
            {
                // ��ٸ� ������Ʈ�� ������ ��쿡�� ���� Ż��
                Debug.Log("��ٸ� ������Ʈ ������ ���� ���� Ż��");
                ExitLadder();
            }
        }

        #endregion

        #region ��ȣ�ۿ� ó��
        /// <summary>
        /// ��ٸ� ��ȣ�ۿ� ó��
        /// </summary>
        void HandleLadder()
        {
            // ��ٸ� Ż�� ������ ���̸� ���� �Ұ�
            if (ladderActionTimer > 0f || !isOnLadder || isClimbing) return;

            // ��ٸ� ��ó���� ���� ����Ű�� ������ Ŭ���̹� ����
            if (Mathf.Abs(verticalInput) > 0.1f)
            {
                EnterLadder();
            }
        }

        /// <summary>
        /// �ɱ� ���� ó��
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

        #region �̵�
        /// <summary>
        /// �̵��� ���� �޼���
        /// </summary>
        private void HandleMove()
        {
            if (isWallJumpInputBlocked) return;

            float targetSpeed = CalculateTargetSpeed();
            ApplyMovement(targetSpeed);
            UpdateFacing();
        }

        /// <summary>
        /// �̵��ӵ� ����
        /// </summary>
        /// <returns>�ӵ� ��ȯ</returns>
        private float CalculateTargetSpeed()
        {
            float speed = horizontalInput * playerStats.FinalSpeed;

            // ���߿��� �̵� �ӵ� ����
            if (!isGrounded)
                speed *= airMoveSpeedMultiplier;

            // �ٴڿ����� �ɱ� ������ �� �ӵ� ����
            if (isCrouching && isGrounded)
                speed *= crouchSpeedMultiplier;

            // �� �浹 �� �̵� ����
            if (isTouchingWall && IsTryingToMoveIntoWall())
                speed = 0f;

            return speed;
        }

        /// <summary>
        /// ����� �̵� ���� �Ǻ�
        /// </summary>
        /// <returns>�̵� ���� ����</returns>
        private bool IsTryingToMoveIntoWall()
        {
            return (facingRight && horizontalInput > 0) || (!facingRight && horizontalInput < 0);
        }

        /// <summary>
        /// ���ӵ�, ���ӵ��� ����Ͽ� �������� ����
        /// </summary>
        /// <param name="targetSpeed">���� �̵�</param>
        private void ApplyMovement(float targetSpeed)
        {
            float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);

            // ������ ���� ���� ���� �ӵ��� �ǵ帮�� ����
            if (isWallJumpInputBlocked)
            {
                // Y�� �ӵ��� �����ϰ� X���� �״�� ��
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
            }
        }

        /// <summary>
        /// �ɸ��� ���� ���� ������Ʈ
        /// </summary>
        private void UpdateFacing()
        {
            if (horizontalInput > 0 && !facingRight)
                Flip();
            else if (horizontalInput < 0 && facingRight)
                Flip();
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
            if (!jumpInputDown || jumpBufferCounter <= 0f) return;

            if (isClimbing)
                LadderJump();
            else if (isWallSliding)
                WallSlideJump();
            else if (isGrounded)
                HandleGroundJump();
        }

        /// <summary>
        /// �Ʒ� ������ �� ��� (�Ŵ޸� ��� ����, �ϴ����� �Ǻ�) 
        /// </summary>
        private void HandleGroundJump()
        {
            if (isCrouching)
                TryDropThroughPlatform();
            else
                ExecuteJump();
        }

        /// <summary>
        /// ������ ���� ������ ����
        /// </summary>
        private void ExecuteJump()
        {
            rb.velocity = new Vector2(rb.velocity.x, playerStats.FinalJump + 5);
            ConsumeJumpInput();
        }

        /// <summary>
        /// ��ٸ� Ż��� �밢 ������
        /// </summary>
        private void HalfExecuteJump()
        {
            rb.velocity = new Vector2(rb.velocity.x, (playerStats.FinalJump + 5) * 0.6f);
            ConsumeJumpInput();
        }

        /// <summary>
        /// ���� ���� ������Ʈ(�����Ҹ�)
        /// </summary>
        private void ConsumeJumpInput()
        {
            jumpInputDown = false;
            jumpBufferCounter = 0f;
        }

        /// <summary>
        /// �ϴ� ���� �õ�
        /// </summary>
        void TryDropThroughPlatform()
        {
            // �� �ؿ� ���� ������ �÷����� �ִ��� Ȯ��
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

        #region ��ٸ�/���� �Ŵ޸���
        /// <summary>
        /// ��ٸ� �ý��� �޼���
        /// </summary>
        private void EnterLadder()
        {
            SetClimbingState(true);
            ResetMovementForLadder();
            PositionPlayerOnLadder();
            StartLadderDelay();
        }

        /// <summary>
        /// ��� ���� ���� (��� ����, �߷� ����)
        /// </summary>
        /// <param name="climbing">��� ����</param>
        private void SetClimbingState(bool climbing)
        {
            isClimbing = climbing;
            rb.gravityScale = climbing ? 0f : originalGravityScale;
        }

        /// <summary>
        /// ��ٸ� �Ŵ޸����� ������ ����
        /// </summary>
        private void ResetMovementForLadder()
        {
            // ��� ���� ���� ���� �ߴ�
            if (isDashing) StopDash();
            if (isCrouching) EndCrouch();

            currentSpeed = 0f;
        }

        /// <summary>
        /// �뽬 �ʱ�ȭ, ����
        /// </summary>
        private void StopDash()
        {
            isDashing = false;
            dashTimeLeft = 0f;
            dashProgress = 0f;
        }

        /// <summary>
        /// ��ٸ��� �÷��̾� ��ġ ��ġ
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
        /// ��ٸ� �۵� ������ (�����۵� ����)
        /// </summary>
        private void StartLadderDelay()
        {
            ladderActionTimer = ladderActionDelay;
        }

        /// <summary>
        /// ��ٸ� Ÿ�� �� �̵� ó��
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
        /// ��ٸ� Ÿ�� �� ĳ���� ���� ���� ������Ʈ
        /// </summary>
        private void UpdateFacingWhileClimbing()
        {
            if (horizontalInput > 0.1f && !facingRight)
                Flip();
            else if (horizontalInput < -0.1f && facingRight)
                Flip();
        }

        /// <summary>
        /// ��� ���� ��� �� ��� ����
        /// </summary>
        /// <returns>��� ���� ��ȯ</returns>
        private float CalculateClimbDirection()
        {
            float direction = verticalInput;
            float playerY = transform.position.y;
            float playerHalfHeight = originalPlayerHeight * 0.5f;

            float ladderTop = currentLadder.bounds.max.y;
            float ladderBottom = currentLadder.bounds.min.y;

            // ������ �̵� ����
            if (direction > 0 && playerY >= ladderTop + playerHalfHeight)
                return 0f;
            if (direction < 0 && playerY <= ladderBottom - playerHalfHeight)
                return 0f;

            return direction;
        }

        /// <summary>
        /// ��ٸ� ��� üũ
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
        /// ��ٸ����� ������
        /// </summary>
        void ExitLadder()
        {
            if (!isClimbing) return;

            SetClimbingState(false);
            ClearLadderState();
            StartLadderDelay();
        }

        /// <summary>
        /// ��ٸ� ����� ���� �ʱ�ȭ
        /// </summary>
        private void ClearLadderState()
        {
            isOnLadder = false;
            currentLadder = null;
        }

        /// <summary>
        /// ��ٸ����� ����
        /// </summary>
        void LadderJump()
        {
            ExitLadder();  // ���� ��ٸ����� ������
            HalfExecuteJump(); // ������ ����

            ConsumeJumpInput();
        }

        /// <summary>
        /// �������̵� �� ����
        /// </summary>
        void WallSlideJump()
        {
            // �� �����̵� ���°� �ƴϸ� ����
            if (!isWallSliding) return;

            // ���� ���� ��� (���� �ٶ󺸴� ������ �ݴ�� 10�� ����)
            Vector2 jumpDirection;

            if (facingRight)
            {
                // �������� ���� �ִٸ� ���� ���� ���� (150��)
                float angle = 150f * Mathf.Deg2Rad;
                jumpDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Flip();
            }
            else
            {
                // ������ ���� �ִٸ� ������ ���� ���� (30��)
                float angle = 30f * Mathf.Deg2Rad;
                jumpDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Flip();
            }

            // ������ ����
            rb.velocity = jumpDirection * (playerStats.FinalJump + 5);

            // �� �����̵� ���� ����
            isWallSliding = false;
            wallTouchTimer = 0f;

            // �Է� ���� ����
            isWallJumpInputBlocked = true;
            wallJumpInputBlockTimer = wallJumpInputBlockTime;

            ConsumeJumpInput();
        }
        #endregion

        #region �ɱ�
        /// <summary>
        /// �ɱ� ����
        /// </summary>
        private void StartCrouch()
        {
            isCrouching = true;
            SetScale(0.5f);
        }

        /// <summary>
        /// �ɱ� ����
        /// </summary>
        private void EndCrouch()
        {
            isCrouching = false;
            SetScale(1f);
        }

        /// <summary>
        /// �ӽ� �ɱ� �ִϸ��̼�
        /// </summary>
        /// <param name="yScale">������ ����</param>
        private void SetScale(float yScale)
        {
            Vector3 scale = transform.localScale;
            scale.y = yScale;
            transform.localScale = scale;
        }
        #endregion

        #region �� �����̵� (�����)
        /// <summary>
        /// �� �����̵� ó��
        /// </summary>
        void HandleWallSlide()
        {
            // ���� ����ְ�, ���߿� �ְ�, �������� ���� ��, ����� ����
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

        #region �뽬
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
        /// �뽬 ��Ÿ�� üũ
        /// </summary>
        /// <returns>���� ����</returns>
        private bool CanStartDash()
        {
            return dashCooldownLeft <= 0f && !isClimbing;
        }

        /// <summary>
        /// �뽬 ���� (�����ð� ���� �޼��� �߰� �ʿ�)
        /// </summary>
        void StartDash()
        {
            isDashing = true;
            dashTimeLeft = dashDuration;
            dashProgress = 0f;

            // �뽬 ���� ���� (�ٶ󺸴� ����)
            dashDirection = new Vector2(facingRight ? 1 : -1, 0);

            // ���� ���� �ϴ� �޼��� �߰� �Ǵ� �ڷ�ƾ���� �۵�
        }

        /// <summary>
        /// �뽬 ���� ������Ʈ
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
        /// �뽬 ���ӵ� ����
        /// </summary>
        private void ApplyDashVelocity()
        {
            float speedRatio = Mathf.Lerp(1f, dashEndSpeedRatio, dashProgress);
            rb.velocity = dashDirection * dashForce * speedRatio;
        }

        /// <summary>
        /// �뽬 ����
        /// </summary>
        void EndDash()
        {
            isDashing = false;
            dashCooldownLeft = dashCooldown;
            // �뽬 ���� �� �ӵ� ���� (������ ����)
            rb.velocity *= 0.3f;
        }
        #endregion

        #region Ÿ�̸� ����
        /// <summary>
        /// Ÿ�̸Ӱ� �ʿ��� ������ �ִ� �޼���
        /// </summary>
        private void UpdateTimers()
        {
            UpdateTimer(ref jumpBufferCounter);
            UpdateTimer(ref dashCooldownLeft);
            UpdateTimer(ref ladderActionTimer);
            UpdateWallJumpInputBlock();
        }

        /// <summary>
        /// ������ Ÿ���� ������Ʈ
        /// </summary>
        /// <param name="timer">Ÿ�̸� ������ �ʿ��� ����</param>
        private void UpdateTimer(ref float timer)
        {
            if (timer > 0)
                timer -= Time.deltaTime;
        }

        /// <summary>
        /// ������ �Է� ���� ���� ó��
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

        #region ����׿� �����
        void OnDrawGizmosSelected()
        {
            // �ٴ� ����
            if (groundCheck == null) return;

            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckBoxSize);

            // �� ���� ����

            if (!Application.isPlaying) return;

            Vector3 direction = facingRight ? Vector3.right : Vector3.left;
            Gizmos.color = isTouchingWall ? Color.blue : Color.gray;
            Gizmos.DrawRay(transform.position, direction * wallCheckDistance);


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
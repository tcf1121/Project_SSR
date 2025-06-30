using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Interactions;

namespace SCR
{

    public enum PlayerState
    {
        Idle,
        Move,
        Dash,
        Sit,
        Climb
    }
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        // ===== ��ũ ��ũ��Ʈ =====
        private Player player;

        // ===== �Է� ���� =====
        public Vector2 InputDirection { get { return _inputDirection; } }
        private Vector2 _inputDirection;

        // ===== �̵� ���� =====
        private float currentSpeed;
        public bool FacingRight { get => facingRight; }
        private bool facingRight = true;

        // ===== ���� �÷��׵� =====
        private bool pressNormalAttack;
        private bool isGrounded;
        private bool isTouchingWall;
        private bool isWallSliding;
        private bool canClimb;
        private bool canJump;
        private PlayerState playerState;

        // ===== Ÿ�̸ӵ� =====
        private float jumpBufferCounter;
        private float wallTouchTimer;
        private float ladderActionTimer;
        private float wallJumpInputBlockTimer = 0f; // ������ Ÿ�̸�

        // ===== ��Ÿ ���� ���� =====
        private Vector2 dashDirection;
        private Collider2D currentLadder;
        private Coroutine pickCor;

        // ===== ��Ÿ�� =====
        private bool[] IsCool = { false, false, false, false, false };
        private float[] CoolTimes = { 0, 0, 3f, 5f, 10f };

        #region ����Ƽ �ֱ�
        void Awake()
        {
            player = GetComponent<Player>();
            playerState = PlayerState.Idle;
            canClimb = true;
            canJump = true;
            pressNormalAttack = false;
        }

        void FixedUpdate()
        {
            if (playerState == PlayerState.Climb)
            {
                HandleClimbing();
            }
            else if (playerState != PlayerState.Dash)
            {
                {
                    HandleMove();
                    HandleWallSlide();
                }
            }

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
        private void OnMove(InputValue value)
        {
            _inputDirection = value.Get<Vector2>().normalized;

            if (playerState != PlayerState.Climb)
            {
                if (InputDirection == Vector2.zero) playerState = PlayerState.Idle;
                else if (InputDirection.y == -1) playerState = PlayerState.Sit;
                else playerState = PlayerState.Move;
            }
        }

        private void OnJump()
        {
            HandleJump();
        }

        private void OnDash()
        {
            if (playerState != PlayerState.Climb)
            {
                HandleDash();
            }
        }

        private void OnInteraction()
        {
            if (pickCor == null)
            {
                pickCor = StartCoroutine(pickup());
            }
        }

        private void OnEquippedUI()
        {
            player.ConditionalUI.EquipUI.OnOffEquipUI();
        }

        private void OnNormalAttack()
        {
            pressNormalAttack = !pressNormalAttack;
            player.AlwaysOnUI.SetAttack(pressNormalAttack);
            player.PlayerWeapon.UseNomalAttack(pressNormalAttack);
        }

        private void OnSkill()
        {
            if (!IsCool[3])
            {
                player.PlayerWeapon.UseSkill();
                StartCoroutine(CoolTime(3));
            }

        }

        private void OnUltimate()
        {
            if (!IsCool[4])
            {
                player.PlayerWeapon.UseUltimateSkill();
                StartCoroutine(CoolTime(4));
            }

        }
        // ���â �ѱ�


        // ��ȣ�ۿ� ���
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
            isGrounded = Physics2D.OverlapBox(player.PlayerPhysical.GroundCheck.position, player.PlayerPhysical.GroundCheckBoxSize, 0f, player.PlayerPhysical.AllGroundLayers);
        }

        /// <summary>
        /// �� ����
        /// </summary>
        void CheckWall()
        {
            // ���� �ٶ󺸴� �������� ����ĳ��Ʈ
            Vector2 wallCheckDirection = facingRight ? Vector2.right : Vector2.left;
            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, wallCheckDirection, player.PlayerPhysical.WallCheckDistance, player.PlayerPhysical.WallLayers);

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
            if (playerState != PlayerState.Climb && canClimb)
            {
                Collider2D ladder = Physics2D.OverlapBox(player.Collider.bounds.center, player.Collider.bounds.size, 0f, player.PlayerPhysical.LadderLayer);
                currentLadder = ladder;
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
            if (playerState == PlayerState.Climb) return;

            // ��ٸ� ��ó���� ���� ����Ű�� ������ Ŭ���̹� ����
            if (currentLadder != null)
                if (Mathf.Abs(InputDirection.y) > 0.1f)
                {
                    EnterLadder();
                }
        }

        /// <summary>
        /// �ɱ� ���� ó��
        /// </summary>
        private void HandleCrouch()
        {
            if (playerState == PlayerState.Sit)
            {
                SetScale(0.5f);
            }
            else SetScale(1f);

        }

        private IEnumerator pickup()
        {
            player.PlayerPhysical.PickTrigger.SetActive(true);
            yield return new WaitForSeconds(1.0f);
            player.PlayerPhysical.PickTrigger.SetActive(false);
            StopCoroutine(pickCor);
            pickCor = null;
        }
        #endregion

        #region �̵�
        /// <summary>
        /// �̵��� ���� �޼���
        /// </summary>
        private void HandleMove()
        {
            if (player.PlayerPhysical.IsWallJumpInputBlocked) return;

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
            float speed = InputDirection.x * player.PlayerPhysical.FinalSpeed; ;//

            // ���߿��� �̵� �ӵ� ����
            if (!isGrounded)
                speed *= player.PlayerPhysical.AirMoveSpeedMultiplier;

            // �ٴڿ����� �ɱ� ������ �� �ӵ� ����
            if (playerState == PlayerState.Sit && isGrounded)
                speed *= player.PlayerPhysical.CrouchSpeedMultiplier;

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
            return (facingRight && InputDirection.x > 0) || (!facingRight && InputDirection.x < 0);
        }

        /// <summary>
        /// ���ӵ�, ���ӵ��� ����Ͽ� �������� ����
        /// </summary>
        /// <param name="targetSpeed">���� �̵�</param>
        private void ApplyMovement(float targetSpeed)
        {
            float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? player.PlayerPhysical.Acceleration : player.PlayerPhysical.Deceleration;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);

            // ������ ���� ���� ���� �ӵ��� �ǵ帮�� ����
            if (player.PlayerPhysical.IsWallJumpInputBlocked)
            {
                // Y�� �ӵ��� �����ϰ� X���� �״�� ��
                player.Rigid.velocity = new Vector2(player.Rigid.velocity.x, player.Rigid.velocity.y);
            }
            else
            {
                player.Rigid.velocity = new Vector2(currentSpeed, player.Rigid.velocity.y);
            }
        }

        /// <summary>
        /// �ɸ��� ���� ���� ������Ʈ
        /// </summary>
        private void UpdateFacing()
        {
            if (InputDirection.x > 0 && !facingRight)
                Flip();
            else if (InputDirection.x < 0 && facingRight)
                Flip();
        }

        /// <summary>
        /// �ɸ��� ���� ���� ������
        /// </summary>
        void Flip()
        {
            Vector3 scale = transform.localScale;
            facingRight = !facingRight;
            scale.x = facingRight == true ? 1 : -1;
            transform.localScale = scale;
        }
        #endregion

        #region ����
        /// <summary>
        /// ���� ������ ���� Ȯ��
        /// </summary>
        void HandleJump()
        {
            if (playerState == PlayerState.Climb)
                LadderJump();
            else if (isWallSliding)
                WallSlideJump();
            else
            {
                if (isGrounded)
                    HandleGroundJump();
            }

        }

        /// <summary>
        /// �Ʒ� ������ �� ��� (�Ŵ޸� ��� ����, �ϴ����� �Ǻ�) 
        /// </summary>
        private void HandleGroundJump()
        {
            if (playerState == PlayerState.Sit)
                TryDropThroughPlatform();
            else
                ExecuteJump();
        }

        /// <summary>
        /// ������ ���� ������ ����
        /// </summary>
        private void ExecuteJump()
        {
            player.Rigid.velocity = new Vector2(player.Rigid.velocity.x, player.PlayerPhysical.FinalJump);
        }

        private void HalfExecuteJump()
        {
            player.Rigid.velocity = new Vector2(player.Rigid.velocity.x, player.PlayerPhysical.FinalJump * 0.6f);
        }

        /// <summary>
        /// �ϴ� ���� �õ�
        /// </summary>
        void TryDropThroughPlatform()
        {
            // �� �ؿ� ���� ������ �÷����� �ִ��� Ȯ��
            Collider2D platform = Physics2D.OverlapBox(player.PlayerPhysical.GroundCheck.position, player.PlayerPhysical.GroundCheckBoxSize, 0f, player.PlayerPhysical.PlatformLayer);

            if (platform != null)
            {
                StartCoroutine(DropThroughPlatform(platform));
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
            Physics2D.IgnoreCollision(player.Collider, platform, true);

            // ���� �ð� ���
            yield return new WaitForSeconds(player.PlayerPhysical.DropThroughTime);

            // �浹 ����
            Physics2D.IgnoreCollision(player.Collider, platform, false);
        }

        #endregion

        #region ��ٸ�/���� �Ŵ޸���

        /// <summary>
        /// ��ٸ� �ý��� �޼���
        /// </summary>
        private void EnterLadder()
        {
            SetClimbingState(true);
            currentSpeed = 0f;
            PositionPlayerOnLadder();
            StartLadderDelay();
        }

        /// <summary>
        /// ��� ���� ���� (��� ����, �߷� ����)
        /// </summary>
        /// <param name="climbing">��� ����</param>
        private void SetClimbingState(bool climbing)
        {
            playerState = climbing ? PlayerState.Climb : PlayerState.Idle;
            player.Rigid.gravityScale = climbing ? 0f : player.OriginalGravityScale;
        }


        /// <summary>
        /// ��ٸ��� �÷��̾� ��ġ ��ġ
        /// </summary>
        private void PositionPlayerOnLadder()
        {
            if (InputDirection.y == -1)
            {
                Collider2D platform = Physics2D.OverlapBox(player.PlayerPhysical.GroundCheck.position, player.PlayerPhysical.GroundCheckBoxSize, 0f, player.PlayerPhysical.PlatformLayer);

                if (platform != null)
                {
                    StartCoroutine(DropThroughPlatform(platform));
                }
            }

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
            ladderActionTimer = player.PlayerPhysical.LadderActionDelay;
        }

        /// <summary>
        /// ��ٸ� Ÿ�� �� �̵� ó��
        /// </summary>
        void HandleClimbing()
        {
            if (playerState != PlayerState.Climb) return;

            float climbDirection = CalculateClimbDirection();
            player.Rigid.velocity = new Vector2(0f, climbDirection * player.PlayerPhysical.FinalSpeed);
        }


        /// <summary>
        /// ��� ���� ��� �� ��� ����
        /// </summary>
        /// <returns>��� ���� ��ȯ</returns>
        private float CalculateClimbDirection()
        {
            float direction = InputDirection.y;

            // ������ �̵� ����
            if (CheckLadderBounds())
                return direction = 0f;
            return direction;
        }

        /// <summary>
        /// ��ٸ� ��� üũ
        /// </summary>
        private bool CheckLadderBounds()
        {
            bool Check = false;
            float playerTop = player.Collider.bounds.max.y;
            float playerBottom = player.Collider.bounds.min.y;
            float ladderTop = currentLadder.bounds.max.y;
            float ladderBottom = currentLadder.bounds.min.y;
            float ladderMid = ladderBottom + (ladderTop - ladderBottom / 2);
            if ((InputDirection.y > 0 && playerBottom >= ladderTop) ||
                (InputDirection.y < 0 &&
                    (playerTop <= ladderBottom || (playerTop <= ladderMid && isGrounded))))
            {
                Check = true;
                ExitLadder();
            }
            return Check;
        }

        /// <summary>
        /// ��ٸ����� ������
        /// </summary>
        void ExitLadder()
        {
            StartCoroutine(EndClimb());
        }

        private IEnumerator EndClimb()
        {
            SetClimbingState(false);
            float delay = 0.5f;
            currentLadder = null;
            playerState = PlayerState.Idle;
            canClimb = false;
            while (delay > 0.0f)
            {
                delay -= Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            canClimb = true;
        }

        /// <summary>
        /// ��ٸ����� ����
        /// </summary>
        void LadderJump()
        {
            ExitLadder();  // ���� ��ٸ����� ������
            HalfExecuteJump(); // ������ ����
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
                // �������� ���� �ִٸ� ���� ���� ���� (170��)
                float angle = 150f * Mathf.Deg2Rad;
                jumpDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Flip();
            }
            else
            {
                // ������ ���� �ִٸ� ������ ���� ���� (10��)
                float angle = 30f * Mathf.Deg2Rad;
                jumpDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Flip();
            }

            // ���� �ӵ� �ʱ�ȭ �� ������ ����
            Vector2 wallJumpVelocity = jumpDirection * player.PlayerPhysical.FinalJump;
            player.Rigid.velocity = wallJumpVelocity;

            // �� �����̵� ���� ����
            isWallSliding = false;
            isTouchingWall = false;
            wallTouchTimer = 0f;

            // �Է� ���� ����
            player.PlayerPhysical.IsWallJumpInputBlocked = true;
            wallJumpInputBlockTimer = player.PlayerPhysical.WallJumpInputBlockTime;
        }
        #endregion

        #region �ɱ�

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
            bool shouldWallSlide = isTouchingWall && !isGrounded && player.Rigid.velocity.y < 0 && wallTouchTimer >= player.PlayerPhysical.WallSlideDelayTime;

            isWallSliding = shouldWallSlide;

            if (shouldWallSlide)
            {
                Vector2 velocity = player.Rigid.velocity;
                velocity.y = Mathf.Max(velocity.y, -player.PlayerPhysical.WallSlideSpeed);
                player.Rigid.velocity = velocity;
            }
        }
        #endregion

        #region �뽬
        private void HandleDash()
        {
            if (!IsCool[2] && playerState != PlayerState.Dash)
            {
                StartCoroutine(StartDash(0.3f));
                StartCoroutine(CoolTime(2));
            }
        }

        /// <summary>
        /// �뽬 ���
        /// </summary>
        /// <param name="delay">�뽬 ������</param>
        /// <returns></returns>
        private IEnumerator StartDash(float delay)
        {
            playerState = PlayerState.Dash;
            dashDirection = new Vector2(facingRight ? 1 : -1, 0);
            player.Rigid.AddForce(dashDirection * 5f, ForceMode2D.Impulse);
            while (delay > 0.0f)
            {
                delay -= Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            playerState = PlayerState.Idle;
        }

        #endregion

        #region Ÿ�̸� ����
        /// <summary>
        /// Ÿ�̸Ӱ� �ʿ��� ������ �ִ� �޼���
        /// </summary>
        private void UpdateTimers()
        {
            UpdateTimer(ref jumpBufferCounter);
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
            if (player.PlayerPhysical.IsWallJumpInputBlocked)
            {
                wallJumpInputBlockTimer -= Time.deltaTime;

                if (wallJumpInputBlockTimer <= 0f)
                {
                    player.PlayerPhysical.IsWallJumpInputBlocked = false;
                    wallJumpInputBlockTimer = 0f;
                }
            }
        }
        #endregion

        #region ��Ÿ�� ����
        private IEnumerator CoolTime(int index)
        {
            IsCool[index] = true;
            float currentCoolTime = CoolTimes[index];
            player.AlwaysOnUI.CoolTime(index, true);
            while (currentCoolTime > 0.0f)
            {
                currentCoolTime -= Time.deltaTime;
                player.AlwaysOnUI.SetCool(index, CoolTimes[index], currentCoolTime);
                yield return new WaitForFixedUpdate();
            }
            player.AlwaysOnUI.CoolTime(index, false);
            IsCool[index] = false;
        }
        #endregion
    }
}
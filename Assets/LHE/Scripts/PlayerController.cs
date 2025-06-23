using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 50f;
    private float currentSpeed;
    private bool facingRight = true;

    [Header("그라운드 체크")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayerMask = 1<<6;

    // 컨트롤 (카운터 등)
    private float jumpBufferCounter;

    // 인풋
    private Vector2 moveInput;
    private bool jumpInputDown;
    private bool jumpInput;
    private bool dashInput;

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

    #region 유니티 주기
    void Start()
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
    }

    void FixedUpdate()
    {
        // 핸들러 및 체크로 조건 확인 후에 실제 작동

        Movement();

    }

    #endregion

    #region 인풋

    public void OnMove(InputValue context)
    {
        moveInput = context.Get<Vector2>();
    }

    //public void OnMove(InputAction.CallbackContext context)
    //{
    //    moveInput = context.ReadValue<Vector2>();
    //}


    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // 누른 즉시
            // jumpInputDown = true;
            // 중복 방지
            jumpBufferCounter = jumpBufferTime;
        }
        
        // 눌려있는 상태 트루, 땔때 flase / 눌린 시간 비례 점프
        jumpInput = context.ReadValue<float>() > 0;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
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
        // 착지 및 점프 타이밍 파악용
        wasGrounded = isGrounded;
        // 딛을 수 있는 땅은 레이어 그라운드
        // 관통 못하는 땅을 구별하기 위해 테그로 사용
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

        if (isGrounded && !wasGrounded)
        {
            // 착지시 대쉬 쿨타임 초기화
            dashCooldownLeft = 0f;
        }
    }

    /// <summary>
    /// 조작키 중복을 방지하기 위한 타이머들
    /// </summary>
    void HandleTimers()
    {
        if (isGrounded)
        {
            // coyoteTimeCounter = coyoteTime;
        }
        else
        {
            // coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (dashCooldownLeft > 0)
        {
            dashCooldownLeft -= Time.deltaTime;
        }

        //if (isWallJumping)
        //{
        //    wallJumpingCounter -= Time.deltaTime;
        //    if (wallJumpingCounter <= 0f)
        //    {
        //        isWallJumping = false;
        //    }
        //}
    }
    #endregion

    #region Movement
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

    // 점프 강도 조절 활용? // Interactions 사용
    //public void OnJump(InputAction.CallbackContext context)
    //{
    //    if (context.started)
    //    {
    //        // 버튼을 누르기 시작했을 때
    //        // 점프 준비 애니메이션 등
    //    }

    //    if (context.performed)
    //    {
    //        // 버튼을 완전히 눌렀을 때
    //        // 실제 점프 실행
    //    }

    //    if (context.canceled)
    //    {
    //        // 버튼을 뗐을 때
    //        // 가변 점프 높이 등
    //    }
    //}
}

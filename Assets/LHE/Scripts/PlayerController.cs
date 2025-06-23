using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("�̵�")]
    public float moveSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 50f;
    private float currentSpeed;
    private bool facingRight = true;

    [Header("�׶��� üũ")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayerMask = 1<<6;

    // ��Ʈ�� (ī���� ��)
    private float jumpBufferCounter;

    // ��ǲ
    private Vector2 moveInput;
    private bool jumpInputDown;
    private bool jumpInput;
    private bool dashInput;

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

    #region ����Ƽ �ֱ�
    void Start()
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
    }

    void FixedUpdate()
    {
        // �ڵ鷯 �� üũ�� ���� Ȯ�� �Ŀ� ���� �۵�

        Movement();

    }

    #endregion

    #region ��ǲ

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
            // ���� ���
            // jumpInputDown = true;
            // �ߺ� ����
            jumpBufferCounter = jumpBufferTime;
        }
        
        // �����ִ� ���� Ʈ��, ���� flase / ���� �ð� ��� ����
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

    #region üũ �� Ÿ�̸�
    /// <summary>
    /// ���� �ִ��� üũ
    /// </summary>
    void CheckGrounded()
    {
        // ���� �� ���� Ÿ�̹� �ľǿ�
        wasGrounded = isGrounded;
        // ���� �� �ִ� ���� ���̾� �׶���
        // ���� ���ϴ� ���� �����ϱ� ���� �ױ׷� ���
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

        if (isGrounded && !wasGrounded)
        {
            // ������ �뽬 ��Ÿ�� �ʱ�ȭ
            dashCooldownLeft = 0f;
        }
    }

    /// <summary>
    /// ����Ű �ߺ��� �����ϱ� ���� Ÿ�̸ӵ�
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

    // ���� ���� ���� Ȱ��? // Interactions ���
    //public void OnJump(InputAction.CallbackContext context)
    //{
    //    if (context.started)
    //    {
    //        // ��ư�� ������ �������� ��
    //        // ���� �غ� �ִϸ��̼� ��
    //    }

    //    if (context.performed)
    //    {
    //        // ��ư�� ������ ������ ��
    //        // ���� ���� ����
    //    }

    //    if (context.canceled)
    //    {
    //        // ��ư�� ���� ��
    //        // ���� ���� ���� ��
    //    }
    //}
}

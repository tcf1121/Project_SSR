using UnityEngine;

public interface IMonsterJumper
{
    bool IsGrounded();
    bool IsMidJump { get; }
    bool ReadyToJump();
    bool PerformJump(int dir, float dy, float jumpForce, float horizontalFactor, float lockDuration);
    void UpdateTimer(float deltaTime);
    Vector2 LastGroundCheckPos { get; }
    float GroundCheckRadius { get; }
}

public class JumpMove : IMonsterJumper
{
    private Rigidbody2D _rigid;
    private Transform _transfrom;
    private MonsterStatEntry _statData;
    private LayerMask _groundMask;

    public Vector2 LastGroundCheckPos => _rigid.position + groundCheckOffset;
    public float GroundCheckRadius => groundCheckRadius;
    public bool LastGrounded { get; private set; }
    private float jumpTimer;
    private bool isMidJump;

    private Vector2 groundCheckOffset;
    private float groundCheckRadius;

    public void Init(Monster monster, MonsterStatEntry statData)
    {
        _rigid = monster.Rigid;
        _transfrom = monster.Transfrom;
        _statData = statData;
        _groundMask = statData.groundMask;

        groundCheckOffset = statData.groundCheckOffset;
        groundCheckRadius = statData.groundCheckRadius;
    }

    public void UpdateTimer(float deltaTime)
    {
        if (jumpTimer > 0f)
            jumpTimer -= deltaTime;
    }

    public bool IsGrounded()
    {
        Vector2 checkOrigin = _rigid.position + groundCheckOffset;
        bool grounded = Physics2D.OverlapCircle(checkOrigin, groundCheckRadius, _groundMask);

        if (grounded && isMidJump)
            isMidJump = false;

        LastGrounded = grounded;
        return grounded;
    }

    public bool IsMidJump => isMidJump;

    public bool ReadyToJump()
    {
        bool grounded = IsGrounded();
        bool ready = !isMidJump && grounded && jumpTimer <= 0f;
        return ready;
    }

    public bool PerformJump(int dir, float dy, float jumpForce, float horizontalFactor, float lockDuration)
    {
        if (!ReadyToJump())
            return false;



        isMidJump = true;
        jumpTimer = lockDuration > 0 ? lockDuration : _statData.jumpCooldown;

        float yForce = jumpForce * 0.6f;
        float xImpulse = 0.35f;  // 고정된 벽 넘기 힘
        _rigid.velocity = Vector2.zero;

        _rigid.AddForce(Vector2.up * yForce, ForceMode2D.Impulse);
        _rigid.AddForce(Vector2.right * dir * xImpulse * horizontalFactor, ForceMode2D.Impulse);

        return true;
    }
}
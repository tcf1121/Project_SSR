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
        float xImpulse = 0.35f;

        // ★★★ 이 부분을 수정해야 합니다. ★★★
        // 모든 속도를 0으로 초기화하는 대신, 점프 직전 Y축 속도만 0으로 설정합니다.
        // 이는 중력에 의한 기존 낙하 속도를 제거하여 점프 높이를 일관되게 하면서도,
        // 중력의 재적용을 방해하지 않습니다.
        _rigid.velocity = new Vector2(_rigid.velocity.x, 0f); // Y축 속도만 0으로 초기화

        _rigid.AddForce(Vector2.up * yForce, ForceMode2D.Impulse);
        _rigid.AddForce(Vector2.right * dir * xImpulse * horizontalFactor, ForceMode2D.Impulse);

        return true;
    }
}
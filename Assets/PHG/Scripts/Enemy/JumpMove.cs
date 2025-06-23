using UnityEngine;
using PHG;
[RequireComponent(typeof(Rigidbody2D))]
public class JumpMove : MonoBehaviour
{
    [Header("점프 설정")]
    [SerializeField] private float jumpCooldown = 0.8f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float forwardImpulse = 2.5f;

    [Header("센서 및 마스크")]
    [SerializeField] private Transform platformSensor;
    [SerializeField] private Vector2 platformBoxSize = new(1.3f, 1f);
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask jumpableMask;

    private Rigidbody2D rb;
    private float timer;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    public bool TryJumpWallOrPlatform(int dir)
    {
        if (!Ready()) return false;

        // 1. 벽 감지
        Vector2 wallOrigin = (Vector2)transform.position + new Vector2(dir * 0.25f, 0f);
        bool wallAhead = Physics2D.Raycast(wallOrigin, Vector2.right * dir, 0.1f, groundMask);
        Debug.DrawRay(wallOrigin, Vector2.right * 0.1f, Color.magenta);

        // 2. 플랫폼 감지
        bool platformAbove = IsPlatformAbove();

        Debug.Log($"[JUMP] 점프 조건 wall:{wallAhead}, platform:{platformAbove}");

        if (wallAhead || platformAbove)
        {
            // 점프 전 위치 보정 → 벽에 붙는 현상 방지
            Vector2 jumpOrigin = (Vector2)transform.position + new Vector2(dir * 0.2f, 0f);
            rb.position = jumpOrigin;
            return DoJump(dir);
        }

        return false;
    }

    public bool TryJumpToPlatformAbove(int dir, Vector2 targetPos)
    {
        if (!Ready()) return false;

        float yDiff = targetPos.y - transform.position.y;
        if (yDiff < 0.5f) return false;

        // [1] 센서 위치 임시 보정
        Vector3 originalPos = platformSensor.localPosition;
        platformSensor.localPosition = new Vector3(originalPos.x, Mathf.Clamp(yDiff, 1.1f, 3f), originalPos.z);

        // [2] 플랫폼 감지
        bool platformAbove = IsPlatformAbove();

        // [3] 원위치 복구
        platformSensor.localPosition = originalPos;
        if (!platformAbove) return false;

        // [4] 점프력 보정
        float heightScale = Mathf.Clamp01(yDiff / 3f);
        float adjustedJump = Mathf.Lerp(jumpForce * 0.8f, jumpForce * 1.6f, heightScale);

        // 점프 전 위치 보정
        Vector2 jumpOrigin = (Vector2)transform.position + new Vector2(dir * 0.2f, 0f);
        rb.position = jumpOrigin;

        return DoJumpWithForce(dir, adjustedJump, forwardImpulse);
    }

    private bool DoJumpWithForce(int dir, float yForce, float xImpulse)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * yForce, ForceMode2D.Impulse);
        rb.AddForce(Vector2.right * dir * xImpulse, ForceMode2D.Impulse);
        timer = 0;
        return true;
    }

    public bool IsPlatformAbove()
    {
        if (platformSensor == null) return false;
        Collider2D platform = Physics2D.OverlapBox(platformSensor.position, platformBoxSize, 0f, jumpableMask);
        return platform != null;
    }

    private bool Ready()
    {
        timer += Time.deltaTime;
        return timer >= jumpCooldown && IsGrounded();
    }

    private bool DoJump(int dir)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        rb.AddForce(Vector2.right * dir * forwardImpulse, ForceMode2D.Impulse);
        timer = 0;
        return true;
    }

    private bool IsGrounded()
    {
        bool grounded = Physics2D.Raycast(transform.position, Vector2.down, 0.3f, groundMask);
        Debug.DrawRay(transform.position, Vector2.down * 0.3f, grounded ? Color.green : Color.red);
        return grounded;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (platformSensor != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(platformSensor.position, platformBoxSize);
        }
    }
#endif
}
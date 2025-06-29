// PlayerController2D.cs
using UnityEngine;

namespace PHG
{
    /// <summary>
    /// 기본 이동·점프 컨트롤러
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public class PlayerController2D : MonoBehaviour
    {
        /* ──────────────── Member 변수 ──────────────── */
        [SerializeField] private float moveSpeed = 5f;          // ←→ 이동 속도
        [SerializeField] private float jumpForce = 12f;         // 점프 힘
        [SerializeField] private LayerMask groundMask;          // 지면 레이어
        [SerializeField] private Transform groundCheck;         // 발 아래 체크 위치
        [SerializeField] private float groundCheckRadius = 0.15f;

        /* ──────────────── Private ──────────────── */
        private Rigidbody2D rb;
        private bool isGrounded;

        /* ──────────────── Unity Lifecycle ──────────────── */
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.freezeRotation = true;               // 2D 평면 고정
        }

        private void Update()
        {
            Move(Input.GetAxisRaw("Horizontal"));
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded) Jump();
        }

        private void FixedUpdate() => CheckGround();

        /* ──────────────── Core ──────────────── */
        private void Move(float dir /* 매개변수 */)
        {
            rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(dir);
            transform.localScale = scale;
        }

        private void Jump()
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);        // 점프 전 Y속도 리셋
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        private void CheckGround()
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
        }

#if UNITY_EDITOR
        // 에디터에서 GroundCheck 시각화
        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
#endif
    }
}
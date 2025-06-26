// --------------------------------------------------------------------------------------------------------------------
// PlatformChaseAI.cs
// 단순 플랫포머 추적 AI (사다리/복잡 FSM 없이) — "슈퍼마리오 Goomba" + 점프 확장형
// --------------------------------------------------------------------------------------------------------------------
// ✔ 사용 상황: 2D 사이드‑뷰, Platform Effector 2D 환경
// ✔ 기능 요약:
//    1. 플레이어 x 방향으로 이동 (moveSpeed)
//    2. 앞에 벽/낭떠러지 or 플레이어 y 가 높으면 점프 (jumpForce)
//    3. 추격 중 플레이어가 chaseRange 이상 벗어나면 Idle 로 복귀 (옵션)
// --------------------------------------------------------------------------------------------------------------------
// PHG 네임스페이스 필수 규약을 따릅니다 — using PHG; 금지 & 코드 전체를 PHG namespace 감싸기.
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;

namespace PHG
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlatformChaseAI : MonoBehaviour
    {
        /* ------------------------------ Inspector --------------------------- */
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2.5f;  // 행군 속도
        [SerializeField] private float jumpForce = 5.0f;  // 점프 힘 (Impulse)
        [SerializeField] private float jumpCooldown = 0.4f;  // 점프 간격 제한

        [Header("Sensors")]
        [SerializeField] private Transform groundCheck;      // 바닥 확인용
        [SerializeField] private Transform wallCheck;        // 전방 벽 확인용
        [SerializeField] private LayerMask groundMask;       // 지면 레이어 (플랫폼 포함)

        [Header("Logic")]
        [SerializeField] private float chaseRange = 12f;    // 추격 범위 (공백은 무제한)
        [SerializeField] private float jumpHeightTol = 0.4f; // 플레이어가 높을 때 점프 트리거

        /* ------------------------------ privates --------------------------- */
        private Rigidbody2D rb;
        private Transform tf;
        private Transform player;
        private float jumpTimer;

        /* =================================================================== */
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            tf = transform;
        }

        private void Start() => player = GameObject.FindWithTag("Player")?.transform;

        /* ------------------------------------------------------------------- */
        private void FixedUpdate()
        {
            if (player == null) return;

            Vector2 toPl = player.position - tf.position;
            float absDx = Mathf.Abs(toPl.x);
            float absDy = Mathf.Abs(toPl.y);
            int dir = toPl.x > 0 ? 1 : -1;

            // 추격 포기 (선택)
            if (chaseRange > 0f && toPl.magnitude > chaseRange)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            // 방향 전환 (스프라이트 플립)
            Vector3 s = tf.localScale;
            s.x = Mathf.Abs(s.x) * dir;
            tf.localScale = s;

            bool grounded = IsGrounded();
            bool wallAhead = Physics2D.Raycast(wallCheck.position, Vector2.right * dir, 0.1f, groundMask);
            bool shouldJump = grounded && (absDy > jumpHeightTol || wallAhead);

            // 점프 (쿨타임)
            if (shouldJump && jumpTimer <= 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                rb.AddForce(new Vector2(dir * 1.5f, 1f).normalized * jumpForce, ForceMode2D.Impulse);
                jumpTimer = jumpCooldown;
            }
            else
            {
                // 지상 이동
                if (grounded)
                {
                    rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
                }
            }

            if (jumpTimer > 0f) jumpTimer -= Time.fixedDeltaTime;
        }

        /* ------------------------------------------------------------------- */
        private bool IsGrounded()
        {
            return Physics2D.Raycast(groundCheck.position, Vector2.down, 0.05f, groundMask);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (groundCheck)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 0.05f);
            }
            if (wallCheck)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * (tf ? Mathf.Sign(tf.localScale.x) : 1) * 0.1f);
            }
        }
#endif
    }
}

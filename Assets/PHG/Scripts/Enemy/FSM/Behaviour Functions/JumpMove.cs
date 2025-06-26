using UnityEngine;

namespace PHG
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class JumpMove : MonoBehaviour
    {
        [Header("Ground Cast")]
        [SerializeField] private float castRadius = 0.12f;
        [SerializeField] private float castDistance = 0.06f;
        [SerializeField] private LayerMask groundMask;

        [Header("Lock")]
        [SerializeField] private float lockDurationDefault = 0.45f; // fallback

        private Rigidbody2D rb;
        private float timer;
        public bool IsMidJump { get; private set; }

        private void Awake() => rb = GetComponent<Rigidbody2D>();

        private void Update()
        {
            if (timer > 0f) timer -= Time.deltaTime;
            if (IsMidJump && IsGrounded()) IsMidJump = false;
        }

        public bool IsGrounded()
        {
            Vector2 p = rb.position + Vector2.up * 0.02f;
            return Physics2D.CapsuleCast(p, new Vector2(castRadius, castRadius * 2f), CapsuleDirection2D.Vertical,
                                         0f, Vector2.down, castDistance, groundMask);
        }

        public bool Ready() => timer <= 0f && IsGrounded();

        /// <summary>
        /// 수직 힘·수평 임펄스 계수를 Stats 에서 받아 점프 수행.
        /// </summary>
        public bool DoJump(int dir, float dy, float baseJumpForce, float horizontalFactor, float lockDuration)
        {
            if (!Ready() || dy < 0.1f) return false;

            GetAdjustedVals(dy, baseJumpForce, out float yForce, out float xImpulse);

            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * yForce, ForceMode2D.Impulse);
            rb.AddForce(Vector2.right * dir * xImpulse * horizontalFactor, ForceMode2D.Impulse);

            IsMidJump = true;
            timer = lockDuration > 0 ? lockDuration : lockDurationDefault;
            return true;
        }

        private void GetAdjustedVals(float yDiff, float baseJumpForce, out float yForce, out float xImpulse)
        {
            float t = Mathf.Clamp01(yDiff / 4f);
            yForce = Mathf.Lerp(baseJumpForce * 0.8f, baseJumpForce * 1.25f, t);
            xImpulse = Mathf.Lerp(0.55f, 0.15f, t); // 기본값 완화
        }

    }
}
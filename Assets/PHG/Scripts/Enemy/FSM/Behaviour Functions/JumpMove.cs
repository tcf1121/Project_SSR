using UnityEngine;

namespace PHG
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class JumpMove : MonoBehaviour
    {
        [SerializeField] private float castRadius = 0.12f;
        [SerializeField] private float castDistance = 0.06f;
        [SerializeField] private LayerMask groundMask;

        [Header("Jump Lock")]
        [SerializeField] private float lockDurationDefault = 0.45f; // 점프 후 재점프 방지 쿨다운

        [SerializeField] public float maxJumpYDiffForAdjustment = 4f;

        private Rigidbody2D rb;
        private float timer;
        public bool IsMidJump { get; private set; }

        private void Awake() => rb = GetComponent<Rigidbody2D>();

        private void Update()
        {
            if (timer > 0f) timer -= Time.deltaTime;

            // 착지했는지 확인하고 IsMidJump 상태를 업데이트합니다.
            if (IsMidJump && IsGrounded())
            {
                IsMidJump = false;
            }
        }

        public bool IsGrounded()
        {
            // 팁: 만약 IsGrounded가 계속 true를 반환한다면,
            // 몬스터의 레이어가 groundMask에 포함되어 있는지 확인해보세요.
            // 몬스터 자신을 땅으로 인식하는 경우일 수 있습니다.
            Vector2 p = rb.position + Vector2.up * 0.02f;
            return Physics2D.CapsuleCast(p, new Vector2(castRadius * 2f, castRadius * 2f), CapsuleDirection2D.Vertical,
                                          0f, Vector2.down, castDistance, groundMask);
        }

        /// <summary>
        /// 점프를 수행할 준비가 되었는지 확인합니다.
        /// 쿨다운이 끝났고, 땅에 있으며, 현재 점프 중이 아닐 때만 true를 반환합니다.
        /// </summary>
        public bool ReadyToPerformJump() => timer <= 0f && IsGrounded() && !IsMidJump;

        /// <summary>
        /// 수직 힘·수평 임펄스 계수를 받아 점프 수행.
        /// </summary>
        public bool DoJump(int dir, float dy, float baseJumpForce, float horizontalFactor, float lockDuration)
        {
            // ReadyToPerformJump가 모든 조건을 확인하므로, 이 검사 하나로 충분합니다.
            if (!ReadyToPerformJump() || dy < 0.1f) return false;

            GetAdjustedVals(dy, baseJumpForce, out float yForce, out float xImpulse);

            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * yForce, ForceMode2D.Impulse);
            rb.AddForce(Vector2.right * dir * xImpulse * horizontalFactor, ForceMode2D.Impulse);

            IsMidJump = true; // 점프 시작을 명시
            timer = lockDuration > 0 ? lockDuration : lockDurationDefault;
            return true;
        }

        private void GetAdjustedVals(float dy, float currentBaseJumpForce, out float yForce, out float xImpulse)
        {
            float t = Mathf.Clamp01(dy / maxJumpYDiffForAdjustment);
            yForce = Mathf.Lerp(currentBaseJumpForce * 0.8f, currentBaseJumpForce * 1.25f, t);
            xImpulse = Mathf.Lerp(0.55f, 0.15f, t);
        }

        private void OnDrawGizmosSelected()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            Vector2 p = rb.position + Vector2.up * 0.02f;
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawLine(p, p + Vector2.down * castDistance);
            Gizmos.DrawWireSphere(p + Vector2.down * castDistance, castRadius);
        }
    }
}
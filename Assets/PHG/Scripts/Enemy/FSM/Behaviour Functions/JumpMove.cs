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

        [Header("Jump Lock")]
        [SerializeField] private float lockDurationDefault = 0.45f; // 점프 후 재점프 방지 쿨다운

        // AI가 DoJump에 넘겨주는 baseJumpForce, horizontalFactor 등의 최종값에 따라
        // 점프력을 동적으로 조절하는 내부 로직에 사용될 보간 기준 값.
        // 이 값들은 JumpMove 자체의 설정이므로 외부 AI 설정과 분리.
        [SerializeField] public float maxJumpYDiffForAdjustment = 4f; // yDiff를 이 값으로 나누어 Clamp01의 t 값을 만듦

        private Rigidbody2D rb;
        private float timer;
        public bool IsMidJump { get; private set; }

        private void Awake() => rb = GetComponent<Rigidbody2D>();

        private void Update()
        {
            if (timer > 0f) timer -= Time.deltaTime;
            if (IsMidJump && IsGrounded()) IsMidJump = false;
        }

        // 현재 땅에 닿아있는지 확인
        public bool IsGrounded()
        {
            Vector2 p = rb.position + Vector2.up * 0.02f;
            return Physics2D.CapsuleCast(p, new Vector2(castRadius * 2f, castRadius * 2f), CapsuleDirection2D.Vertical,
                                         0f, Vector2.down, castDistance, groundMask);
        }

        // 점프할 준비가 되었는지 (쿨다운X, 땅에 닿음)
        public bool ReadyToPerformJump() => timer <= 0f && IsGrounded();

        /// <summary>
        /// 수직 힘·수평 임펄스 계수를 받아 점프 수행.
        /// 이 메서드는 JumpMove가 점프 준비가 되었을 때 외부 AI (ChaseState)에서 호출.
        /// </summary>
        /// <param name="dir">점프할 수평 방향 (-1:왼쪽, 1:오른쪽)</param>
        /// <param name="yDiff">점프할 대상과의 Y차이. 이 값으로 점프력을 동적으로 조절.</param>
        /// <param name="baseJumpForce">AI가 지정하는 기본 점프 힘.</param>
        /// <param name="horizontalFactor">AI가 지정하는 수평 임펄스 계수.</param>
        /// <param name="lockDuration">AI가 지정하는 점프 후 락 시간. 0이면 lockDurationDefault 사용.</param>
        public bool DoJump(int dir, float yDiff, float baseJumpForce, float horizontalFactor, float lockDuration)
        {
            if (!ReadyToPerformJump() || yDiff < 0.1f) return false; // yDiff < 0.1f는 유효하지 않은 점프라고 판단하는 최소값

            // yDiff에 따라 점프 힘과 수평 임펄스 계수 조정
            GetAdjustedVals(yDiff, baseJumpForce, out float yForce, out float xImpulse);

            rb.velocity = new Vector2(rb.velocity.x, 0f); // Y속도를 0으로 초기화하여 일관된 점프 시작
            rb.AddForce(Vector2.up * yForce, ForceMode2D.Impulse);
            rb.AddForce(Vector2.right * dir * xImpulse * horizontalFactor, ForceMode2D.Impulse);

            IsMidJump = true;
            timer = lockDuration > 0 ? lockDuration : lockDurationDefault; // AI가 지정한 락 시간이 없으면 기본값 사용
            return true;
        }

        // yDiff에 따라 점프 힘과 수평 임펄스 계수 조정하는 내부 메서드
        private void GetAdjustedVals(float yDiff, float currentBaseJumpForce, out float yForce, out float xImpulse)
        {
            // yDiff가 maxJumpYDiffForAdjustment에 가까울수록 점프 힘을 높이고 수평 임펄스를 줄임
            float t = Mathf.Clamp01(yDiff / maxJumpYDiffForAdjustment); // yDiff를 0~1 범위로 정규화
            yForce = Mathf.Lerp(currentBaseJumpForce * 0.8f, currentBaseJumpForce * 1.25f, t);
            xImpulse = Mathf.Lerp(0.55f, 0.15f, t); // 높이 점프 시 수평 이동 완화
        }

        // 디버그 시각화를 위한 Gizmos (IsGrounded 영역만)
        private void OnDrawGizmosSelected()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();

            // IsGrounded() 영역 시각화
            Vector2 p = rb.position + Vector2.up * 0.02f;
            Vector2 capsuleSize = new Vector2(castRadius * 2f, castRadius * 2f);
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawLine(p, p + Vector2.down * castDistance);
            Gizmos.DrawWireSphere(p + Vector2.down * castDistance, castRadius);
        }
    }
}
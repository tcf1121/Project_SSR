using UnityEngine;

namespace PHG
{

    [RequireComponent(typeof(Rigidbody2D))]
    public class JumpMove : MonoBehaviour
    {
        [Header("점프 설정")]
        [SerializeField] private float jumpForce = 6f;
        [SerializeField] private float jumpLockDuration = 1f;

        [Header("센서 및 마스크")]
        [SerializeField] private Transform platformSensor;
        [SerializeField] private Transform wallSensor;
        [SerializeField] private Vector2 platformBoxSize = new(1.3f, 1f);
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private LayerMask jumpableMask;

        private Rigidbody2D rb;
        private float jumpLockTimer;
        private bool isMidJump;

        public bool IsMidJump => isMidJump;

        private void Awake() => rb = GetComponent<Rigidbody2D>();

        private void Update()
        {
            if (jumpLockTimer > 0f)
                jumpLockTimer -= Time.deltaTime;
            if (isMidJump && IsGrounded())
                isMidJump = false;
        }

        public bool TryJumpWallOrPlatform(int dir, float targetY)
        {
            Vector2 wallOrigin = wallSensor != null
                ? (Vector2)wallSensor.position
                : (Vector2)transform.position + new Vector2(dir * 0.25f, 0f);

            float wallDistance = 0.15f;
            bool wallAhead = Physics2D.Raycast(wallOrigin, Vector2.right * dir, wallDistance, groundMask);
            Debug.DrawRay(wallOrigin, Vector2.right * dir * wallDistance, Color.magenta);

            bool platformAbove = IsPlatformAbove();
            if (!wallAhead && !platformAbove) return false;

            float yDiff = targetY - transform.position.y;
            if (yDiff < 0.3f) return false; // ← 위가 아니면 무조건 안 뛰게 보조 필터
            GetAdjustedJumpValues(yDiff, out float adjustedJump, out float adjustedImpulse);
            return DoJumpWithForce(dir, adjustedJump, adjustedImpulse);
        }

        public bool TryJumpToPlatformAbove(int dir, Vector2 targetPos)
        {
            float yDiff = targetPos.y - transform.position.y;
            if (yDiff < 0.5f) return false;

            Vector3 originalPos = platformSensor.localPosition;
            platformSensor.localPosition = new Vector3(originalPos.x, Mathf.Clamp(yDiff, 1.1f, 3f), originalPos.z);
            bool platformAbove = IsPlatformAbove();
            platformSensor.localPosition = originalPos;

            if (!platformAbove) return false;

            GetAdjustedJumpValues(yDiff, out float adjustedJump, out float adjustedImpulse);
            return DoJumpWithForce(dir, adjustedJump, adjustedImpulse);
        }

        private void GetAdjustedJumpValues(float yDiff, out float adjustedJump, out float adjustedImpulse)
        {
            float heightScale = Mathf.Clamp01(yDiff / 4f);
            adjustedJump = Mathf.Lerp(jumpForce * 1f, jumpForce * 1.6f, heightScale);
            adjustedImpulse = Mathf.Lerp(0.6f, 0.2f, heightScale);
        }

        private bool DoJumpWithForce(int dir, float yForce, float xImpulse)
        {
            Vector2 jumpOrigin = (Vector2)transform.position + new Vector2(dir * 0.2f, 0f);
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * yForce, ForceMode2D.Impulse);
            jumpLockTimer = jumpLockDuration;
            isMidJump = true;
            return true;
        }

        public bool IsPlatformAbove()
        {
            Collider2D platform = Physics2D.OverlapBox(platformSensor.position, platformBoxSize, 0f, jumpableMask);
            return platform != null;
        }

        public bool Ready()
        {
            return jumpLockTimer <= 0f && IsGrounded();
        }

        private bool IsGrounded()
        {
            Vector2 origin = (Vector2)transform.position + Vector2.down * 0.05f;
            float checkDist = 0.1f;
            LayerMask totalMask = groundMask | jumpableMask;
            return Physics2D.Raycast(origin, Vector2.down, checkDist, totalMask);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (platformSensor != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(platformSensor.position, platformBoxSize);
            }
            if (wallSensor != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(wallSensor.position, 0.05f);
            }
        }
#endif
    }
}
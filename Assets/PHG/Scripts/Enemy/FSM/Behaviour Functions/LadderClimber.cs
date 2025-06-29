using System.Collections;
using UnityEngine;

namespace PHG
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class LadderClimber : MonoBehaviour
    {
        /* ────── inspector tunables ────── */
        [Header("Climb Settings")]
        [SerializeField] private float climbSpeed = 3f;                // (멤버 변수) 등반 속도
        [SerializeField] private float alignSpeed = 4f;                // (멤버 변수) X 중앙 정렬 속도
        [SerializeField] private float jumpAwayForce = 3f;             // (멤버 변수) 안착 임펄스
        [SerializeField] private float climbYThreshold = 0.7f;         // (멤버 변수) y 차 최소값
        [SerializeField] private LayerMask ladderMask;                 // (멤버 변수) Ladder 레이어
        [SerializeField] private float detectRadius = 0.35f;           // (멤버 변수) Overlap 감지 반경
        [SerializeField] private Vector2 forwardOffset = new(0.15f, 0); // (멤버 변수) 전방 오프셋
        [SerializeField] private bool debugDraw = false;               // (멤버 변수) 기즈모
        [SerializeField] private bool verboseLog = true;               // (멤버 변수) 상세 로그
        [SerializeField] private bool showThreshold = true;

        /* ────── const ────── */
        private const string CLIMBING_LAYER_NAME = "MonsterClimbing";
        private const float CLIMB_COOLDOWN = 0.75f;

        /* ────── refs ────── */
        private Rigidbody2D rb;              // (멤버 변수) 리지드바디
        private Transform tf;              // (멤버 변수) 트랜스폼
        private MonsterBrain brain;           // (멤버 변수) AI 브레인

        /* ────── state ────── */
        public bool IsClimbing { get; private set; }
        private float cooldownTimer;
        public float MinYThreshold => climbYThreshold;

        /* ================================================================= */
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            tf = transform;
            brain = GetComponent<MonsterBrain>();
        }

        /* ================================================================= */
        private void Update()
        {
            if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
            if (IsClimbing || cooldownTimer > 0f) return;
            if (brain.Sm.CurrentStateID != StateID.Chase || !brain.CanClimbLadders) return;

            /* ―― 사다리 탐색 ―― */
            int facingDir = tf.localScale.x >= 0 ? 1 : -1;
            Vector2 probe = (Vector2)tf.position + forwardOffset * facingDir;
            Collider2D[] hs = Physics2D.OverlapCircleAll(probe, detectRadius, ladderMask);
            if (hs.Length == 0) { Log($"[Update] No ladder hit (dir {facingDir})"); return; }

            TryBeginClimb(hs[0]);               // 가장 가까운 것 생략
        }

        private void OnTriggerEnter2D(Collider2D other) => TryBeginClimb(other);
        private void OnTriggerStay2D(Collider2D other) => TryBeginClimb(other);

        /* ----------------------------------------------------------------- */
        private void TryBeginClimb(Collider2D other)
        {
            Log($"TryBeginClimb ▶ {other.name}");

            if (IsClimbing) { Log("✗ already climbing"); return; }
            if (cooldownTimer > 0f) { Log("✗ cooldown"); return; }
            if (((1 << other.gameObject.layer) & ladderMask) == 0)
            { Log("✗ not Ladder layer"); return; }

            if (brain.Sm.CurrentStateID != StateID.Chase) { Log("✗ not Chase state"); return; }
            if (!brain.CanClimbLadders) { Log("✗ CanClimb=false"); return; }

            Transform player = ChaseState.Player;
            if (player == null) { Log("✗ player=null"); return; }

            float yDiff = player.position.y - tf.position.y;
            if (Mathf.Abs(yDiff) < climbYThreshold)
            { Log($"✗ yDiff {yDiff:F2} < {climbYThreshold}"); return; }

            LadderBounds lb = other.GetComponentInParent<LadderBounds>();
            if (lb == null) { Log("✗ LadderBounds missing"); return; }

            StartCoroutine(ClimbRoutine(lb, yDiff > 0f, player));
        }

        /* ----------------------------------------------------------------- */
        private IEnumerator ClimbRoutine(LadderBounds lb, bool goUp, Transform player)
        {
            IsClimbing = true;

            int originalLayer = gameObject.layer;
            int climbLayer = LayerMask.NameToLayer(CLIMBING_LAYER_NAME);
            if (climbLayer != -1) gameObject.layer = climbLayer;

            rb.simulated = false;

            /* X 정렬 */
            float xMid = lb.bottom.position.x;
            tf.position = new Vector3(xMid, tf.position.y, tf.position.z);
            Log("⟶ X 스냅 완료");
            yield return null; // 충돌 갱신

            /* 실제 등반 */
            if (goUp)
            {
                while (tf.position.y < lb.top.position.y - 0.05f)
                {
                    float newY = Mathf.MoveTowards(tf.position.y, lb.top.position.y, climbSpeed * Time.deltaTime);
                    tf.position = new Vector3(tf.position.x, newY, tf.position.z);
                    Log($" climbing ↑  y={newY:F2}");
                    yield return null;
                }
                Log("⟶ Reached TOP");
            }
            else
            {
                while (tf.position.y > lb.bottom.position.y + 0.05f)
                {
                    float newY = Mathf.MoveTowards(tf.position.y, lb.bottom.position.y, climbSpeed * Time.deltaTime);
                    tf.position = new Vector3(tf.position.x, newY, tf.position.z);
                    Log($" climbing ↓  y={newY:F2}");
                    yield return null;
                }
                Log("⟶ Reached BOTTOM");
            }

            /* 안착 처리 */
            rb.simulated = true;
            gameObject.layer = originalLayer;
            IsClimbing = false;
            cooldownTimer = CLIMB_COOLDOWN;

            if (player != null)
            {
                int hDir = player.position.x > tf.position.x ? 1 : -1;
                Vector2 imp = new(hDir * jumpAwayForce, jumpAwayForce * 0.5f);
                rb.AddForce(imp, ForceMode2D.Impulse);
                Log("⟶ Impulse off ladder");
            }

            brain.ChangeState(StateID.Chase);
        }

        /* ----------------------------------------------------------------- */
        public void ScanAheadAndClimb(int dir)
        {
            if (IsClimbing || cooldownTimer > 0f) return;

            Vector2 probe = (Vector2)tf.position + forwardOffset * dir;
            Collider2D col = Physics2D.OverlapCircle(probe, detectRadius, ladderMask);
            Log($"[ScanAhead] dir {dir} → {(col ? col.name : "null")}");
            if (col != null) TryBeginClimb(col);
        }

        /* ----------------------------------------------------------------- */
        private void OnDrawGizmosSelected()
        {
            if (!debugDraw) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + (Vector3)forwardOffset, detectRadius);

            if (showThreshold) DrawThesholdGizmo();

        }

        private void DrawThesholdGizmo()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f); //주황색

            //위아래 경계선
            Vector3 pos = transform.position;
            Vector3 upLine = pos + Vector3.up * climbYThreshold;
            Vector3 dnLine = pos + Vector3.down * climbYThreshold;

            float halfW = 0.4f; //가시성용 가로길이

            Gizmos.DrawLine(upLine + Vector3.left * halfW, upLine + Vector3.right * halfW);
            Gizmos.DrawLine(dnLine + Vector3.left * halfW, dnLine + Vector3.right * halfW);

            //두선 연결
            Gizmos.DrawLine(upLine, dnLine);

        }

        /* ----------------------------------------------------------------- */
        private void Log(string msg)
        {
            //if (verboseLog) Debug.Log($"[LadderClimber] {msg}", this);
        }
    }
}
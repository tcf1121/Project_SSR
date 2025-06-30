using System.Collections;
using UnityEngine;

namespace PHG
{
    public interface IMonsterClimber
    {
        bool IsClimbing { get; }
        void Init(MonsterBrain brain);
        void TryFindAndClimb(int dir, Vector2 playerPos);
        void UpdateClimbTimer(float dt);
    }

    public class LadderClimber : IMonsterClimber
    {
        private float climbSpeed = 3f;
        private float alignSpeed = 4f;
        private float jumpAwayForce = 3f;
        private float climbYThreshold = 0.7f;
        private LayerMask ladderMask;
        private float detectRadius = 0.35f;
        private Vector2 forwardOffset = new(0.15f, 0);

        private const string CLIMBING_LAYER_NAME = "MonsterClimbing";
        private const float CLIMB_COOLDOWN = 0.75f;

        private Rigidbody2D rb;
        private Transform tf;
        private MonsterBrain brain;

        public bool IsClimbing { get; private set; }
        private float cooldownTimer;

        public float MinYThreshold => climbYThreshold;
        public Vector2 ForwardOffset => forwardOffset;
        public float DetectRadius => detectRadius;

        public void Init(MonsterBrain brain)
        {
            this.brain = brain;
            this.rb = brain.rb;
            this.tf = brain.tf;

            var stat = brain.statData;
            climbSpeed = stat.climbSpeed;
            forwardOffset = stat.ladderForwardOffset;
            detectRadius = stat.ladderDetectRadius;
            climbYThreshold = stat.climbYThreshold;
            ladderMask = stat.ladderMask;

            Debug.Log($"[LadderClimber] Init 완료: offset={forwardOffset}, radius={detectRadius}, threshold={climbYThreshold}");
        }

        public void UpdateClimbTimer(float dt)
        {
            if (cooldownTimer > 0f)
                cooldownTimer -= dt;
        }

        public void TryFindAndClimb(int dir, Vector2 playerPos)
        {
            if (IsClimbing || cooldownTimer > 0f) return;

            Vector2 probe = (Vector2)tf.position + forwardOffset * dir;
            Collider2D col = Physics2D.OverlapCircle(probe, detectRadius, ladderMask);

#if UNITY_EDITOR
            Debug.DrawLine(tf.position, probe, Color.cyan, 1f);
#endif


            if (col == null) return;

            float yDiff = playerPos.y - tf.position.y;
            Debug.Log($"[LadderClimber] yDiff={yDiff}, climbYThreshold={climbYThreshold}");

            // 예외 처리: 플레이어가 아래에 있고 yDiff가 작지만 여전히 bottom에 가까울 때는 내려가야 함
            if (Mathf.Abs(yDiff) < climbYThreshold)
            {
                LadderBounds tmpLB = col.GetComponentInParent<LadderBounds>();
                if (tmpLB != null)
                {
                    bool goDownBecausePlayerClearlyBelow = (yDiff < 0 && tf.position.y > tmpLB.bottom.position.y + 0.2f);
                    if (!goDownBecausePlayerClearlyBelow) return;
                }
                else
                {
                    return;
                }
            }

            LadderBounds lb = col.GetComponentInParent<LadderBounds>();
            if (lb == null)
            {
                Debug.LogWarning("[LadderClimber] LadderBounds 없음");
                return;
            }

            brain.StartCoroutine(ClimbRoutine(lb, yDiff > 0f, playerPos));
        }

        private IEnumerator ClimbRoutine(LadderBounds lb, bool goUp, Vector2? playerPos)
        {
            IsClimbing = true;

            int originalLayer = brain.gameObject.layer;
            int climbLayer = LayerMask.NameToLayer(CLIMBING_LAYER_NAME);
            if (climbLayer != -1) brain.gameObject.layer = climbLayer;

            rb.simulated = false;

            float xMid = lb.bottom.position.x;
            tf.position = new Vector3(xMid, tf.position.y, tf.position.z);
            yield return null;

            if (goUp)
            {
                while (tf.position.y < lb.top.position.y - 0.05f)
                {
                    float newY = Mathf.MoveTowards(tf.position.y, lb.top.position.y, climbSpeed * Time.deltaTime);
                    tf.position = new Vector3(tf.position.x, newY, tf.position.z);
                    yield return null;
                }
            }
            else
            {
                while (tf.position.y > lb.bottom.position.y + 0.05f)
                {
                    float newY = Mathf.MoveTowards(tf.position.y, lb.bottom.position.y, climbSpeed * Time.deltaTime);
                    tf.position = new Vector3(tf.position.x, newY, tf.position.z);
                    yield return null;
                }
            }

            rb.simulated = true;
            brain.gameObject.layer = originalLayer;
            IsClimbing = false;
            cooldownTimer = CLIMB_COOLDOWN;

            if (playerPos.HasValue)
            {
                int hDir = playerPos.Value.x > tf.position.x ? 1 : -1;
                Vector2 imp = new(hDir * jumpAwayForce, jumpAwayForce * 0.5f);
                rb.AddForce(imp, ForceMode2D.Impulse);
            }

            brain.ChangeState(StateID.Chase);
        }
    }
}

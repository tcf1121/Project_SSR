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
        private float jumpAwayForce = 3f; // Current code might not directly use this but good to keep.
        private float climbYThreshold = 0.7f; // Minimum Y offset from ladder bottom to start climbing.
        private LayerMask ladderMask;
        private float detectRadius = 0.35f;
        private Vector2 forwardOffset = new(0.15f, 0);

        private const string CLIMBING_LAYER_NAME = "MonsterClimbing";
        private const float CLIMB_COOLDOWN = 0.75f;

        // Exit related constants (adjusted for stronger push)
        // 상수 값을 더욱 공격적으로 상향 조정하여 강제 이탈 보장
        private const float LADDER_EXIT_JUMP_FORCE_Y = 8.0f; // 이전 5.0f -> 8.0f (더 높은 점프)
        private const float LADDER_EXIT_HORIZONTAL_VELOCITY = 7.0f; // 이전 4.0f -> 7.0f (더 빠른 수평 이동)
        // Increased buffer for monster's feet to clear the ladder top.
        private const float LADDER_TOP_EXIT_HEIGHT_OFFSET = 1.0f; // 이전 0.75f -> 1.0f (더 높이 순간이동)
        private const float FORCE_EXIT_X_OFFSET = 1.0f; // 이전 0.75f -> 1.0f (더 멀리 수평 순간이동)

        // Constants for detecting if the monster is stuck at the ladder top and forcing an exit.
        private const float CLIMB_STUCK_VELOCITY_THRESHOLD = 0.05f; // Vertical velocity below this is considered 'stuck'.
        private const float CLIMB_STUCK_DURATION = 0.7f; // Time (in seconds) the monster must be stuck for.
        // Within this distance from the top limit, stuck detection begins.
        private const float CLIMB_STUCK_NEAR_TOP_BUFFER = 0.6f;

        private Monster _monster; // Reference to Monster component
        private Rigidbody2D rb;
        private Transform tf;
        private MonsterBrain brain;
        private BoxCollider2D monsterHitBox; // Added direct reference to Monster's HitBox

        public bool IsClimbing { get; private set; }
        private float cooldownTimer;
        private float _stuckClimbTimer; // Stuck detection timer

        public float MinYThreshold => climbYThreshold;
        public Vector2 ForwardOffset => forwardOffset;
        public float DetectRadius => detectRadius;

        public void Init(MonsterBrain brain)
        {
            this.brain = brain;
            this._monster = brain.Monster; // Initialize _monster field via MonsterBrain
            this.rb = brain.Monster.Rigid; // Get Rigidbody2D from Monster via MonsterBrain
            this.tf = brain.Monster.transform; // Get Transform from Monster via MonsterBrain
            this.monsterHitBox = brain.Monster.HitBox; // Get HitBox from Monster via MonsterBrain

            var stat = brain.StatData;
            climbSpeed = stat.climbSpeed;
            forwardOffset = stat.ladderForwardOffset;
            detectRadius = stat.ladderDetectRadius;
            climbYThreshold = stat.climbYThreshold;
            ladderMask = stat.ladderMask;

            // Debug.Log($"[LadderClimber] Init 완료: offset={forwardOffset}, radius={detectRadius}, threshold={climbYThreshold}");
        }

        public void UpdateClimbTimer(float dt)
        {
            if (cooldownTimer > 0f)
                cooldownTimer -= dt;
        }

        public void TryFindAndClimb(int dir, Vector2 playerPos)
        {
            if (IsClimbing || cooldownTimer > 0f) return;

            // Check for a ladder ahead of the monster from its current position by forwardOffset.
            Vector2 probe = (Vector2)tf.position + forwardOffset * Mathf.Sign(tf.localScale.x);
            Collider2D col = Physics2D.OverlapCircle(probe, detectRadius, ladderMask);

#if UNITY_EDITOR
            Debug.DrawLine(tf.position, probe, Color.cyan, 1f); //
#endif

            if (col == null) return; // If no ladder, don't try to climb.

            float yDiff = playerPos.y - tf.position.y; //
            LadderBounds lb = col.GetComponentInParent<LadderBounds>(); //
            if (lb == null) //
            {
                Debug.LogWarning("[LadderClimber] LadderBounds 컴포넌트를 찾을 수 없습니다."); //
                return; // Cannot climb without LadderBounds.
            }

            bool goUp = yDiff > 0; // Climb up if player is above the monster.
            bool canClimb = false;

            if (goUp) //
            {
                // When going up: Cannot climb if monster is not grounded AND below the ladder's bottom.
                if (!_monster.Brain.IsGrounded() && tf.position.y < lb.bottom.position.y) //
                {
                    return;
                }
                // Can climb if monster can reach the top of the ladder.
                canClimb = tf.position.y < lb.top.position.y + monsterHitBox.size.y * 0.5f; //
            }
            else // When going down.
            {
                // Can climb if monster is sufficiently above the ladder's bottom (considering climbYThreshold).
                canClimb = tf.position.y > lb.bottom.position.y + climbYThreshold; //
            }

            if (!canClimb) //
            {
                return; // If climb conditions are not met, exit.
            }

            // Start climb coroutine.
            brain.StartCoroutine(ClimbRoutine(lb, goUp));
        }

        private IEnumerator ClimbRoutine(LadderBounds lb, bool initialGoUp) // Removed playerPos parameter as it's directly queried
        {
            IsClimbing = true; //
            int originalLayer = brain.gameObject.layer; //
            int climbLayer = LayerMask.NameToLayer(CLIMBING_LAYER_NAME); //
            if (climbLayer != -1) brain.gameObject.layer = climbLayer; // Change monster layer to climbing layer

            rb.isKinematic = true; // <--- 변경: isKinematic 유지
            rb.velocity = Vector2.zero; // Clear existing velocity (prevents issues from residual velocity)
            _monster.PlayAnim(AnimNames.Walk); // Play climbing animation (e.g., walk animation)

            _stuckClimbTimer = 0f; // Initialize stuck detection timer

            // Align X-axis to the center of the ladder.
            float xMid = lb.bottom.position.x; //
            while (Mathf.Abs(tf.position.x - xMid) > 0.05f) // Allow X-axis error margin
            {
                tf.position = Vector3.MoveTowards(tf.position, new Vector3(xMid, tf.position.y, tf.position.z), alignSpeed * Time.deltaTime); //
                yield return null;
            }
            tf.position = new Vector3(xMid, tf.position.y, tf.position.z); // Exactly center the monster

            float tolerance = 0.05f; // Error margin for judging ladder limit reach
            Transform playerTf = _monster.Target; // Get player Transform reference (more reliable than FindWithTag)

            // Calculate ladder bounds based on the monster's "foot" position, not its center.
            // monsterHitBox.offset.y is the Y offset between the collider center and Transform center.
            // monsterHitBox.size.y * 0.1f is a custom 'foot' position correction value.
            float monsterFootToCenterOffset = monsterHitBox.offset.y - monsterHitBox.size.y * 0.1f;
            float climbUpperLimit = lb.top.position.y + monsterFootToCenterOffset + LADDER_TOP_EXIT_HEIGHT_OFFSET;
            float climbLowerLimit = lb.bottom.position.y + monsterFootToCenterOffset;

            bool goUp = initialGoUp; // Use the initial direction

            while (true)
            {
                // Exit ladder if player target is lost or out of range.
                if (playerTf == null || !_monster.PlayerInRange(brain.StatData.chaseRange + 1f))
                {
                    Debug.Log("[ClimbRoutine] 플레이어 범위 이탈 또는 타겟 없음 → 사다리 이탈");
                    break;
                }

                // Continuously check for ladder presence below the monster's foot (prevents falling into void).
                Vector2 checkPos = (Vector2)tf.position + monsterHitBox.offset - Vector2.up * (monsterHitBox.size.y * 0.5f - 0.01f);
                Collider2D currentLadderCol = Physics2D.OverlapCircle(checkPos, detectRadius * 0.5f, ladderMask);
                if (currentLadderCol == null)
                {
                    Debug.Log("[ClimbRoutine] 사다리 이탈 (발 아래 사다리 없음) → 강제 종료");
                    break;
                }

                float yDiffToPlayer = playerTf.position.y - tf.position.y;
                float xDiffToPlayer = Mathf.Abs(playerTf.position.x - tf.position.x);

                // Update direction if player's relative Y changes significantly
                if ((goUp && yDiffToPlayer < -0.5f) || (!goUp && yDiffToPlayer > 0.5f))
                {
                    goUp = yDiffToPlayer > 0;
                }

                float currentMonsterFootY = tf.position.y + monsterFootToCenterOffset;

                // Check if monster's foot has reached the upper/lower ladder limit.
                bool atTopLimit = currentMonsterFootY >= climbUpperLimit - tolerance;
                bool atBottomLimit = !goUp && currentMonsterFootY <= climbLowerLimit + tolerance;

                // Exit conditions:
                // 1. Reached top of ladder.
                // 2. Reached bottom of ladder (and trying to go down).
                // 3. Stuck near top of ladder (detected by timer).
                if (atTopLimit)
                {
                    Debug.Log("[ClimbRoutine] 사다리 상단 도달 → 이탈 시도.");
                    break;
                }
                else if (atBottomLimit)
                {
                    Debug.Log("[ClimbRoutine] 사다리 하단 도달. 등반 종료.");
                    break;
                }
                else
                {
                    // If monster is near the top of the ladder and not moving vertically, start stuck timer.
                    if (goUp && currentMonsterFootY >= (climbUpperLimit - CLIMB_STUCK_NEAR_TOP_BUFFER) && Mathf.Abs(rb.velocity.y) < CLIMB_STUCK_VELOCITY_THRESHOLD)
                    {
                        _stuckClimbTimer += Time.deltaTime;
                        if (_stuckClimbTimer >= CLIMB_STUCK_DURATION)
                        {
                            Debug.Log("[ClimbRoutine] 몬스터가 사다리 상단 근처에서 정지 감지 (강제 이탈) → 이탈 시도.");
                            break; // Exit loop to trigger exit sequence.
                        }
                    }
                    else
                    {
                        _stuckClimbTimer = 0f; // Reset timer if conditions are not met.
                    }

                    // Calculate target Y position for the monster's transform (center) based on player's Y.
                    float targetMonsterFootY = playerTf.position.y;
                    float actualTargetTransformY = targetMonsterFootY - monsterFootToCenterOffset;

                    // Move monster towards the target Y.
                    float newY = Mathf.MoveTowards(tf.position.y, actualTargetTransformY, climbSpeed * Time.deltaTime);
                    tf.position = new Vector3(tf.position.x, newY, tf.position.z);
                }

                yield return null;
            }

            // --- Final Ladder Exit Processing ---
            rb.isKinematic = false; // <--- 변경: isKinematic 유지
            rb.velocity = Vector2.zero; // Clear residual velocity on ladder exit

            if (playerTf != null)
            {
                int playerXDir = (int)Mathf.Sign(playerTf.position.x - tf.position.x);

                Vector3 exitPos = tf.position; // Monster's current position

                // Calculate the target Y for the monster's transform.position (center)
                // to ensure its hitbox bottom is LADDER_TOP_EXIT_HEIGHT_OFFSET away from the ladder top.
                float targetExitCenterY = lb.top.position.y
                                          + (monsterHitBox.size.y * 0.5f - monsterHitBox.offset.y)
                                          + LADDER_TOP_EXIT_HEIGHT_OFFSET;

                // Directly set the monster's Y position to the calculated safe point (teleport).
                // This ensures the monster always clears the ladder top.
                exitPos.y = targetExitCenterY;

                // Apply horizontal offset
                exitPos.x += playerXDir * FORCE_EXIT_X_OFFSET;

                // Apply the calculated exit position to the monster's Transform.
                tf.position = exitPos;

                // Apply jump and horizontal propulsion after position adjustment
                rb.velocity = new Vector2(playerXDir * LADDER_EXIT_HORIZONTAL_VELOCITY, LADDER_EXIT_JUMP_FORCE_Y);
                Debug.Log($"[ClimbRoutine] 사다리 이탈 후 점프 및 수평 추진력 적용: Velocity X={rb.velocity.x:F2}, Y={rb.velocity.y:F2}");
            }
            else
            {
                // If no player target, just give a default jump force.
                rb.velocity = new Vector2(0, LADDER_EXIT_JUMP_FORCE_Y);
            }

            brain.gameObject.layer = originalLayer; // Restore original layer
            IsClimbing = false;
            cooldownTimer = CLIMB_COOLDOWN; // Apply cooldown after climbing

            Debug.Log("[ClimbRoutine] 사다리 등반 코루틴 종료. FSM 상태 전환 시도.");
            brain.ChangeState(StateID.Chase); // Change FSM state to Chase
        }
    }
}
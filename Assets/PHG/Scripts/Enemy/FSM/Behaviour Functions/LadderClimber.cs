using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PHG
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class LadderClimber : MonoBehaviour
    {
        [Header("Climb Settings")]
        [SerializeField] float climbSpeed = 3.0f;
        [SerializeField] float alignSpeed = 4.0f;
        [SerializeField] float jumpAwayForce = 3.0f;
        const string LAYER_CLIMBING = "MonsterClimbing";
        const int LAYER_LADDER = 8; // Ladder 레이어 번호 (에디터에서 확인 필요)

        Rigidbody2D rb;
        Transform tf;
        MonsterBrain brain;
        MonsterStats stats;
        public bool IsClimbing { get; private set; }

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            tf = transform;
            brain = GetComponent<MonsterBrain>();
            stats = GetComponent<MonsterStats>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!brain.CanClimbLadders) return;
            if (IsClimbing) return;
            if (other.gameObject.layer != LAYER_LADDER) return;
            if (stats == null) return;

            // Chase 상태가 아닐 경우 무시 (CurrentStateID 사용 안함 → FSM 정보에서 직접 확인)
            if (brain.Sm.CurrentStateID != StateID.Chase) return;


            Transform player = GameObject.FindWithTag("Player")?.transform;
            if (player != null)
            {
                float dx = player.position.x - tf.position.x;
                float dy = player.position.y - tf.position.y;
                float dist = Vector2.Distance(player.position, tf.position);

                // 플레이어가 가까이 있거나 y차이가 적으면 무시
                if (dist < stats.PatrolRange && Mathf.Abs(dy) < 0.5f)
                    return;
            }

            LadderBounds lb = other.GetComponentInParent<LadderBounds>();
            if (lb != null && Mathf.Abs(tf.position.y - lb.bottom.position.y) < 0.2f)
                StartCoroutine(ClimbRoutine(lb, true));
            else if (lb != null && Mathf.Abs(tf.position.y - lb.top.position.y) < 0.2f)
                StartCoroutine(ClimbRoutine(lb, false));
            if (lb != null && Mathf.Abs(tf.position.y - lb.bottom.position.y) >= 0.2f && Mathf.Abs(tf.position.y - lb.top.position.y) >= 0.2f)
                return;
        }

        IEnumerator ClimbRoutine(LadderBounds lb, bool goUp)
        {
            IsClimbing = true;
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;

            int originalLayer = gameObject.layer;
            int climbingLayer = LayerMask.NameToLayer(LAYER_CLIMBING);
            if (climbingLayer != -1) gameObject.layer = climbingLayer;

            float midX = lb.bottom.position.x;
            tf.position = new Vector3(midX, tf.position.y, tf.position.z);

            if (goUp)
            {
                while (tf.position.y < lb.top.position.y - 0.05f)
                {
                    rb.velocity = Vector2.up * climbSpeed;
                    yield return new WaitForFixedUpdate();
                }
            }
            else
            {
                while (tf.position.y > lb.bottom.position.y + 0.05f)
                {
                    rb.velocity = Vector2.down * climbSpeed;
                    yield return new WaitForFixedUpdate();
                }
            }

            rb.velocity = Vector2.zero;
            rb.gravityScale = 1f;
            IsClimbing = false;
            gameObject.layer = originalLayer;

            Transform player = GameObject.FindWithTag("Player")?.transform;
            if (player != null)
            {
                int dir = player.position.x > tf.position.x ? 1 : -1;
                tf.position += new Vector3(dir * 0.7f, 0.5f, 0f);

                // 🔽 변경 전: 무조건 Chase
                // brain.ChangeState(StateID.Chase);
                rb.AddForce(new Vector2(dir * jumpAwayForce, jumpAwayForce * 0.5f), ForceMode2D.Impulse);
                // 🔽 변경 후: 충분히 가까우면 추적 시작

                brain.ChangeState(StateID.Chase);
            }

        }
    }
}

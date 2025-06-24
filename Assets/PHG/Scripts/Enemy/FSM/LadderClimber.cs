using System.Collections;
using UnityEngine;
using PHG;

[RequireComponent(typeof(Rigidbody2D))]
public class LadderClimber : MonoBehaviour
{
    [Header("Climb Settings")]
    [SerializeField] float climbSpeed = 3.0f;
    [SerializeField] float alignSpeed = 4.0f;
    [SerializeField] float detectRadius = 1.0f;
    const float TOL_X = 0.06f;
    const int LAYER_LADDER = 8;
    const string LAYER_CLIMBING = "MonsterClimbing";

    Rigidbody2D rb;
    Transform tf;
    MonsterBrain brain;
    public bool IsClimbing { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        tf = transform;
        brain = GetComponent<MonsterBrain>();
    }

    public bool TryFindAndClimb(Vector2 playerPos)
    {
        if (IsClimbing) return false;

        LadderBounds targetLadder = FindLadderInPath(playerPos);
        if (targetLadder == null) return false;

        Debug.Log("[Climb] Starting climb coroutine");
        StartCoroutine(ClimbRoutine(targetLadder, playerPos));
        return true;
    }

    IEnumerator ClimbRoutine(LadderBounds lb, Vector2 playerPos)
    {
        IsClimbing = true;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;

        int originalLayer = gameObject.layer;
        int climbingLayer = LayerMask.NameToLayer(LAYER_CLIMBING);
        if (climbingLayer != -1) gameObject.layer = climbingLayer;

        tf.position = new Vector3(lb.bottom.position.x, tf.position.y, tf.position.z);

        bool goUp = playerPos.y > tf.position.y + 0.05f;
        Debug.Log($"[Climb] goUp: {goUp}, tf.y: {tf.position.y}, player.y: {playerPos.y}, bottom.y: {lb.bottom.position.y}, top.y: {lb.top.position.y}");

        if (goUp)
        {
            tf.position = new Vector3(lb.bottom.position.x, lb.bottom.position.y, tf.position.z);
            while (Mathf.Abs(tf.position.y - lb.top.position.y) > 0.05f)
            {
                rb.velocity = Vector2.up * climbSpeed;
                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            tf.position = new Vector3(lb.top.position.x, lb.top.position.y, tf.position.z);
            while (Mathf.Abs(tf.position.y - lb.bottom.position.y) > 0.05f)
            {
                rb.velocity = Vector2.down * climbSpeed;
                yield return new WaitForFixedUpdate();
            }
        }

        rb.velocity = Vector2.zero;
        rb.gravityScale = 1f;
        IsClimbing = false;
        gameObject.layer = originalLayer;
        brain.ChangeState(StateID.Chase);
    }

    LadderBounds FindLadderInPath(Vector2 playerPos)
    {
        Vector2 dirToPlayer = (playerPos - (Vector2)tf.position).normalized;
        Collider2D[] hits = Physics2D.OverlapCircleAll(tf.position, detectRadius, 1 << LAYER_LADDER);

        Debug.DrawLine(tf.position, playerPos, Color.red, 1f);
        Debug.Log($"[ClimbCheck] Ladder Hits: {hits.Length}");

        foreach (Collider2D hit in hits)
        {
            Debug.Log($"[ClimbCheck] Hit: {hit.name}");
            LadderBounds lb = hit.GetComponentInParent<LadderBounds>();
            if (lb == null) continue;

            float dot = Vector2.Dot(dirToPlayer, (lb.bottom.position - tf.position).normalized);
            Debug.Log($"[ClimbCheck] Dot: {dot}, Accept: {dot > 0.5f}");
            if (lb != null) return lb;
        }
        return null;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
#endif
}
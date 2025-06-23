using UnityEngine;
using PHG;
public class ChaseState : IState
{
    private readonly MonsterBrain brain;
    private readonly Rigidbody2D rb;
    private readonly Transform tf;
    private readonly Transform player;
    private readonly JumpMove jumper;

    public ChaseState(MonsterBrain brain)
    {
        this.brain = brain;
        rb = brain.GetComponent<Rigidbody2D>();
        tf = brain.transform;
        player = GameObject.FindWithTag("Player")?.transform;
        jumper = brain.GetComponent<JumpMove>();
    }

    public void Enter() { }

    public void Tick()
    {
        if (player == null)
        {
            brain.ChangeState(StateID.Idle);
            return;
        }

        float distX = player.position.x - tf.position.x;
        float distY = player.position.y - tf.position.y;
        int dir = distX > 0 ? 1 : -1;

        tf.localScale = new Vector3(dir, 1, 1);

        if (Mathf.Abs(rb.velocity.y) > 0.1f) return;

        if (Mathf.Abs(distX) <= brain.attackRange)
        {
            rb.velocity = Vector2.zero;
            brain.ChangeState(StateID.Attack);
            return;
        }

        if (Mathf.Abs(distX) > brain.chaseRange)
        {
            rb.velocity = Vector2.zero;
            brain.ChangeState(StateID.Idle);
            return;
        }

        bool wallAhead = Physics2D.Raycast(brain.sensor.position, Vector2.right * dir, 0.15f, brain.groundMask);
        bool yAbove = distY > 0.8f;
        bool jumpableAbove = jumper != null && jumper.IsPlatformAbove();

        if (jumper != null)
        {
            if (wallAhead)
            {
                if (jumper.TryJumpWallOrPlatform(dir)) return;
            }
            else if (yAbove && jumpableAbove)
            {
                if (jumper.TryJumpToPlatformAbove(dir, player.position)) return;
            }
        }

        rb.velocity = new Vector2(dir * brain.moveSpeed, rb.velocity.y);
    }

    public void Exit() => rb.velocity = Vector2.zero;
}
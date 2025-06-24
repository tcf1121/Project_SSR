using UnityEngine;
using PHG;

public class ChaseState : IState
{
    private readonly MonsterBrain brain;
    private readonly Rigidbody2D rb;
    private readonly Transform tf;
    private readonly JumpMove jumper;

    public ChaseState(MonsterBrain brain)
    {
        this.brain = brain;
        tf = brain.transform;
        rb = brain.GetComponent<Rigidbody2D>();
        jumper = brain.GetComponent<JumpMove>();
    }

    public void Enter() { }

    public void Tick()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            brain.ChangeState(StateID.Patrol);
            return;
        }

        float distance = Vector2.Distance(tf.position, player.transform.position);
        float yDiff = player.transform.position.y - tf.position.y;
        int dir = player.transform.position.x > tf.position.x ? 1 : -1;

        // 너무 높으면 추적 중단
        if (Mathf.Abs(yDiff) > 5f)
        {
            brain.ChangeState(StateID.Patrol);
            return;
        }

        // 시도: 점프해서 위 플랫폼 추적
        if (yDiff > 0.5f && jumper.Ready())
        {
            if (jumper.TryJumpToPlatformAbove(dir, player.transform.position))
                return;
        }

        // 기본 수평 추적
        rb.velocity = new Vector2(dir * brain.Stats.MoveSpeed, rb.velocity.y);

        // 공격 전환 조건
        if (distance <= brain.Stats.AttackRange)
            brain.ChangeState(StateID.Attack);
    }

    public void Exit()
    {
        rb.velocity = Vector2.zero;
    }
}